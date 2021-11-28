using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using QuanLyTourDuLich.SearchModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristAttractionController : ControllerBase
    {
        /// <summary>
        /// [Thái Trần Kiều Diễm 20211109- xử lý danh sách địa điểm du lịch]
        /// </summary>

        private readonly HUFI_09DHTH_TourManagerContext _context;

        public TouristAttractionController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Nguyễn Tấn Hải][11/21/2021] - load danh sách các địa điểm theo vùng miền
        /// </summary>
        [Route("Adm_GetTouristAttByRegions")]
        [HttpGet]
        public async Task<IActionResult> Adm_GetTouristAttByRegions(int? regions=null)
        {
            try
            {
                var result = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        where (t.IsDelete == null || t.IsDelete == true)
                                            && p.Regions== regions
                                        orderby t.DateUpdate descending
                                        select new
                                        {
                                            value= t.TouristAttrId,
                                            label= t.TouristAttrName,
                                        }).ToListAsync();
                return Ok(result);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        } 


        /// [Get danh sách địa điểm du lịch]
        /// 
        [HttpPost]
        [Route("Adm_GetTouristAttractionList")]
        public async Task<IActionResult> Admin_GetTouristAttractionList([FromBody] TouristAttactionSearchModel trsa)
        {
            
            try
            {
                bool checkModelSearchIsNull = true;
                bool istouristAttrID = Guid.TryParse(trsa.TouristAttrID.ToString(), out Guid touristAttcID);
                bool istouristAttrName = (!string.IsNullOrEmpty(trsa.TouristAttrName));
                bool isprovinceID = false;
                if (trsa.ProvinceID.Length > 0)
                {
                    isprovinceID = true;
                }

                if (isprovinceID || istouristAttrID || istouristAttrName)
                {
                    checkModelSearchIsNull = false;
                }

                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                        where (t.IsDelete == null || t.IsDelete == true)
                                        && checkModelSearchIsNull == true ?true: istouristAttrID==true?
                                        ((istouristAttrID && t.TouristAttrId == trsa.TouristAttrID))
                                        :
                                        (
                                             (istouristAttrName && t.TouristAttrName.Contains(trsa.TouristAttrName))
                                            || (isprovinceID && trsa.ProvinceID.Contains(t.ProvinceId))
                                        )
                                        orderby t.DateUpdate descending
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            Description=t.Description!="null"?t.Description:"Chưa cập nhật",
                                            t.ImagesList,
                                            DateUpdate=t.DateUpdate!=null? DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture):null,
                                            p.ProvinceName,
                                            e.EmpName,
                                        }).ToListAsync();
                if (tourAttrac == null)
                {
                    return NotFound();
                }

                return Ok(tourAttrac);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //Get 

        ///Them moi 1 dia diem  
        ///chưa thêm được
        ///
        [HttpPost("Adm_CreateTourAttraction")]
        public async Task<ActionResult> Admin_CreateTourAttraction([FromBody] TouristAttraction tourAttrac)
        {
            try
            {
                if (tourAttrac == null)
                {
                    return BadRequest();
                }

                tourAttrac.DateInsert = DateTime.Now.Date;
                tourAttrac.DateUpdate = DateTime.Now.Date;
                tourAttrac.IsDelete = null;
                tourAttrac.EmpIdinsert = null;
                tourAttrac.EmpIdupdate = null;

                await _context.TouristAttraction.AddAsync(tourAttrac);
                await _context.SaveChangesAsync();
                return Ok(tourAttrac);

            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///Get thong tin dia diem
        ///chưa get được
        ///

        [HttpGet("Adm_GetTourAttrByID")]
        public async Task<IActionResult> Adm_GetTouristAttractionById(Guid? touristAttrId=null)
        {
            try
            {
                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                        where (t.IsDelete == null || t.IsDelete == true)
                                        && t.TouristAttrId == touristAttrId
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            Description = t.Description != "null" ? t.Description : "Chưa cập nhật",
                                            t.ImagesList,
                                            DateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                            p.ProvinceName,
                                            t.ProvinceId,
                                            e.EmpName,
                                        }).FirstOrDefaultAsync();
                 if (tourAttrac == null)
                {
                    return NotFound();
                }

                return Ok(tourAttrac);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///update theo mã
        ///
        [HttpPut("Adm_UpdateTouristAttraction")]
        public async Task<IActionResult> Adm_UpdateTouristAttraction([FromBody] TouristAttraction tour)
        {

            try
            {
                var rs = await (from t in _context.TouristAttraction
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TouristAttrId == tour.TouristAttrId
                                select t).FirstOrDefaultAsync();
                if(rs == null)
                {
                    return NotFound();
                }

                rs.TouristAttrName = tour.TouristAttrName;
                rs.Description = tour.Description;
                rs.ImagesList = tour.ImagesList;
                rs.ProvinceId = tour.ProvinceId;
                rs.EmpIdupdate = null;
                rs.Status = tour.Status;
                rs.DateUpdate = DateTime.Now.Date;

                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///Xóa một điểm du lịch
        ///

        [HttpPut("Adm_deleteTouristAttraction")]
        public async Task<IActionResult> Adm_deleteTouristAttraction( [FromBody] Guid[] Ids)
        {
           try
            {
                var rs = await _context.TouristAttraction.Where(m => Ids.Contains(m.TouristAttrId)).ToListAsync();
                rs.ForEach(m =>
                {
                    m.DateUpdate = DateTime.Now.Date;
                    m.IsDelete = false;
                });
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

    }
}
