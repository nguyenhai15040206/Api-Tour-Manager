using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        // Nguyễn Tấn Hải [22/10/2021]
        //
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public CustomerController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // gửi Asset token
        [HttpPost("AccessToken")]
        public async Task<IActionResult> POST([FromBody] Customer CustomerData)
        {
            // Ý nghĩa: khi login sẽ gửi token lại cho user để duy trì đăng nhập
            //
            if (CustomerData != null && CustomerData.Email != null && CustomerData.Password != null)
            {
                var customer = await GetCustomer(CustomerData.Email, CustomerData.Password);
                if (customer != null)
                {
                    // Jwt 
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("MaKhachHang",customer.CustomerId.ToString()),
                        new Claim("TenKhachHang",customer.CustomerName),
                        new Claim("SoDienThoai",customer.PhoneNumber),
                        new Claim("Email",customer.Email)
                    };

                    // mã hóa => set thời gian tồn tại cho token
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"],
                            claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: signIn);

                    // trả dữ liệu cần thiết về cho client (giấu các thông tin như password)
                    return Ok(new {
                        data = new { 
                            customer.CustomerId,
                            customer.CustomerName,
                            customer.Gender,
                            customer.PhoneNumber,
                            customer.Email,
                            customer.Address,
                        },
                        accessToken = new JwtSecurityTokenHandler().WriteToken(token)
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

        // Login với email và mật khẩu
        public async Task<Customer> GetCustomer(string email, string password)
        {
            return await (from c in _context.Customer
                          where c.Email == email && c.Password == password
                            && (c.IsDelete == null || c.IsDelete == false)
                          select c
                          ).FirstOrDefaultAsync();
        }

        // Đăng kí
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Customer>>> Register([FromBody] Customer khachHang)
        {
            // Ý Nghĩa:  đăng kí thông tin khách
            if(khachHang != null)
            {
                _context.Customer.Add(khachHang);
                await _context.SaveChangesAsync();
                return new ObjectResult(khachHang);
            }
            else
            {
                return BadRequest();
            }
        }

        // Update Khách hàng
        [Authorize]
        [HttpPut("{maKhachHang}")]
        public async Task<IActionResult> UpdateCustomer(int customerID, [FromBody] Customer khachHang)
        {
            // Ý nghĩa : Cập nhật thông tin một khách hàng
            if(customerID != khachHang.CustomerId)
            {
                return BadRequest();
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!CustomerExits(customerID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // Kiểm tra tồn tại moot khách hàng
        private bool CustomerExits(int customerID)
        {
            return _context.Customer.Any(m => m.CustomerId == customerID);
        }


    }
}
