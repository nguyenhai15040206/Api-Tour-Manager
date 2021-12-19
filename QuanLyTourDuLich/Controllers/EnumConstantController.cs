using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.SearchModels;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumConstantController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public EnumConstantController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - Lấy ra bộ enum dượi vào enum type
        
        [HttpGet("Adm_GetEnumConstant")]
        public async Task<IActionResult> Adm_GetEnumConstantCbo(string enumTypeName)
        {
            try
            {
                // truy van thong tin lay nhung j can thiet
                var enumlist = await (from ce in _context.CatEnumeration
                                      where ce.EnumerationType==enumTypeName.Trim()
                                      select new
                                      {
                                          value= ce.EnumerationId,
                                          label= ce.EnumerationTranslate,
                                      }).ToListAsync();
               
                return Ok(enumlist);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        

        // [tanhai]
        [HttpGet("Adm_GetEnumInfo")]
        public async Task<IActionResult> Adm_GetEnumInfo(string enumTypeName)
        {

            try
            {
                // truy van thong tin lay nhung j can thiet
                var enumlist = await (from ce in _context.CatEnumeration
                                      where ce.EnumerationType == enumTypeName.Trim()
                                      select new
                                      {
                                          ce.EnumerationType,
                                          ce.EnumerationId,
                                          ce.EnumerationName,
                                          ce.EnumerationTranslate,
                                      }).ToListAsync();

                return Ok(enumlist);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    
    }
}
