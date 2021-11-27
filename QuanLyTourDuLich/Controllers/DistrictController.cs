using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;
using QuanLyTourDuLich.SearchModels;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;

        public DistrictController (HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // lấy tất tả danh danh sách quận huyện // ham nay  bo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<District>>> GET()
        {
            var rs = await _context.District.ToListAsync();
            if(rs ==null || rs.Count == 0)
            {
                return BadRequest();
            }
            return Ok(rs);
        }

        [HttpPost("Adm_GetDistrictByProvinceID")]
        public async Task<ActionResult<IEnumerable<District>>> Adm_GetDistrictList([FromBody] int []provinceID)
            // xu ly lai 
        {
            try
            {
                //var rs = await _context.District.Where(m => m.ProvinceId == provinceID).ToListAsync();
                var rs = await (from d in _context.District
                                join p in _context.Province on d.ProvinceId equals p.ProvinceId
                                where provinceID.Contains(p.ProvinceId)
                                select new
                                {
                                    d.DistrictId,
                                    d.DistrictName,
                                    d.DivisionType,
                                    p.ProvinceName,
                                    count=_context.Wards.Where(m=>m.DistrictId==d.DistrictId).Count()
                                }).ToListAsync();
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
            
        }

        [HttpPost("Adm_GetDistrictByProvinceID")]
        public async Task<ActionResult<IEnumerable<District>>> Adm_GetDistrictList([FromBody] int[] provinceID)
        // xu ly lai 
        {
            try
            {
                //var rs = await _context.District.Where(m => m.ProvinceId == provinceID).ToListAsync();
                var rs = await (from d in _context.District
                                join p in _context.Province on d.ProvinceId equals p.ProvinceId
                                where provinceID.Contains(p.ProvinceId)
                                select new
                                {
                                    d.DistrictId,
                                    d.DistrictName,
                                    d.DivisionType,
                                    p.ProvinceName,
                                    count = _context.Wards.Where(m => m.DistrictId == d.DistrictId).Count()
                                }).ToListAsync();
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }

        }
    }
}
