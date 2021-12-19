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
    public class ProvinceController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;

        public ProvinceController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        [HttpGet("Adm_GetProvince")]
        public async Task<ActionResult> Adm_GetProvince()
        {
            try {
                var rs = await (from p in _context.Province
                                select new
                                {
                                    value = p.ProvinceId,
                                    label = p.ProvinceName,
                                }).ToListAsync();
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("Adm_GetProvinceByRegions")]
        public async Task<ActionResult> Adm_GetProvinceByRegions(int? regions)
        {
            try
            {
                int pRegions = 0;
                bool isCheck = int.TryParse(regions.ToString(), out pRegions);
                var rs = await (from p in _context.Province
                                where p.Regions == pRegions
                                select new
                                {
                                    value = p.ProvinceId,
                                    label = p.ProvinceName,
                                }).ToListAsync();
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        public string getRegions(int? regions)
        {
            if (regions == 1) return "Miền Bắc";
            if (regions == 2) return "Miền Trung";
            if (regions == 3) return "Miền Nam";
            return "";
        }

        [HttpPost("Adm_GetProvinceAndSearch")]
        public async Task<IActionResult> Adm_GetProvinceAndSearch([FromBody] ProvinceSearchModel provinceSearch)
        {
            try
            {
                bool checkModelSearchIsNull = true;
                bool isProvinceID = false;
                if (provinceSearch.ProvinceID.Length > 0)
                {
                    isProvinceID = true;
                }
                //bool isProvinceID = int.TryParse(provinceSearch.provinceID.ToString(), out int provinceID);
                bool isProvinceName = (!string.IsNullOrEmpty(provinceSearch.ProvinceName));
                if (isProvinceID || isProvinceName)
                {
                    checkModelSearchIsNull = false;
                }
                var rs = await (from p in _context.Province
                                where 
                                checkModelSearchIsNull == true ? true
                                : isProvinceID == true ? (provinceSearch.ProvinceID.Contains(p.ProvinceId))
                                : (
                                    (isProvinceName && p.ProvinceName.Contains(provinceSearch.ProvinceName))
                                )
                                select new
                                {
                                    
                                    provinceId = p.ProvinceId,
                                    provinceName = p.ProvinceName,
                                    p.DivisionType,
                                    regions = p.Regions==1? "Miền bắc": (p.Regions==2? "Miền Trung": "Miền Nam"),
                                    count = _context.TouristAttraction.Where(m => m.ProvinceId == p.ProvinceId).Count()
                                }).ToListAsync();
                if (rs == null)
                {
                    return BadRequest();
                }
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
