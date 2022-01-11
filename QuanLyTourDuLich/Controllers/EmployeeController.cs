using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuanLyTourDuLich.Models;
using QuanLyTourDuLich.SearchModels;
using System;
using System.Globalization;
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

        public const string BaseUrlServer = "http://localhost:8000/ImagesEmployee/";
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
            try
            {
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
                                //emp.Gender,
                                //emp.DateOfBirth,
                                //emp.PhoneNumber,
                                emp.Email,
                                Avatar = emp.Avatar==null? "": BaseUrlServer+ emp.Avatar.Trim(),
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
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
        public async Task<IActionResult> Adm_GetDataEmployeeList([FromBody] EmployeeSearchModel pSearch)
        {

            try
            {
                string[] separatorAddress = { "||" };
                var listObj = await (from emp in _context.Employee
                                     where (emp.IsDelete == null || emp.IsDelete == true)
                                     orderby emp.DateUpdate descending, emp.DateInsert descending
                                     select new
                                     {
                                         emp.EmpId,
                                         emp.EmpName,
                                         Gender = emp.Gender == true ? "Nam" : "Nữ",
                                         DateOfBirth = emp.DateOfBirth == null ? "Chưa cập nhật" : DateTime.Parse(emp.DateOfBirth.ToString()).ToString("dd /MM/yyyy", CultureInfo.InvariantCulture),
                                         WorkingDate = emp.WorkingDate == null ? null : DateTime.Parse(emp.WorkingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                         emp.PhoneNumber,
                                         emp.Email,
                                         emp.Avatar,
                                         Status = emp.Status==null? true: emp.Status,
                                         Address = emp.Address==null? "Chưa cập nhật" : emp.Address.Split(separatorAddress, System.StringSplitOptions.RemoveEmptyEntries)[0].ToString(),
                                         DateUpdate = DateTime.Parse(emp.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                     }).ToListAsync();
                if (pSearch.EmpName != "")
                {
                    listObj = listObj.Where(m => m.EmpName.ToLower().Contains(pSearch.EmpName.ToLower().Trim())).ToList();
                }
                if (pSearch.PhoneNumber != "")
                {
                    listObj = listObj.Where(m => m.PhoneNumber.Contains(pSearch.PhoneNumber.Trim())).ToList();
                }
                if(pSearch.Email !="")
                {
                    listObj = listObj.Where(m => m.Email.ToLower().Contains(pSearch.Email.ToLower().Trim())).ToList();
                }
                return Ok(listObj);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //[Thai Tran Kieu Diem][11/06/2021]
        //get thông tin nhân viên theo mã nhân viên
        // TanHai - Note: Kiem tra ngay null hay khong => de elect ngay
        [HttpGet("Adm_getEmployeeById")]
        public async Task<IActionResult> Adm_GetEmployeeById(Guid? empId = null)
        {
            try
            {
                //var rs = await _context.Employee.FirstOrDefaultAsync(m => m.EmpId == empId);
                var rs = await (from emp in _context.Employee
                                where (emp.IsDelete == null || emp.IsDelete == true)
                                && emp.EmpId == empId
                                select new
                                {
                                    emp.EmpId,
                                    emp.EmpName,
                                    emp.Gender,
                                    // check
                                    emp.DateOfBirth,
                                    emp.WorkingDate,
                                    emp.PhoneNumber,
                                    emp.Email,
                                    avatar = emp.Avatar==null? null: BaseUrlServer + emp.Avatar.Trim(),
                                    emp.UserName,
                                    emp.Password,
                                    emp.Address
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
        [Authorize]
        public async Task<ActionResult<Employee>> Adm_CreateEmployee([FromBody] Employee emp)
        {
            try
            {
                if (emp == null)
                {
                    return BadRequest();
                }

                var phone = _context.Employee.Where(m => m.PhoneNumber == emp.PhoneNumber).Count();
                if (phone > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Số điện thoại đã tồn tại");
                }
                var email = _context.Employee.Where(m => m.Email == emp.Email).Count();
                if (email > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Email đã tồn tại");
                }
                var user = _context.Employee.Where(m => m.UserName == emp.UserName).Count();
                if (user > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Tên người dùng đã tồn tại");
                }

                emp.Avatar = emp.Avatar == "" ? null : emp.Avatar;
                emp.WorkingDate = DateTime.Now.Date;
                emp.DateInsert = DateTime.Now.Date;
                emp.DateUpdate = DateTime.Now.Date;
                emp.IsDelete = null;
                await _context.Employee.AddAsync(emp);
                await _context.SaveChangesAsync();
                return Ok(emp);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }



        // [Thai Tran Kieu Diem][11/06/2021]
        // Sửa thông tin nhân viên
        [HttpPut("Adm_UpdateEmployee")]
        [Authorize]
        public async Task<IActionResult> Adm_UpdateEmployee([FromBody] Employee emp)
        {

            try
            {
                var empUpdate = await (from e in _context.Employee
                                       where (e.IsDelete == null || e.IsDelete == true)
                                       && e.EmpId == emp.EmpId
                                       select e).FirstOrDefaultAsync();
                if (empUpdate == null)
                {
                    return NotFound();
                }
                else
                {
                    if (empUpdate.PhoneNumber != emp.PhoneNumber)
                    {
                        var phone = _context.Employee.Where(m => m.PhoneNumber == emp.PhoneNumber).Count();
                        if (phone > 0)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, "Số điện thoại đã tồn tại");
                        }
                    }
                    if (empUpdate.Email != emp.Email)
                    {
                        var email = _context.Employee.Where(m => m.Email == emp.Email).Count();
                        if (email > 0)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, "Email đã tồn tại");
                        }
                    }
                    empUpdate.EmpName = emp.EmpName;
                    empUpdate.Gender = emp.Gender;
                    empUpdate.DateOfBirth = emp.DateOfBirth;
                    if(emp.Avatar != "")
                    {
                        empUpdate.Avatar = emp.Avatar;
                    }
                    empUpdate.PhoneNumber = emp.PhoneNumber;
                    empUpdate.Email = emp.Email;
                    empUpdate.DateUpdate = DateTime.Now.Date;
                    empUpdate.Address = emp.Address;
                    empUpdate.Status = emp.Status;
                    // ai update ghi ra
                    await _context.SaveChangesAsync();
                    return Ok(empUpdate);
                }

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //[Thai Tran Kieu Diem][11/06/2021]
        //Xóa nhân viên, tình trạng isDelete==true
        [HttpPut("Adm_DeleteEmployee")]
        [Authorize]
        public async Task<IActionResult> Adm_DeleteEmployee([FromBody] DeleteModels pDelete)
        {

            try
            {
                var empDelete = await _context.Employee.Where(m => pDelete.SelectByIds.Contains(m.EmpId)).ToListAsync();
                empDelete.ForEach(m =>
                {
                    m.DateUpdate = DateTime.Now.Date;
                    m.IsDelete = false;
                });
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
