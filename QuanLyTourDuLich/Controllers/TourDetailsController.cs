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
        
        [HttpPut("Adm_UpdateTourDetails")]
        public async Task<ActionResult> Adm_UpdateTourDetails([FromBody] UpdateTourDetailsModels tourDetails)
        {
            try
            {
                if (tourDetails == null) return BadRequest();
                var listObj = await _context.TourDetails.Where(m => m.TourId == tourDetails.TourID && 
                                                    (m.IsDelete==null || m.IsDelete == true)).Select(m => m.TouristAttrId).ToArrayAsync();
                if(listObj.Length == 0) return NotFound();           
                // kiểm tra trong tour details có sự thay đổi gì không => nếu tour được xóa or cập nhật một tour khác thì tiến hành thực thi
                if (listObj.SequenceEqual(tourDetails.TourAttrIds))
                {
                    return Ok();
                }
                else
                {
                    // lấy ra các giá trị giống nhau 
                    var listObjDiff = tourDetails.TourAttrIds.Intersect(listObj).ToArray();
                    // Nếu không có tg nào giống => Xóa các tour Details này => thêm mới danh sách tourDetails
                    if (listObjDiff.Length == 0)
                    {
                        // set isDelete == false 
                        foreach(var item in listObj)
                        {
                            var tourIsDelete = await _context.TourDetails.Where(m => m.TourId == tourDetails.TourID && m.TouristAttrId== item &&
                                (m.IsDelete == null || m.IsDelete == true)).FirstOrDefaultAsync();
                            tourIsDelete.IsDelete = false;
                            tourIsDelete.DateUpdate = DateTime.Now.Date;
                            tourIsDelete.EmpIdupdate = tourDetails.EmpId;
                        }
                        // Insert tourDetails moi
                        foreach(var itemDiff in tourDetails.TourAttrIds)
                        {
                            var CheckTourDetailsAlready = await _context.TourDetails.Where(m => m.TourId == tourDetails.TourID 
                                && m.IsDelete == false && m.TouristAttrId==itemDiff).FirstOrDefaultAsync();
                            if(CheckTourDetailsAlready == null)
                            {
                                TourDetails tourInsert = new TourDetails();
                                tourInsert.TourId = tourDetails.TourID;
                                tourInsert.TouristAttrId = itemDiff;
                                tourInsert.HotelId = null;
                                tourInsert.EmpIdinsert = tourDetails.EmpId;
                                tourInsert.DateInsert = DateTime.Now.Date;
                                tourInsert.EmpIdupdate = tourDetails.EmpId;
                                tourInsert.DateUpdate = DateTime.Now.Date;
                                tourInsert.IsDelete = null;
                                await _context.TourDetails.AddAsync(tourInsert);
                            }
                            else
                            {
                                CheckTourDetailsAlready.EmpIdupdate = tourDetails.EmpId;
                                CheckTourDetailsAlready.DateUpdate = DateTime.Now.Date;
                                CheckTourDetailsAlready.IsDelete = null;
                            }
                        }
                        await _context.SaveChangesAsync();
                        return Ok();
                    }
                    else
                    {
                        // đã có trường hợp trùng nhau
                        // trường hợp thứ nhất => listObj
                        
                        // lấy ra các tour Details không dùng nữa => cập nhật cột IsDelete = false
                        var listObjArr1 = listObj.Except(tourDetails.TourAttrIds).ToArray();
                        foreach(var item1 in listObjArr1)
                        {
                            var result = await _context.TourDetails.Where(m => m.TourId == tourDetails.TourID && m.TouristAttrId == item1 &&
                                (m.IsDelete == null || m.IsDelete == true)).FirstOrDefaultAsync();
                            result.IsDelete = false;
                            result.DateUpdate = DateTime.Now.Date;
                            result.EmpIdupdate = tourDetails.EmpId;
                        }


                        // lấy ra các giá trị muốn thêm mới vào
                        var listObjArr2 = tourDetails.TourAttrIds.Except(listObj).ToArray();
                        foreach(var item2 in listObjArr2)
                        {
                            TourDetails tourInsert = new TourDetails();
                            tourInsert.TourId = tourDetails.TourID;
                            tourInsert.TouristAttrId = item2;
                            tourInsert.HotelId = null;
                            tourInsert.EmpIdinsert = tourDetails.EmpId;
                            tourInsert.DateInsert = DateTime.Now.Date;
                            tourInsert.EmpIdupdate = tourDetails.EmpId;
                            tourInsert.DateUpdate = DateTime.Now.Date;
                            tourInsert.IsDelete = null;
                            await _context.TourDetails.AddAsync(tourInsert);
                        }
                        await _context.SaveChangesAsync();
                        return Ok();
                    }
                }
                
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
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
