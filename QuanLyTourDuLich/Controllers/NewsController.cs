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
    public class NewsController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public const string BaseUrlServer = "http://localhost:8000/ImagesNews/";
        public NewsController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - Xứ lý các dữ liệu về tin tức 
        
        //[Thái Trần Kiều Diễm 20211109]
        //lấy dữ liệu tin tức
        [HttpPost("Adm_GetDataNewsList")]
        public async Task<IActionResult> Adm_GetDataNewsList([FromBody] NewsSearchModel pSearch)
        {

            try
            {
                // truy van thong tin lay nhung j can thiet
                var listObj = await (from n in _context.News
                                     where (n.IsDelete == null || n.IsDelete == true)
                                     orderby n.DateUpdate descending
                                     select new
                                     {
                                         n.NewsId,
                                         n.NewsName,
                                         //n.Content,
                                         n.NewsImg,
                                         KindOfNew = n.Enumeration.EnumerationTranslate,
                                         n.EnumerationId,
                                         n.Active,
                                         n.EmpIdupdateNavigation.EmpName,
                                         DateUpdate = DateTime.Parse(n.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                         DateCheck = n.DateUpdate

                                     }).ToListAsync();
                if(pSearch.NewsName != "")
                {
                    listObj = listObj.Where(m => m.NewsName.Trim().ToLower().Contains(pSearch.NewsName.Trim().ToLower())).ToList();
                }

                if (pSearch.DateUpdate != null)
                {
                    listObj = listObj.Where(m => m.DateCheck == pSearch.DateUpdate).ToList();
                }
                if(pSearch.KindOfNewsID !=null )
                {
                    listObj = listObj.Where(m => m.EnumerationId == pSearch.KindOfNewsID).ToList();
                }
                return Ok(listObj);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        
        [HttpGet("Adm_GetNewsDetails")]
        public async Task<IActionResult> Adm_GetNewsDetails(Guid? newsID )
        {
            try
            {
                var rs = await (from n in _context.News
                       where (n.IsDelete == null || n.IsDelete == true)
                            && n.NewsId == newsID
                       select new
                       {
                           n.NewsId,
                           n.NewsName,
                           n.Content,
                           NewsImg = BaseUrlServer+ n.NewsImg.Trim(),
                           n.EnumerationId,
                           n.Active,
                           n.DateUpdate,
                       }).FirstOrDefaultAsync();
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


        //[Thái Trần Kiều Diễm 20211109]
        //thêm mới 1 tin tức

        [HttpPost("Adm_InsertNews")]
        [Authorize]
        public async Task<ActionResult> Adm_InsertNews([FromBody] News news)
        {
            try
            {
                if (news == null)
                {
                    return BadRequest();
                }
                news.EmpIdinsert = news.EmpIdinsert;
                news.DateInsert = DateTime.Now.Date;
                news.EmpIdupdate = news.EmpIdupdate;
                news.DateUpdate = DateTime.Now.Date;
                news.IsDelete = null;

                await _context.News.AddAsync(news);
                await _context.SaveChangesAsync();
                return Ok(news);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        //[Thái Trần Kiều Diễm 20211109]
        // Update một tin tức

        [HttpPut("Adm_UpdateNews")]
        [Authorize]
        public async Task<IActionResult> Adm_UpdateNews([FromBody] News news)
        {
            try
            {
                var update = await (from n in _context.News
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.NewsId== news.NewsId
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);

                update.NewsName = news.NewsName;
                update.Content = news.Content;
                if(news.NewsImg != "")
                {
                    update.NewsImg = news.NewsImg;
                }
                update.EmpIdupdate = news.EmpIdupdate;
                update.EnumerationId = news.EnumerationId;
                update.DateUpdate = DateTime.Now;
                update.Active = news.Active;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpGet("Adm_ActiveNews")]
        [Authorize]
        public async Task<IActionResult> Adm_ActiveNews(Guid? pID)
        {
            try
            {
                var update = await (from n in _context.News
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.NewsId == pID
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);
                update.Active = update.Active == false ? true : false;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }




        ////[Thái Trần Kiều Diễm 20211109]
        ///Xóa một tin tức
        ///
        [HttpPut("Adm_DeleteNews")]
        public async Task<IActionResult> Adm_DeleteNews([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.News.Where(m => deleteModels.SelectByIds.Contains(m.NewsId)).ToListAsync();
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
    }
}
