using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;
using QuanLyTourDuLich.SearchModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public const string BaseUrlServer = "http://localhost:8000/ImagesBanner/";
        public BannerController(HUFI_09DHTH_TourManagerContext context )
        {
            _context = context;
        }

        [HttpGet("Adm_GetDataBanner")]
        public async Task<IActionResult> Adm_GetDataBanner(bool? active, Guid? bannerType)
        {
            try
            {
                var rs = await (from c in _context.Banner
                                where (c.IsDelete == null || c.IsDelete == true) && c.Active==active
                                orderby c.DateUpdate descending, c.DateUpdate descending
                                select new
                                {
                                    c.BannerImg,
                                    c.BannerId,
                                    c.Active,
                                    c.EnumerationId,
                                    c.Enumeration.EnumerationTranslate,
                                    c.EmpIdupdateNavigation.EmpName,
                                    DateUpdate = DateTime.Parse(c.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                }).ToListAsync();
                if(bannerType != null)
                {
                    rs = rs.Where(m => m.EnumerationId == bannerType).ToList();
                }
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("Adm_BannerDetails")]
        public async Task<IActionResult> Adm_BannerDetails(Guid? pID)
        {
            try
            {
                var rs = await (from c in _context.Banner
                                where (c.IsDelete == null || c.IsDelete == true) && c.BannerId==pID
                                orderby c.DateUpdate descending, c.DateUpdate descending
                                select new
                                {
                                    BannerImg = BaseUrlServer+ c.BannerImg.Trim(),
                                    c.BannerId,
                                    c.Active,
                                    c.EnumerationId,
                                }).FirstOrDefaultAsync();
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

        [HttpPost("Adm_InsertBanner")]
        [Authorize]
        public async Task<IActionResult> Adm_InsertBanner([FromBody] Banner banner)
        {
            try
            {
                if (banner == null)
                {
                    return BadRequest();
                }
                banner.BannerImg = banner.BannerImg;
                banner.EmpIdinsert = banner.EmpIdinsert;
                banner.DateInsert = DateTime.Now.Date;
                banner.EmpIdupdate = banner.EmpIdupdate;
                banner.DateUpdate = DateTime.Now.Date;
                banner.IsDelete = null;

                await _context.Banner.AddAsync(banner);
                await _context.SaveChangesAsync();
                return Ok(banner);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpPut("Adm_UpdateBanner")]
        [Authorize]
        public async Task<ActionResult> Adm_UpdateBanner([FromBody] Banner update)
        {
            try
            {

                if (update == null)
                {
                    return BadRequest();
                }
                var rs = await (from c in _context.Banner
                                       where (c.IsDelete == null || c.IsDelete == true) && c.BannerId == update.BannerId
                                select c).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

                if (update.BannerImg != string.Empty)
                {
                    rs.BannerImg = update.BannerImg;
                }
                rs.EnumerationId = update.EnumerationId;
                rs.DateUpdate = DateTime.Now.Date;
                rs.EmpIdupdate = update.EmpIdupdate;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }


        [HttpPut("Adm_DeleteBanner")]
        [Authorize]
        public async Task<IActionResult> Adm_DeleteBanner([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.Banner.Where(m => deleteModels.SelectByIds.Contains(m.BannerId)).ToListAsync();
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

        [HttpGet("Adm_ActiveBanner")]
        [Authorize]
        public async Task<IActionResult> Adm_ActiveBanner(Guid? pID, Guid? pEmpID)
        {
            try
            {
                var update = await (from n in _context.Banner
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.BannerId == pID
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);
                update.Active = update.Active == false ? true : false;
                update.EmpIdupdate = pEmpID;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }
    }
}
