using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuanLyTourDuLich.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

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
        [HttpGet("TourGuideList")]
        public async Task<IActionResult> getTourGuide_List(int page=1, int limit = 20)
        {
            try 
            {
                var list=await (from guide in _context.TourGuide
                                where (guide.IsDelete==null||guide.IsDelete==false)
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
                                }).Skip((page - 1) * limit).Take(limit).ToListAsync();
                int totalRecord = list.Count();
                // lay du lieu phan trang, tinh ra duoc tong so trang, page thu may,... Ham nay cu coppy
                var pagination = new Pagination
                {
                    count = totalRecord,
                    currentPage = page,
                    pagsize = limit,
                    totalPage = (int)Math.Ceiling(decimal.Divide(totalRecord, limit)),
                    indexOne = ((page - 1) * limit + 1),
                    indexTwo = (((page - 1) * limit + limit) <= totalRecord ? ((page - 1) * limit * limit) : totalRecord)
                };
                // status code 200
                return Ok(new
                {
                    data = list,
                    pagination = pagination

                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //get thông tin theo mã
        [HttpGet("{id:int}")]
        public async Task<TourGuide> getTourGuideById (int id)
        {
            return await (from gui in _context.TourGuide
                          where (gui.IsDelete == null || gui.IsDelete == false)
                          && gui.TourGuideId == id
                          select gui).FirstOrDefaultAsync();
        }

        //thêm một hướng dẫn viên

        [HttpPost("CreateTourGuide")]
        public async Task<ActionResult> createTourGuide([FromBody] TourGuide gui)
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
                newGui.EmpIdupdate = null; 
                newGui.DateInsert = DateTime.Now;
                newGui.DateUpdate = null;
                newGui.Status = true;
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

        [HttpPut("UpdateTourGuide/{id:int}")]
        public async Task<IActionResult> UpdateTourGuideById(int id,[FromBody] TourGuide gui)
        {
            if (id != gui.TourGuideId)
            {
                return BadRequest();
            }
            try
            {
                var guiUpdate = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == false)
                                       && nv.TourGuideId == id
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
                await _context.SaveChangesAsync();
                return Ok(guiUpdate);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //xóa một nhân viên
        [HttpPut("DeleteTourGuide/{id:int}")]
        public async Task<IActionResult> DeleteTourGuideById(int id, [FromBody] TourGuide gui)
        {
            if (id != gui.TourGuideId)
            {
                return BadRequest();
            }
            try
            {
                var guiDelete = await (from nv in _context.TourGuide
                                       where (nv.IsDelete == null || nv.IsDelete == false)
                                       && nv.TourGuideId == id
                                       select nv).FirstOrDefaultAsync();
                if (guiDelete == null)
                    return NotFound();
                guiDelete.EmpIdupdate = gui.EmpIdupdate;
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
