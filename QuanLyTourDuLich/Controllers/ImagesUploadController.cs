using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyTourDuLich.SearchModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ImagesUploadController(IWebHostEnvironment webHostEnvironment)
        {
            this._webHostEnvironment = webHostEnvironment;
        }

        // Nguyễn Tấn Hải - 20211126 Upload Image Tour
        [HttpPost]
        [Route("Adm_UploadImageTour")]
        [Authorize]
        public async Task<IActionResult> UploadImageTour([FromForm] IFormFile file)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesTour";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if(file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image  = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(1000, 667));
                        await image.SaveAsync(fullPath);
                    }
                }
                return Ok(new { FileName =  fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpPost]
        [Route("Adm_UploadImageNews")]
        [Authorize]
        public async Task<IActionResult> UploadImageNews([FromForm] IFormFile file)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesNews";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(1000, 667));
                        await image.SaveAsync(fullPath);
                    }
                }
                return Ok(new { FileName = fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        [HttpPost]
        [Route("Adm_UploadImageTourGuide")]
        [Authorize]
        public async Task<IActionResult> UploadImageTourGuide([FromForm] IFormFile file)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesEmployee";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(1000, 667));
                        await image.SaveAsync(fullPath);
                    }
                }
                return Ok(new { FileName = fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpPost]
        [Route("UploadImageCompany")]
        [Authorize]
        public async Task<IActionResult> UploadImageCompany([FromForm] IFormFile file)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesCompanyTravel";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(1000, 667));
                        await image.SaveAsync(fullPath);
                    }
                }
                return Ok(new { FileName = fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // Upload image Employee
        [HttpPost]
        [Route("Adm_UploadImageEmployee")]
        [Authorize]
        public async Task<IActionResult> UploadImageEmployee([FromForm] IFormFile file)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesEmployee";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(1000, 667));
                        await image.SaveAsync(fullPath);
                    }
                }
                return Ok(new { FileName = fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // Upload Multi Images TouristAttr
        [HttpPost]
        [Route("UploadImagesTouristAttr")]
        [Authorize]
        public async Task<IActionResult> UploadImagesTouristAttr(List<IFormFile> files)
        {
            try
            {
                //
                var result = new List<FileUploadResult>();
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesTouristAttractions";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if(files.Count ==0 || files == null)
                {
                    return BadRequest();
                }
                foreach (var file in files)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(810, 540));
                        await image.SaveAsync(fullPath);
                    }
                    result.Add(new FileUploadResult() { FileName = fileName, Length = file.Length });
                    continue;
                }
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        //
        [HttpPost]
        [Route("Adm_UploadImageBanner/{type}")]
        [Authorize]
        public async Task<IActionResult> UploadImageBanner([FromForm] IFormFile file, int? type=1)
        {
            try
            {
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\ImagesBanner";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (file.Length > 0)
                {
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        if (type == 1)
                        {
                            image.Mutate(m => m.Resize(1980, 488));
                            await image.SaveAsync(fullPath);
                        }
                        else
                        {
                            image.Mutate(m => m.Resize(600, 376));
                            await image.SaveAsync(fullPath);
                        }
                        
                    }
                }
                return Ok(new { FileName = fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


    }
}
