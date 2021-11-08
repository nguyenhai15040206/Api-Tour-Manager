using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.SearchModels;

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

        // Lấy những tour nào được đề xuất max 6 tour
        [HttpGet("TourIsSuggest")]
        public async Task<IActionResult> TourIsSuggest()
        {
            try
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
                if (tourList == null || tourList.Count == 0)
                {
                    return NotFound();
                }

                return Ok(tourList);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        // Lấy tất tả danh sách tour phân trang cho Client
        [HttpPost("GetData_TourList")]
        public async Task<IActionResult> TourList_Client([FromBody] TourSearchModel tourSearch)
        {
            try
            {
                // xuất theo model search -- start
                bool isTourID = int.TryParse(tourSearch.TourID.ToString(), out int tourID);
                bool isTourName = (!string.IsNullOrEmpty(tourSearch.TourName));
                bool isDateStart = DateTime.TryParse(tourSearch.DateStart.ToString(), out DateTime dateStart);
                // -- end
                var result = await (from t in _context.Tour
                                    join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                    join up in _context.UnitPrice on t.TourId equals up.TourId
                                    join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                    from a in ttg.DefaultIfEmpty()
                                    where (t.IsDelete == null || t.IsDelete == true) &&
                                    // Xử lý search
                                    // Start
                                    (
                                        (isTourID && t.TourId == tourSearch.TourID)
                                        || (isTourName && t.TourName.Contains(tourSearch.TourName))
                                        || (isDateStart && t.DateStart == tourSearch.DateStart)
                                    )
                                    // End
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
                                    }).ToListAsync();
                return Ok(result);


            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
            
        }

        // [Nguyễn Tấn Hải -] - Xử lý Lấy dữ liệu tourDetails
        [HttpGet("TourDetails/{tourID:int}")]
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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        

        // [Nguyễn Tấn Hải - 20211105]: Thực hiện Post Data
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Tour>>> POST([FromBody] Tour tour)
        {
            try
            {
                if(tour==null)
                {
                    // 400
                    return BadRequest();
                }
                await _context.Tour.AddAsync(tour);
                await _context.SaveChangesAsync();
                // nếu oke => stattus code 201
                return CreatedAtAction("GET", new { maTour = tour.TourId }, tour);
            }
            catch (Exception)
            {
                // 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
