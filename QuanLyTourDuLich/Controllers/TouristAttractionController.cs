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
        public const string BaseUrlServer = "http://localhost:8000/ImagesTouristAttractions/";
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
                                        orderby t.DateUpdate descending, t.DateInsert descending
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

        /// <summary>
        /// [Nguyễn Tấn Hải][11/21/2021] - load danh sách các địa điểm theo vùng miền
        /// </summary>
        [Route("Adm_GetTouristAttByProvince")]
        [HttpGet]
        public async Task<IActionResult> Adm_GetTouristAttByProvince(string provinceIDs)
        {
            try
            {
                string[] arr = provinceIDs.Split(',').ToArray();
                int[] arrProvince = Array.ConvertAll(arr, s => int.Parse(s));
                var result = await (from t in _context.TouristAttraction
                                    join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                    where (t.IsDelete == null || t.IsDelete == true)
                                        && arrProvince.Contains(p.ProvinceId)
                                    orderby t.DateUpdate descending
                                    select new
                                    {
                                        value = t.TouristAttrId,
                                        label = t.TouristAttrName,
                                    }).ToListAsync();
                return Ok(result);
            }
            catch (Exception)
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
                string[] separator = { "||" };
                bool checkModelSearchIsNull = true;
                bool istouristAttrName = (!string.IsNullOrEmpty(trsa.TouristAttrName));
                bool isprovinceID = false;
                if (trsa.ProvinceID.Length > 0)
                {
                    isprovinceID = true;
                }

                if (isprovinceID || istouristAttrName)
                {
                    checkModelSearchIsNull = false;
                }

                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                        where checkModelSearchIsNull == true ? (t.IsDelete == null || t.IsDelete == true)
                                         : (
                                            (t.IsDelete == null || t.IsDelete == true)
                                             && (istouristAttrName && t.TouristAttrName.Contains(trsa.TouristAttrName.Trim()))
                                            || (isprovinceID && trsa.ProvinceID.Contains(t.ProvinceId))
                                        )
                                        orderby t.DateUpdate descending, t.DateInsert descending
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            Description=t.Description!="null"? t.Description:"Chưa cập nhật",
                                            imagesList= t.ImagesList==null? null :t.ImagesList.Split(separator, System.StringSplitOptions.RemoveEmptyEntries),
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
                tourAttrac.EmpIdinsert = tourAttrac.EmpIdinsert;
                tourAttrac.EmpIdupdate = tourAttrac.EmpIdupdate;
                tourAttrac.IsDelete = null;

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
                string[] separator = { "||" };
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
                                            imagesList = t.ImagesList == null ? null : t.ImagesList.Split(separator, System.StringSplitOptions.RemoveEmptyEntries),
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
                if(tour.ImagesList != "")
                {
                    rs.ImagesList = tour.ImagesList;
                }
                rs.TouristAttrName = tour.TouristAttrName;
                rs.Description = tour.Description;
                
                rs.ProvinceId = tour.ProvinceId;
                rs.EmpIdupdate = tour.EmpIdupdate;
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
        public async Task<IActionResult> Adm_deleteTouristAttraction([FromBody] DeleteModels deleteModels)
        {
           try
            {
                var rs = await _context.TouristAttraction.Where(m => deleteModels.SelectByIds.Contains(m.TouristAttrId)).ToListAsync();
                rs.ForEach(m =>
                {
                    m.DateUpdate = DateTime.Now.Date;
                    m.IsDelete = false;
                    m.EmpIdupdate = deleteModels.EmpId;
                });
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        /// <summary>
        /// get tour theo ten dia diem
        /// </summary>
        /// <param name="TouristAttrName"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("MB_Cli_GetTourAttractByProAndId")]
        public async Task<IActionResult> Cli_GetTourAttractByProAndId(string TouristAttrName=null, int page=1, int limit=10)
        {
            try
            {
                string[] separator = { "||" };
                bool checkModelSearchIsNull = true;
                bool istouristAttrName = (!string.IsNullOrEmpty(TouristAttrName));
                bool isprovinceID = false;
                //if (ProvinceID.Length > 0)
                //{
                //    isprovinceID = true;
                //}

                if (isprovinceID || istouristAttrName)
                {
                    checkModelSearchIsNull = false;
                }

                var tourAttrac = await (from t in _context.TouristAttraction
                                        join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                        where checkModelSearchIsNull == true ? (t.IsDelete == null || t.IsDelete == true)
                                         : (
                                            (t.IsDelete == null || t.IsDelete == true)
                                             && (istouristAttrName && t.TouristAttrName.Contains(TouristAttrName))
                                        )
                                        orderby t.DateUpdate descending
                                        select new
                                        {
                                            t.TouristAttrId,
                                            t.TouristAttrName,
                                            Description = t.Description != "null" ? t.Description : "Chưa cập nhật",
                                            ImagesList = BaseUrlServer + t.ImagesList==null?null: t.ImagesList.Trim().Split(separator, StringSplitOptions.RemoveEmptyEntries)[0].ToString().Trim(),
                                            p.ProvinceName
                                        }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = _context.TouristAttraction.Where(m => (m.IsDelete == null || m.IsDelete == true)&&m.TouristAttrName.Contains(TouristAttrName)).Count();
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
                    data = tourAttrac,
                    pagination = pagination

                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///get touristAttractionDetails
        ///
        [HttpGet("MB_Cli_GetTouristAttrDetails")]
        public async Task<IActionResult>Cli_GetTouristAttrDetails(string touristAttrId )
        {
            //
            Guid pID = new Guid();
            bool check = Guid.TryParse(touristAttrId, out pID);

            try
            {
                var rs = await (from t in _context.TouristAttraction
                                join p in _context.Province on t.ProvinceId equals p.ProvinceId
                                where (t.IsDelete==null || t.IsDelete == true) && (check==false? t.TouristAttrName.Trim().Contains(touristAttrId): t.TouristAttrId == pID)
                                select new
                                {
                                    t.TouristAttrId,
                                    t.TouristAttrName,
                                    Description = t.Description != "null" ? t.Description : "Chưa cập nhật",//
                                    ImagesList =  t.ImagesList.Trim(),
                                    p.ProvinceName
                                }).FirstOrDefaultAsync();
                return Ok(rs);
            }
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
