
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTourDuLich.SearchModels;
using System.Globalization;
using System.IO;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using SelectPdf;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingTourController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public const string BaseUrlServer = "http://localhost:8000/ImagesTour/";
        public IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookingTourController(HUFI_09DHTH_TourManagerContext context, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _config = config;
            this._webHostEnvironment = webHostEnvironment;
        }


        [HttpPost("Adm_GetDataBooking")]
        public async Task<IActionResult> Adm_GetDataBooking([FromBody] BookingTourSearch bookingTour)
        {
            try
            {
                #region truy vấn thống tin
                string[] separator = { "," };
                var rs = await (from bk in _context.BookingTour
                                join c in _context.Customer on bk.CustomerId equals c.CustomerId
                                join t in _context.Tour on bk.TourId equals t.TourId
                                where (bk.IsDelete == null || bk.IsDelete == true)
                                orderby bk.DateConfirm descending, bk.BookingDate descending
                                select new
                                {
                                    bk.BookingTourId,
                                    bookingDate = DateTime.Parse(bk.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    BookingDateCheck = bk.BookingDate,
                                    t.TourId,
                                    t.TourName,
                                    c.CustomerId,
                                    c.CustomerName,
                                    c.PhoneNumber,
                                    c.Email,
                                    bk.QuanityAdult,
                                    bk.QuanityChildren,
                                    bk.QuanityBaby,
                                    bk.QuanityInfant,
                                    adultUnitPrice = string.Format("{0:0,0đ}", bk.AdultUnitPrice),
                                    childrenUnitPrice = string.Format("{0:0,0đ}", bk.ChildrenUnitPrice),
                                    babyUnitPrice = string.Format("{0:0,0đ}", bk.BabyUnitPrice),
                                    surcharge = string.Format("{0:0,0đ}", bk.Surcharge),
                                    discount = string.Format("{0:0,0đ}", bk.Discount),
                                    totalMoney = string.Format("{0:0,0đ}", bk.TotalMoney),
                                    totalMoneyBooking = string.Format("{0:0,0đ}", bk.TotalMoneyBooking),
                                    bk.OptionsNote,
                                    bk.Note,
                                    bk.Qrcode,
                                    typePayment = bk.TypePayment == 1 ? "Thanh toán tiền mặt" : "Chuyển khoản",
                                    status = bk.Status == false ? "Chưa thanh toán" : "Đã xác nhận thanh toán",
                                    statusCheck = bk.Status,
                                    empIDConfirm = bk.EmpIdconfirm == null ? "Chưa cập nhật" : bk.EmpIdconfirmNavigation.EmpName,
                                    dateConfirm = bk.DateConfirm == null ? "Chua xác nhận" : DateTime.Parse(bk.DateConfirm.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),

                                }).ToListAsync();
                #endregion
                var ListObj = rs;
                foreach (var item in ListObj.ToList())
                {
                    var dateCheck = DateTime.Parse(item.BookingDateCheck.ToString()).AddDays(2);
                    if (item.statusCheck == false && dateCheck < DateTime.Now.Date)
                    {
                        rs.Remove(item);
                        var bookingOutOfDate = await _context.BookingTour.Where(m => (m.IsDelete == true || m.IsDelete == null) && m.BookingTourId == item.BookingTourId).FirstOrDefaultAsync();
                        bookingOutOfDate.IsDelete = false;
                    }
                }
                await _context.SaveChangesAsync();
                if (bookingTour.TourID != null)
                {
                    rs = rs.Where(m => m.TourId == bookingTour.TourID).ToList();
                }
                if (bookingTour.BookingDate != null)
                {
                    rs = rs.Where(m => m.BookingDateCheck == bookingTour.BookingDate).ToList();
                }
                if (bookingTour.Status != null)
                {
                    rs = rs.Where(m => m.statusCheck == bookingTour.Status).ToList();
                }
                //if(bookingTour)
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        // [Nguyen Tan Hai][12/17/2021]
        // Đặt tour

        [HttpPost("Adm_CreateBookingTour")]
        public async Task<ActionResult<Employee>> Adm_CreateBookingTour([FromBody] BookingTourModels bookingTourModels)
        {
            try
            {
                string hostName = "http://localhost:3000/my-tour/show-customer-for-booking-tour-details/bookingID=";
                if (bookingTourModels == null)
                {
                    return BadRequest();
                }
                // kiểm tra khách hàng này đã có trong db chwua nếu chưa insert vào db
                var rs = await (from c in _context.Customer
                                where (c.Email.Equals(bookingTourModels.CustomerEmail)
                                            || c.PhoneNumber.Equals(bookingTourModels.CustomerPhone))
                                        && (c.IsDelete == null || c.IsDelete == true)
                                select c).FirstOrDefaultAsync();
                if (rs == null)
                {
                    Customer customer = new Customer();
                    customer.CustomerName = bookingTourModels.CustomerName;
                    customer.PhoneNumber = bookingTourModels.CustomerPhone;
                    customer.Email = bookingTourModels.CustomerEmail;
                    customer.Address = bookingTourModels.Address;
                    customer.DateInsert = DateTime.Now.Date;
                    customer.DateUpdate = DateTime.Now.Date;
                    customer.IsDelete = null;
                    await _context.Customer.AddAsync(customer);
                    await _context.SaveChangesAsync();
                    rs = customer;
                }

                // Theem thoong tin Booking tour
                BookingTour bookingTour = new BookingTour();
                bookingTour.TourId = bookingTourModels.TourId;
                bookingTour.CustomerId = rs.CustomerId;
                bookingTour.QuanityAdult = bookingTourModels.QuanityAdult;
                bookingTour.QuanityBaby = bookingTourModels.QuanityBaby;
                bookingTour.QuanityChildren = bookingTourModels.QuanityChildren;
                bookingTour.QuanityInfant = bookingTourModels.QuanityInfant;
                bookingTour.AdultUnitPrice = bookingTourModels.AdultUnitPrice;
                bookingTour.ChildrenUnitPrice = bookingTourModels.ChildrenUnitPrice;
                bookingTour.BabyUnitPrice = bookingTourModels.BabyUnitPrice;
                bookingTour.Discount = bookingTourModels.Discount;
                bookingTour.Surcharge = bookingTourModels.Surcharge;
                bookingTour.TotalMoneyBooking = bookingTourModels.TotalMoneyBooking;
                bookingTour.TotalMoney = bookingTourModels.TotalMoney;
                bookingTour.OptionsNote = bookingTourModels.OptionsNote;
                bookingTour.Note = bookingTourModels.Note;
                bookingTour.TypePayment = bookingTourModels.TypePayment;
                bookingTour.Status = false;
                bookingTour.BookingDate = DateTime.Now.Date;
                bookingTour.IsDelete = null;
                await _context.BookingTour.AddAsync(bookingTour);
                await _context.SaveChangesAsync();
                var bookingTourID = bookingTour.BookingTourId;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(hostName + bookingTourID, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                bookingTour.Qrcode = BitmapToBytes(qrCodeImage);
                await _context.SaveChangesAsync();
                return Ok(bookingTour);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new Employee record");
            }
        }
        public byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        [HttpPut("Adm_AcceptBooking")]
        [Authorize]
        public async Task<ActionResult> Adm_AcceptBooking([FromBody] DeleteModels accept)
        {
            try
            {
                var rs = await (from bt in _context.BookingTour
                                where bt.BookingTourId == accept.SelectByIds[0] && (bt.IsDelete == true || bt.IsDelete == null)
                                orderby bt.DateConfirm descending, bt.BookingDate

                                select bt).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                if (rs.Status == true)
                {
                    return StatusCode(StatusCodes.Status204NoContent, "");
                }
                rs.Status = true;
                rs.DateConfirm = DateTime.Now.Date;
                rs.EmpIdconfirm = accept.EmpId;
                int? currentQuanity = rs.QuanityAdult + rs.QuanityChildren + rs.QuanityBaby;
                var checkQuanitytCurrentTourByID = await _context.Tour.Where(m => (m.IsDelete == null || m.IsDelete == true) && m.TourId == rs.TourId).FirstOrDefaultAsync();
                if (checkQuanitytCurrentTourByID != null)
                {
                    int? currentQuanitytOld = checkQuanitytCurrentTourByID.CurrentQuanity;
                    checkQuanitytCurrentTourByID.CurrentQuanity = currentQuanitytOld + currentQuanity;
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        // phiếu xác nhận booking tour
        [HttpGet("Adm_BookingTourDetails")]
        public async Task<ActionResult> Adm_GetBookingTourDetails(Guid pID)
        {
            try
            {
                string[] separatorAddress = { "||" };
                //
                #region truy vấn thông tin
                var rs = await (from bt in _context.BookingTour
                                join c in _context.Customer on bt.CustomerId equals c.CustomerId
                                join t in _context.Tour on bt.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                where bt.BookingTourId == pID
                                select new
                                {
                                    bt.BookingTourId,
                                    bt.Discount,
                                    bt.TotalMoneyBooking,
                                    bt.IsDelete,
                                    bt.TotalMoney,
                                    bt.Surcharge,
                                    bt.TypePayment,
                                    bt.AdultUnitPrice,
                                    bt.ChildrenUnitPrice,
                                    bt.BabyUnitPrice,
                                    t.TourId,
                                    c.CustomerId,
                                    c.CustomerName,
                                    c.Email,
                                    c.Address,
                                    c.PhoneNumber,
                                    bookingDate = DateTime.Parse(bt.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    bt.Status,
                                    duration = DateTime.Parse(bt.BookingDate.ToString()).AddDays(2).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    tourImg = BaseUrlServer + t.TourImg.Trim(),
                                    t.TourName,
                                    bt.QuanityAdult,
                                    bt.QuanityChildren,
                                    bt.QuanityBaby,
                                    bt.QuanityInfant,
                                    bt.OptionsNote,
                                    bt.Note,
                                    dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    departurePlaceFrom = pf.ProvinceName,
                                    journeys = pf.ProvinceName + " - " + pt.ProvinceName + " - " + pf.ProvinceName,
                                    qrCode = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(bt.Qrcode))

                                }).FirstOrDefaultAsync();
                #endregion
                if (rs == null)
                {
                    return NotFound();
                }
                string Address = rs.Address;
                if (rs.Address != null)
                {
                    string[] arrAdress = rs.Address.Split(separatorAddress, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                    Address = arrAdress[0].ToString();
                    string wards = "";
                    string provice = "";
                    string districts = "";
                    if (arrAdress.Length == 4)
                    {

                        wards = await _context.Wards.Where(m => m.WardId == int.Parse(arrAdress[1].Trim().ToString())).Select(m => m.WardName).FirstOrDefaultAsync();
                        districts = await _context.District.Where(m => m.DistrictId == int.Parse(arrAdress[2].Trim().ToString())).Select(m => m.DistrictName).FirstOrDefaultAsync();
                        provice = await _context.Province.Where(m => m.ProvinceId == int.Parse(arrAdress[3].Trim().ToString())).Select(m => m.ProvinceName).FirstOrDefaultAsync();
                        Address = Address + ", " + wards + ", " + districts + ", " + provice;
                    }

                }
                string Options = "";
                if (rs.OptionsNote != null && rs.OptionsNote.Trim() != "")
                {
                    string[] arrOptionNote = rs.OptionsNote.Split(",").ToArray();
                    var listObjOptions = await _context.CatEnumeration.Where(m => arrOptionNote.Contains(m.EnumerationName.Trim())).Select(m => m.EnumerationTranslate).ToArrayAsync();
                    Options = string.Join(", ", listObjOptions);
                }

                return Ok(new
                {
                    data = rs,
                    address = Address,
                    OptionsNote = Options,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        [HttpGet("Adm_SendEmailAfterBooking")]
        public async Task<ActionResult> Adm_SendEmailAfterBooking(Guid? pID, int? type = null)
        {
            try
            {
                string[] separator = { "," };
                string[] separatorAddress = { "||" };
                #region
                var rs = await (from bt in _context.BookingTour
                                join c in _context.Customer on bt.CustomerId equals c.CustomerId
                                join t in _context.Tour on bt.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                where bt.BookingTourId == pID
                                select new
                                {
                                    bt.BookingTourId,
                                    bt.Discount,
                                    bt.TotalMoneyBooking,
                                    bt.IsDelete,
                                    bt.TotalMoney,
                                    bt.Surcharge,
                                    bt.TypePayment,
                                    t.TourId,
                                    c.CustomerName,
                                    c.Email,
                                    c.Address,
                                    c.PhoneNumber,
                                    bookingDate = DateTime.Parse(bt.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    bt.Status,
                                    duration = DateTime.Parse(bt.BookingDate.ToString()).AddDays(2).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    tourImg = BaseUrlServer + t.TourImg.Trim(),
                                    t.TourName,
                                    bt.QuanityAdult,
                                    bt.QuanityChildren,
                                    bt.QuanityBaby,
                                    bt.QuanityInfant,
                                    optionsNote = bt.OptionsNote == null ? null : bt.OptionsNote.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).ToArray(),
                                    dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    departurePlaceFrom = pf.ProvinceName,
                                    journeys = pf.ProvinceName + " - " + pt.ProvinceName + " - " + pf.ProvinceName,
                                    qrCode = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(bt.Qrcode))

                                }).FirstOrDefaultAsync();
                #endregion
                string arrayOptonsNote = string.Join(", ", _context.CatEnumeration.Where(m => rs.optionsNote.Contains(m.EnumerationName)).Select(m => m.EnumerationTranslate).ToList());
                string Address = rs.Address;
                if (rs.Address != null)
                {
                    string[] arrAdress = rs.Address.Split(separatorAddress, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                    Address = arrAdress[0].ToString();
                    string wards = "";
                    string provice = "";
                    string districts = "";
                    if (arrAdress.Length == 4)
                    {

                        wards = await _context.Wards.Where(m => m.WardId == int.Parse(arrAdress[1].Trim().ToString())).Select(m => m.WardName).FirstOrDefaultAsync();
                        districts = await _context.District.Where(m => m.DistrictId == int.Parse(arrAdress[2].Trim().ToString())).Select(m => m.DistrictName).FirstOrDefaultAsync();
                        provice = await _context.Province.Where(m => m.ProvinceId == int.Parse(arrAdress[3].Trim().ToString())).Select(m => m.ProvinceName).FirstOrDefaultAsync();
                        Address = Address + ", " + wards + ", " + districts + ", " + provice;
                    }

                }
                if (type != null && type == 1 && rs.Status == false)
                {
                    return StatusCode(StatusCodes.Status204NoContent, "Vui lòng xác nhận thanh toán booking này trước khi gửi mail!");
                }
                // send email
                string FilePath = $"{this._webHostEnvironment.WebRootPath}\\HtmlExport\\BookingDetails.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();
                MailText = MailText.Replace("{ImageQRCode}", $"{rs.qrCode}");


                //
                MailText = MailText.Replace("{FullName}", $"{rs.CustomerName}");
                MailText = MailText.Replace("{Email}", $"{rs.Email}");
                MailText = MailText.Replace("{Address}", $"{Address}");
                MailText = MailText.Replace("{PhoneNumber}", $"{rs.PhoneNumber}");
                MailText = MailText.Replace("{QuanityAdult}", $"{rs.QuanityAdult}");
                MailText = MailText.Replace("{QuanityChildren}", $"{rs.QuanityChildren}");
                MailText = MailText.Replace("{QuanityBaby}", $"{rs.QuanityBaby}");
                MailText = MailText.Replace("{QuanityInfant}", $"{rs.QuanityInfant}");
                MailText = MailText.Replace("{OptionsNote}", $"{arrayOptonsNote}");

                //
                MailText = MailText.Replace("{BookingID}", $"{rs.BookingTourId}");
                MailText = MailText.Replace("{TourID}", $"{rs.TourId}");
                MailText = MailText.Replace("{TotalMoney}", $"{rs.TotalMoney}");
                MailText = MailText.Replace("{PhoneNumber}", $"{rs.PhoneNumber}");
                MailText = MailText.Replace("{DateStart}", $"{rs.dateStart}");
                MailText = MailText.Replace("{DateEnd}", $"{rs.dateEnd}");
                MailText = MailText.Replace("{DeparturePlaceFrom}", $"{rs.departurePlaceFrom}");
                string typePayment = rs.TypePayment == 1 ? "Thanh toán tiền mặt" : "Chuyển khoản";
                MailText = MailText.Replace("{TypePayment}", $"{typePayment}");
                string host = $"http://localhost:3000/my-tour/show-customer-for-booking-tour-details/bookingID={rs.BookingTourId}";
                MailText = MailText.Replace("{HrefBookingDetails}", host);
                string status = rs.Status == false ? "Chưa thanh toán" : "Đã thanh toán";
                MailText = MailText.Replace("{Status}", $"{status}");
                MailText = MailText.Replace("{Duration}", $"{rs.duration}");
                MailText = MailText.Replace("{ImageLogo}", "https://storage.googleapis.com/tripi-assets/mytour/icons/icon_company_group.svg");
                string sending = SendEmail($"{rs.Email.Trim()}", "Phiếu xác nhận Booking", MailText);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }


        //======================== send email
        public string SendEmail(string to, string subject, string html)
        {
            try
            {
                // create message
                var sysLogin = _config["Emailkey:SmtpUser"].ToString();
                var sysPass = _config["Emailkey:SmtpPass"].ToString();
                var sysAddress = new MailAddress(sysLogin, "Mytour.surge.sh");
                var receiverAddress = new MailAddress(to);

                // send email
                var sMTPClient = new SmtpClient
                {
                    Host = _config["Emailkey:SmtpHost"].ToString(),
                    EnableSsl = true,
                    Port = int.Parse(_config["Emailkey:SmtpPort"].ToString()),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(sysLogin, sysPass)
                };
                using (var email = new MailMessage(sysAddress, receiverAddress)
                {
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = html,

                })
                {
                    sMTPClient.Send(email);
                }
                return "Send mail success";
            }
            catch (Exception ex)
            {
                return $"{ex}";
            }

        }


        //
        ///get tour theo id customer,thái trần kiều diễm
        ///
        [HttpGet("MB_GetBookedByCustomer")]
        public async Task<IActionResult> MB_GetBookedByCustomer(Guid? customerId, bool isDelete)
        {
            try
            {
                var rs = await (from b in _context.BookingTour
                                join c in _context.Customer on b.CustomerId equals c.CustomerId
                                join t in _context.Tour on b.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                where isDelete == true
                                    ?
                                    ((b.IsDelete == null || b.IsDelete == true)
                                    && b.CustomerId == customerId)
                                    :
                                    ((b.IsDelete == false)
                                    && b.CustomerId == customerId)
                                select new
                                {
                                    b.BookingTourId,
                                    t.TourName,
                                    b.Status,
                                    BookingDateCheck = b.BookingDate,
                                    tourImg = BaseUrlServer + t.TourImg.Trim(),
                                    t.Rating,
                                    dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    bookingDate = DateTime.Parse(b.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    departurePlaceFrom = pf.ProvinceName,
                                    journeys = pf.ProvinceName + " - " + pt.ProvinceName + " - " + pf.ProvinceName,
                                    b.TotalMoney,
                                    b.TotalMoneyBooking,
                                }).ToListAsync();
                var ListObj = rs;
                foreach (var item in ListObj.ToList())
                {
                    var dateCheck = DateTime.Parse(item.BookingDateCheck.ToString()).AddDays(2);
                    if (item.Status == false && dateCheck < DateTime.Now.Date)
                    {
                        rs.Remove(item);
                        var bookingOutOfDate = await _context.BookingTour.Where(m => (m.IsDelete == true || m.IsDelete == null) && m.BookingTourId == item.BookingTourId).FirstOrDefaultAsync();
                        bookingOutOfDate.IsDelete = false;
                    }
                }
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }

        }



        //hủy tour thái trần kiều diễm
        [HttpPut("MB_DeleteTourBooked")]
        public async Task<IActionResult> MB_DeleteTourBooked(BookingTourDelete booked)
        {
            try
            {
                var rs = _context.BookingTour.Where(m => m.BookingTourId == booked.BookingTourID).FirstOrDefault();
                var tour = _context.Tour.Where(m => m.TourId == rs.TourId).FirstOrDefault();
                int date = (int)(int?)((TimeSpan)(tour.DateStart - DateTime.Now.Date)).TotalDays;
                if (date <= 5)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Đã quá thời gian hủy tour");
                }
                rs.IsDelete = false;
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }


        [HttpGet("Cli_GetBookingByCustomer")]
        public async Task<IActionResult> Cli_GetBookingByCustomer(Guid? customerId)
        {
            try
            {
                var rs = await (from b in _context.BookingTour
                                join c in _context.Customer on b.CustomerId equals c.CustomerId
                                join t in _context.Tour on b.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                orderby b.BookingDate descending
                                where b.CustomerId == customerId
                                select new
                                {
                                    b.BookingTourId,
                                    t.TourName,
                                    b.Status,
                                    BookingDateCheck = b.BookingDate,
                                    tourImg = BaseUrlServer + t.TourImg.Trim(),
                                    totalDay = (int?)((TimeSpan)(t.DateEnd - t.DateStart)).TotalDays,
                                    t.Rating,
                                    dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    bookingDate = DateTime.Parse(b.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    b.TotalMoney,
                                    b.TotalMoneyBooking,
                                    b.IsDelete
                                }).ToListAsync();
                var ListObj = rs;
                foreach (var item in ListObj.ToList())
                {
                    var dateCheck = DateTime.Parse(item.BookingDateCheck.ToString()).AddDays(2);
                    if (item.Status == false && dateCheck < DateTime.Now.Date)
                    {
                        rs.Remove(item);
                        var bookingOutOfDate = await _context.BookingTour.Where(m => (m.IsDelete == true || m.IsDelete == null) && m.BookingTourId == item.BookingTourId).FirstOrDefaultAsync();
                        bookingOutOfDate.IsDelete = false;
                    }
                }
                await _context.SaveChangesAsync();
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        //===============================
        [HttpGet("Adm_ExportDataBookingTour")]
        public async Task<IActionResult> ExportDataBookingTour(Guid? pID, int type)
        {
            try
            {
                
                string[] separator = { "^||||^" };
                string[] separatorShedule = { "^||^" };
                var rs = await (from bt in _context.BookingTour
                                  join c in _context.Customer on bt.CustomerId equals c.CustomerId
                                  join t in _context.Tour on bt.TourId equals t.TourId
                                  join emp in _context.Employee on bt.EmpIdconfirm equals emp.EmpId
                                    join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                    join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                    where (bt.IsDelete == null || bt.IsDelete == true) && bt.BookingTourId ==pID
                          select new {
                              bt.BookingTourId,
                              bt.Discount,
                              bt.TotalMoneyBooking,
                              bt.IsDelete,
                              bt.TotalMoney,
                              bt.Surcharge,
                              bt.TypePayment,
                              bt.AdultUnitPrice,
                              bt.ChildrenUnitPrice,
                              bt.BabyUnitPrice,
                              bt.QuanityAdult,
                              bt.QuanityChildren,
                              bt.QuanityBaby,
                              bt.QuanityInfant,
                              qrCode = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(bt.Qrcode)),
                              bt.Note,
                              optionsNote = bt.OptionsNote == null ? null : bt.OptionsNote.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).ToArray(),
                              bt.Status,
                              t.TourId,
                              t.TourName,
                              dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                              dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                              c.CustomerName,
                              c.Email,
                              c.Address,
                              c.PhoneNumber,
                              emp.EmpName,
                              journeys = pf.ProvinceName + " - " + pt.ProvinceName + " - " + pf.ProvinceName,
                              bookingDate = DateTime.Parse(bt.BookingDate.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                              Schedule = t.Schedule.Replace("&nbsp;", "").Replace("\n", "")
                          }).FirstOrDefaultAsync();

                if (rs == null) return NotFound();
                string arrayOptonsNote = string.Join(", ", _context.CatEnumeration.Where(m => rs.optionsNote.Contains(m.EnumerationName)).Select(m => m.EnumerationTranslate).ToList());
                if(rs.Note !="" || rs.Note != null)
                {
                    arrayOptonsNote += ", " + rs.Note;
                }
                string Address = rs.Address;
                string[] separatorAddress = { "||" };
                if (rs.Address != null)
                {
                    string[] arrAdress = rs.Address.Split(separatorAddress, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                    Address = arrAdress[0].ToString();
                    string wards = "";
                    string provice = "";
                    string districts = "";
                    if (arrAdress.Length == 4)
                    {

                        wards = await _context.Wards.Where(m => m.WardId == int.Parse(arrAdress[1].Trim().ToString())).Select(m => m.WardName).FirstOrDefaultAsync();
                        districts = await _context.District.Where(m => m.DistrictId == int.Parse(arrAdress[2].Trim().ToString())).Select(m => m.DistrictName).FirstOrDefaultAsync();
                        provice = await _context.Province.Where(m => m.ProvinceId == int.Parse(arrAdress[3].Trim().ToString())).Select(m => m.ProvinceName).FirstOrDefaultAsync();
                        Address = Address + ", " + wards + ", " + districts + ", " + provice;
                    }

                }
                // lấy ra lịch trình ngày
                var arrayShedule = rs.Schedule.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                string htmlShedule = "";
                foreach (var item in arrayShedule)
                {
                    var arrayCheduleOfDay = item.Split(separatorShedule, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                    htmlShedule += $"<div class='timeline'>" +
                                        $"<div class='timeline-title'>{arrayCheduleOfDay[0]}</div>" +
                                        $"<div class='timeline-details'>{arrayCheduleOfDay[1]}</div>" +
                                    $"</div>";  
                }


                //=============== xử lý đọc thông tin file html => export docs or pdf
                string FilePath = $"{this._webHostEnvironment.WebRootPath}\\HtmlExport\\ExportShedule.html";
                StreamReader str = new StreamReader(FilePath);
                string HtmlExport = str.ReadToEnd();
                str.Close();
                HtmlExport = HtmlExport.Replace("{h-shedule-mytour}", $"{htmlShedule}");
                HtmlExport = HtmlExport.Replace("{h-booking-date}", $"{rs.bookingDate}");
                HtmlExport = HtmlExport.Replace("{h-BookingTourID}", $"{rs.BookingTourId}");
                HtmlExport = HtmlExport.Replace("{h-TourID}", $"{rs.TourId}");
                HtmlExport = HtmlExport.Replace("{h-QR}", $"{rs.qrCode}");
                string status = rs.Status == false ? "Chưa thanh toán" : "Đã thanh toán";
                HtmlExport = HtmlExport.Replace("{h-status}", $"{status}");
                string typePayment = rs.TypePayment == 1 ? "Thanh toán tiền mặt" : "Chuyển khoản";
                HtmlExport = HtmlExport.Replace("{h-type-payment}", $"{typePayment}");
                HtmlExport = HtmlExport.Replace("{h-total-money-booking}", $"{string.Format("{0:0,0đ}", rs.TotalMoneyBooking)}");
                HtmlExport = HtmlExport.Replace("{h-total-money-discount}", $"{string.Format("{0:0,0đ}", rs.Discount)}");
                HtmlExport = HtmlExport.Replace("{h-total-money}", $"{string.Format("{0:0,0đ}", rs.TotalMoney)}");
                HtmlExport = HtmlExport.Replace("{h-Tour-Name}", $"{rs.TourName}");
                HtmlExport = HtmlExport.Replace("{h-journeys}", $"{rs.journeys}");
                HtmlExport = HtmlExport.Replace("{h-Date-Start}", $"{rs.dateStart}");
                HtmlExport = HtmlExport.Replace("{h-Date-End}", $"{rs.dateEnd}");
                HtmlExport = HtmlExport.Replace("{h-customer-name}", $"{rs.CustomerName}");
                HtmlExport = HtmlExport.Replace("{h-customer-email}", $"{rs.Email}");
                HtmlExport = HtmlExport.Replace("{h-customer-address}", $"{Address}");
                HtmlExport = HtmlExport.Replace("{h-phone-number}", $"{rs.PhoneNumber}");
                HtmlExport = HtmlExport.Replace("{h-customer-optionsNote}", $"{arrayOptonsNote}");
                HtmlExport = HtmlExport.Replace("{h-qadult}", $"{rs.QuanityAdult}");
                HtmlExport = HtmlExport.Replace("{h-qChildren}", $"{rs.QuanityChildren}");
                HtmlExport = HtmlExport.Replace("{h-qbaby}", $"{rs.QuanityBaby}");
                HtmlExport = HtmlExport.Replace("{h-qInfant}", $"{rs.QuanityInfant}");
                HtmlExport = HtmlExport.Replace("{h-employee}", $"{rs.EmpName}");

                switch (type)
                    {
                        case  1:
                            string outFile = @"E:\Down\PhieuBooking"+pID.ToString()+".docx";
                            HtmlExport = HtmlExport.Replace("{stylesMarrgin}", "");
                            SautinSoft.HtmlToRtf htmlToRtf = new SautinSoft.HtmlToRtf();
                            htmlToRtf.TextStyle.SingleFontFamily = "Times New Roman";
                            htmlToRtf.TextStyle.SingleFontSize = 13;
                            htmlToRtf.PageStyle.PageFooter.Html("<p style='text-align: center;font-size: 13pt;color: #003d71 !important;'>Copyright © 2020 - CÔNG TY CỔ PHẦN DU LỊCH VIỆT NAM VNTRAVEL</p>");
                            htmlToRtf.PageStyle.PageSize.A4();
                            if (htmlToRtf.OpenHtml(HtmlExport))
                            {
                                if (htmlToRtf.ToDocx(outFile))
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outFile) { UseShellExecute = true });
                                }
                            }
                                
                            break;
                        case  2:
                            HtmlExport = HtmlExport.Replace("{stylesMarrgin}", @"style='margin: 20pt'");
                            
                            HtmlToPdf htmlToPdf = new HtmlToPdf();
                            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), "A4", true);
                            htmlToPdf.Options.PdfPageSize = pageSize;
                            htmlToPdf.Options.MinPageLoadTime = 1;


                            htmlToPdf.Options.DisplayFooter = true;
                            PdfDocument pdfDocument = htmlToPdf.ConvertHtmlString(HtmlExport);
                            pdfDocument.Save(@"E:\Down\PhieuBooking" + pID.ToString()+".pdf"); 
                            pdfDocument.Close();
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"E:\Down\PhieuBooking" + pID.ToString() + ".pdf") { UseShellExecute = true });
                            break;
                        default: break;
                    
                }
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

    }
}
