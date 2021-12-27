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
    public class PromotionController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public PromotionController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - Xứ lý các dữ liệu về Khuyến mãi
        
        [HttpPost("Adm_GetDataPromotion")]
        public async Task<IActionResult> Adm_GetDataPromotion([FromBody] PromotionSearchModel pSearch)
        {

            try
            {
                bool checkModelSearchIsNull = true;
                bool isProName = (!string.IsNullOrEmpty(pSearch.PromotionName.Trim()));
                if (isProName)
                {
                    checkModelSearchIsNull = false;
                }
                // truy van thong tin lay nhung j can thiet
                var newsList = await (from p in _context.Promotion
                                      where pSearch.IsApplyAll==true?
                                      ((p.IsDelete == null || p.IsDelete == true) && p.IsApplyAll == true):
                                      (checkModelSearchIsNull == true?
                                        ((p.IsDelete == null || p.IsDelete == true)):
                                        (
                                            (isProName && p.PromotionName.Contains(pSearch.PromotionName.Trim()))
                                            && (p.IsDelete == null || p.IsDelete == true)
                                        )
                                      )
                                      orderby p.DateUpdate descending, p.DateEnd descending
                                      select new
                                      {
                                          p.PromotionId,
                                          p.PromotionName,
                                          dateStart= DateTime.Parse(p.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ,
                                          dateEnd= DateTime.Parse(p.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                          p.Discount,
                                          p.IsApplyAll,
                                      }).ToListAsync();
               
                return Ok(newsList);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        
        [HttpGet("Adm_PromotionDetails")]
        public async Task<IActionResult> Adm_GetDataDetails(Guid? pID )
        {
            try
            {
                var rs = await (from p in _context.Promotion
                                where (p.IsDelete == null || p.IsDelete == true)
                                     && p.PromotionId == pID
                                select new
                                {
                                    p.PromotionId,
                                    p.PromotionName,
                                    p.DateEnd,
                                    p.DateStart,
                                    p.Discount,
                                    p.IsApplyAll,
                                    tourList =  _context.PromotionalTour.Where(m => (m.IsDelete == null || m.IsDelete == true) && m.PromotionId == p.PromotionId).Select(m=>m.TourId).ToArray()
                                }).FirstOrDefaultAsync(); // Hạn chế dùng Single vì => xãy ra exception
                if(rs== null)
                {
                    return NotFound(); // 404
                }
                return Ok(rs); // 200

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        [HttpPost("Adm_CreatePromotion")]
        [Authorize]
        public async Task<ActionResult> Adm_CreatePromotion([FromBody] PromotionModels promotion)
        {
            try
            {
                if (promotion == null)
                {
                    return BadRequest();
                }
                var checkConflit = await _context.Promotion.Where(m => m.PromotionName.Trim().Equals(promotion.PromotionName.Trim())
                        && (m.IsDelete==true || m.IsDelete==null) && m.DateStart == promotion.DateStart && m.DateEnd==promotion.DateEnd).FirstOrDefaultAsync();

                if (checkConflit != null)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Khyến mãi đã tồn tại trong hệ thống!");
                }
                Promotion insert = new Promotion();
                insert.PromotionName = promotion.PromotionName;
                insert.Discount = promotion.Discount;
                insert.DateStart = promotion.DateStart;
                insert.DateEnd = promotion.DateEnd;
                insert.IsApplyAll = promotion.IsApplyAll;
                insert.DateInsert = DateTime.Now.Date;
                insert.DateUpdate = DateTime.Now.Date;
                insert.EmpIdinsert = promotion.EmpIdinsert;
                insert.EmpIdupdate = promotion.EmpIdupdate;
                insert.IsDelete = null;
                
                await _context.Promotion.AddAsync(insert);
                await _context.SaveChangesAsync();
                if (promotion.IsApplyAll == true)
                {

                    var rs = await _context.Tour.Where(m => (m.IsDelete == null || m.IsDelete == true)).ToListAsync();
                    foreach(var item in rs)
                    {
                        var obj = await _context.PromotionalTour.Where(m => m.TourId == item.TourId 
                        && (m.IsDelete==null || m.IsDelete==true)).FirstOrDefaultAsync();
                        if(obj !=null) obj.IsDelete = false;
                        PromotionalTour pt = new PromotionalTour();
                        pt.PromotionId = insert.PromotionId;
                        pt.TourId = item.TourId;
                        pt.DateInsert = DateTime.Now.Date;
                        pt.DateUpdate = DateTime.Now.Date;
                        pt.EmpIdinsert = promotion.EmpIdinsert;
                        pt.EmpIdupdate = promotion.EmpIdupdate;
                        pt.IsDelete = null;
                        await _context.PromotionalTour.AddAsync(pt);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    foreach(var item in promotion.TourList)
                    {
                        var obj2 = await _context.PromotionalTour.Where(m => m.TourId == item
                        && (m.IsDelete == null || m.IsDelete == true)).FirstOrDefaultAsync();
                        if (obj2 != null) obj2.IsDelete = false;
                        PromotionalTour pt = new PromotionalTour();
                        pt.PromotionId = insert.PromotionId;
                        pt.TourId = item;
                        pt.DateInsert = DateTime.Now.Date;
                        pt.DateUpdate = DateTime.Now.Date;
                        pt.EmpIdinsert = promotion.EmpIdinsert;
                        pt.EmpIdupdate = promotion.EmpIdupdate;
                        pt.IsDelete = null;
                        await _context.PromotionalTour.AddAsync(pt);

                    }
                    await _context.SaveChangesAsync();
                }
                return Ok(promotion);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        [HttpPut("Adm_UpdatePromotion")]
        [Authorize]
        public async Task<IActionResult> Adm_UpdatePromotion([FromBody] Promotion promotion)
        {
            if (promotion.PromotionId ==Guid.Empty)
            {
                return BadRequest();
            }
            try
            {
                var update = await (from n in _context.Promotion
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.PromotionId==promotion.PromotionId
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);
                update.PromotionName = promotion.PromotionName;
                update.DateStart = promotion.DateStart;
                update.DateEnd = promotion.DateEnd;
                update.Discount = promotion.Discount;
                //update.IsApplyAll = promotion.IsApplyAll;
                update.EmpIdupdate = promotion.EmpIdupdate;
                update.DateUpdate = DateTime.Now.Date;
                await _context.SaveChangesAsync();
                return Ok(update);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpPut("Adm_DeletePromotionExpired")]
        [Authorize]
        public async Task<IActionResult> Adm_DeletePromotionExpired()
        {
            try
            {
                var rs = await _context.Promotion.Where(m => (m.IsDelete == null || m.IsDelete == true) 
                        && m.DateEnd < DateTime.Now.Date).ToListAsync();
                if (rs.Count == 0)
                {
                    return NotFound();
                }
                rs.ForEach(m => m.IsDelete = false);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // Delete Multi row
        [HttpPut("Adm_DeletePromotionByIds")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Promotion>>> Adm_DeletePromotionByIds([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.Promotion.Where(m => deleteModels.SelectByIds.Contains(m.PromotionId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = deleteModels.EmpId;
                });

                foreach(var item in listObj)
                {
                    var rs = await _context.PromotionalTour.Where(m => m.PromotionId == item.PromotionId).ToListAsync();
                    rs.ForEach(m =>
                    {
                        m.IsDelete = false;
                        m.DateUpdate = DateTime.Now.Date;
                        m.EmpIdupdate = deleteModels.EmpId;
                    });
                    await _context.SaveChangesAsync();
                }
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
