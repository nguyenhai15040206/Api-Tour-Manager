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
    public class TravelConpanyTransportController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public TravelConpanyTransportController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - lấy các hảng xe, máy bay về du lịch
        
        [HttpPost("Adm_GetCompanyList")]
        public async Task<IActionResult> Adm_GetCompanyList([FromBody] TransportSearchModel searchModel)
        {
            try
            {
                string[] separator = { "||" };
                bool checkModelSearchIsNull = true;
                bool isTravelType = Guid.TryParse(searchModel.TransportTypeID.ToString(), out Guid rs);
                bool isCompanyName = (!string.IsNullOrEmpty(searchModel.CompanyName));

                if(isCompanyName || isTravelType)
                {
                    checkModelSearchIsNull = false;
                }
                // truy van thong tin lay nhung j can thiet
                var newsList = await (from cat in _context.CatEnumeration
                                      join tct in _context.TravelCompanyTransport on cat.EnumerationId equals tct.EnumerationId
                                      join emp in _context.Employee on tct.EmpIdupdate equals emp.EmpId
                                      where checkModelSearchIsNull == true ?
                                      ((tct.IsDelete == null || tct.IsDelete == true) &&
                                                (cat.IsDelete == null || cat.IsDelete == true)) :
                                        (
                                            (isTravelType == true ? (cat.EnumerationId == searchModel.TransportTypeID) :
                                            tct.CompanyName == searchModel.CompanyName)
                                            && ((tct.IsDelete == null || tct.IsDelete == true) &&
                                                (cat.IsDelete == null || cat.IsDelete == true))
                                        )
                                      orderby tct.DateUpdate descending
                                      select new
                                      {
                                          tct.CompanyId,
                                          tct.CompanyName,
                                          enumerationID = cat.EnumerationTranslate,
                                          tct.CompanyImage,
                                          tct.PhoneNumber,
                                          address = tct.Address==null? "Chưa cập nhật": tct.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0].ToString(),
                                          empIDUpdate = emp.EmpName,
                                          dateUpdate = DateTime.Parse(tct.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),

                                      }).ToListAsync();
               
                return Ok(newsList);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }
       
        [HttpGet("Adm_GetCompanyByTravelTypeCbo")]
        public async Task<IActionResult> Adm_GetCompanyByTravelTypeCbo(string enumType=null)
        {
            try
            {
                var condition = new Guid();
                bool isGuid = Guid.TryParse(enumType, out condition);
                var rs = await (from c in _context.TravelCompanyTransport
                          join e in _context.CatEnumeration on c.EnumerationId equals e.EnumerationId
                          where (c.IsDelete == null || c.IsDelete == true) 
                          && (e.IsDelete == null || e.IsDelete == true)
                          && e.EnumerationId == condition
                                select new
                          {
                              value= c.CompanyId,
                              label= c.CompanyName
                          }
                          ).ToListAsync();
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }

        }

        [HttpGet("Adm_GetCompanyDetails")]
        public async Task<IActionResult> Adm_GetCompanyDetails(Guid? companyID)
        {
            try
            {
                string[] separator = { "||" };
                var rs = await (from cat in _context.CatEnumeration
                                join tct in _context.TravelCompanyTransport on cat.EnumerationId equals tct.EnumerationId
                                join emp in _context.Employee on tct.EmpIdupdate equals emp.EmpId
                                where (tct.IsDelete == null || tct.IsDelete == true) &&
                                           (cat.IsDelete == null || cat.IsDelete == true) &&
                                           tct.CompanyId == companyID
                                select new
                                {
                                    tct.CompanyId,
                                    tct.CompanyName,
                                    enumerationID = cat.EnumerationId,
                                    tct.CompanyImage,
                                    tct.PhoneNumber,
                                    //address = tct.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0].ToString(),
                                    //districtID = int.Parse(tct.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[2].ToString()),
                                    //wardsID = int.Parse(tct.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[1].ToString()),
                                    //provinceID = int.Parse(tct.Address.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[3].ToString()),
                                    empIDUpdate = emp.EmpName,
                                    dateUpdate = DateTime.Parse(tct.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),

                                }).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
            
        }
        //[Nguyeenx Tan Hai]

        [HttpPost("Adm_InsertCompany")]
        [Authorize]
        public async Task<ActionResult> Adm_InsertCompany([FromBody] TravelCompanyTransport insert)
        {
            try
            {
                if (insert == null)
                {
                    return BadRequest();
                }
                insert.DateInsert = DateTime.Now.Date;
                insert.DateUpdate = DateTime.Now.Date;
                insert.EmpIdinsert = insert.EmpIdinsert;
                insert.EmpIdupdate = insert.EmpIdupdate;
                insert.IsDelete = null;
                await _context.TravelCompanyTransport.AddAsync(insert);
                await _context.SaveChangesAsync();
                return Ok(insert);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        [HttpPut("Adm_UpdateCompany")]
        [Authorize]
        public async Task<ActionResult> Adm_UpdateCompany([FromBody] TravelCompanyTransport update)
        {
            try
            {
                
                if (update == null)
                {
                    return BadRequest();
                }
                var rs = await (from cat in _context.CatEnumeration
                                join tct in _context.TravelCompanyTransport on cat.EnumerationId equals tct.EnumerationId
                                join emp in _context.Employee on tct.EmpIdupdate equals emp.EmpId
                                where (tct.IsDelete == null || tct.IsDelete == true) &&
                                           (cat.IsDelete == null || cat.IsDelete == true) &&
                                           tct.CompanyId == update.CompanyId
                                select tct).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

                rs.CompanyName = update.CompanyName;
                if (update.CompanyImage != string.Empty)
                {
                    rs.CompanyImage = update.CompanyImage;
                }
                rs.PhoneNumber = update.PhoneNumber;
                rs.Address = update.Address;
                rs.ProvinceId = update.ProvinceId;
                rs.EnumerationId = update.EnumerationId;
                rs.DateUpdate = DateTime.Now.Date;
                rs.EmpIdupdate = update.EmpIdupdate;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        // Delete Multi row
        [HttpPut("Adm_DeleteCompanyByIds")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TravelCompanyTransport>>> Adm_DeleteCompanyByIds([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.TravelCompanyTransport.Where(m => deleteModels.SelectByIds.Contains(m.CompanyId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = deleteModels.EmpId;
                });
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, "Xóa thành công!");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
