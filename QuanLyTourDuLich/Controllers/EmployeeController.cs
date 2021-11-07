using Microsoft.AspNetCore.Authorization;
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
                            && (c.IsDelete == null || c.IsDelete == false)
                          select c
                          ).FirstOrDefaultAsync();
        }


        //[Thai Tran Kieu Diem][11/06/2021]
        // get danh sách nhân viên với page=1 limit=20

        [HttpGet("EmployeesList")]
        public async Task<IActionResult> GetEmployee(int page=1,int limit = 20)
        {
            try
            {
                var newEmp = await (from emp in _context.Employee
                                    where (emp.IsDelete == null || emp.IsDelete == false)
                                    orderby emp.DateUpdate == null ? emp.DateInsert : emp.DateUpdate descending
                                    select new
                                    {
                                        emp.EmpId,
                                        emp.EmpName,
                                        emp.Gender,
                                        emp.DateOfBirth,
                                        emp.WorkingDate,
                                        emp.PhoneNumber,
                                        emp.Email,
                                        emp.Avatar
                                    }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = newEmp.Count();
                // lay du lieu phan trang, tinh ra duoc tong so trang, page thu may,... Ham nay cu coppy
                var pagination = new Pagination
                {
                    count = totalRecord,
                    currentPage = page,
                    pagsize = limit,
                    totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, limit)),
                    indexOne = ((page - 1) * limit + 1),
                    indexTwo = (((page - 1) * limit + limit) <= totalRecord ? ((page - 1) * limit * limit) : totalRecord)
                };
                // status code 200
                return Ok(new
                {
                    data = newEmp,
                    pagination = pagination

                });
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // [Thai Tran Kieu Diem][11/06/2021]
        // Thêm nhân viên
        //[Authorize]
        [HttpPost("CreateEmp")]
        public async Task<ActionResult<Employee>> CreateEmp([FromBody] Employee emp)
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
                newEmp.DateUpdate = null;
                newEmp.Status = true;
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
        [HttpPut("UpdateEmp/{empID:int}")]
        public async Task<IActionResult> UpdateEmp([FromBody] Employee emp, int empID)
        {
            if (empID != emp.EmpId)
            {
                return BadRequest();
            }
            try
            {
                var empUpdate = await (from e in _context.Employee
                                       where (e.IsDelete == null || e.IsDelete == false)
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

        [HttpPut("DeleteEmp/{empId:int}")]
        public async Task<IActionResult> DeleteEmp(int empId, Employee emp)
        {
            if (empId !=emp.EmpId)
            {
                return BadRequest();
            }
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
                empDelete.IsDelete = true;
                await _context.SaveChangesAsync();
                return Ok(empDelete);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");


            }
        }

        //[Thai Tran Kieu Diem][11/06/2021]
        //get thông tin nhân viên theo mã nhân viên
        
        [HttpGet("getEmpId/{empId:int}")]
        public async Task<Employee> GetEmployeeById(int empId)
        {
                return await (from e in _context.Employee
                                 where (e.IsDelete == null || e.IsDelete == false)
                                 && e.EmpId == empId
                                 select e).FirstOrDefaultAsync();
     
        }

    }
}
