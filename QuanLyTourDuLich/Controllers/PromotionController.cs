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
                                      orderby p.DateUpdate descending
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
        public async Task<ActionResult> Adm_CreatePromotion([FromBody] Promotion promotion)
        {
            try
            {
                if (promotion == null)
                {
                    return BadRequest();
                }
                promotion.DateInsert = DateTime.Now.Date;
                promotion.DateUpdate = DateTime.Now.Date;
                promotion.EmpIdinsert = promotion.EmpIdinsert;
                promotion.EmpIdupdate = promotion.EmpIdupdate;
                promotion.IsDelete = null;

                await _context.Promotion.AddAsync(promotion);
                await _context.SaveChangesAsync();
                return Ok(promotion);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        //[Thái Trần Kiều Diễm 20211109]
        // Update một tin tức

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
                update.IsApplyAll = promotion.IsApplyAll;
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
