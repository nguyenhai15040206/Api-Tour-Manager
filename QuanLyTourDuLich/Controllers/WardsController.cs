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

        [HttpGet("{maDistrict}")]
        public async Task<ActionResult<IEnumerable<Wards>>> GET(int districtID)
        {
            var rs = await _context.Wards.Where(m => m.DistrictId == districtID).ToListAsync();
            return Ok(rs);
        }
    }
}
