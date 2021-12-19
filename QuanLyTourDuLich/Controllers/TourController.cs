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
        public const string BaseUrlServer = "http://localhost:8000/ImagesTour/";
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
                                      join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                      join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                      join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                      from a in ttg.DefaultIfEmpty()
                                      where t.Suggest == true && (t.IsDelete == null || t.IsDelete == true)
                                      orderby t.DateStart descending
                                      select new
                                      {
                                          t.TourId,
                                          t.TourName,
                                          tourImg = BaseUrlServer + t.TourImg.Trim(),
                                          t.DateStart,
                                          t.Rating,
                                          t.AdultUnitPrice,
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
                bool isDeparturePlace = true;
                bool isDateFromTo = false;
                bool isTourName = (!string.IsNullOrEmpty(tourSearch.TourName));
                bool isTravelTypeID = Guid.TryParse(tourSearch.TravelTypeID.ToString(), out Guid tourTypeID);
                if (tourSearch.DeparturePlace == null || tourSearch.DeparturePlace.Length==0)
                {
                    isDeparturePlace = false;
                }    
                bool isDateStart = DateTime.TryParse(tourSearch.DateStart.ToString(), out DateTime dateStart);
                bool isDateEnd = DateTime.TryParse(tourSearch.DateEnd.ToString(), out DateTime dateEnd);
                // -- end
                if(isDateEnd== true && isDateStart== true)
                {
                    isDateFromTo = true;
                }
                if (isTourName || isDateFromTo || isTravelTypeID || isDeparturePlace)
                {
                    checkModelSearchIsNull = false;
                }
                #region truy vấn dữ liệu
                var result = await (from t in _context.Tour
                                    join type in _context.CatEnumeration on t.TravelTypeId equals type.EnumerationId
                                    join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                    join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                    join td in _context.TourDetails on t.TourId equals td.TourId
                                    join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                    join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                    from a in ttg.DefaultIfEmpty()
                                    // Xử lý search 
                                    // nếu check == true => xuất tất cả => ngược lại (với trạng thái isDelete là Null)
                                    where tourSearch.Suggest== true? 
                                    ((t.IsDelete == null || t.IsDelete == true) && (td.IsDelete == null || td.IsDelete == true) && t.Suggest==true)
                                    : (checkModelSearchIsNull == true? 
                                        ((t.IsDelete == null || t.IsDelete == true) && (td.IsDelete == null || td.IsDelete == true)) :
                                        (
                                            (
                                                (isTourName && t.TourName.Contains(tourSearch.TourName))
                                                || (isTravelTypeID && t.TravelTypeId == tourSearch.TravelTypeID)
                                                || (isDeparturePlace && tourSearch.DeparturePlace.Contains(t.DeparturePlaceFrom))
                                                || (isDateFromTo  && (t.DateStart >= tourSearch.DateStart && t.DateEnd <= tourSearch.DateEnd))
                                            )
                                            && ((t.IsDelete == null || t.IsDelete == true) && (td.IsDelete==null || td.IsDelete==true))
                                        )
                                    )
                                    orderby t.DateUpdate descending
                                    select new
                                    {
                                        t.TourId,
                                        t.TourName,
                                        t.Description,
                                        tourImg = BaseUrlServer + t.TourImg.Trim(),
                                        dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        t.Rating,
                                        t.QuanityMax,
                                        t.QuanityMin,
                                        t.CurrentQuanity,
                                        t.Schedule,
                                        departurePlaceFrom = p.ProvinceName,
                                        departurePlaceTo = pt.ProvinceName,
                                        t.Suggest,
                                        emp.EmpName,
                                        dateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        adultUnitPrice = string.Format("{0:0,0đ}", t.AdultUnitPrice),
                                        childrenUnitPrice = string.Format("{0:0,0đ}", t.ChildrenUnitPrice),
                                        babyUnitPrice = string.Format("{0:0,0đ}", t.BabyUnitPrice),
                                        surcharge = string.Format("{0:0,0đ}", t.Surcharge),
                                        travelTypeName= type.EnumerationTranslate,
                                        //type.TravelTypeName,

                                        tourGuideName = t.TourGuideId==null?"Chưa cập nhật": a.TourGuideName
                                    }).Distinct().ToListAsync();
                #endregion

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // [Nguyễn Tấn Hải -] - Xử lý Lấy dữ liệu tourDetails
        [HttpGet]
        [Route("Adm_GetTourDetails")]
        public async Task<IActionResult> Adm_GetTourDetails(Guid? tourID = null)
        {
            try
            {
                #region truy vấn dữ liệu
                var tourDetails = await (from t in _context.Tour
                                         join type in _context.CatEnumeration on t.TravelTypeId equals type.EnumerationId
                                         join td in _context.TourDetails on t.TourId equals td.TourId
                                         join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                         join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                         join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                         from a in ttg.DefaultIfEmpty()
                                         where t.TourId == tourID 
                                            && (t.IsDelete == null || t.IsDelete == true) 
                                         orderby t.DateUpdate descending
                                         select new
                                         {
                                             t.TourId,
                                             t.TourName,
                                             t.Rating,
                                             t.Description,
                                             tourImg = BaseUrlServer + t.TourImg.Trim(),
                                             t.DateStart,
                                             t.DeparturePlaceFrom,
                                             t.DeparturePlaceTo,
                                             t.DateEnd,
                                             t.QuanityMax,
                                             t.QuanityMin,
                                             t.CurrentQuanity,
                                             quanity = t.QuanityMax > t.CurrentQuanity ? (t.QuanityMax - t.CurrentQuanity) : 0,
                                             t.Schedule,
                                             t.TravelTypeId,
                                             t.TourGuideId,
                                             t.AdultUnitPrice,
                                             t.ChildrenUnitPrice,
                                             t.BabyUnitPrice,
                                             t.Surcharge,
                                             t.Suggest,
                                             p.ProvinceId,
                                             p.ProvinceName,
                                             regions = (from td in _context.TourDetails
                                                        join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                        join pv in _context.Province on tatt.ProvinceId equals pv.ProvinceId
                                                        where td.TourId == tourID
                                                            && (tatt.IsDelete == null || tatt.IsDelete == true)
                                                            && (td.IsDelete == null || tatt.IsDelete == true)
                                                        select pv.Regions).FirstOrDefault(),
                                             tourDetails = (from td in _context.TourDetails
                                                            join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                            where td.TourId == tourID
                                                                && (tatt.IsDelete == null || tatt.IsDelete == true)
                                                                && (td.IsDelete == null || tatt.IsDelete == true)
                                                            select tatt.TouristAttrId).ToList(),
                                         }).FirstOrDefaultAsync();
                #endregion
                if (tourDetails==null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"Không tìm thấy dữ liệu vui lòng thử lại!");
                }
                return Ok(tourDetails);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        // [Nguyễn Tấn Hải -] - Xử lý Lấy dữ liệu tourDetails
        [HttpGet("TourDetails")]
        public async Task<ActionResult> Cli_TourDetails(Guid? tourID=null)
        {
            try
            {
                var tourDetails = await (from t in _context.Tour
                                         join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                         join td in _context.TourDetails on t.TourId equals td.TourId
                                         join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                         from a in ttg.DefaultIfEmpty()
                                         where t.TourId == tourID && (t.IsDelete == null || t.IsDelete == true)
                                         orderby t.DateUpdate descending
                                         select new
                                         {
                                             t.TourId,
                                             t.TourName,
                                             t.Rating,
                                             t.Description,
                                             tourImg = BaseUrlServer + t.TourImg.Trim(),
                                             dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             totalDay = (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,  // thời hạn của tour => 3 ngày 2 đêm
                                             t.QuanityMax,  
                                             quanity = t.QuanityMax > t.CurrentQuanity? (t.QuanityMax-t.CurrentQuanity) : 0,
                                             schedule = t.Schedule.Replace("&nbsp;", "").Replace("\n",""),
                                             //touGuideName = a.TourGuideName ?? null,
                                             adultUnitPrice = t.AdultUnitPrice,
                                             childrenUnitPrice = t.ChildrenUnitPrice,
                                             babyUnitPrice =  t.BabyUnitPrice,
                                             surcharge =  t.Surcharge,
                                             p.ProvinceName,
                                             tourDetails = (from td in _context.TourDetails
                                                            join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                            where td.TourId == tourID
                                                                && (tatt.IsDelete == null || tatt.IsDelete == true)
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
        [Authorize]
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
                tour.EmpIdinsert = tour.EmpIdinsert;
                tour.EmpIdupdate = tour.EmpIdupdate;
                tour.CurrentQuanity = tour.CurrentQuanity == null ? 0 : tour.CurrentQuanity;
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

        [HttpPut("Adm_UpdateTourById")]
        [Authorize]
        public async Task<ActionResult> Adm_UpdateTour([FromBody] Tour tour)
        {
            try
            {
                if(tour.TourId == Guid.Empty) return StatusCode(StatusCodes.Status400BadRequest, $"Vui lòng kiểm tra dữ liệu cập nhật");
                var rs = await _context.Tour.Where(m => m.TourId == tour.TourId).FirstOrDefaultAsync();
                if(rs == null) return StatusCode(StatusCodes.Status404NotFound, $"Không tìm thấy dữ liệu");

                
                rs.TourName = tour.TourName.Trim();
                rs.Description = tour.Description.Trim();
                if (tour.TourImg != string.Empty)
                {
                    rs.TourImg = tour.TourImg;
                }
                rs.DateStart = tour.DateStart;
                rs.DateEnd = tour.DateEnd;
                rs.Rating = tour.Rating;
                rs.QuanityMax = tour.QuanityMax;
                rs.QuanityMin = tour.QuanityMin;
                rs.AdultUnitPrice = tour.AdultUnitPrice;
                rs.ChildrenUnitPrice = tour.ChildrenUnitPrice;
                rs.BabyUnitPrice = tour.BabyUnitPrice;
                rs.Surcharge = tour.Surcharge;
                rs.Schedule = tour.Schedule.Trim();
                rs.DeparturePlaceFrom = tour.DeparturePlaceFrom;
                rs.UpTransportIdstart = tour.UpTransportIdstart;
                rs.UpTransportIdstart = tour.UpTransportIdend;
                rs.CompanyTransportId = tour.CompanyTransportId;
               //=========
                rs.TourGuide = tour.TourGuide;
                rs.TravelTypeId = tour.TravelTypeId;
                rs.Suggest = tour.Suggest;
                rs.NoteByTour = tour.NoteByTour;
                rs.ConditionByTour = tour.ConditionByTour;
                rs.NoteByMyTour = tour.NoteByMyTour;
                //===
                rs.EmpIdupdate = tour.EmpIdupdate;
                rs.DateUpdate = DateTime.Now.Date;
                
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        // Delete Multi row
        [HttpPut("Adm_DeleteTourByIds")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Tour>>> DeleteTour([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.Tour.Where(m => deleteModels.SelectByIds.Contains(m.TourId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = deleteModels.EmpId;
                });
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, "Xóa thành công!");
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
