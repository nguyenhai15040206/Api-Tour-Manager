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
    public class UnitPriceTransportController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public UnitPriceTransportController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        // [Taans hai]: 20211105 - Xứ lý các dữ liệu về Khuyến mãi
        
        [HttpPost("Adm_GetDataUnitPrice")]
        public async Task<IActionResult> Adm_GetDataUnitPrice([FromBody] UnitPriceSearch pSearch)
        {

            try
            {
                bool checkModelSearchIsNull = true;
                var CompanyID = new Guid();
                int ProvinceFrom=0;
                int ProvinceTo=0;
                bool isTravelTypeID = Guid.TryParse(pSearch.TravelTypeID.ToString(), out Guid TravelTypeID);
                bool isCompanyID = Guid.TryParse(pSearch.CompanyID.ToString(), out  CompanyID);
                bool isProvinceFrom = int.TryParse(pSearch.ProvinceFrom.ToString(), out ProvinceFrom);
                bool isProvinceTo = int.TryParse(pSearch.ProvinceTo.ToString(), out ProvinceTo);
                if(isProvinceFrom || isProvinceTo)
                {
                    checkModelSearchIsNull = false;
                }
                var rs = await (from u in _context.UnitPriceTransport
                                join c in _context.TravelCompanyTransport on u.CompanyId equals c.CompanyId
                                join pf in _context.Province on u.ProvinceFrom equals pf.ProvinceId
                                join pt in _context.Province on u.ProvinceTo equals pt.ProvinceId
                                join emp in _context.Employee on u.EmpIdupdate equals emp.EmpId
                                where checkModelSearchIsNull ==true?
                                ((u.IsDelete == null || u.IsDelete == true)
                                && (c.IsDelete == null || c.IsDelete == true)
                                && c.CompanyId == CompanyID)
                                :
                                ((u.IsDelete == null || u.IsDelete == true)
                                && (c.IsDelete == null || c.IsDelete == true)
                                && c.CompanyId == CompanyID
                                && (isProvinceFrom && u.ProvinceFrom== ProvinceFrom)
                                && (isProvinceTo && u.ProvinceTo == ProvinceTo))

                                orderby u.DateUpdate descending
                                select new
                                {
                                    u.UpTransportId,
                                    timeStart = TimeSpan.Parse(u.TimeStart.ToString()).ToString(@"hh\:mm"),
                                    timeEnd = TimeSpan.Parse(u.TimeEnd.ToString()).ToString(@"hh\:mm"),
                                    provinceFrom = pf.ProvinceName,
                                    provinceTo = pt.ProvinceName,
                                    c.CompanyName,
                                    adultUnitPrice = string.Format("{0:0,0đ}", u.AdultUnitPrice),
                                    childrenUnitPrice= string.Format("{0:0,0đ}", u.ChildrenUnitPrice),
                                    babyUnitPrice = string.Format("{0:0,0đ}", u.BabyUnitPrice) ,
                                    emp.EmpName,
                                    dateUpdate = DateTime.Parse(u.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) 
                                }).ToListAsync();
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        
        [HttpGet("Adm_UnitPriceTransportDetails")]
        public async Task<IActionResult> Adm_GetDataDetails(Guid? pID )
        {
            try
            {
                var rs = await (from u in _context.UnitPriceTransport
                                join c in _context.TravelCompanyTransport on u.CompanyId equals c.CompanyId
                                where (u.IsDelete == null || u.IsDelete == true)
                                && (c.IsDelete == null || c.IsDelete == true)
                            && u.UpTransportId == pID
                                select new
                                {
                                    u.UpTransportId,
                                    timeStart = TimeSpan.Parse(u.TimeStart.ToString()).ToString(@"hh\:mm"),
                                    timeEnd = TimeSpan.Parse(u.TimeEnd.ToString()).ToString(@"hh\:mm"),
                                    u.ProvinceFrom,
                                    u.ProvinceTo,
                                    c.CompanyId,
                                    c.EnumerationId,
                                    u.AdultUnitPrice,
                                    u.ChildrenUnitPrice,
                                    u.BabyUnitPrice,
                                }).FirstOrDefaultAsync(); // Hạn chế dùng Single vì => xãy ra exception
                if(rs== null)
                {
                    return NotFound(); // 404
                }
                return Ok(rs); // 200

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        [HttpPost("Adm_CreateUnitPriceTransport")]
        [Authorize]
        public async Task<ActionResult> Adm_CreateUnitPriceTransport([FromBody] UnitPriceTransport unitPrice)
        {
            try
            {
                if (unitPrice == null)
                {
                    return BadRequest();
                }
                var checkExist = await (from u in _context.UnitPriceTransport
                                          join c in _context.TravelCompanyTransport on u.CompanyId equals c.CompanyId
                                          where (u.IsDelete == null || u.IsDelete == true)
                                            && (c.IsDelete == null || c.IsDelete == true)
                                            && u.CompanyId ==unitPrice.CompanyId
                                            && u.TimeStart == unitPrice.TimeStart
                                            && u.TimeEnd == unitPrice.TimeEnd
                                            && u.ProvinceFrom == unitPrice.ProvinceFrom
                                            && u.ProvinceTo== unitPrice.ProvinceTo
                                            select u
                                          ).FirstOrDefaultAsync();
                if(checkExist != null)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Đã tồn tại thông tin vận chuyển này!");
                }
                unitPrice.DateInsert = DateTime.Now.Date;
                unitPrice.DateUpdate = DateTime.Now.Date;
                unitPrice.EmpIdinsert = unitPrice.EmpIdinsert;
                unitPrice.EmpIdupdate = unitPrice.EmpIdupdate;
                unitPrice.IsDelete = null;

                await _context.UnitPriceTransport.AddAsync(unitPrice);
                await _context.SaveChangesAsync();
                return Ok(unitPrice);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }

        //[Thái Trần Kiều Diễm 20211109]
        // Update một tin tức

        [HttpPut("Adm_UpdateUnitPriceTransport")]
        [Authorize]
        public async Task<IActionResult> Adm_UpdateUnitPriceTransport([FromBody] UnitPriceTransport unitPrice)
        {
            if (unitPrice.UpTransportId ==Guid.Empty)
            {
                return BadRequest();
            }
            try
            {
                var update = await (from n in _context.UnitPriceTransport
                                    join c in _context.TravelCompanyTransport on n.CompanyId equals c.CompanyId
                                    where (n.IsDelete == null || n.IsDelete == true) 
                                            && (c.IsDelete==null || n.IsDelete==true)
                                            && n.UpTransportId == unitPrice.UpTransportId
                                    select n).FirstOrDefaultAsync();
                if (update == null)
                    return NotFound(update);
                if (update.TimeStart != unitPrice.TimeStart || update.TimeEnd !=unitPrice.TimeEnd 
                    || update.ProvinceFrom != unitPrice.ProvinceFrom || update.ProvinceTo != unitPrice.ProvinceTo )
                {
                    var checkExist = await (from u in _context.UnitPriceTransport
                                            join c in _context.TravelCompanyTransport on u.CompanyId equals c.CompanyId
                                            where (u.IsDelete == null || u.IsDelete == true)
                                              && (c.IsDelete == null || c.IsDelete == true)
                                              && u.CompanyId == unitPrice.CompanyId
                                              && u.TimeStart == unitPrice.TimeStart
                                              && u.TimeEnd == unitPrice.TimeEnd
                                              && u.ProvinceFrom == unitPrice.ProvinceFrom
                                              && u.ProvinceTo == unitPrice.ProvinceTo
                                            select u
                                          ).FirstOrDefaultAsync();
                    if (checkExist != null)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Đã tồn tại thông tin vận chuyển này!");
                    }
                }
                update.TimeStart = unitPrice.TimeStart;
                update.TimeEnd = unitPrice.TimeEnd;
                update.ProvinceFrom = unitPrice.ProvinceFrom;
                update.ProvinceTo = unitPrice.ProvinceTo;
                update.CompanyId = unitPrice.CompanyId;
                update.AdultUnitPrice = unitPrice.AdultUnitPrice;
                update.BabyUnitPrice = unitPrice.BabyUnitPrice;
                update.ChildrenUnitPrice = unitPrice.ChildrenUnitPrice;
                update.EmpIdupdate = unitPrice.EmpIdupdate;
                update.DateUpdate = DateTime.Now.Date;
                
                await _context.SaveChangesAsync();
                return Ok(update);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new News record");
            }
        }


        // Delete Multi row
        [HttpPut("Adm_DeleteUnitPriceTransportByIds")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Promotion>>> Adm_DeleteUnitPriceTransportByIds([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.UnitPriceTransport.Where(m => deleteModels.SelectByIds.Contains(m.UpTransportId)).ToListAsync();
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
