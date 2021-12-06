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
        private string _baseUrl = "http://192.168.1.9:8000/";
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
                return Ok(new { FileName = _baseUrl + "ImagesTour/"+ fileName });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // Upload image Employee
        [HttpPost]
        [Route("Adm_UploadImageEmployee")]
        public async Task<IActionResult> UploadImageEmployee([FromForm] IFormFile file)
        {
            try
            {
                var path = $"{this._webHostEnvironment.WebRootPath}\\ImagesEmployee";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                FileInfo fileInfo = new FileInfo(file.FileName);
                var fullPath = Path.Combine(path, fileInfo.Name);
                var fullPathNew = fullPath;
                if (!System.IO.File.Exists(fullPath))
                {
                    //fullPathNew = GetUniqueFilePath(fullPath);
                }
                using (FileStream fileStream = new FileStream(fullPathNew, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                return Ok(new { FileName = _baseUrl + "ImagesEmployee/" + fullPathNew.Split("\\").LastOrDefault() });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        // Upload Multi Images TouristAttr
        [HttpPost()]
        [Route("Adm_UploadImagesTouristAttr")]
        public async Task<IActionResult> UploadImagesTouristAttr(List<IFormFile> files)
        {
            try
            {
                var result = new List<FileUploadResult>();
                var path = $"{this._webHostEnvironment.WebRootPath}\\ImagesTouristAttractions";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //var path = $"{this._webHostEnvironment.WebRootPath}\\TourImages";
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    var fullPath = Path.Combine(path, fileInfo.Name);
                    var fullPathNew = fullPath;
                    if (System.IO.File.Exists(fullPath))
                    {
                        //fullPathNew = GetUniqueFilePath(fullPath);
                    }
                    using (FileStream fileStream = new FileStream(fullPathNew, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    result.Add(new FileUploadResult() { FileName = _baseUrl + "ImagesTouristAttractions/" + 
                        fullPathNew.Split("\\").LastOrDefault(), Length = file.Length });
                    continue;
                }
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }



    }
}
