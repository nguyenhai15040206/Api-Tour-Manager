
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

namespace QuanLyTourDuLich.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingTourController : ControllerBase
    {
        private readonly HUFI_09DHTH_TourManagerContext _context;
        public const string BaseUrlServer = "http://192.168.1.81:8000/ImagesTour/";
        public BookingTourController(HUFI_09DHTH_TourManagerContext context)
        {
            _context = context;
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
                                        && (c.IsDelete == null || c.IsDelete == true )
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
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(hostName+bookingTourID, QRCodeGenerator.ECCLevel.Q);
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
        
        // phiếu xác nhận booking tour
        [HttpGet("Adm_BookingTourDetails")]
        public async Task<ActionResult> Adm_GetBookingTourDetails(Guid pID)
        {
            try
            {
                string[] separator = { "^||||^" };
                string a = "anh asdhsads ^||^ sadhajsdhjashdajsd ^||||^ anh asdhsads ^||^ sadhajsdhjashdajsd anh asdhsads ^||^ sadhajsdhjashdajsd ^||||^ anh asdhsads ^||^ sadhajsdhjashdajsd";
                string[] arrr = a.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                var rs = await (from bt in _context.BookingTour
                                join c in _context.Customer on bt.CustomerId equals c.CustomerId
                                join t in _context.Tour on bt.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf .ProvinceId
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
                                    duration = DateTime.Parse(bt.BookingDate.ToString()).AddDays(2).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                                    tourImg = BaseUrlServer + t.TourImg.Trim(),
                                    t.TourName,
                                    bt.QuanityAdult,
                                    bt.QuanityChildren,
                                    bt.QuanityBaby,
                                    bt.QuanityInfant,
                                    dateStart = DateTime.Parse(t.DateStart.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    dateEnd = DateTime.Parse(t.DateEnd.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    departurePlaceFrom = pf.ProvinceName,
                                    journeys= pf.ProvinceName + " - "+ pt.ProvinceName+ " - "+ pf.ProvinceName,
                                    qrCode = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(bt.Qrcode))
                                }).FirstOrDefaultAsync();
                if (rs == null)
                {
                    return NotFound();
                }
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }
        }

        ///get tour theo id customer
        ///
        [HttpGet("MB_GetBookedByCustomer")] 
        public async Task<IActionResult> MB_GetBookedByCustomer(Guid? customerId)
        {
            try
            {
                var rs = await (from b in _context.BookingTour
                                join c in _context.Customer on b.CustomerId equals c.CustomerId
                                join t in _context.Tour on b.TourId equals t.TourId
                                join pf in _context.Province on t.DeparturePlaceFrom equals pf.ProvinceId
                                join pt in _context.Province on t.DeparturePlaceTo equals pt.ProvinceId
                                where b.CustomerId == customerId
                                select new
                                {
                                    b.BookingTourId,
                                    t.TourName,
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
                return Ok(rs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex}");
            }

        }

    }
}
