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
using System.Globalization;

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
                            //customer.Gender,
                            //customer.PhoneNumber,
                            customer.Email,
                            //customer.Address,
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
        public async Task<IActionResult> Adm_GetDataCustomerList([FromBody] CustomerSearchModel pSearch)
        {
            try {
                string[] separatorAddress = { "||" };
                var listObj = await (from kh in _context.Customer
                                  where (kh.IsDelete == null || kh.IsDelete == true) 
                                  orderby kh.DateUpdate descending
                                  select new
                                  {
                                      kh.CustomerId,
                                      kh.CustomerName,
                                      gender= kh.Gender==null? "Chưa xác định": kh.Gender==true?"Nam":"Nữ",
                                      kh.Email,
                                      kh.PhoneNumber,
                                      Address= kh.Address == null ? "Chưa cập nhật" : kh.Address.Split(separatorAddress, System.StringSplitOptions.RemoveEmptyEntries)[0].ToString(),
                                      empIdUpdate = kh.EmpIdupdate==null? "": kh.EmpIdupdateNavigation.EmpName,
                                      dateUpdate = kh.DateUpdate==null? "": DateTime.Parse(kh.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                  }).ToListAsync();

                if (pSearch.CustomerName != null && pSearch.CustomerName != "")
                {
                    listObj = listObj.Where(m => m.CustomerName.Contains(pSearch.CustomerName)).ToList();
                }
                if (pSearch.PhoneNumber != null && pSearch.PhoneNumber != "")
                {
                    listObj = listObj.Where(m => m.PhoneNumber.Contains(pSearch.PhoneNumber)).ToList();
                }
                if (pSearch.Email != null && pSearch.Email != "")
                {
                    listObj = listObj.Where(m => m.Email.Contains(pSearch.Email)).ToList();
                }

                return Ok(listObj);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }



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
        [HttpPut("Adm_DeleteCustomer")]
        [Authorize]
        public async Task<IActionResult> Adm_DeleteCustomer([FromBody] DeleteModels deleteModels)
        {
            try 
            {
                var listObj = await _context.Customer.Where(m => deleteModels.SelectByIds.Contains(m.CustomerId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.EmpIdupdate = deleteModels.EmpId;
                    m.DateUpdate = DateTime.Now;
                    m.IsDelete = false;
                });
                await _context.SaveChangesAsync();
                return Ok();
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
                var rs = await (from c in _context.Customer
                                where c.PhoneNumber == phoneNumber
                                select c).FirstOrDefaultAsync();
                if (rs != null && rs.Password!=null )
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Số điện thoại đã tồn tại");
                }
                if (rs != null)
                {
                    return Ok(rs);
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
                if (email > 0)
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

        //regisster khasch hangf khi da co so dien thoai(update)
        [HttpPut("MB_RegisterPhoneNumber")]
        public async Task<IActionResult> MB_RegisterPhoneNumber([FromBody] Customer cus)
        {
            try
            {
                var rs = await (from c in _context.Customer
                                where c.PhoneNumber == cus.PhoneNumber
                                select c).FirstOrDefaultAsync();
                var email = _context.Customer.Where(m => m.Email == cus.Email).Count();
                if (email > 0)
                {
                    if(rs.Email!=cus.Email)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, "Tài khoản email đã tồn tại");
                    }
                    else {
                        rs.CustomerName = cus.CustomerName;
                        rs.Email = cus.Email;
                        rs.Password = cus.Password;
                        rs.DateUpdate = DateTime.Now.Date;
                        await _context.SaveChangesAsync();
                        return Ok(rs);
                    }
                }
                rs.CustomerName = cus.CustomerName;
                rs.Email = cus.Email;
                rs.Password = cus.Password;
                rs.DateUpdate = DateTime.Now.Date;
                await _context.SaveChangesAsync();
                return Ok(rs);

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
                string[] separator = { "||" };
                var rs = await (from c in _context.Customer
                                where c.CustomerId == CustomerId
                                select new
                                {
                                    c.CustomerId,
                                    c.CustomerName,
                                    c.Email,
                                    c.Gender,
                                    c.PhoneNumber,
                                    c.Address,
                                }).FirstOrDefaultAsync();
                string Address = rs.Address;
                if (rs.Address != null)
                {
                    string[] arrAdress = rs.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                    Address = arrAdress[0].ToString();
                    string wards = "";
                    string provice = "";
                    string districts = "";
                    if (arrAdress.Length == 4)
                    {

                        wards = await _context.Wards.Where(m => m.WardId == int.Parse(arrAdress[1].Trim().ToString())).Select(m => m.WardName).FirstOrDefaultAsync();
                        districts = await _context.District.Where(m => m.DistrictId == int.Parse(arrAdress[2].Trim().ToString())).Select(m => m.DistrictName).FirstOrDefaultAsync();
                        provice = await _context.Province.Where(m => m.ProvinceId == int.Parse(arrAdress[3].Trim().ToString())).Select(m => m.ProvinceName).FirstOrDefaultAsync();
                        Address = Address + ", " + wards + ", " + districts + ", " + provice;
                    }

                }
                return Ok(new
                {
                    data = rs,
                    address = Address,
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //Cập nhật thông tin khách hàng
        [HttpPut("MB_Cli_UpdateCustomer")]
        [Authorize]
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
                        return StatusCode(StatusCodes.Status409Conflict, "Email đã tồn tại");
                    }
                }
                if(cus.CustomerName != null)
                {
                    rs.CustomerName = cus.CustomerName;
                }
                if(cus.Gender != null)
                {
                    rs.Gender = cus.Gender;
                }
                if(cus.PhoneNumber != null)
                {
                    rs.PhoneNumber = cus.PhoneNumber;
                }
                
                rs.DateUpdate = DateTime.Now.Date;
                if (cus.Email != null)
                {
                    rs.Email = cus.Email;
                }
                if(cus.Address != null)
                {
                    rs.Address = cus.Address;
                }
                
                
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }

        //đổi mật khẩu

        
        [HttpPut("MB_Cli_ChangePassword")]

        public async Task<IActionResult> MB_Cli_ChangePassword([FromBody] CustomerUpdatePass cus)
        {
            try
            {
                var rs = await (from c in _context.Customer
                                where c.CustomerId == cus.CustomerId
                                select c).FirstOrDefaultAsync();
                if (rs.Password != cus.PasswordOld)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Sai mật khẩu");
                }
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
