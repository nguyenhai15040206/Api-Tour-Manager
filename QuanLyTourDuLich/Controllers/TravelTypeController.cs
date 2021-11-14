using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace QuanLyTourDuLich.Controllers
{
    /// <summary>
    /// [Thái Trần Kiều Diễm 20211111]
    /// Xử lý travelType
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    public class TravelTypeController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;

        DateTime DateUpdate;
        public TravelTypeController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }

        ///get tất cả các loại tour
        ///

        [HttpGet("Adm_GetDataTravelType")]
        public async Task<IActionResult> Adm_GetDataTravelType()
        {
            try
            {
                var rs = await (from t in _context.TravelType
                                join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                where (t.IsDelete == null || t.IsDelete == true)
                                select new
                                {
                                    t.TravelTypeId,
                                    t.TravelTypeName,
                                    t.Note,
                                    e.EmpName,
                                    DateUpdate = DateTime.Parse(t.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                }).ToListAsync();
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

        ///get một loại tour
        ///

        [HttpGet("Adm_GetTravelTypeById/{travelTypeId:int}")]
        public async Task<IActionResult> Adm_GetTravelTypeById (int travelTypeId)
        {
            try
            {
                var rs = await (from t in _context.TravelType
                                join e in _context.Employee on t.EmpIdupdate equals e.EmpId
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TravelTypeId == travelTypeId
                                select new
                                {
                                    t.TravelTypeId,
                                    t.TravelTypeName,
                                    t.Note,
                                    e.EmpName,
                                    t.DateUpdate
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



        ///Thêm một loại tour
        ///

       [HttpPost("Adm_CreateTravelType")]
        public async Task<ActionResult> Adm_CreateTravelType ([FromBody] TravelType travel)
        {
            try
            {
                if (travel == null)
                {
                    return NotFound();
                }

                TravelType t = new TravelType();
                t.TravelTypeName = travel.TravelTypeName;
                t.Note = travel.Note;
                t.EmpIdinsert = travel.EmpIdinsert;
                t.DateInsert = DateTime.Now;
                t.EmpIdupdate = travel.EmpIdupdate;
                t.DateUpdate = DateTime.Now;
                t.Status = travel.Status;
                t.IsDelete = null;

                await _context.TravelType.AddAsync(t);
                await _context.SaveChangesAsync();

                return Ok(t);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        ///Update thông tin một loại tour
        ///

        [HttpPut("Adm_UpdateTravelType/{travelTypeId:int}")]
        public async Task<IActionResult> Adm_UpdateTravelType ([FromBody] TravelType travel, int travelTypeId)
        {
            if (travel.TravelTypeId != travelTypeId)
            {
                return BadRequest();
            }
            try
            {
                var rs = await (from t in _context.TravelType
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TravelTypeId == travelTypeId
                                select t).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }

                rs.TravelTypeName = travel.TravelTypeName;
                rs.Note = travel.Note;
                rs.EmpIdupdate = travel.EmpIdupdate;
                rs.Status = travel.Status;
                rs.DateUpdate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }



        /// <summary>
        /// xóa một loại tour
        /// </summary>
        /// <param name="travel"></param>
        /// <param name="travelTypeId"></param>
        /// <returns></returns>


        // cấp nhật tình trạng thôi, nên không cần đưa mết cái modal vào đâu
        [HttpPut("Adm_DeleteTravelType/{travelTypeId:int}/{empID:int}")]

        public async Task<IActionResult> Adm_DeleteTravelType(int travelTypeId, int? empID = null)
        {
            
            try
            {
                var rs = await (from t in _context.TravelType
                                where (t.IsDelete == null || t.IsDelete == true)
                                && t.TravelTypeId == travelTypeId
                                select t).FirstOrDefaultAsync();
                if (rs == null)
                    return NotFound();
                rs.DateUpdate = DateTime.Now.Date;
                rs.EmpIdupdate = empID; // ?
                rs.IsDelete = false;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
