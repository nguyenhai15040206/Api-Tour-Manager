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
                                                && (ce.IsDelete == null || ce.IsDelete==true)
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
                                        && (ce.IsDelete == null || ce.IsDelete == true)
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
    


        //====================

        [HttpGet("Adm_GetDataEnumListByType")]
        public async Task<IActionResult> Adm_GetDataEnumListByType(string enumType, string enumTranslate)
        {
            try
            {
                var rs = await (from c in _context.CatEnumeration
                                where c.EnumerationType.Trim().ToLower().Contains(enumType.ToLower().Trim())
                                    && (c.IsDelete == null || c.IsDelete == true)
                                orderby c.DateUpdate descending, c.DateUpdate descending
                                select new
                                {
                                    c.EnumerationId,
                                    EnumerationType=  c.EnumerationType== "KindOfNews"?"Loại tin tức": 
                                                      c.EnumerationType == "TransportType" ? "Loại vận chuyển":
                                                      c.EnumerationType == "OptionNoteByCustomer"? "Các options ghi chú đặt tour" :
                                                      c.EnumerationType== "TravelType"?"Loại hình du lịch":"",
                                    c.EnumerationName,
                                    c.EnumerationTranslate,
                                    c.EmpIdupdateNavigation.EmpName,
                                    DateUpdate = DateTime.Parse(c.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                }).ToListAsync();
                if(enumTranslate != "" && enumTranslate !=null)
                {
                    rs = rs.Where(m => m.EnumerationTranslate.ToLower().Contains(enumTranslate.ToLower())).ToList();
                }
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // [tanhai]
        [HttpGet("Adm_GetEnumIDetails")]
        public async Task<IActionResult> Adm_GetEnumIDetails(Guid? pID)
        {

            try
            {
                // truy van thong tin lay nhung j can thiet
                var rs = await (from ce in _context.CatEnumeration
                                      where ce.EnumerationId == pID
                                        && (ce.IsDelete == null || ce.IsDelete == true)
                                      select new
                                      {
                                          ce.EnumerationType,
                                          ce.EnumerationId,
                                          ce.EnumerationName,
                                          ce.EnumerationTranslate,
                                      }).FirstOrDefaultAsync();

                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        [HttpPost("Adm_InsertCatEnum")]
        [Authorize]
        public async Task<IActionResult> Adm_InsertCatEnum([FromBody] CatEnumeration pEnum)
        {
            try
            {
                if (pEnum == null)
                {
                    return BadRequest();
                }
                string enumName = pEnum.EnumerationType == "KindOfNews" ? "E_KindOfNew" :
                                                      pEnum.EnumerationType == "TransportType" ? "E_TransportType" :
                                                      pEnum.EnumerationType == "OptionNoteByCustomer" ? "E_OptionNoteByCustomer" :
                                                      pEnum.EnumerationType == "TravelType" ? "E_TravelType" : "";

                enumName = enumName + "_" + Guid.NewGuid();
                var rs = await _context.CatEnumeration
                                .Where(m => m.EnumerationType.ToLower().Equals(pEnum.EnumerationType.ToLower())
                                        && m.EnumerationName.ToLower().Equals(enumName.ToLower()))
                                .Select(m => m.EnumerationName).ToListAsync();
                if(rs.Count() > 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Vui lòng thêm mới!!!");
                }
                pEnum.EnumerationName = enumName;
                pEnum.EnumerationTranslate = pEnum.EnumerationTranslate;
                pEnum.EmpIdinsert = pEnum.EmpIdinsert;
                pEnum.DateInsert = DateTime.Now.Date;
                pEnum.EmpIdupdate = pEnum.EmpIdupdate;
                pEnum.DateUpdate = DateTime.Now.Date;
                pEnum.IsDelete = null;

                await _context.CatEnumeration.AddAsync(pEnum);
                await _context.SaveChangesAsync();
                return Ok(pEnum);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpPut("Adm_UpdateCatEnum")]
        [Authorize]
        public async Task<IActionResult> Adm_UpdateCatEnum([FromBody] CatEnumeration pEnum)
        {
            try
            {
                if (pEnum == null)
                {
                    return BadRequest();
                }
                var rs = await _context.CatEnumeration.Where(m => m.EnumerationId == pEnum.EnumerationId).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                rs.EnumerationTranslate = pEnum.EnumerationTranslate;
                rs.EmpIdupdate = pEnum.EmpIdupdate;
                rs.DateUpdate = DateTime.Now.Date;
                rs.IsDelete = null;

                await _context.SaveChangesAsync();
                return Ok(pEnum);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpPut("Adm_DeleteCatEnum")]
        [Authorize]
        public async Task<IActionResult> Adm_DeleteCatEnum([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.CatEnumeration.Where(m => deleteModels.SelectByIds.Contains(m.EnumerationId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.EmpIdupdate = deleteModels.EmpId;
                    m.DateUpdate = DateTime.Now;
                    m.IsDelete = false;
                });
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
