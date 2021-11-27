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

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public NewsController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - Xứ lý các dữ liệu về tin tức 
        
        //[Thái Trần Kiều Diễm 20211109]
        //lấy dữ liệu tin tức
        [HttpPost("Adm_GetDataNewsList")]
        public async Task<IActionResult> Adm_GetDataNewsList([FromBody] NewsSearchModel newSearch)
        {

            try
            {
                bool checkModelSearchIsNull = true;

                bool isNewsId = int.TryParse(newSearch.newsId.ToString(), out int newsId);
                bool isNewsName = (!string.IsNullOrEmpty(newSearch.newsName));
                bool isKindOfNewsId = int.TryParse(newSearch.kindOfNewsId.ToString(), out int kindOfNews);

                if (isNewsId || isNewsName || isKindOfNewsId)
                {
                    checkModelSearchIsNull = false;
                }
   
                // truy van thong tin lay nhung j can thiet
                var newsList = await (from n in _context.News
                                      join k in _context.KindOfNews on n.KindOfNewsId equals k.KindOfNewsId
                                      join e in _context.Employee on n.EmpIdupdate equals e.EmpId
                                      where (n.IsDelete == null || n.IsDelete == true)
                                      && checkModelSearchIsNull == true ? true 
                                      :(
                                        (isNewsId && n.NewsId==newSearch.newsId)
                                        ||(isNewsName && n.NewsName.Contains(newSearch.newsName))
                                        ||(isKindOfNewsId && n.KindOfNewsId==newSearch.kindOfNewsId)
                                        )
                                      orderby n.DateUpdate descending
                                      select new
                                      {
                                          n.NewsId,
                                          n.NewsName,
                                          n.Content,
                                          n.NewsImg,
                                          n.Active,
                                          k.KindOfNewsName,
                                          e.EmpName,
                                          DateUpdate=DateTime.Parse(n.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),

                                      }).ToListAsync();
               
                return Ok(newsList);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        
        [HttpGet("NewsDetails/{newsID:int}")] // chỉ chấp nhận nó là int
        public async Task<IActionResult> News_GetDataDetails(Guid? newsID )
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
                           n.NewsImg,
                           n.Active,
                           n.KindOfNewsId,
                           DateUpdate = DateTime.Parse(n.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                           n.EmpIdinsertNavigation.EmpId,
                           n.EmpIdinsertNavigation.EmpName,
                           // tùy thuột vào mức độ tư duy mà truy vấn
                           // cảm thấy đủ là oke không đủ sau vào thêm
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

        //[Thái Trần Kiều Diễm 20211109]
        //thêm mới 1 tin tức

        [HttpPost("Adm_CreateNews")]
        public async Task<ActionResult> Adm_CreateNews([FromBody] News news)
        {
            try
            {
                if (news == null)
                {
                    return BadRequest();
                }
                News n = new News();
                n.NewsName = news.NewsName;
                n.Content = news.Content;
                n.NewsImg = news.NewsImg;
                n.ImagesList = news.ImagesList;
                n.EmpIdinsert = news.EmpIdinsert;
                n.DateInsert = DateTime.Now;
                n.EmpIdupdate = news.EmpIdupdate;
                n.DateUpdate = DateTime.Now;
                n.KindOfNewsId = news.KindOfNewsId;
                n.Active = news.Active;
                n.IsDelete = null;

                await _context.AddAsync(n);
                await _context.SaveChangesAsync();
                return Ok(n);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        //[Thái Trần Kiều Diễm 20211109]
        // Update một tin tức

        [HttpPost("Adm_UpdateNews/{id:int}")]
        public async Task<IActionResult> Adm_UpdateNews([FromBody] News news, Guid? id)
        {
            if (news.NewsId != id)
            {
                return BadRequest();
            }
            try
            {
                var update = await (from n in _context.News
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.NewsId==id
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);

                update.NewsName = news.NewsName;
                update.Content = news.Content;
                update.NewsImg = news.NewsImg;
                update.ImagesList = news.ImagesList;
                update.EmpIdupdate = news.EmpIdupdate;
                update.DateUpdate = DateTime.Now;
                update.KindOfNewsId = news.KindOfNewsId;
                update.Active = news.Active;


                await _context.SaveChangesAsync();
                return Ok(update);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }


        ////[Thái Trần Kiều Diễm 20211109]
        ///Xóa một tin tức
        ///
        [HttpPost("Adm_DeleteNews/{NewsID:int}/{empId:int}")]
        public async Task<IActionResult> Adm_DeleteNews ( Guid? NewsID, Guid? empId = null)
        {
            try
            {
                var delete = await (from n in _context.News
                                    where (n.IsDelete == null || n.IsDelete == true)
                                    && n.NewsId == NewsID
                                    select n).FirstOrDefaultAsync();
                if (delete == null)
                    return NotFound();
                delete.IsDelete = false;
                delete.EmpIdupdate = empId;
                delete.DateUpdate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(delete);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }
    }
}
