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
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        // Nguyễn Tấn Hải [24/10/2021] - Rest full api Tour

        public const string BaseUrlServer = "http://localhost:8000/ImagesTour/";
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public TourController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Lấy những tour nào được đề xuất max 6 tour
        [HttpGet("TourIsSuggest")]
        public async Task<IActionResult> TourIsSuggest(Guid? tourFamily, int? regions)
        
        {
            try
            {
                Guid travelType = new Guid();
                bool isGuid = Guid.TryParse(tourFamily.ToString(), out travelType);
                var tourList = await (from t in _context.Tour
                                      join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                      join ptt in _context.Province on t.DeparturePlaceTo equals ptt.ProvinceId
                                      join type in _context.CatEnumeration on t.TravelTypeId equals type.EnumerationId
                                      join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                      join pt in _context.PromotionalTour on t.TourId equals pt.TourId into tpt
                                      from a in tpt.DefaultIfEmpty()
                                      join km in _context.Promotion on a.PromotionId equals km.PromotionId into kma
                                      from b in kma.DefaultIfEmpty()
                                      where (t.IsDelete == null || t.IsDelete == true)
                                                && t.DateStart >= DateTime.Now.Date.AddDays(1)
                                      orderby t.DateUpdate descending, t.DateStart ascending
                                      select new
                                      {
                                          t.TourId,
                                          t.TourName,
                                          tourImg = BaseUrlServer + t.TourImg.Trim(),
                                          t.DateStart,
                                          dateStartFormat = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                          TotalDays = (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,
                                          TotalCurrentQuanity = t.QuanityMax - t.CurrentQuanity,
                                          t.Rating,
                                          t.AdultUnitPrice,
                                          p.ProvinceName,
                                          t.TravelTypeId,
                                          type.EnumerationTranslate,
                                          t.GroupNumber,
                                          //promotion = a.IsDelete ==false? null: b.DateEnd < DateTime.Now.Date? null: b.Discount,
                                          promotion =  _context.PromotionalTour.Where(m => (m.IsDelete == null || m.IsDelete == true) 
                                                                && m.TourId==t.TourId && m.Promotion.DateEnd >= DateTime.Now.Date)
                                                                .OrderByDescending(m=>m.Promotion.DateEnd).Select(m=>m.Promotion.Discount).FirstOrDefault(),
                                          ptt.Regions,
                                          t.Suggest,
                                          //tourGuideName = a.TourGuideName?? null,
                                      }).Distinct().ToListAsync();
                var listObj = tourList.GroupBy(x => x.TourName.Trim()).Select(m => m.FirstOrDefault());
                // lọc các tour khi người dùng click vào trang details => lọc ra các tour có chứa 
                if(regions != null)
                {
                    listObj = listObj.Where(m => m.Regions == regions).ToList();
                    return Ok(listObj.Take(4));
                }
                if (isGuid)
                {
                    listObj = listObj.Where(m => m.TravelTypeId == travelType && m.Suggest == true).ToList();
                    return Ok(listObj.Take(3));
                }
                else
                {
                    listObj = listObj.Where(m => m.TravelTypeId != new Guid("8F64FB01-91FE-4850-A004-35CF26A1C1EF") && m.Suggest == true).ToList();
                    return Ok(listObj.Take(6));
                }
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
                                    where (t.IsDelete == null || t.IsDelete == true) && (td.IsDelete == null || td.IsDelete == true)
                                    orderby t.DateUpdate descending, t.DateStart ascending
                                    select new
                                    {
                                        t.TourId,
                                        t.TourName,
                                        t.Description,
                                        tourImg = BaseUrlServer + t.TourImg.Trim(),
                                        dateStartCheck = t.DateStart,
                                        dateEndCheck = t.DateEnd,
                                        dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        t.Rating,
                                        t.QuanityMax,
                                        t.QuanityMin,
                                        t.CurrentQuanity,
                                        t.Schedule,
                                        departurePlaceFrom = p.ProvinceName,
                                        departurePlaceFromCheck = p.ProvinceId,
                                        departurePlaceTo = pt.ProvinceName,
                                        t.Suggest,
                                        emp.EmpName,
                                        dateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        adultUnitPrice = string.Format("{0:0,0đ}", t.AdultUnitPrice),
                                        childrenUnitPrice = string.Format("{0:0,0đ}", t.ChildrenUnitPrice),
                                        babyUnitPrice = string.Format("{0:0,0đ}", t.BabyUnitPrice),
                                        surcharge = string.Format("{0:0,0đ}", t.Surcharge),
                                        t.TravelTypeId,
                                        travelTypeName = type.EnumerationTranslate,
                                        //type.TravelTypeName,

                                        tourGuideName = t.TourGuideId == null ? "Chưa cập nhật" : a.TourGuideName
                                    }).Distinct().ToListAsync();
                #endregion

                #region lọc dữ liệu với các điều kiện
                if (tourSearch.TourName != "" && tourSearch.TourName !=null)
                {
                    result = result.Where(m => m.TourName.Contains(tourSearch.TourName.Trim())).ToList();
                }
                if(tourSearch.DateStart !=null && tourSearch.DateEnd != null)
                {
                    result = result.Where(m => m.dateStartCheck >= tourSearch.DateStart 
                    && m.dateEndCheck <= tourSearch.DateEnd).ToList();
                }
                if(tourSearch.DeparturePlace.Length != 0)
                {
                    result = result.Where(m => tourSearch.DeparturePlace.Contains(m.departurePlaceFromCheck)).ToList();
                }
                if (tourSearch.Suggest == true)
                {
                    result = result.Where(m => m.Suggest == true).ToList();
                }
                if(tourSearch.TravelTypeID != Guid.Empty && tourSearch.TravelTypeID !=null)
                {
                    result = result.Where(m => m.TravelTypeId == tourSearch.TravelTypeID).ToList();
                }
                if (tourSearch.TourIsExpired == true)
                {
                    result = result.Where(m => m.dateStartCheck <= DateTime.Now.Date.AddDays(5)).ToList();
                }
                #endregion
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        
        [HttpPost("Cli_GetDataTourSearch")]
        public async Task<ActionResult> Cli_GetDataTourSearch([FromBody] TourSearchModelClient tourSearch = null)
        {
            try
            {
                // lấy danh sách tất cả các tour thỏa
                var rs = await (from t in _context.Tour
                                join tt in _context.CatEnumeration on t.TravelTypeId equals tt.EnumerationId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                join ptt in _context.PromotionalTour on t.TourId equals ptt.TourId into tpt
                                from a in tpt.DefaultIfEmpty()
                                join km in _context.Promotion on a.PromotionId equals km.PromotionId into kma
                                from b in kma.DefaultIfEmpty()
                                where (t.IsDelete == null || t.IsDelete == true) && t.DateStart >= DateTime.Now.Date.AddDays(1)
                                orderby t.DateUpdate descending
                                select new TourModel {
                                    TourId = t.TourId,
                                    TourName = t.TourName,
                                    TourImg = BaseUrlServer + t.TourImg.Trim(),
                                    DateStart = t.DateStart,
                                    DateStartFormat = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    DateEnd = t.DateEnd,
                                    TotalDays = (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,
                                    Rating = t.Rating,
                                    GroupNumber = t.GroupNumber,
                                    AdultUnitPrice = t.AdultUnitPrice,
                                    ChildrenUnitPrice = t.ChildrenUnitPrice,
                                    BabyUnitPrice = t.BabyUnitPrice,
                                    DeparturePlaceFromName = pf.ProvinceName,
                                    DeparturePlaceToName = pt.ProvinceName,
                                    DeparturePlaceFrom = pf.ProvinceId,
                                    DeparturePlaceTo = pt.ProvinceId,
                                    QuanityMax = t.QuanityMax,
                                    QuanityMin = t.QuanityMin,
                                    TotalCurrentQuanity = t.QuanityMax - t.CurrentQuanity,
                                    CurrentQuanity = t.CurrentQuanity,
                                    TravelTypeId = t.TravelTypeId,
                                    TravelTypeName = tt.EnumerationTranslate,
                                    Promotion = _context.PromotionalTour.Where(m => (m.IsDelete == null || m.IsDelete == true)
                                                                && m.TourId == t.TourId && m.Promotion.DateEnd >= DateTime.Now.Date)
                                                                .OrderByDescending(m => m.Promotion.DateEnd).Select(m => m.Promotion.Discount).FirstOrDefault(),

                                }).Distinct().ToListAsync();

                var listObjTemp = rs.GroupBy(x => x.TourId).Select(m => m.FirstOrDefault());

                // lấy loại phương tiên => để lọc theo phương tiện
                var cat_Enum = await _context.CatEnumeration.Where(m => (m.IsDelete == null || m.IsDelete == true)
                                        && m.EnumerationType == "TransportType").ToListAsync();

                #region lọc bởi các điều kiện cơ bản
                if (tourSearch.TravelTypeID != null)
                {
                    listObjTemp = listObjTemp.Where(m => m.TravelTypeId == tourSearch.TravelTypeID).ToList();
                }
                if (tourSearch.DeparturePlaceFrom != null)
                {
                    listObjTemp = listObjTemp.Where(m => m.DeparturePlaceFrom == tourSearch.DeparturePlaceFrom).ToList();
                }
                if (tourSearch.DeparturePlaceTo != null)
                {
                    listObjTemp = listObjTemp.Where(m => m.DeparturePlaceTo == tourSearch.DeparturePlaceTo).ToList();
                }
                if (tourSearch.DateStart != null)
                {
                    listObjTemp = listObjTemp.Where(m => m.DateStart >= tourSearch.DateStart).ToList();
                }
                #endregion

                #region kiểm tra số lượng search nếu có checked

                // check số lượng nếu có lọc
                if (tourSearch.QuantityPeople1 == true || tourSearch.QuantityPeople2 == true ||
                            tourSearch.QuantityPeople3 == true || tourSearch.QuantityPeople4 == true)
                {
                    int checkCount = 0;
                    if (tourSearch.QuantityPeople1 == true) checkCount = 1;
                    if (tourSearch.QuantityPeople2 == true) checkCount = 2;
                    if (tourSearch.QuantityPeople3 == true) checkCount = 3;
                    if (tourSearch.QuantityPeople4 == true) checkCount = 5;
                    List<TourModel> listObjNew = new List<TourModel>();
                    for (int i = 0; i < rs.Count; i++)
                    {
                        if ((rs[i].QuanityMax - rs[i].CurrentQuanity) >= checkCount)
                        {
                            listObjNew.Add(rs[i]);
                        }
                    }
                    listObjTemp = listObjNew;
                }

                #endregion

                #region kiểm ngày số ngày đi có thỏa không?
                if (tourSearch.QuantityDate1 == true || tourSearch.QuantityDate2 == true
                    || tourSearch.QuantityDate3 == true || tourSearch.QuantityDate4 == true)
                {
                    int totalDaysStart = 0;
                    int totalDaysEnd = 0;
                    if (tourSearch.QuantityDate1 == true)
                    {
                        totalDaysStart = 1;
                        totalDaysEnd = 3;
                    }
                    if (tourSearch.QuantityDate2 == true)
                    {
                        totalDaysStart = 4;
                        totalDaysEnd = 7;
                    }
                    if (tourSearch.QuantityDate3 == true)
                    {
                        totalDaysStart = 8;
                        totalDaysEnd = 14;
                    }
                    if (tourSearch.QuantityDate4 == true)
                    {
                        totalDaysStart = 14;
                        totalDaysEnd = Int32.MaxValue - 1;
                    }
                    List<TourModel> listObjNew = new List<TourModel>();
                    for (int i = 0; i < rs.Count; i++)
                    {
                        DateTime datStart = DateTime.Parse(rs[i].DateStart.ToString());
                        DateTime dateEnd = DateTime.Parse(rs[i].DateEnd.ToString());
                        TimeSpan time = dateEnd - datStart;
                        int totalDayOfMyTour = time.Days;
                        if (totalDayOfMyTour >= totalDaysStart && totalDayOfMyTour <= totalDaysEnd)
                        {
                            listObjNew.Add(rs[i]);
                        }
                    }
                    listObjTemp = listObjNew;
                }

                #endregion

                #region lọc bởi trainsport

                if (tourSearch.TransportType1 == true)
                {

                }
                if (tourSearch.TransportType2 == true)
                {

                }
                #endregion

                #region phân trang

                int totalRecord = listObjTemp.Count();
                var pagination = new Pagination
                {
                    count = totalRecord,
                    currentPage = tourSearch.Page,
                    pagsize = tourSearch.Limit,
                    totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, tourSearch.Limit)),
                    indexOne = ((tourSearch.Page - 1) * tourSearch.Limit + 1),
                    indexTwo = (((tourSearch.Page - 1) * tourSearch.Limit + tourSearch.Limit) <= totalRecord ? ((tourSearch.Page - 1) * tourSearch.Limit * tourSearch.Limit) : totalRecord)
                };

                var listObj = listObjTemp.Skip((tourSearch.Page - 1) * tourSearch.Limit).Take(tourSearch.Limit).ToList();
                #endregion
                return Ok(new {
                    data = listObj,
                    pagination = pagination
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
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
                                             t.GroupNumber,
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
                                             // transport type
                                             t.NoteByMyTour,
                                             t.NoteByTour,
                                             t.ConditionByTour,
                                             TransportTypeID = t.CompanyTransportStart.EnumerationId,
                                             CompanyTransportStart = t.CompanyTransportStartId,
                                             CompanyTransportInTour = t.CompanyTransportInTourId,
                                         }).FirstOrDefaultAsync();
                #endregion
                if (tourDetails == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"Không tìm thấy dữ liệu vui lòng thử lại!");
                }
                return Ok(tourDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        // [Nguyễn Tấn Hải -] - Xử lý Lấy dữ liệu tourDetails
        [HttpGet("TourDetails")]
        public async Task<ActionResult> Cli_TourDetails(Guid? tourID = null)
        {
            try
            {
                var tourDetails = await (from t in _context.Tour
                                         join p in _context.Province on t.DeparturePlaceFrom equals p.ProvinceId
                                         join ptt in _context.Province on t.DeparturePlaceTo equals ptt.ProvinceId
                                         //join td in _context.TourDetails on t.TourId equals td.TourId
                                         join pt in _context.PromotionalTour on t.TourId equals pt.TourId into tpt
                                         from a in tpt.DefaultIfEmpty()
                                         join km in _context.Promotion on a.PromotionId equals km.PromotionId into kma
                                         from b in kma.DefaultIfEmpty()
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
                                             quanity = t.QuanityMax > t.CurrentQuanity ? (t.QuanityMax - t.CurrentQuanity) : 0,
                                             schedule = t.Schedule.Replace("&nbsp;", "").Replace("\n", ""),
                                             touGuideName = t.TourGuideId ==null? null: t.TourGuide.TourGuideName,
                                             transportStart = t.CompanyTransportStartId ==null? null: t.CompanyTransportStart.Enumeration.EnumerationTranslate,
                                             transportInTour = t.CompanyTransportInTourId ==null? null: t.CompanyTransportInTour.Enumeration.EnumerationTranslate,
                                             adultUnitPrice = t.AdultUnitPrice,
                                             childrenUnitPrice = t.ChildrenUnitPrice,
                                             babyUnitPrice = t.BabyUnitPrice,
                                             surcharge = t.Surcharge,
                                             p.ProvinceName,
                                             t.GroupNumber,
                                             promotion = _context.PromotionalTour.Where(m => (m.IsDelete == null || m.IsDelete == true)
                                                               && m.TourId == t.TourId && m.Promotion.DateEnd >= DateTime.Now.Date)
                                                                .OrderByDescending(m => m.Promotion.DateEnd).Select(m => m.Promotion.Discount).FirstOrDefault(),
                                             t.TravelTypeId,
                                             t.TravelType.EnumerationTranslate,
                                             tourDetails = (from td in _context.TourDetails
                                                            join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                            where td.TourId == tourID
                                                                && (tatt.IsDelete == null || tatt.IsDelete == true)
                                                                && (td.IsDelete == null || tatt.IsDelete == true)
                                                            select new
                                                            {
                                                                tatt.TouristAttrName,
                                                                imagesList = tatt.ImagesList.Trim(),
                                                                tatt.Description
                                                            }).ToList(),
                                             ptt.Regions,
                                         }).FirstOrDefaultAsync();
                if (tourDetails == null)
                {
                    return NotFound();
                }
                return Ok(tourDetails);
            }
            catch (Exception ex)
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
                if (tour == null)
                {
                    // 400
                    return BadRequest();
                }
                var rs = await _context.Tour.Where(m => m.TourName.Trim().Equals(tour.TourName.Trim())
                        && m.DateStart == tour.DateStart && m.TravelTypeId == tour.TravelTypeId
                         && m.DeparturePlaceFrom == tour.DeparturePlaceFrom
                         && m.DeparturePlaceTo ==tour.DeparturePlaceTo
                         && (m.IsDelete==null || m.IsDelete==true)).FirstOrDefaultAsync();
                if (rs != null)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Tour đã tồn tại trong hệ thống");
                }
                if (tour.TravelTypeId == new Guid("8f64fb01-91fe-4850-a004-35cf26a1c1ef"))
                {
                    tour.GroupNumber = tour.GroupNumber;
                    
                }
                else
                {
                    tour.ChildrenUnitPrice =  tour.ChildrenUnitPrice;
                    tour.BabyUnitPrice = tour.BabyUnitPrice;
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
                if (tour.TourId == Guid.Empty) return StatusCode(StatusCodes.Status400BadRequest, $"Vui lòng kiểm tra dữ liệu cập nhật");
                var rs = await _context.Tour.Where(m => m.TourId == tour.TourId).FirstOrDefaultAsync();
                if (rs == null) return StatusCode(StatusCodes.Status404NotFound, $"Không tìm thấy dữ liệu");


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
                if (tour.TravelTypeId == new Guid("8f64fb01-91fe-4850-a004-35cf26a1c1ef"))
                {
                    rs.GroupNumber = tour.GroupNumber;
                    rs.ChildrenUnitPrice = 0;
                    rs.BabyUnitPrice = 0;
                }
                else
                {
                    rs.GroupNumber = 0;
                    rs.ChildrenUnitPrice = tour.ChildrenUnitPrice;
                    rs.BabyUnitPrice = tour.BabyUnitPrice;
                }
                rs.AdultUnitPrice = tour.AdultUnitPrice;
                rs.Surcharge = tour.Surcharge;
                rs.Schedule = tour.Schedule.Trim();
                rs.DeparturePlaceFrom = tour.DeparturePlaceFrom;
                rs.CompanyTransportStartId = tour.CompanyTransportStartId;
                rs.CompanyTransportInTourId = tour.CompanyTransportInTourId;
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
            catch (Exception ex)
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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("Adm_UpdateSuggest")]
        public async Task<ActionResult> Adm_UpdateSuggest(Guid? pID=null, Guid? pEmpID=null)
        {
            try
            {
                var rs = await _context.Tour.Where(m => m.TourId == pID && (m.IsDelete == null || m.IsDelete == true)).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return BadRequest();
                }
                rs.Suggest = !rs.Suggest;
                rs.DateUpdate = DateTime.Now.Date;
                rs.EmpIdinsert = pEmpID;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //============ sendmessage
        // kiểm tra tour gần tới ngày hết hạn thì thông báo tới người dùng
        [HttpGet("SendMessageTourExpired")]
        public async Task<ActionResult> SendMessageTourExpired()
        {
            try
            {
                var rs =await (from t in _context.Tour
                               join td in _context.TourDetails on t.TourId equals td.TourId
                          where (t.IsDelete == null || t.IsDelete == true)
                                && t.DateStart < DateTime.Now.Date.AddDays(1)
                          select new
                          {
                              MessageTour= t.TourId 
                          }).Distinct().ToListAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///get tất cả các tour
        ///
        [HttpGet("MB_Cli_GetTourListPagination")]
        public async Task<IActionResult> Cli_GetTourListPagination(string provinceName,Guid? tourFamily, int page = 1, int limit = 10)
        {
            try
            {
                bool isTourName = (!string.IsNullOrEmpty(provinceName));
                Guid travelType = new Guid();
                bool isGuid = Guid.TryParse(tourFamily.ToString(), out travelType);
                var tourList = await (from t in _context.Tour
                                      join pf in _context.Province on t.DeparturePlaceTo equals pf.ProvinceId // ty em check lai cho nay nha
                                      //join up in _context.UnitPrice on t.TourId equals up.TourId
                                      join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                      join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                      from a in ttg.DefaultIfEmpty()
                                      where isTourName == true ?
                                       ((t.IsDelete == null || t.IsDelete == true)
                                      && pf.ProvinceName.Contains(provinceName)
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))
                                      : isGuid == true ?
                                      (t.TravelTypeId == tourFamily
                                      && (t.IsDelete == null || t.IsDelete == true)
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))
                                      : ((t.IsDelete == null || t.IsDelete == true)
                                      && t.Suggest == true
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))

                                      orderby t.DateUpdate descending, t.DateStart ascending
                                      select new
                                      {
                                          t.TourId,
                                          t.TourName,
                                          tourImg = BaseUrlServer + t.TourImg.Trim(),
                                          DateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                          Time= (t.DateEnd - t.DateStart).Value.Days,
                                          t.Rating,
                                          t.QuanityMax,
                                          t.QuanityMin,
                                          t.AdultUnitPrice,
                                          pf.ProvinceName, //dau
                                          t.Suggest,
                                          t.TravelTypeId,
                                      }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = (from t in _context.Tour
                                   join pf in _context.Province on t.DeparturePlaceTo equals pf.ProvinceId
                                   where isTourName == true ?
                                       ((t.IsDelete == null || t.IsDelete == true)
                                      && pf.ProvinceName.Contains(provinceName)
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))
                                      : isGuid == true ?
                                      (t.TravelTypeId == tourFamily 
                                      && (t.IsDelete == null || t.IsDelete == true)
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))
                                      : ((t.IsDelete == null || t.IsDelete == true) 
                                      && t.Suggest == true
                                      && t.DateStart >= DateTime.Now.Date.AddDays(1))
                                   select t).Count();
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

        [HttpGet("MB_Cli_GetTourDescriptionById")]
        public async Task<IActionResult> Cli_GetTourDescriptionById(Guid TourId)
        {
            try
            {
                var tour = await (from t in _context.Tour
                                  join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                  join emp in _context.Employee on t.EmpIdupdate equals emp.EmpId
                                  //join tg in _context.TourGuide on t.TourGuideId equals tg.TourGuideId into ttg
                                  //from a in ttg.DefaultIfEmpty()
                                  where t.TourId == TourId
                                  select new
                                  {
                                      t.TourId,
                                      t.TourName,
                                      tourImg = BaseUrlServer + t.TourImg.Trim(),
                                      t.Description,
                                      DateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                      Time = (t.DateEnd - t.DateStart).Value.Days,
                                      t.BabyUnitPrice,
                                      t.ChildrenUnitPrice,
                                      t.QuanityMax,
                                      t.QuanityMin,
                                      schedule = t.Schedule.Replace("&nbsp;", "").Replace("\n", ""),
                                      t.Rating,
                                      t.AdultUnitPrice,
                                      quanity = t.QuanityMax > t.CurrentQuanity ? (t.QuanityMax - t.CurrentQuanity) : 0,
                                      t.Surcharge,
                                      DeparturePlaceTo = _context.Province.Where(m => m.ProvinceId == t.DeparturePlaceTo).Select(m => m.ProvinceName).FirstOrDefault(),
                                      t.TravelTypeId,
                                      pf.ProvinceName,
                                      touGuideName = t.TourGuideId == null ? null : t.TourGuide.TourGuideName,
                                      transportStart = t.CompanyTransportStartId == null ? null : t.CompanyTransportStart.Enumeration.EnumerationTranslate,
                                      transportInTour = t.CompanyTransportInTourId == null ? null : t.CompanyTransportInTour.Enumeration.EnumerationTranslate,
                                      t.NoteByMyTour,
                                      t.NoteByTour,
                                      t.ConditionByTour,
                                      t.GroupNumber,
                                      tourDetails = (from td in _context.TourDetails
                                                     join tatt in _context.TouristAttraction on td.TouristAttrId equals tatt.TouristAttrId
                                                     where td.TourId == TourId
                                                         && (tatt.IsDelete == null || tatt.IsDelete == true)
                                                         && (td.IsDelete == null || tatt.IsDelete == true)
                                                     select new
                                                     {
                                                         tatt.TouristAttrName,
                                                     }).ToList(),
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
