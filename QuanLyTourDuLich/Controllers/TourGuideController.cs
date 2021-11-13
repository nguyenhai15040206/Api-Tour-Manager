using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using QuanLyTourDuLich.SearchModels;
using Newtonsoft.Json;
using System.Globalization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourGuideController : ControllerBase
    {
        //Thái Trần Kiều Diễm [06/11/2021]
        //
        DateTime DateUpdate;
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public TourGuideController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //get danh sách tour guide
        [HttpPost("Adm_GetDataTourGuide")]
        public async Task<IActionResult> Adm_GetDataTourGuide([FromBody] TourGuideSearchModel guiSearch)
        {
            
            try 
            {
                bool checkModelSearchIsNull = true;

                bool isTourGuideId = int.TryParse(guiSearch.touGuideId.ToString(), out int tourGuideId);
                bool isTourGuideName = (!string.IsNullOrEmpty(guiSearch.tourGuideName));
                bool isGender = bool.TryParse(guiSearch.gender.ToString(), out bool gender);
                bool isPhoneNumber = (!string.IsNullOrEmpty(guiSearch.phoneNumber));
                bool isEmail = (!string.IsNullOrEmpty(guiSearch.email));

                if (isTourGuideId || isTourGuideName || isGender || isPhoneNumber || isEmail)
                {
                    checkModelSearchIsNull = false;
                }

                var list = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdupdate equals e.EmpId
                                  where (guide.IsDelete == null || guide.IsDelete == false)
                                  
                                  && checkModelSearchIsNull == true ? true :
                                  (
                                      (isTourGuideId && guide.TourGuideId == guiSearch.touGuideId)
                                      || (isTourGuideName && guide.TourGuideName.Contains(guiSearch.tourGuideName))
                                      || (isGender && guide.Gender == guiSearch.gender)
                                      || (isPhoneNumber && guide.PhoneNumber.Contains(guiSearch.phoneNumber))
                                      || (isEmail && guide.Email.Contains(guiSearch.email))
                                  )
                                  orderby guide.DateUpdate descending
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      guide.Gender,
                                      guide.DateOfBirth,
                                      guide.PhoneNumber,
                                      guide.Email,
                                      guide.Address,
                                      guide.Avatar,
                                      e.EmpName,
                                      DateUpdate=DateTime.Parse(guide.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                  }).ToListAsync();

                if (list == null)
                {
                    return NotFound();
                }
                // status code 200
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //get thông tin theo mã
        [HttpGet("Adm_GetTourGuideByID/{TourGuideId:int}")]
        public async Task<IActionResult> Adm_GetTourGuideById (int TourGuideId)
        {
            try {

                var list = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdupdate equals e.EmpId
                                  where (guide.IsDelete == null || guide.IsDelete == false)
                                  && guide.TourGuideId== TourGuideId
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      guide.Gender,
                                      guide.DateOfBirth,
                                      guide.PhoneNumber,
                                      guide.Email,
                                      guide.Address,
                                      guide.Avatar,
                                      e.EmpName,
                                      DateUpdate = DateTime.Parse(guide.DateUpdate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                  }).FirstOrDefaultAsync();

                // status code 200
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //thêm một hướng dẫn viên

        [HttpPost("Adm_CreateTourGuide")]
        public async Task<ActionResult> Adm_CreateTourGuide([FromBody] TourGuide gui)
        {
            try {
                if (gui == null)
                    return BadRequest();


                TourGuide newGui = new TourGuide();
                newGui.TourGuideName = gui.TourGuideName;
                newGui.Gender = gui.Gender;
                newGui.DateOfBirth = gui.DateOfBirth;
                newGui.PhoneNumber = gui.PhoneNumber;
                newGui.Email = gui.Email;
                newGui.Address = gui.Address;
                newGui.Avatar = gui.Avatar;
                newGui.EmpIdinsert = gui.EmpIdinsert;
                newGui.EmpIdupdate = gui.EmpIdupdate; 
                newGui.DateInsert = DateTime.Now;
                newGui.DateUpdate = DateTime.Now;
                newGui.Status = gui.Status;
                newGui.IsDelete = null;


                await _context.TourGuide.AddAsync(newGui);
                await _context.SaveChangesAsync();
                return Ok(newGui);
            }
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //update thông tin của một hướng dẫn viên bằng mã

        [HttpPut("Adm_UpdateTourGuide/{TourGuideId:int}")]
        public async Task<IActionResult> Adm_UpdateTourGuide(int TourGuideId, [FromBody] TourGuide gui)
        {
            if (TourGuideId != gui.TourGuideId)
            {
                return BadRequest();
            }
            try
            {
                var guiUpdate = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == false)
                                       && nv.TourGuideId == TourGuideId
                                       select nv).FirstOrDefaultAsync();
                if (guiUpdate == null)
                    return NotFound();


                guiUpdate.TourGuideName = gui.TourGuideName;
                guiUpdate.Gender = gui.Gender;
                guiUpdate.DateOfBirth = gui.DateOfBirth;
                guiUpdate.PhoneNumber = gui.PhoneNumber;
                guiUpdate.Address = gui.Address;
                guiUpdate.Avatar = gui.Avatar;
                guiUpdate.EmpIdupdate = gui.EmpIdupdate;
                guiUpdate.DateUpdate = DateTime.Now;
                guiUpdate.Status = gui.Status;


                await _context.SaveChangesAsync();
                return Ok(guiUpdate);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //xóa một nhân viên
        [HttpPut("Adm_DeleteTourGuide/{TourGuideId:int}/{empID:int}")]
        public async Task<IActionResult> Adm_DeleteTourGuide(int TourGuideId, int? empID = null)
        {

            try
            {
                var guiDelete = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == false)
                                       && nv.TourGuideId == TourGuideId
                                       select nv).FirstOrDefaultAsync();
                if (guiDelete == null)
                    return NotFound();


                guiDelete.EmpIdupdate = empID;
                guiDelete.DateUpdate = DateTime.Now;
                guiDelete.IsDelete = true;


                await _context.SaveChangesAsync();
                return Ok(guiDelete);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
    }
}
