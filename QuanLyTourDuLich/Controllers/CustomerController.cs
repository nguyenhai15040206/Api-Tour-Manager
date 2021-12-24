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
                        new Claim("CustomerID",customer.CustomerId.ToString()),
                        new Claim("CustomerName",customer.CustomerName),
                        new Claim("PhoneNumber",customer.PhoneNumber),
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
            // check email đó là số điện thoại => where sđt
            bool isPhone = ValidateInput.IsPhoneNumber(email);

            return await (from c in _context.Customer
                          where isPhone == true?
                              (c.PhoneNumber == email && c.Password == password
                                && (c.IsDelete == null || c.IsDelete == true))
                            :
                              (c.Email == email && c.Password == password
                                && (c.IsDelete == null || c.IsDelete == true))
                          select c
                          ).FirstOrDefaultAsync();
        }


        // [Thai Tran Kieu Diem][11/09/2021]
        // search danh sách khách hàng
        [HttpPost("Adm_GetDataCustomerList")]
        public async Task<IActionResult> Adm_GetDataCustomerList([FromBody] CustomerSearchModel customerSearch)
        {
            try {

                

                bool isCustomerId = Guid.TryParse(customerSearch.CustomerId.ToString(), out Guid cusomerID);
                bool isCustomerName = (!string.IsNullOrEmpty(customerSearch.CustomerName));
                bool isGender = bool.TryParse(customerSearch.Gender.ToString(), out bool gender);
                bool isPhoneNumber = (!string.IsNullOrEmpty(customerSearch.PhoneNumber));
                bool isEmail = (!string.IsNullOrEmpty(customerSearch.Email));

                bool checkModelSearchIsNull = true;

                if (isCustomerId || isCustomerName || isGender || isPhoneNumber || isEmail)
                {
                    checkModelSearchIsNull= false;
                }

                var cusSearch = await (from kh in _context.Customer
                                  where (kh.IsDelete == null || kh.IsDelete == true) 
                                  && checkModelSearchIsNull == true ? true:
                                  (
                                    (isCustomerId && kh.CustomerId==customerSearch.CustomerId)
                                    ||(isCustomerName && kh.CustomerName.Contains(customerSearch.CustomerName))
                                    ||(isGender && kh.Gender==customerSearch.Gender)
                                    ||(isPhoneNumber && kh.PhoneNumber.Contains(customerSearch.PhoneNumber))
                                    ||(isEmail && kh.Email.Contains(customerSearch.Email))
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
        public async Task<IActionResult> Adm_GetCustomerById(Guid? CustomerId)
        {
            try {

                var cusSearch = await (from kh in _context.Customer
                                       where (kh.IsDelete == null || kh.IsDelete == true)
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
        public async Task<IActionResult> Adm_UpdateCustomer(Guid? customerID, [FromBody] Customer khachHang)
        {
            // Ý nghĩa : Cập nhật thông tin một khách hàng
            if(customerID != khachHang.CustomerId)
            {
                return BadRequest();
            }
            try
            {
                var UpdateCus = await (from kh in _context.Customer
                                       where (kh.IsDelete == null || kh.IsDelete == true)
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
        public async Task<IActionResult> Adm_DeleteCustomer(Guid? customerId)
        {
            try 
            {
                var delCus = await (from cus in _context.Customer
                                    where (cus.IsDelete == null || cus.IsDelete == true)
                                    && cus.CustomerId == customerId
                                    select cus).FirstOrDefaultAsync();
                if (delCus == null)
                    return NotFound();
                delCus.DateUpdate = DateTime.Now;
                delCus.IsDelete = false;
                await _context.SaveChangesAsync();
                return Ok(delCus);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //kiểm tra số điện thoại có tồn tại
        [HttpGet("MB_Cli_CheckPhoneCustomer")]

        public async Task<IActionResult> Cli_CheckPhoneCustomer(string phoneNumber)
        {
            try
            {
                var phone =  _context.Customer.Where(m => m.PhoneNumber == phoneNumber).Count();
                if (phone > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Số điện thoại đã tồn tại");
                }
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }

        }

        //regíster

        [HttpPost("MB_Cli_RegisterCustomer")]

        public async Task<IActionResult> Cli_RegisterCustomer([FromBody] Customer cus)
        {
            try
            {
                var phone = _context.Customer.Where(m => m.PhoneNumber == cus.PhoneNumber).Count();
                if (phone > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Số điện thoại đã tồn tại");
                }

                var email = _context.Customer.Where(m => m.Email == cus.Email).Count();
                if (phone > 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Tài khoản email đã tồn tại");
                }
                cus.DateInsert = DateTime.Now.Date;
                cus.DateUpdate = DateTime.Now.Date;

                await _context.Customer.AddAsync(cus);
                await _context.SaveChangesAsync();
                return Ok(cus);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }

        }

        //get thông tin khashc hàng theo mã
        [HttpGet("MB_Cli_GetInforCustumer")]
        public async Task<IActionResult> MB_Cli_GetInforCustumer(Guid? CustomerId)
        {
            try
            {
                var rs = await (from c in _context.Customer
                                where c.CustomerId == CustomerId
                                select new
                                {
                                    c.CustomerId,
                                    c.CustomerName,
                                    c.Email,
                                    c.PhoneNumber,
                                    c.Password,
                                    c.Address,
                                }).FirstOrDefaultAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //Cập nhật thông tin khách hàng
        [HttpPut("MB_Cli_UpdateCustomer")]
        public async Task<IActionResult> Cli_UpdateCustomer([FromBody] Customer cus)
        {
            try
            {
                var rs = await (from c in _context.Customer
                                where c.CustomerId == cus.CustomerId
                                select c).FirstOrDefaultAsync();
                if (rs.Email != cus.Email)
                {
                    var email = _context.Customer.Where(m => m.Email == cus.Email).Count();
                    if (email > 0)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, "Email đã tồn tại");
                    }
                }
                rs.CustomerName = cus.CustomerName;
                rs.DateUpdate = DateTime.Now.Date;
                rs.Email = cus.Email;
                rs.Address = cus.Address;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //đổi mật khẩu

        [HttpPut("MB_Cli_ChangePassword")]
        public async Task<IActionResult> MB_Cli_ChangePassword([FromBody] Customer cus)
        {
            try
            {
                var rs = await (from c in _context.Customer
                                where c.CustomerId == cus.CustomerId
                                select c).FirstOrDefaultAsync();
                rs.Password = cus.Password;
                rs.DateUpdate = DateTime.Now.Date;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }
    }
}
