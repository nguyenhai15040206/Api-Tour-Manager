using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;
using QuanLyTourDuLich.SearchModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public PermissionController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
        }


        // load danh sách nhóm người dùng
        [HttpGet("Adm_GetDatatUserGroup")]
        public async Task<IActionResult> Adm_GetUserGroup(string UserGroupName=null)
        {
            try
            {
                var rs = await _context.UserGroup.ToListAsync();
                if(UserGroupName != null)
                {
                    rs = rs.Where(m => m.UserGroupName.Contains(UserGroupName)).ToList();
                }
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        // loadnh danh sách quyền chức năng
        [HttpGet("Adm_GetPermistion")]
        public async Task<IActionResult> Adm_GetPermistion(Guid? pID = null)
        {
            try
            {
                var rs = await (from mh in _context.CatScreen
                          join pe in _context.Permission 
                          on new { mh.ScreenId } 
                          equals new { pe.ScreenId } into Permisstion_Temp
                          from a in Permisstion_Temp.DefaultIfEmpty()
                          where a.UserGroupId == pID
                          select new
                          {
                              UserGroupID = a.UserGroupId,
                              ScreenID = mh.ScreenId,
                              mh.ScreenName,
                              Status = (bool?) a.Status
                          }).ToListAsync();
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        [HttpPost("Adm_InsertUserGroup")]
        [Authorize]
        public async Task<IActionResult> Adm_InsertUserGroup([FromBody] UserGroup userGroup)
        {
            try
            {
                var listScreen = await _context.CatScreen.ToListAsync();
                if (userGroup == null)
                {
                    return BadRequest();
                }

                await _context.UserGroup.AddAsync(userGroup);
                await _context.SaveChangesAsync();

                foreach(var item in listScreen)
                {
                    Permission p = new Permission();
                    p.ScreenId = item.ScreenId;
                    p.Status = false;
                    p.UserGroupId = userGroup.UserGroupId;
                    await _context.Permission.AddAsync(p);
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        [HttpPut("Adm_UpdateUserGroup")]
        public async Task<IActionResult> Adm_UpdateUserGroup([FromBody] UserGroup userGroup)
        {
            try
            {
                if (userGroup == null)
                {
                    return BadRequest();
                }
                var rs = await _context.UserGroup.Where(m => m.UserGroupId == userGroup.UserGroupId).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                rs.UserGroupName = userGroup.UserGroupName;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        //

        [HttpGet("Adm_ChangePermissionStatus")]
        [Authorize]
        public async Task<IActionResult> Adm_ChangePermissionStatus(Guid? pScreenID,Guid?pUserGroupID)
        {
            try
            {
                var rs = await _context.Permission.Where(m=> m.ScreenId==pScreenID && m.UserGroupId== pUserGroupID).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                rs.Status = rs.Status == true ? false : true;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }
        

        [HttpGet("Adm_GetAllPermissionByEmpID")]
        public async Task<IActionResult> Adm_GetAllPermissionByEmpID(Guid? pID)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                var rs = await _context.EmpUserGroup.Where(m => m.EmpId == pID).Select(m => m.UserGroupId).ToListAsync();
                List<Permission> listPermission = new List<Permission>();
                foreach(var item in rs)
                {
                    // lấy quyền các nhóm mà nhân viên này có
                    var listObj = await _context.Permission.Where(m => m.UserGroupId == item).ToListAsync();
                    foreach(var obj in listObj)
                    {
                        if (obj.Status == true)
                        {
                            listPermission.Add(obj);
                        }
                    }
                }
                var _listNotDuplicate = listPermission.GroupBy(m => m.ScreenId).Select(m => m.FirstOrDefault());
                var rsPermission = _listNotDuplicate.Select(m => m.ScreenId).ToList();
                return Ok(rsPermission);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        //============
        // xóa nhóm quyền => xóa tất cả các quyền permission của nhóm quyền đó
        [HttpPost("Adm_DeleteUserGroup")]
        public async Task<IActionResult> Adm_DeleteUserGroup([FromBody] DeleteModels deleteModels)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                var rs = await _context.UserGroup.Where(m => deleteModels.SelectByIds.Contains(m.UserGroupId)).ToListAsync();

                foreach(var item in rs)
                {
                    var deletePermission = await _context.Permission.Where(m => m.UserGroupId == item.UserGroupId).ToListAsync();
                    _context.Permission.RemoveRange(deletePermission);
                    _context.UserGroup.Remove(item);
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        // danh sách người dùng chưa có nhóm
        [HttpGet("Adm_GetDataEmpNoGroup")]
        public async Task<IActionResult> Adm_GetDataEmpNoGroup(Guid? pID)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                var rs = await (from emp in _context.Employee
                                where (emp.IsDelete == null || emp.IsDelete == true) &&
                                      !  _context.EmpUserGroup.Any(m => m.EmpId == emp.EmpId && m.UserGroupId== pID)
                                select new
                                {
                                    emp.EmpId,
                                    emp.EmpName,
                                    Gender = emp.Gender == true ? "Nam" : "Nữ",
                                    DateOfBirth = DateTime.Parse(emp.DateOfBirth.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    WorkingDate = DateTime.Parse(emp.WorkingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    emp.PhoneNumber,
                                    emp.Email,
                                }).ToListAsync();

                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        //
        // danh sách người dùng đã có nhóm
        [HttpGet("Adm_GetDataEmpByGroupID")]
        public async Task<IActionResult> Adm_GetDataEmpByGroupID(Guid? pID)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                var rs = await (from emp in _context.Employee
                                join gr in _context.EmpUserGroup on emp.EmpId equals gr.EmpId
                                where (emp.IsDelete == null || emp.IsDelete == true) && gr.UserGroupId == pID
                                select new
                                {
                                    emp.EmpId,
                                    emp.EmpName,
                                    Gender = emp.Gender == true ? "Nam" : "Nữ",
                                    DateOfBirth = DateTime.Parse(emp.DateOfBirth.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    WorkingDate = DateTime.Parse(emp.WorkingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    emp.PhoneNumber,
                                    emp.Email,
                                    GroupName = gr.UserGroup.UserGroupName
                                }).ToListAsync();

                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        //
        // thêm người dùng  vao nhóm
        [HttpPost("Adm_InsertEmpInGroup")]
        [Authorize]
        public async Task<IActionResult> Adm_InsertEmpInGroup([FromBody] EmpUserGroupModels pInsert)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                foreach(var item in pInsert.EmpIds)
                {
                    EmpUserGroup eu = new EmpUserGroup();
                    eu.EmpId = item;
                    eu.UserGroupId = pInsert.UserGroupId;
                    await _context.EmpUserGroup.AddAsync(eu);
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }
        // xoa người dùng  vao nhóm
        [HttpPost("Adm_DeleteEmpInGroup")]
        [Authorize]
        public async Task<IActionResult> Adm_DeleteEmpInGroup([FromBody] EmpUserGroupModels pInsert)
        {
            try
            {
                // lấy tất cả các nhóm quyền
                foreach (var item in pInsert.EmpIds)
                {
                    var rs = await _context.EmpUserGroup.Where(m => m.EmpId == item && m.UserGroupId == pInsert.UserGroupId).FirstOrDefaultAsync();
                    _context.EmpUserGroup.Remove(rs);
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }
    }
}
