using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.SearchModels;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitPriceController : ControllerBase
    {
        // Nguyễn Tấn Hải [18/11/2021] - Rest full api TourDetails
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public UnitPriceController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }
        

        // [Nguyễn Tấn Hải - 20211118]: Thực hiện Post Data
        [HttpPost("Adm_InsertUnitPirce")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<UnitPrice>>> InsertTourDetails([FromBody] UnitPrice unitPrice)
        {
            try
            {
                if(unitPrice == null)
                {
                    // 400
                    return BadRequest();
                }
                // tim thấy đơn giá => có => 
                var rs = await _context.UnitPrice.Where(m => m.TourId == unitPrice.TourId &&
                                    (m.IsDelete == null || m.IsDelete == true)).FirstOrDefaultAsync();
                if(rs != null)
                {
                    if(rs.DateUpdate == DateTime.Now.Date)
                    {
                        rs.AdultUnitPrice = unitPrice.AdultUnitPrice;
                        rs.BabyUnitPrice = unitPrice.BabyUnitPrice;
                        rs.ChildrenUnitPrice = unitPrice.ChildrenUnitPrice;
                        rs.EmpIdupdate = unitPrice.EmpIdupdate;
                        await _context.SaveChangesAsync();
                        return Ok(unitPrice);
                    }
                    // không trùng ngày
                    rs.IsDelete = false;
                }

                // nếu không tìm thấy tiến hành thêm
                //unitPrice.DateInsert = DateTime.Now.Date;
                unitPrice.DateUpdate = DateTime.Now.Date;
                unitPrice.IsDelete = null;
                await _context.UnitPrice.AddAsync(unitPrice);
                await _context.SaveChangesAsync();
                // nếu oke => stattus code 200
                return Ok(unitPrice);
            }
            catch (Exception)
            {
                // 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
