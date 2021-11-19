﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using QuanLyTourDuLich.SearchModels;
using Newtonsoft.Json;
using System.Globalization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public EmployeeController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // gửi Asset token
        [HttpPost("AccessToken")]
        public async Task<IActionResult> POST([FromBody] Employee empData)
        {
            // Ý nghĩa: khi login sẽ gửi token lại cho user để duy trì đăng nhập
            //
            if (empData != null && empData.UserName != null && empData.Password != null)
            {
                var emp = await GetEmployee(empData.UserName, empData.Password);
                if (emp != null)
                {
                    // Jwt 
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("EmpID",emp.EmpId.ToString()),
                        new Claim("EmpName",emp.EmpName),
                        new Claim("PhoneNumber",emp.PhoneNumber),
                        new Claim("Email",emp.Email)
                    };

                    // mã hóa => set thời gian tồn tại cho token
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"],
                            claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: signIn);

                    // trả dữ liệu cần thiết về cho client (giấu các thông tin như password)
                    return Ok(new
                    {
                        data = new
                        {
                            emp.EmpId,
                            emp.EmpName,
                            emp.Gender,
                            emp.DateOfBirth,
                            emp.WorkingDate,
                            emp.PhoneNumber,
                            emp.Email,
                            emp.Avatar,
                        },
                        accessTokenEmp = new JwtSecurityTokenHandler().WriteToken(token)
                    });
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        // Login với username và mật khẩu
        public async Task<Employee> GetEmployee(string userName, string password)
        {
            return await (from c in _context.Employee
                          where c.UserName == userName && c.Password == password
                            && (c.IsDelete == null || c.IsDelete == true)
                          select c
                          ).FirstOrDefaultAsync();
        }


        //[Thai Tran Kieu Diem][11/08/2021]
        // search danh sách nhân viên với id,name,gender,workingdate,phone,email

        [HttpPost("Adm_GetDataEmployeeList")]
        public async Task<IActionResult> Adm_GetDataEmployeeList([FromBody] EmployeeSearchModel empSearch)
        {
            
            try
            {
                bool checkModelSearchIsNull = true;

                bool isEmployeeId = int.TryParse(empSearch.empID.ToString(), out int empID);
                bool isEmployeeName = (!string.IsNullOrEmpty(empSearch.empName));
                bool isGender = bool.TryParse(empSearch.gender.ToString(), out bool gender);
                bool isWorkingDate = DateTime.TryParse(empSearch.workingDate.ToString(), out DateTime workingdate);
                bool isPhoneNumber = (!string.IsNullOrEmpty(empSearch.phoneNumber));
                bool isEmail = (!string.IsNullOrEmpty(empSearch.email));

                if (isEmployeeId || isEmployeeName || isGender || isWorkingDate || isPhoneNumber || isEmail)
                {
                    checkModelSearchIsNull = false;
                }

                var searchEmp = await (from emp in _context.Employee
                                    where (emp.IsDelete == null || emp.IsDelete == true)
                                    && checkModelSearchIsNull==true? true
                                     : (
                                        (isEmployeeId && emp.EmpId == empSearch.empID)
                                        || (isEmployeeName && emp.EmpName.Contains(empSearch.empName))
                                        || (isGender && emp.Gender == empSearch.gender)
                                        || (isWorkingDate && emp.WorkingDate == empSearch.workingDate)
                                        || (isPhoneNumber && emp.PhoneNumber.Contains(empSearch.phoneNumber))
                                        || (isEmail && emp.Email.Contains(empSearch.email))
                                    )
                                       orderby  emp.DateUpdate descending
                                    select new
                                    {
                                        emp.EmpId,
                                        emp.EmpName,
                                        emp.Gender,
                                        emp.DateOfBirth,
                                        emp.WorkingDate,
                                        emp.PhoneNumber,
                                        emp.Email,
                                        emp.Avatar,
                                        DateUpdate= DateTime.Parse(emp.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                    }).ToListAsync();

                if (searchEmp == null)
                {
                    return NotFound();
                }
                return Ok(searchEmp);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //[Thai Tran Kieu Diem][11/06/2021]
        //get thông tin nhân viên theo mã nhân viên

        [HttpGet("Adm_getEmployeeById/{empId:int}")]  
        public async Task<IActionResult> Adm_GetEmployeeById(int empId)
        {
            try
            {
                var rs = await (from emp in _context.Employee
                                where (emp.IsDelete == null || emp.IsDelete == true)
                                && emp.EmpId == empId
                                select new
                                {
                                    emp.EmpId,
                                    emp.EmpName,
                                    emp.Gender,
                                    emp.DateOfBirth,
                                    emp.WorkingDate,
                                    emp.PhoneNumber,
                                    emp.Email,
                                    emp.Avatar,
                                    DateUpdate = DateTime.Parse(emp.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                }).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }

        }

        // [Thai Tran Kieu Diem][11/06/2021]
        // Thêm nhân viên
        //[Authorize]
        [HttpPost("Adm_CreateEmployee")]
        public async Task<ActionResult<Employee>> Adm_CreateEmployee([FromBody] Employee emp)
        {
            try
            {
                if (emp == null)
                {
                    return BadRequest();
                }
                Employee newEmp = new Employee();
                newEmp.EmpName = emp.EmpName;
                newEmp.Gender = emp.Gender;
                newEmp.DateOfBirth = emp.DateOfBirth;
                newEmp.WorkingDate = DateTime.Now;
                newEmp.PhoneNumber = emp.PhoneNumber;
                newEmp.Email = emp.Email;
                newEmp.UserName = emp.UserName;
                newEmp.Password = emp.Password;
                newEmp.Avatar = emp.Avatar;
                newEmp.DateInsert = DateTime.Now;
                newEmp.DateUpdate = DateTime.Now;
                newEmp.Status = emp.Status;
                newEmp.IsDelete = null;
                await _context.Employee.AddAsync(newEmp);
                await _context.SaveChangesAsync();
                return Ok(newEmp);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }



        // [Thai Tran Kieu Diem][11/06/2021]
        // Sửa thông tin nhân viên
        [HttpPut("Adm_UpdateEmployee/{empID:int}")]
        public async Task<IActionResult> Adm_UpdateEmployee([FromBody] Employee emp, int empID)
        {
            if (empID != emp.EmpId)
            {
                return BadRequest();
            }
            try
            {
                var empUpdate = await (from e in _context.Employee
                                       where (e.IsDelete == null || e.IsDelete == true)
                                       && e.EmpId==empID
                                       select e).FirstOrDefaultAsync();
                if (empUpdate == null)
                {
                    return NotFound();
                }
                empUpdate.EmpName = emp.EmpName;
                empUpdate.Gender = emp.Gender;
                empUpdate.DateOfBirth = emp.DateOfBirth;
                empUpdate.Avatar = emp.Avatar;
                empUpdate.PhoneNumber = emp.PhoneNumber;
                empUpdate.Email = emp.Email;
                empUpdate.DateUpdate = DateTime.Now;
                empUpdate.Status = emp.Status;
                await _context.SaveChangesAsync();
                return Ok(empUpdate);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //[Thai Tran Kieu Diem][11/06/2021]
        //Xóa nhân viên, tình trạng isDelete==true

        [HttpPut("Adm_DeleteEmployee/{empId:int}")]
        public async Task<IActionResult> Adm_DeleteEmployee(int empId)
        {

            try {
                var empDelete = await (from e in _context.Employee
                                       where (e.IsDelete == null || e.IsDelete == false)
                                       && e.EmpId == empId
                                       select e).FirstOrDefaultAsync();
                if (empDelete == null)
                {
                    return NotFound();
                }
                empDelete.DateUpdate = DateTime.Now;
                empDelete.IsDelete = false;
                await _context.SaveChangesAsync();
                return Ok(empDelete);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");


            }
        }

        

    }
}
