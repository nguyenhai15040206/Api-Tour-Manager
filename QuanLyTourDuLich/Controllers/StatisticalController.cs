
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.SearchModels;
using System.Globalization;
using System.IO;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using SelectPdf;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticalController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public const string BaseUrlServer = "http://localhost:8000/ImagesTour/";
        public IConfiguration _config;
        public StatisticalController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        // lấy dữ liệu thống kê booking trong trong năm
        [HttpGet("Adm_StatisticalBookingTour")]
        public async Task<IActionResult> Adm_StatisticalBookingTour()
        {
            try
            {
                #region truy vấn thống tin
                var rs = await (from bk in _context.BookingTour
                          where bk.Status == true && (bk.IsDelete == null || bk.IsDelete == true)
                            && bk.DateConfirm !=null
                            
                          select bk
                          ).ToListAsync();

                var listObj = rs.GroupBy(x => DateTime.Parse(x.DateConfirm.ToString()).Month, (key, value) => new
                { Month = key, totalCountBooking = value.Count(), totalMoneyBooking = value.Sum(m => m.TotalMoney) }).ToList();
                #endregion
                return Ok(listObj.OrderBy(m=>m.Month).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }

        }
    
        // thống kê nhân viên tại công ty
        [HttpGet("Adm_StatisticalEmployee")]
        public async Task<IActionResult> Adm_StatisticalEmployee()
        {
            try
            {
                var rs = await _context.Employee.ToListAsync();
                return Ok(new { 
                    TotalCustomer = rs.Count(),
                    TotalCustomerNoActivity = rs.Where(m => m.IsDelete == false).Count(),
                    TotalCustomerInWorkingMonth = rs.Where(m => DateTime.Parse(m.WorkingDate.ToString()).Month == DateTime.Now.Month
                                    && (m.IsDelete == null || m.IsDelete == true)).Count()
            });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
