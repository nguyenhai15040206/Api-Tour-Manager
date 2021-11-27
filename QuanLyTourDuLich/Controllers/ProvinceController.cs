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

        [HttpPost("Adm_GetProvinceAndSearch")]
        public async Task<IActionResult> Adm_GetProvinceAndSearch([FromBody] ProvinceSearchModel provinceSearch)
        {
            try
            {
                bool checkModelSearchIsNull = true;

                bool isProvinceID = false;
                if (provinceSearch.provinceID.Length>0)
                {
                    isProvinceID = true;
                }
                //bool isProvinceID = int.TryParse(provinceSearch.provinceID.ToString(), out int provinceID);
                bool isProvinceName = (!string.IsNullOrEmpty(provinceSearch.provinceName));
                bool isDivisionType = (!string.IsNullOrEmpty(provinceSearch.divisionType));

                if (isDivisionType||isProvinceID||isProvinceName)
                {
                    checkModelSearchIsNull = false;
                }
                var rs = await (from p in _context.Province
                                where checkModelSearchIsNull == true ? true
                                : isProvinceID == true ? (provinceSearch.provinceID.Contains(p.ProvinceId))
                                : (
                                    (isProvinceName && p.ProvinceName.Contains(provinceSearch.provinceName))
                                    || (isDivisionType && p.DivisionType.Contains(provinceSearch.divisionType))
                                )
                                //group p by p.ProvinceId into g
                                select new
                                {
                                    value = p.ProvinceId,
                                    label = p.ProvinceName,
                                    p.DivisionType,
                                    p.PhoneCode,
                                    count=_context.TouristAttraction.Where(m=>m.ProvinceId==p.ProvinceId).Count()
                                }).ToListAsync();
                if (rs == null)
                {
                    return BadRequest();
                }
                return Ok(rs);
            }
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

    }
}
