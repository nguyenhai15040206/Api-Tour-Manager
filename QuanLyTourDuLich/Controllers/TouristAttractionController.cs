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


        /// [Get danh sách địa điểm du lịch]
        /// 
        [HttpPost]
        [Route("Adm_GetTouristAttractionList")]
        public async Task<IActionResult> Admin_GetTouristAttractionList([FromBody] touristAttactionSearchModel trsa)
        {
            
            try
            {
                bool checkModelSearchIsNull = true;
                bool istouristAttrID = int.TryParse(trsa.touristAttrID.ToString(), out int touristAttcID);
                bool istouristAttrName = (!string.IsNullOrEmpty(trsa.touristAttrName));
                bool isprovinceID = int.TryParse(trsa.provinceID.ToString(), out int provinceID);

                if (isprovinceID || istouristAttrID || istouristAttrName)
                {
                    checkModelSearchIsNull = false;
                }

                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                        where (t.IsDelete == null || t.IsDelete == true)
                                        && checkModelSearchIsNull == true ?true: istouristAttrID==true?
                                        ((istouristAttrID && t.TouristAttrId == trsa.touristAttrID))
                                        :
                                        (
                                             (istouristAttrName && t.TouristAttrName.Contains(trsa.touristAttrName))
                                            || (isprovinceID && t.ProvinceId == trsa.provinceID)
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
                tourAttrac.EmpIdinsert = 1;
                tourAttrac.EmpIdupdate = 1;

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
        public async Task<IActionResult> Adm_GetTouristAttractionById(int? touristAttrId=null)
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
                rs.EmpIdupdate = 1;
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
        public async Task<IActionResult> Adm_deleteTouristAttraction( [FromBody] int[] Ids)
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
