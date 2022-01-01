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
using Microsoft.AspNetCore.Authorization;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourGuideController : ControllerBase
    {
        //Thái Trần Kiều Diễm [06/11/2021]
        //
        public const string BaseUrlServer = "http://localhost:8000/ImagesEmployee/";
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public IConfiguration _config;
        public TourGuideController(HUFI_09DHTH_TourManagerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //get danh sách tour guide
        [HttpPost("Adm_GetDataTourGuide")]
        public async Task<IActionResult> Adm_GetDataTourGuide([FromBody] TourGuideSearchModel pSearch)
        {
            
            try 
            {

                var listObj = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdinsert equals e.EmpId
                                  where  (guide.IsDelete == null || guide.IsDelete == true)
                                  orderby guide.DateUpdate descending
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      gender = guide.Gender == true ? "Nam" : "Nữ",
                                      dateOfBirth= guide.DateOfBirth ==null? "": DateTime.Parse(guide.DateOfBirth.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                      guide.PhoneNumber,
                                      guide.Email,
                                      guide.Address,
                                      guide.Avatar,
                                      e.EmpName,
                                      DateUpdate=DateTime.Parse(guide.DateUpdate.ToString()).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                  }).ToListAsync();
                if(pSearch.TourGuideName!=null && pSearch.TourGuideName != "")
                {
                    listObj = listObj.Where(m => m.TourGuideName.Contains(pSearch.TourGuideName)).ToList();
                }
                if (pSearch.PhoneNumber !=null && pSearch.PhoneNumber !="")
                {
                    listObj = listObj.Where(m => m.PhoneNumber.Contains(pSearch.PhoneNumber)).ToList();
                }
                if(pSearch.Email !=null && pSearch.Email != "")
                {
                    listObj = listObj.Where(m => m.Email.Contains(pSearch.Email)).ToList();
                }
                // status code 200
                return Ok(listObj);
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
        [HttpGet("Adm_GetTourGuideByID")]
        public async Task<IActionResult> Adm_GetTourGuideById(Guid? TourGuideId)
        {
            try {

                var rs = await (from guide in _context.TourGuide
                                  join e in _context.Employee on guide.EmpIdupdate equals e.EmpId
                                  where (guide.IsDelete == null || guide.IsDelete == true)
                                  && guide.TourGuideId== TourGuideId
                                  select new
                                  {
                                      guide.TourGuideId,
                                      guide.TourGuideName,
                                      gender=guide.Gender,
                                      guide.DateOfBirth,
                                      guide.PhoneNumber,
                                      guide.Email,
                                      guide.Address,
                                      avatar = BaseUrlServer + guide.Avatar,
                                      e.EmpName,
                                      DateUpdate = guide.DateUpdate,
                                  }).FirstOrDefaultAsync();

                // status code 200
                return Ok(rs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        //thêm một hướng dẫn viên

        [HttpPost("Adm_CreateTourGuide")]
        [Authorize]
        public async Task<ActionResult> Adm_CreateTourGuide([FromBody] TourGuide gui)
        {
            try {
                if (gui == null)
                    return BadRequest();
                if(gui.Avatar == "")
                {
                    gui.Avatar = null;
                }
                gui.DateInsert = DateTime.Now;
                gui.DateUpdate = DateTime.Now;
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
        [Authorize]
        public async Task<IActionResult> Adm_UpdateTourGuide([FromBody] TourGuide gui)
        {
            try
            {
                var guiUpdate = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == true)
                                       && nv.TourGuideId == gui.TourGuideId
                                       select nv).FirstOrDefaultAsync();
                if (guiUpdate == null)
                    return NotFound();

                guiUpdate.TourGuideName = gui.TourGuideName;
                guiUpdate.Gender = gui.Gender;
                guiUpdate.DateOfBirth = gui.DateOfBirth;
                guiUpdate.PhoneNumber = gui.PhoneNumber;
                if(gui.Avatar != "")
                {
                    guiUpdate.Avatar = gui.Avatar;
                }
                guiUpdate.Address = gui.Address;
                guiUpdate.EmpIdupdate = gui.EmpIdupdate;
                guiUpdate.DateUpdate = DateTime.Now;
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
        [Authorize]
        public async Task<IActionResult> Adm_DeleteTourGuide([FromBody] DeleteModels deleteModels)
        {

            try
            {
                var guiDelete = await _context.TourGuide.Where(m => deleteModels.SelectByIds.Contains(m.TourGuideId)).ToListAsync();
                guiDelete.ForEach(m =>
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
