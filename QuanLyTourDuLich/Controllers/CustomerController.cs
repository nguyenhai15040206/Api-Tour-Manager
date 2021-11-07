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
using Microsoft.AspNetCore.Http;

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


        // [Thai Tran Kieu Diem][11/06/2021]
        // get danh sách khách hàng
        [HttpGet("CustomerList")]
        public async Task<IActionResult> GetCustomer_List(int page=1,int limit = 10)
        {
            try {
                var list = await (from kh in _context.Customer
                                  where (kh.IsDelete == null || kh.IsDelete == false)
                                  select new
                                  {
                                      kh.CustomerName,
                                      kh.Gender,
                                      kh.PhoneNumber,
                                      kh.Address
                                  }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = list.Count();
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
                    data = list,
                    pagination = pagination

                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //get danh sách khách hàng theo mã khách hàng
        [HttpGet("{cusId:int}")]
        public async Task<Customer> getCustomerById(int cusId)
        {
            return await (from kh in _context.Customer
                          where (kh.IsDelete == null || kh.IsDelete == false)
                          && kh.CustomerId==cusId
                          select kh).FirstOrDefaultAsync();
        }

        // [Thai Tran Kieu Diem][11/06/2021]
        // Đăng kí khách hàng
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Customer>>> Register([FromBody] Customer khachHang)
        {
            // Ý Nghĩa:  đăng kí thông tin khách
            try {
                if (khachHang != null)
                {
                    Customer kh = new Customer();
                    kh.CustomerName = khachHang.CustomerName;
                    kh.Gender = khachHang.Gender;
                    kh.PhoneNumber = khachHang.PhoneNumber;
                    kh.Email = khachHang.Email;
                    kh.Address = khachHang.Address;
                    kh.Password = khachHang.Password;
                    kh.DateInsert = DateTime.Now;
                    kh.DateUpdate = null;
                    kh.IsDelete = null;
                    _context.Customer.Add(kh);
                    await _context.SaveChangesAsync();
                    return Ok(kh);
                }
                else
                {
                    return BadRequest();
                }
            } 
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        // [Thai Tran Kieu Diem][11/06/2021]
        // Update Khách hàng
        //[Authorize]
        [HttpPut("UpdateCustomer/{customerID:int}")]
        public async Task<IActionResult> UpdateCustomer(int customerID, [FromBody] Customer khachHang)
        {
            // Ý nghĩa : Cập nhật thông tin một khách hàng
            if(customerID != khachHang.CustomerId)
            {
                return BadRequest();
            }
            try
            {
                var UpdateCus = await (from kh in _context.Customer
                                       where (kh.IsDelete == null || kh.IsDelete == false)
                                       && kh.CustomerId == customerID
                                       select kh).FirstOrDefaultAsync();
                if (UpdateCus == null)
                {
                    return NotFound();
                }
                UpdateCus.CustomerName = khachHang.CustomerName;
                UpdateCus.PhoneNumber = khachHang.PhoneNumber;
                UpdateCus.Gender = khachHang.Gender;
                UpdateCus.Address = khachHang.Address;
                UpdateCus.Password = khachHang.Password;
                UpdateCus.DateUpdate = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(UpdateCus);
            }
            catch(DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        // [Thai Tran Kieu Diem][11/06/2021]
        //Vô hiệu hóa tài khoản khách hàng  
        [HttpPut("DeleteCustomer/{customerId:int}")]
        public async Task<IActionResult> DeleteCustomer (int customerId)
        {
            try 
            {
                var delCus = await (from cus in _context.Customer
                                    where (cus.IsDelete == null || cus.IsDelete == false)
                                    && cus.CustomerId == customerId
                                    select cus).FirstOrDefaultAsync();
                if (delCus == null)
                    return NotFound();
                delCus.DateUpdate = DateTime.Now;
                delCus.IsDelete = true;
                await _context.SaveChangesAsync();
                return Ok(delCus);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        // Kiểm tra tồn tại moot khách hàng
        private bool CustomerExits(int customerID)
        {
            return _context.Customer.Any(m => m.CustomerId == customerID);
        }


    }
}
