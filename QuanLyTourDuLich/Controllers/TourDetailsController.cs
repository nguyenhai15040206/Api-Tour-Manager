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
    public class TourDetailsController : ControllerBase
    {
        // Nguyễn Tấn Hải [18/11/2021] - Rest full api TourDetails
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public TourDetailsController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }
        

        // [Nguyễn Tấn Hải - 20211118]: Thực hiện Post Data
        [HttpPost("Adm_InsertTourDetails")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<TourDetails>>> InsertTourDetails([FromBody] TourDetails tourDetails)
        {
            try
            {
                if(tourDetails == null)
                {
                    // 400
                    return BadRequest();
                }
                tourDetails.DateInsert = DateTime.Now.Date;
                tourDetails.DateUpdate = DateTime.Now.Date;
                await _context.TourDetails.AddAsync(tourDetails);
                await _context.SaveChangesAsync();
                // nếu oke => stattus code 200
                return Ok(tourDetails);
            }
            catch (Exception)
            {
                // 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }



        // Delete  Multi row Tour Details By TourIDs 
        [HttpPut("Adm_DeleteTourDetailsByTourID")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<TourDetails>>> DeleteTourDetailsByTourID([FromBody] DeleteModels deleteModels)
        {
            try
            {
                var listObj = await _context.TourDetails.Where(m => deleteModels.SelectByIds.Contains(m.TourId)).ToListAsync();
                listObj.ForEach(m =>
                {
                    m.IsDelete = false;
                    m.DateUpdate = DateTime.Now.Date;
                    m.EmpIdupdate = deleteModels.EmpId;
                });
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, "Delete tour success!");
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
