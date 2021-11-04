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
    public class TourController : ControllerBase
    {
        // Nguyễn Tấn Hải [24/10/2021] - Rest full api Tour
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public TourController (HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // Lấy những tour nào được đề xuất max 9 tour
        [HttpGet("TourIsSuggest")]
        public async Task<IActionResult> TourIsSuggest()
        {
            var tourList = await (from t in _context.Tour
                                  join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                  join up in _context.UnitPrice on t.TourId equals up.TourId
                                  join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                  from a in ttg.DefaultIfEmpty()
                                  where t.Suggest == true && (t.IsDelete == null || t.IsDelete == true)
                                  orderby t.DateStart descending
                                    select new
                                    {
                                        t.TourId,
                                        t.TourName,
                                        t.TourImg,
                                        t.DateStart,
                                        t.Rating,
                                        up.AdultUnitPrice,
                                        p.ProvinceName,
                                        //tourGuideName = a.TourGuideName?? null,
                                    }).Take(6).ToListAsync();
            if(tourList == null || tourList.Count == 0)
            {
                return NoContent();
            }

            return Ok(tourList);
        }


        //Phân trang
        public async Task<IActionResult> TourPaginationList(int page = 1, int limit = 9)
        {
            //List<Tour> tourListModels = new List<Tour>();
            var tourList = await (from t in _context.Tour
                                  join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                  join up in _context.UnitPrice on t.TourId equals up.TourId
                                  join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                  from a in ttg.DefaultIfEmpty()
                                  where t.Suggest == true && (t.IsDelete == null || t.IsDelete == true)
                                  orderby t.DateStart descending
                                  select new {
                                      t.TourId,
                                      t.TourName,
                                      t.TourImg,
                                      t.DateStart,
                                      t.Rating,
                                      up.AdultUnitPrice,
                                      p.ProvinceName,
                                  } ) .Skip((page - 1) * limit).Take(limit).ToListAsync();
            var tourListModels = tourList;
            int totalRecord = tourList.Count();
            var pagination = new Pagination
            {
                count = totalRecord,
                currentPage = page,
                pagsize = limit,
                totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, limit)),
                indexOne = ((page - 1) * limit + 1),
                indexTwo = (((page - 1) * limit + limit) <= totalRecord ? ((page - 1) * limit * limit) : totalRecord)
            };
            return new ObjectResult(new
            {
                data = tourListModels,
                pagination = pagination
            });
        }

        //get danh sách tour
        [HttpGet("TourPaginationList")]
        public async Task<IActionResult> GET(int page, int limit)
        {
            try
            {
                var rs = await TourPaginationList(page, limit);
                if (rs == null)
                {
                    return NotFound();
                }
                return new ObjectResult(rs);
            }
            catch
            {
                return BadRequest();
            }
        }


        // [Nguyễn Tấn Hải -] - Xử lý Lấy dữ liệu tourDetails
        [HttpGet("TourDetails/{tourID}")]
        public async Task<ActionResult> TourDetails(int tourID)
        {
            try
            {
                var tourDetails = await (from t in _context.Tour
                                         join up in _context.UnitPrice on t.TourId equals up.TourId
                                         join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                         join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                         from a in ttg.DefaultIfEmpty()
                                         where t.TourId == tourID && (t.IsDelete == null || t.IsDelete == true)
                                         orderby up.DateUpdate descending
                                         select new
                                         {
                                             t.TourId,
                                             t.TourName,
                                             t.Rating,
                                             t.Description,
                                             t.TourImg,
                                             t.DateStart,
                                             totalDay = (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,  // thời hạn của tour => 3 ngày 2 đêm
                                             t.PhuongTienXuatPhat,
                                             t.QuanityMax,  
                                             quanity = t.QuanityMax > t.CurrentQuanity? (t.QuanityMax-t.CurrentQuanity) : 0,
                                             t.Schedule,
                                             t.TravelType.TravelTypeName,
                                             touGuideName = a.TourGuideName ?? null,
                                             up.AdultUnitPrice,
                                             p.ProvinceName,
                                             tourDetails = (from td in _context.TourDetails
                                                            join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                            where td.TourId == tourID
                                                                && (tatt.IsDelete == null || tatt.IsDelete== true)
                                                                && (td.IsDelete == null || tatt.IsDelete == true)
                                                            select new
                                                            {
                                                                tatt.TouristAttrName,
                                                                tatt.ImagesList,
                                                                tatt.Description
                                                            }).ToList(),
                                         }).FirstOrDefaultAsync();
                if (tourDetails == null)
                {
                    return NotFound();
                }
                return Ok(tourDetails);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Tour>>> InsertTour([FromBody] Tour tour)
        {
            try
            {
                if (tour != null)
                {
                    await _context.Tour.AddAsync(tour);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction("GET", new { maTour = tour.TourId }, tour);
                }
                else
                {
                    return NoContent();
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
