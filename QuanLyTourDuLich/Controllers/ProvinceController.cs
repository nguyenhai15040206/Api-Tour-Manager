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
        public async Task<ActionResult> Adm_GetProvince(int regions)
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
    }
}
