using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KindOfNewsController : ControllerBase
    {
        /// <summary>
        /// [Thái Trần Kiều Diễm 20211110]
        /// loại tin tức
        /// </summary>
        /// 
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public KindOfNewsController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        ///get các loại tin tức
        ///

        [HttpGet("Adm_GetDataKindOfNews")]
        public async Task<IActionResult> Adm_GetDataKindOfNews()
        {
            
            try
            {
                var rs = await (from k in _context.KindOfNews
                                join e in _context.Employee on k.EmpIdupdate equals e.EmpId
                                where (k.IsDelete == null || k.IsDelete == true)
                                select new
                                {
                                    k.KindOfNewsId,
                                    k.KindOfNewsName,
                                    e.EmpName,
                                    DateUpdate=DateTime.Parse(k.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                }).ToListAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///Get thông tin loại tin tức theo mã
        ///
        [HttpGet("Adm_GetKindOfNewsById/{id:int}")]
        public async Task<IActionResult> Adm_GetKindOfNewsById(int id)
        {
            try
            {
                var rs = await (from k in _context.KindOfNews
                                join e in _context.Employee on k.EmpIdupdate equals e.EmpId
                                where (k.IsDelete == null || k.IsDelete == true)
                                select new
                                {
                                    k.KindOfNewsId,
                                    k.KindOfNewsName,
                                    e.EmpName,
                                    DateUpdate = DateTime.Parse(k.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                }).ToListAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///Thêm một loại tin tức mới
        ///
        [HttpPost("Adm_CreateKindOfNews")]
        public async Task<ActionResult> Adm_CreateKindOfNews([FromBody] KindOfNews kind)
        {
            try
            {
                if (kind == null)
                {
                    return NotFound();
                }
                KindOfNews k = new KindOfNews();
                k.KindOfNewsName = kind.KindOfNewsName;
                k.EmpIdinsert = kind.EmpIdinsert;
                k.DateInsert = DateTime.Now;
                k.EmpIdupdate = kind.EmpIdupdate;
                k.DateUpdate = DateTime.Now;
                k.Status = kind.Status;
                k.IsDelete = null;

                await _context.KindOfNews.AddAsync(k);
                await _context.SaveChangesAsync();

                return Ok(k);
                
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///cap nhay thon tin mot loai tin tuc
        ///

        [HttpPut("Adm_UpdateKindOfNews/{KindOfNewsId:int}")]
        public async Task<IActionResult> Adm_UpdateKindOfNews([FromBody] KindOfNews kind, Guid? KindOfNewsId)
        {
            if (kind.KindOfNewsId != KindOfNewsId)
            {
                return BadRequest();
            }
            try
            {
                var rs = await (from k in _context.KindOfNews
                                where (k.IsDelete == null || k.IsDelete == true)
                                && k.KindOfNewsId==KindOfNewsId
                                select k).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

                rs.KindOfNewsName = kind.KindOfNewsName;
                rs.EmpIdupdate = kind.EmpIdupdate;
                rs.Status = kind.Status;
                rs.DateUpdate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///xoa mot loai tin tuc
        ///

        [HttpPut("Adm_DeleteKindOfNews/{KindOfNewsId:int}/{empID:int}")]
        public async Task<IActionResult> Adm_DeleteKindOfNews (Guid? KindOfNewsId, Guid? empID = null)
        {
            try
            {
                var rs = await (from k in _context.KindOfNews
                                where (k.IsDelete == null || k.IsDelete == true)
                                && k.KindOfNewsId == KindOfNewsId
                                select k).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

                rs.EmpIdupdate = empID;
                rs.DateUpdate = DateTime.Now;
                rs.IsDelete = false;

                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
