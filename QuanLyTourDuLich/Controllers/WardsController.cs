using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WardsController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;

        public WardsController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        [HttpGet("Adm_GetDataWardsByDistrict")]
        public async Task<ActionResult<IEnumerable<Wards>>> Adm_GetDataWardsByDistrict(int? districtID)
        {
            try
            {
                //var rs = await _context.District.Where(m => m.ProvinceId == provinceID).ToListAsync();
                var rs = await (from w in _context.Wards
                                join d in _context.District on w.DistrictId equals d.DistrictId
                                where w.DistrictId == districtID
                                select new
                                {
                                    w.WardId,
                                    w.WardName,
                                    w.DivisionType,
                                    d.DistrictName
                                }).ToListAsync();
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("Adm_GetDataWardsByDistrictCbb")]
        public async Task<ActionResult<IEnumerable<Wards>>> Adm_GetDataWardsByDistrictCbb(int? districtID)
        {
            try
            {
                //var rs = await _context.District.Where(m => m.ProvinceId == provinceID).ToListAsync();
                var rs = await (from w in _context.Wards
                                where w.DistrictId == districtID
                                select new
                                {
                                    value=w.WardId,
                                    label=w.WardName
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
