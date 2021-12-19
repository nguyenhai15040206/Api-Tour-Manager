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
                                      join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
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


        // Lấy tất tả danh sách tour cho Admin
        [HttpPost]
        [Route("Adm_GetDataTourList")]
        public async Task<IActionResult> Adm_TourList([FromBody] TourSearchModel tourSearch = null)
        {
            try
            {
                // xuất theo model search -- start
                // nếu có dữ liệu => nhã tất cả
                bool checkModelSearchIsNull = true;
                // Ngược lại xử lý với luồng dữ liệu này
                bool isTourID = Guid.TryParse(tourSearch.TourID.ToString(), out Guid tourID);
                bool isTourName = (!string.IsNullOrEmpty(tourSearch.TourName));
                bool isDateStart = DateTime.TryParse(tourSearch.DateStart.ToString(), out DateTime dateStart);
                bool isDateEnd = DateTime.TryParse(tourSearch.DateStart.ToString(), out DateTime dateEnd);
                bool isTravelTypeID = Guid.TryParse(tourSearch.TravelTypeID.ToString(), out Guid tourTypeID);
                bool isDeparturePlace = int.TryParse(tourSearch.DeparturePlace.ToString(), out int departurePlace);
                // -- end

                if(isDateEnd || isTourID || isTourName || isDateStart || isTravelTypeID || isDeparturePlace)
                {
                    checkModelSearchIsNull = false;
                }    
                var result = await (from t in _context.Tour
                                    join type in _context.TravelType on t.TravelTypeId equals type.TravelTypeId
                                    join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                    join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                    join up in _context.UnitPrice on t.TourId equals up.TourId
                                    join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                    from a in ttg.DefaultIfEmpty()
                                    where
                                    // Xử lý search 
                                    // nếu check == true => xuất tất cả => ngược lại
                                    checkModelSearchIsNull == true? ((t.IsDelete == null || t.IsDelete == true) &&
                                    (up.IsDelete == null || up.IsDelete == true)) :
                                    // Start
                                    (
                                       (     (isTourID && t.TourId == tourSearch.TourID)
                                        || (isTourName && t.TourName.Contains(tourSearch.TourName))
                                        || (isDateStart && t.DateStart == tourSearch.DateStart)
                                        || (isDateEnd && t.DateEnd == tourSearch.DateEnd)
                                        || (isTravelTypeID && t.TravelTypeId == tourSearch.TravelTypeID)
                                        || (isDeparturePlace && t.DeparturePlace == tourSearch.DeparturePlace)
                                        ) && ((t.IsDelete == null || t.IsDelete == true) &&
                                                (up.IsDelete == null || up.IsDelete == true))
                                    )
                                    // End
                                    orderby t.DateUpdate descending, t.DateStart
                                    select new
                                    {
                                        t.TourId,
                                        t.TourName,
                                        t.Description,
                                        t.TourImg,
                                        t.Transport,
                                        dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        t.QuanityMax,
                                        t.QuanityMin,
                                        t.CurrentQuanity,
                                        t.Schedule,
                                        t.Rating,
                                        t.Suggest,
                                        emp.EmpName,
                                        dateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        up.AdultUnitPrice,
                                        p.ProvinceName,
                                        type.TravelTypeName,

                                        tourGuideName = a.TourGuideName ?? null
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
        public async Task<ActionResult> Cli_TourDetails(Guid? tourID)
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
                                             t.Transport,
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
        [HttpPost("Adm_InsertTour")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<Tour>>> InsertTour([FromBody] Tour tour)
        {
            try
            {
                if(tour==null)
                {
                    // 400
                    return BadRequest();
                }
                tour.DateInsert = DateTime.Now.Date;
                tour.DateUpdate = DateTime.Now.Date;
                tour.IsDelete = null;
                await _context.Tour.AddAsync(tour);
                await _context.SaveChangesAsync();
                // nếu oke => stattus code 200
                return Ok(tour);
            }
            catch (Exception)
            {
                // 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }



        // Delete Multi row
        [HttpPost("Adm_DeleteTourByIds/{empID:int}")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<Tour>>> DeleteTour([FromBody] Guid[] Ids, Guid empID)
        {
            try
            {
                // cách 1, nếu cần kết nhiều bảng
                //var rs = (from t in _context.Tour
                //          join .....
                //          where Ids.Contains(t.TourId)
                //          select ....
                //          ).ToListAsync();

                //chỉ cần có 1 bảng 
                // Nếu listObj = null, thì client call lại dữ liệu, nên không cần check lại listObj == null
                var listObj = await _context.Tour.Where(m => Ids.Contains(m.TourId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = empID;
                });
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, "Delete tour success!");
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///get tất cả các tour
        ///
        [HttpGet("Cli_GetTourListPagination")]
        public async Task<IActionResult> Cli_GetTourListPagination(int page = 1, int limit = 10)
        {
            try
            {
                var tourList = await (from t in _context.Tour
                                      join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                      join up in _context.UnitPrice on t.TourId equals up.TourId
                                      join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                      join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                      from a in ttg.DefaultIfEmpty()
                                      where t.Suggest == true && (t.IsDelete == null || t.IsDelete == true)
                                      orderby t.DateStart descending
                                      select new
                                      {
                                          t.TourId,
                                          t.TourName,
                                          t.TourImg,
                                          DateStart= DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                          Time= (t.DateEnd - t.DateStart).Value.Days,
                                          t.Rating,
                                          up.AdultUnitPrice,
                                          p.ProvinceName,
                                      }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = _context.Tour.Where(m => m.Suggest == true && (m.IsDelete == null || m.IsDelete == true)).Count();
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
                    data = tourList,
                    pagination = pagination

                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("Cli_GetTourDescriptionById")]
        public async Task<IActionResult> Cli_GetTourDescriptionById(Guid TourId)
        {
            try
            {
                var tour = await (from t in _context.Tour
                                  join p in _context.Province on t.DeparturePlace equals p.ProvinceId
                                  join up in _context.UnitPrice on t.TourId equals up.TourId
                                  join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                  join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                  from a in ttg.DefaultIfEmpty()
                                  where t.TourId == TourId
                                  select new
                                  {
                                      t.TourId,
                                      t.TourName,
                                      t.TourImg,
                                      t.Description,
                                      DateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                      Time = (t.DateEnd - t.DateStart).Value.Days,
                                      t.Transport,
                                      t.Schedule,
                                      t.Rating,
                                      up.AdultUnitPrice,
                                      p.ProvinceName,
                                  }).FirstOrDefaultAsync();
                return Ok(tour);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
