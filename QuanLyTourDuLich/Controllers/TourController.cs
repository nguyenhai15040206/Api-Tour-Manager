﻿using Microsoft.AspNetCore.Http;
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

        [HttpPost("Cli_GetDataTourSearch")]
        public async Task<ActionResult> Cli_GetDataTourSearch([FromBody] TourSearchModelClient tourSearch =null)
        {
            try
            {
                // lấy danh sách tất cả các tour thỏa
                var rs = await (from t in _context.Tour
                                join tt in _context.CatEnumeration on t.TravelTypeId equals tt.EnumerationId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                where (t.IsDelete ==null || t.IsDelete == true)
                                orderby t.DateUpdate descending
                                select new TourModel { 
                                    TourId = t.TourId,
                                    TourName = t.TourName,
                                    TourImg = BaseUrlServer + t.TourImg.Trim(),
                                    DateStart = t.DateStart,
                                    DateStartFormat= DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    DateEnd = t.DateEnd,
                                    TotalDays= (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,
                                    Rating = t.Rating,
                                    AdultUnitPrice = t.AdultUnitPrice,
                                    ChildrenUnitPrice = t.ChildrenUnitPrice,
                                    BabyUnitPrice = t.BabyUnitPrice,
                                    DeparturePlaceFromName = pf.ProvinceName,
                                    DeparturePlaceToName = pt.ProvinceName,
                                    DeparturePlaceFrom = pf.ProvinceId,
                                    DeparturePlaceTo = pt.ProvinceId,
                                    QuanityMax = t.QuanityMax,
                                    QuanityMin = t.QuanityMin,
                                    TotalCurrentQuanity= t.QuanityMax - t.CurrentQuanity,
                                    CurrentQuanity = t.CurrentQuanity,
                                    TravelTypeId = t.TravelTypeId,
                                    TravelTypeName = tt.EnumerationTranslate
                                    
                                }).ToListAsync();

                // lấy loại phương tiên => để lọc theo phương tiện
                var cat_Enum = await _context.CatEnumeration.Where(m => (m.IsDelete == null || m.IsDelete == true)
                                        && m.EnumerationType== "TransportType").ToListAsync();

                #region lọc bởi các điều kiện cơ bản
                if (tourSearch.TravelTypeID != null)
                {
                    rs = rs.Where(m => m.TravelTypeId == tourSearch.TravelTypeID).ToList();
                }
                if (tourSearch.DeparturePlaceFrom != null)
                {
                    rs = rs.Where(m => m.DeparturePlaceFrom == tourSearch.DeparturePlaceFrom).ToList();
                }
                if (tourSearch.DeparturePlaceTo != null)
                {
                    rs = rs.Where(m => m.DeparturePlaceTo == tourSearch.DeparturePlaceTo).ToList();
                }
                if (tourSearch.DateStart != null)
                {
                    rs = rs.Where(m => m.DateStart >= tourSearch.DateStart).ToList();
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
                    rs = listObjNew;
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
                        totalDaysEnd = Int32.MaxValue-1;
                    }
                    List<TourModel> listObjNew = new List<TourModel>();
                    for (int i = 0; i < rs.Count; i++)
                    {
                        DateTime datStart = DateTime.Parse(rs[i].DateStart.ToString());
                        DateTime dateEnd = DateTime.Parse(rs[i].DateEnd.ToString());
                        TimeSpan time = dateEnd - datStart;
                        int totalDayOfMyTour = time.Days;
                        if (totalDayOfMyTour >= totalDaysStart && totalDayOfMyTour <=totalDaysEnd)
                        {
                            listObjNew.Add(rs[i]);
                        }
                    }
                    rs = listObjNew;
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

                int totalRecord  = rs.Count();
                var pagination = new Pagination
                {
                    count = totalRecord,
                    currentPage = tourSearch.Page,
                    pagsize = tourSearch.Limit,
                    totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, tourSearch.Limit)),
                    indexOne = ((tourSearch.Page - 1) * tourSearch.Limit + 1),
                    indexTwo = (((tourSearch.Page - 1) * tourSearch.Limit + tourSearch.Limit) <= totalRecord ? ((tourSearch.Page - 1) * tourSearch.Limit * tourSearch.Limit) : totalRecord)
                };

                var listObj = rs.Skip((tourSearch.Page - 1) * tourSearch.Limit).Take(tourSearch.Limit).ToList();
                #endregion
                return Ok(new { 
                    data= listObj,
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
                rs.CompanyTransportStartId = tour.CompanyTransportStartId;
                rs.CompanyTransportInTour = tour.CompanyTransportInTour;
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
