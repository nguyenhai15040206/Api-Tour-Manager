using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;

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

        // lấy tất tả danh danh sách quận huyện
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

        [HttpGet("{maTinhThanh}")]
        public async Task<ActionResult<IEnumerable<District>>> GET(int provinceID)
        {
            var rs = await _context.District.Where(m => m.ProvinceId == provinceID).ToListAsync();
            return Ok(rs);
        }
    }
}
