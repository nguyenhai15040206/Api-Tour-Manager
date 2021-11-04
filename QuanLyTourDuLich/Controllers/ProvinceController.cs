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
    public class ProvinceController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;

        public ProvinceController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Province>>> GET()
        {
            var rs = await (from p in _context.Province
                            select new Province
                            {
                                ProvinceId = p.ProvinceId,
                                ProvinceName = p.ProvinceName,
                                DivisionType = p.DivisionType,
                                PhoneCode = p.PhoneCode
                            }).ToListAsync(); 
            if(rs== null || rs.Count == 0)
            {
                return BadRequest();
            }
            else
            {
                return Ok(rs);
            }
        }
    }
}
