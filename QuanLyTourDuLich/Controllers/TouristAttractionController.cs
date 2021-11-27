﻿using System;
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
        [Route("Admin_GetTouristAttractionList")]
        public async Task<IActionResult> Admin_GetTouristAttractionList([FromForm] touristAttactionSearchModel trsa = null)
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
                                        && checkModelSearchIsNull == true ? true
                                        :
                                        (
                                            (istouristAttrID && t.TouristAttrId == trsa.touristAttrID)
                                            || (istouristAttrName && t.TouristAttrName.Contains(trsa.touristAttrName))
                                            || (isprovinceID && t.ProvinceId == trsa.provinceID)
                                        )
                                        orderby t.DateUpdate descending
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            t.Description,
                                            t.ImagesList,
                                            DateUpdate=DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                            p.ProvinceName,
                                            e.EmpName
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
        [HttpPost("Admin_CreateTourAttraction")]
        public async Task<ActionResult> Admin_CreateTourAttraction([FromBody] TouristAttraction tourAttrac)
        {
            try
            {
                if (tourAttrac == null)
                {
                    return BadRequest();
                }
                TouristAttraction t = new TouristAttraction();
                t.TouristAttrName = tourAttrac.TouristAttrName;
                t.Description = tourAttrac.Description;
                t.ImagesList = tourAttrac.ImagesList;
                t.EmpIdinsert = tourAttrac.EmpIdinsert;
                t.DateInsert = DateTime.Now;
                t.EmpIdupdate = tourAttrac.EmpIdupdate;
                t.DateUpdate = DateTime.Now;
                t.ProvinceId = tourAttrac.ProvinceId;
                t.Status = tourAttrac.Status;
                t.IsDelete = null;

                await _context.TouristAttraction.AddAsync(t);
                await _context.SaveChangesAsync();
                return Ok(t);

            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        ///Get thong tin dia diem
        ///chưa get được
        ///
        [HttpGet("Adm_GetTourAttrByID/{TourAttrId:int}")]
        public async Task<IActionResult> Adm_GetTouristAttractionById(Guid? TourAttrId)
        {
            try
            {
                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                        where (t.IsDelete == null || t.IsDelete == true)
                                        && t.TouristAttrId == TourAttrId
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            t.Description,
                                            t.ImagesList,
                                            DateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                            p.ProvinceName,
                                            e.EmpName
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

        [HttpPut("Adm_UpdateTouristAttraction/{TourAttrId:int}")]
        public async Task<IActionResult> Adm_UpdateTouristAttraction([FromBody] TouristAttraction tour, Guid? TourAttrId)
        {
            if (tour.TouristAttrId != TourAttrId)
            {
                return BadRequest();
            }
            try
            {
                var rs = await (from t in _context.TouristAttraction
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TouristAttrId == TourAttrId
                                select t).FirstOrDefaultAsync();
                if(rs == null)
                {
                    return NotFound();
                }

                rs.TouristAttrName = tour.TouristAttrName;
                rs.Description = tour.Description;
                rs.ImagesList = tour.ImagesList;
                rs.ProvinceId = tour.ProvinceId;
                rs.EmpIdupdate = tour.EmpIdupdate;
                rs.Status = tour.Status;
                rs.DateUpdate = DateTime.Now;

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

        [HttpPut("Adm_deleteTouristAttraction/{TourAttrId:int}/{empID:int}")]
        public async Task<IActionResult> Adm_deleteTouristAttraction( Guid TourAttrId, Guid? empID)
        {

            try
            {
                var rs = await (from t in _context.TouristAttraction
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TouristAttrId == TourAttrId
                                select t).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

       
                rs.EmpIdupdate = empID;
                rs.DateUpdate = DateTime.Now;
                rs.IsDelete = false;

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