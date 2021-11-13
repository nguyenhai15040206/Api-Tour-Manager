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
using QuanLyTourDuLich.SearchModels;
using Newtonsoft.Json;

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


        // [Thai Tran Kieu Diem][11/09/2021]
        // search danh sách khách hàng
        [HttpPost("Adm_GetDataCustomerList")]
        public async Task<IActionResult> Adm_GetDataCustomerList([FromBody] CustomerSearchModel customerSearch)
        {
            try {

                

                bool isCustomerId = int.TryParse(customerSearch.customerId.ToString(), out int cusomerID);
                bool isCustomerName = (!string.IsNullOrEmpty(customerSearch.customerName));
                bool isGender = bool.TryParse(customerSearch.gender.ToString(), out bool gender);
                bool isPhoneNumber = (!string.IsNullOrEmpty(customerSearch.phoneNumber));
                bool isEmail = (!string.IsNullOrEmpty(customerSearch.email));

                bool checkModelSearchIsNull = true;

                if (isCustomerId || isCustomerName || isGender || isPhoneNumber || isEmail)
                {
                    checkModelSearchIsNull= false;
                }

                var cusSearch = await (from kh in _context.Customer
                                  where (kh.IsDelete == null || kh.IsDelete == false) 
                                  && checkModelSearchIsNull == true ? true:
                                  (
                                    (isCustomerId && kh.CustomerId==customerSearch.customerId)
                                    ||(isCustomerName && kh.CustomerName.Contains(customerSearch.customerName))
                                    ||(isGender && kh.Gender==customerSearch.gender)
                                    ||(isPhoneNumber && kh.PhoneNumber.Contains(customerSearch.phoneNumber))
                                    ||(isEmail && kh.Email.Contains(customerSearch.email))
                                  )
                                  orderby kh.DateUpdate descending
                                  select new
                                  {
                                      kh.CustomerId,
                                      kh.CustomerName,
                                      kh.Gender,
                                      kh.Email,
                                      kh.PhoneNumber,
                                      kh.Address
                                  }).ToListAsync();
                if (cusSearch == null)
                {
                    return NotFound();
                }
                
                return Ok(cusSearch);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///get danh sách khách hàng theo mã khách hàng
        ///chưa get được
        ///


        [HttpGet("Adm_GetCustomerById/{CustomerId:int}")]
        public async Task<IActionResult> Adm_GetCustomerById(int CustomerId)
        {
            try {

                var cusSearch = await (from kh in _context.Customer
                                       where (kh.IsDelete == null || kh.IsDelete == false)
                                       && kh.CustomerId== CustomerId
                                       select new
                                       {
                                           kh.CustomerId,
                                           kh.CustomerName,
                                           kh.Gender,
                                           kh.Email,
                                           kh.PhoneNumber,
                                           kh.Address
                                       }).FirstOrDefaultAsync();
                if (cusSearch == null)
                {
                    return NotFound();
                }

                return Ok(cusSearch);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // [Thai Tran Kieu Diem][11/06/2021]
        // Đăng kí khách hàng
        [HttpPost ("Adm_RegisterCustomer")]
        public async Task<ActionResult<IEnumerable<Customer>>> Adm_RegisterCustomer([FromBody] Customer khachHang)
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
                    kh.DateUpdate = DateTime.Now;
                    kh.IsDelete = null;

                    await _context.Customer.AddAsync(kh);
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
        [HttpPut("Adm_UpdateCustomer/{customerID:int}")]
        public async Task<IActionResult> Adm_UpdateCustomer(int customerID, [FromBody] Customer khachHang)
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
        [HttpPut("Adm_DeleteCustomer/{customerId:int}")]
        public async Task<IActionResult> Adm_DeleteCustomer(int customerId)
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
