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
    }
}
