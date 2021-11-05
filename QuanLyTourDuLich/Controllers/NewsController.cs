using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;

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
        // page = 1, limit = 20
        [HttpGet("NewsPaginationList")]
        public async Task<IActionResult> News_GetDataPagination(int page = 1, int limit = 20)
        {
            try
            {
                // truy van thong tin lay nhung j can thiet
                var newsList = await (from n in _context.News
                                      where (n.IsDelete == null || n.IsDelete == false)
                                      select new
                                      {
                                          n.NewsId,
                                          n.NewsName,
                                          n.Content,
                                          n.NewsImg,
                                          n.Active,
                                          n.KindOfNewsId
                                      }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = newsList.Count();
                // lay du lieu phan trang, tinh ra duoc tong so trang, page thu may,... Ham nay cu coppy
                var pagination = new Pagination
                {
                    count = totalRecord,
                    currentPage = page,
                    pagsize = limit,
                    totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, limit)),
                    indexOne = ((page - 1) * limit + 1),
                    indexTwo = (((page - 1) * limit + limit) <= totalRecord ? ((page - 1) * limit * limit) : totalRecord)
                };
                // status code 200
                return Ok(new
                {
                    data = newsList,
                    pagination = pagination

                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("NewsDetails/{newsID:int}")] // chỉ chấp nhận nó là int
        public async Task<IActionResult> News_GetDataDetails(int newsID )
        {
            try
            {
                var rs = await (from n in _context.News
                       where (n.IsDelete == null || n.IsDelete == false)
                            && n.NewsId == newsID
                       select new
                       {
                           n.NewsId,
                           n.NewsName,
                           n.Content,
                           n.NewsImg,
                           n.Active,
                           n.KindOfNewsId,
                           n.DateInsert,
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

        // Them tin tuc
        [HttpPost]
        public async Task<ActionResult<News>> CreateNews([FromBody] News news)
        {
            try
            {
                if (news == null)
                {
                    return BadRequest();
                }
                await _context.AddAsync(news);
                await _context.SaveChangesAsync();
                return Ok(news);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }


        // 


    }
}
