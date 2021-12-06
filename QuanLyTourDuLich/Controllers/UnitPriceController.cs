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
        public async Task<ActionResult<IEnumerable<UnitPrice>>> Adm_CreateUnitPrice([FromBody] UnitPrice unitPrice)
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
                        rs.DateUpdate = DateTime.Now.Date;
                        rs.ChildrenUnitPrice = unitPrice.ChildrenUnitPrice;
                        rs.EmpIdupdate = unitPrice.EmpIdupdate;
                        await _context.SaveChangesAsync();
                        return Ok(unitPrice);
                    }
                    // không trùng ngày
                    rs.IsDelete = false;
                }

                // nếu không tìm thấy tiến hành thêm
                unitPrice.DateInsert = DateTime.Now.Date;
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

        /// <summary>
        /// [Nguyễn Tấn hải - 20211203] Hàm cập nhật đơn giá khi có sự thay đổi về giá cả
        /// </summary>
        /// <param name="tourID"></param>
        /// <param name="adultUnitPrice"></param>
        /// <param name="babyUnitPrice"></param>
        /// <param name="childrenUnitPrice"></param>
        /// <returns>true or false</returns>
        [HttpPut("Adm_UpdateUnitPrice")]
        public async Task<ActionResult<IEnumerable<UnitPrice>>> Adm_UpdateUnitPrice([FromBody] UnitPrice unitPrice)
        {
            try
            {
                #region truy vấn dữ liệu
                var rs = await (from up in _context.UnitPrice
                                join t in _context.Tour on up.TourId equals t.TourId
                                where up.TourId == unitPrice.TourId &&
                                        (up.IsDelete == null || up.IsDelete == true)
                                        && (t.IsDelete == null || up.IsDelete == true)
                                        orderby up.DateUpdate descending
                                select up
                                ).FirstOrDefaultAsync();
                #endregion
                if (rs == null)
                    return BadRequest() ;
                else
                {
                    // check xem có sự thay đổi về đơn giá không => có => update
                    if(rs.AdultUnitPrice != unitPrice.AdultUnitPrice || rs.BabyUnitPrice != unitPrice.BabyUnitPrice
                        || rs.ChildrenUnitPrice != unitPrice.ChildrenUnitPrice)
                    {
                        rs.AdultUnitPrice = unitPrice.AdultUnitPrice;
                        rs.BabyUnitPrice = unitPrice.BabyUnitPrice;
                        rs.ChildrenUnitPrice = unitPrice.ChildrenUnitPrice;
                        rs.EmpIdupdate = unitPrice.EmpIdupdate;
                        rs.DateUpdate = DateTime.Now.Date;
                        await _context.SaveChangesAsync();
                    }

                    return Ok(rs);
                }
               
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }



        // Delete Multi row
        [HttpPut("Adm_DeleteUnitPriceByTourID")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<TourDetails>>> DeleteUnitPriceByTourID([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.UnitPrice.Where(m => deleteModels.SelectByIds.Contains(m.TourId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = deleteModels.EmpId;
                });
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, "Xóa thành công!");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
