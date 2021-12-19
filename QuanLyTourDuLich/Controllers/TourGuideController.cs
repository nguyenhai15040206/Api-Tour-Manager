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

                bool isTourGuideName = (!string.IsNullOrEmpty(guiSearch.TourGuideName));
                bool isPhoneNumber = (!string.IsNullOrEmpty(guiSearch.PhoneNumber));
                bool isEmail = (!string.IsNullOrEmpty(guiSearch.Email));

                if (isTourGuideName || isPhoneNumber || isEmail)
                {
                    checkModelSearchIsNull = false;
                }

                var list = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdinsert equals e.EmpId
                                  where  checkModelSearchIsNull == true ? (guide.IsDelete == null || guide.IsDelete == true) :
                                  (
                                      (guide.IsDelete == null || guide.IsDelete == true)
                                      &&((isTourGuideName && guide.TourGuideName.Contains(guiSearch.TourGuideName))
                                      || (isPhoneNumber && guide.PhoneNumber.Contains(guiSearch.PhoneNumber))
                                      || (isEmail && guide.Email.Contains(guiSearch.Email)))
                                  )
                                  orderby guide.DateUpdate descending
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      gender = guide.Gender == true ? "Nam" : "Nữ",
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

        // get thông tin tour guide check điều kiện thỏa nếu đã có trong tour khác
        [HttpGet("Adm_GetDataTourGuidCondition")]
        public async Task<IActionResult> Adm_GetTourGuidCbo(DateTime? pDateStart=null , DateTime? pDateEnd = null)
        {
            try
            {
                var listObj = await (from tg in _context.TourGuide
                               where (tg.IsDelete==null || tg.IsDelete==true)
                               select tg).ToListAsync();
                var listExist = await (from tg in _context.TourGuide
                                       join t in _context.Tour on tg.TourGuideId equals t.TourGuideId
                                       where (tg.IsDelete == null || tg.IsDelete == true) && (tg.IsDelete == null || tg.IsDelete == true)
                                        && ((t.DateStart <= pDateStart && t.DateEnd >= pDateStart) || (t.DateStart <= pDateEnd && t.DateEnd >= pDateEnd && t.DateStart > pDateStart) ||
                                  t.DateStart >= pDateStart && t.DateEnd <= pDateEnd)
                                       select tg).Distinct().ToListAsync();
                if(listExist.Count != 0)
                {
                    listObj = listObj.Except(listExist).ToList();
                }
                var rs = (from a in listObj
                          select new
                          {
                              value = a.TourGuideId,
                              label = a.TourGuideName + " - " + a.PhoneNumber
                          }).ToList();
                return Ok(rs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //get thông tin theo mã
        [HttpGet("Adm_GetTourGuideByID/{TourGuideId:int}")]
        public async Task<IActionResult> Adm_GetTourGuideById (Guid? TourGuideId)
        {
            try {

                var list = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdupdate equals e.EmpId
                                  where (guide.IsDelete == null || guide.IsDelete == true)
                                  && guide.TourGuideId== TourGuideId
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      gender=guide.Gender==true?"Nam":"Nữ",
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

                gui.DateInsert = DateTime.Now;
                gui.DateUpdate = DateTime.Now;
                gui.Status = gui.Status;
                gui.IsDelete = null;

                await _context.TourGuide.AddAsync(gui);
                await _context.SaveChangesAsync();
                return Ok(gui);
            }
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //update thông tin của một hướng dẫn viên bằng mã

        [HttpPut("Adm_UpdateTourGuide")]
        public async Task<IActionResult> Adm_UpdateTourGuide(Guid? TourGuideId, [FromBody] TourGuide gui)
        {
            if (TourGuideId != gui.TourGuideId)
            {
                return BadRequest();
            }
            try
            {
                var guiUpdate = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == true)
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
                guiUpdate.EmpIdupdate = TourGuideId;
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
        [HttpPut("Adm_DeleteTourGuide")]
        public async Task<IActionResult> Adm_DeleteTourGuide(Guid? TourGuideId, [FromBody]Guid? []Ids)
        {

            try
            {
                var guiDelete = await _context.TourGuide.Where(m => Ids.Contains(m.TourGuideId)).ToListAsync();
                guiDelete.ForEach(m =>
                {
                    m.EmpIdupdate = TourGuideId;
                    m.DateUpdate = DateTime.Now;
                    m.IsDelete = false;
                });
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
