using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QuanLyTourDuLich.Models
{
    public partial class HUFI_09DHTH_TourManagerContext : DbContext
    {
        public HUFI_09DHTH_TourManagerContext()
        {
        }

        public HUFI_09DHTH_TourManagerContext(DbContextOptions<HUFI_09DHTH_TourManagerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BookingTour> BookingTour { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<District> District { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Hotel> Hotel { get; set; }
        public virtual DbSet<HotelType> HotelType { get; set; }
        public virtual DbSet<KindOfNews> KindOfNews { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<Tour> Tour { get; set; }
        public virtual DbSet<TourDetails> TourDetails { get; set; }
        public virtual DbSet<TourGuide> TourGuide { get; set; }
        public virtual DbSet<TouristAttraction> TouristAttraction { get; set; }
        public virtual DbSet<TravelType> TravelType { get; set; }
        public virtual DbSet<UnitPrice> UnitPrice { get; set; }
        public virtual DbSet<Wards> Wards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=DESKTOP-4MGR8RB\\SQLEXPRESS;Database=HUFI_09DHTH_TourManager;User ID=sa;Password=tanhai123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingTour>(entity =>
            {
                entity.Property(e => e.BookingTourId)
                    .HasColumnName("bookingTourID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.BookingDate)
                    .HasColumnName("bookingDate")
                    .HasColumnType("date");

                entity.Property(e => e.CustomerId).HasColumnName("customerID");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EmpIdconfirm).HasColumnName("empIDConfirm");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(500);

                entity.Property(e => e.QuanityAdult).HasColumnName("quanityAdult");

                entity.Property(e => e.QuanityBaby).HasColumnName("quanityBaby");

                entity.Property(e => e.QuanityChildren).HasColumnName("quanityChildren");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Surcharge)
                    .HasColumnName("surcharge")
                    .HasColumnType("money");

                entity.Property(e => e.TotalMoney)
                    .HasColumnName("totalMoney")
                    .HasColumnType("money");

                entity.Property(e => e.TotalMoneyBooking)
                    .HasColumnName("totalMoneyBooking")
                    .HasColumnType("money");

                entity.Property(e => e.TourId).HasColumnName("tourID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.BookingTour)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("fk_BookingTour_Customer");

                entity.HasOne(d => d.EmpIdconfirmNavigation)
                    .WithMany(p => p.BookingTour)
                    .HasForeignKey(d => d.EmpIdconfirm)
                    .HasConstraintName("fk_BookingTour_Employee");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.BookingTour)
                    .HasForeignKey(d => d.TourId)
                    .HasConstraintName("fk_BookingTour_Tour");
            });

            modelBuilder.Entity<Comments>(entity =>
            {
                entity.HasKey(e => e.CommentId)
                    .HasName("PK__Comments__CDDE91BDCD0A123C");

                entity.Property(e => e.CommentId)
                    .HasColumnName("commentID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.CommentDate)
                    .HasColumnName("commentDate")
                    .HasColumnType("date");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CustomerId).HasColumnName("customerID");

                entity.Property(e => e.DateActive)
                    .HasColumnName("dateActive")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.ImagesList).HasColumnName("imagesList");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.TourId).HasColumnName("tourID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("fk_Comments_Customer");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Comments_Employee");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.TourId)
                    .HasConstraintName("fk_Comments_Tour");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customerID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(250);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("customerName")
                    .HasMaxLength(100);

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(37)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(11)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.Property(e => e.DistrictId).HasColumnName("districtID");

                entity.Property(e => e.DistrictName)
                    .HasColumnName("districtName")
                    .HasMaxLength(151);

                entity.Property(e => e.DivisionType)
                    .HasColumnName("divisionType")
                    .HasMaxLength(151);

                entity.Property(e => e.ProvinceId).HasColumnName("provinceID");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.District)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("fk_District_Province");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmpId)
                    .HasName("PK__Employee__AFB3EC6DF0978E25");

                entity.Property(e => e.EmpId)
                    .HasColumnName("empID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Avatar)
                    .HasColumnName("avatar")
                    .HasMaxLength(1000);

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateOfBirth)
                    .HasColumnName("dateOfBirth")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.EmpName)
                    .HasColumnName("empName")
                    .HasMaxLength(100);

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(101)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserName)
                    .HasColumnName("userName")
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.WorkingDate)
                    .HasColumnName("workingDate")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.Property(e => e.HotelId)
                    .HasColumnName("hotelID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(500);

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.HotelName)
                    .HasColumnName("hotelName")
                    .HasMaxLength(500);

                entity.Property(e => e.HotelTypeId).HasColumnName("hotelTypeID");

                entity.Property(e => e.ImagesList)
                    .HasColumnName("imagesList")
                    .HasMaxLength(2000);

                entity.Property(e => e.Introduce).HasColumnName("introduce");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Note).HasColumnName("note");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.Representative)
                    .HasColumnName("representative")
                    .HasMaxLength(100);

                entity.Property(e => e.RoomNumber).HasColumnName("roomNumber");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.WardId).HasColumnName("wardID");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.HotelEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Hotel_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.HotelEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_Hotel_Employee_Update");

                entity.HasOne(d => d.HotelType)
                    .WithMany(p => p.Hotel)
                    .HasForeignKey(d => d.HotelTypeId)
                    .HasConstraintName("fk_Hotel_HotelType");

                entity.HasOne(d => d.Ward)
                    .WithMany(p => p.Hotel)
                    .HasForeignKey(d => d.WardId)
                    .HasConstraintName("fk_Hotel_Wards");
            });

            modelBuilder.Entity<HotelType>(entity =>
            {
                entity.Property(e => e.HotelTypeId)
                    .HasColumnName("hotelTypeID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.HotelTypeName)
                    .HasColumnName("hotelTypeName")
                    .HasMaxLength(500);

                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasMaxLength(500);

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.HotelTypeEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_HotelType_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.HotelTypeEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_HotelType_Employee_Update");
            });

            modelBuilder.Entity<KindOfNews>(entity =>
            {
                entity.Property(e => e.KindOfNewsId)
                    .HasColumnName("kindOfNewsID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.KindOfNewsName)
                    .HasColumnName("kindOfNewsName")
                    .HasMaxLength(500);

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.KindOfNewsEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_KindOfNews_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.KindOfNewsEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_KindOfNews_Employee_Update");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.Property(e => e.NewsId)
                    .HasColumnName("newsID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.ImagesList).HasColumnName("imagesList");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.KindOfNewsId).HasColumnName("kindOfNewsID");

                entity.Property(e => e.NewsImg).HasColumnName("newsImg");

                entity.Property(e => e.NewsName)
                    .HasColumnName("newsName")
                    .HasMaxLength(500);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.NewsEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_News_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.NewsEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_News_Employee_Update");

                entity.HasOne(d => d.KindOfNews)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.KindOfNewsId)
                    .HasConstraintName("fk_News_KindOfNews");
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.Property(e => e.ProvinceId).HasColumnName("provinceID");

                entity.Property(e => e.DivisionType)
                    .HasColumnName("divisionType")
                    .HasMaxLength(151);

                entity.Property(e => e.ProvinceName)
                    .HasColumnName("provinceName")
                    .HasMaxLength(151);

                entity.Property(e => e.Regions).HasColumnName("regions");
            });

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.Property(e => e.TourId)
                    .HasColumnName("tourID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CurrentQuanity).HasColumnName("currentQuanity");

                entity.Property(e => e.DateEnd)
                    .HasColumnName("dateEnd")
                    .HasColumnType("date");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateStart)
                    .HasColumnName("dateStart")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.DeparturePlace).HasColumnName("departurePlace");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.QuanityMax).HasColumnName("quanityMax");

                entity.Property(e => e.QuanityMin).HasColumnName("quanityMin");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.Schedule).HasColumnName("schedule");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Suggest).HasColumnName("suggest");

                entity.Property(e => e.TourGuideId).HasColumnName("tourGuideID");

                entity.Property(e => e.TourImg).HasColumnName("tourImg");

                entity.Property(e => e.TourName).HasColumnName("tourName");

                entity.Property(e => e.Transport)
                    .HasColumnName("transport")
                    .HasMaxLength(50);

                entity.Property(e => e.TravelTypeId).HasColumnName("travelTypeID");

                entity.HasOne(d => d.DeparturePlaceNavigation)
                    .WithMany(p => p.Tour)
                    .HasForeignKey(d => d.DeparturePlace)
                    .HasConstraintName("fk_Tour_Province");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TourEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Tour_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TourEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_Tour_Employee_Update");

                entity.HasOne(d => d.TourGuide)
                    .WithMany(p => p.Tour)
                    .HasForeignKey(d => d.TourGuideId)
                    .HasConstraintName("fk_Tour_TourGuide");

                entity.HasOne(d => d.TravelType)
                    .WithMany(p => p.Tour)
                    .HasForeignKey(d => d.TravelTypeId)
                    .HasConstraintName("fk_Tour_TravelType");
            });

            modelBuilder.Entity<TourDetails>(entity =>
            {
                entity.HasKey(e => new { e.TourId, e.TouristAttrId })
                    .HasName("PK__TourDeta__ED339C4015792552");

                entity.Property(e => e.TourId).HasColumnName("tourID");

                entity.Property(e => e.TouristAttrId).HasColumnName("touristAttrID");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.HotelId).HasColumnName("hotelID");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TourDetailsEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TourDetails_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TourDetailsEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TourDetails_Employee_Update");

                entity.HasOne(d => d.Hotel)
                    .WithMany(p => p.TourDetails)
                    .HasForeignKey(d => d.HotelId)
                    .HasConstraintName("fk_TourDetails_Hotel");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.TourDetails)
                    .HasForeignKey(d => d.TourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_TourDetails_Tour");

                entity.HasOne(d => d.TouristAttr)
                    .WithMany(p => p.TourDetails)
                    .HasForeignKey(d => d.TouristAttrId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_TourDetails_DiaDiem");
            });

            modelBuilder.Entity<TourGuide>(entity =>
            {
                entity.Property(e => e.TourGuideId)
                    .HasColumnName("tourGuideID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(150);

                entity.Property(e => e.Avatar)
                    .HasColumnName("avatar")
                    .HasMaxLength(500);

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateOfBirth)
                    .HasColumnName("dateOfBirth")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(51)
                    .IsUnicode(false);

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TourGuideName)
                    .HasColumnName("tourGuideName")
                    .HasMaxLength(150);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TourGuideEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TourGuide_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TourGuideEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TourGuide_Employee_Update");
            });

            modelBuilder.Entity<TouristAttraction>(entity =>
            {
                entity.HasKey(e => e.TouristAttrId)
                    .HasName("PK__TouristA__CAE8143AE69C3364");

                entity.Property(e => e.TouristAttrId)
                    .HasColumnName("touristAttrID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.ImagesList).HasColumnName("imagesList");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.ProvinceId).HasColumnName("provinceID");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TouristAttrName)
                    .HasColumnName("touristAttrName")
                    .HasMaxLength(250);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TouristAttractionEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TourAttr_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TouristAttractionEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TourAttr_Employee_Update");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.TouristAttraction)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_TourAttr_District");
            });

            modelBuilder.Entity<TravelType>(entity =>
            {
                entity.Property(e => e.TravelTypeId)
                    .HasColumnName("travelTypeID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(500);

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TravelTypeName)
                    .HasColumnName("travelTypeName")
                    .HasMaxLength(500);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TravelTypeEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TravelType_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TravelTypeEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TravelType_Employee_Update");
            });

            modelBuilder.Entity<UnitPrice>(entity =>
            {
                entity.HasKey(e => new { e.TourId, e.DateUpdate })
                    .HasName("PK__UnitPric__470A97E44F431611");

                entity.Property(e => e.TourId).HasColumnName("tourID");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.AdultUnitPrice)
                    .HasColumnName("adultUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.BabyUnitPrice)
                    .HasColumnName("babyUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.ChildrenUnitPrice)
                    .HasColumnName("childrenUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.DateInsert).HasColumnName("dateInsert");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Surcharge)
                    .HasColumnName("surcharge")
                    .HasColumnType("money");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.UnitPriceEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_UnitPrice_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.UnitPriceEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_UnitPrice_Employee_Update");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.UnitPrice)
                    .HasForeignKey(d => d.TourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UnitPrice_Tour");
            });

            modelBuilder.Entity<Wards>(entity =>
            {
                entity.HasKey(e => e.WardId)
                    .HasName("PK__Wards__A14E2C70F7051529");

                entity.Property(e => e.WardId).HasColumnName("wardID");

                entity.Property(e => e.DistrictId).HasColumnName("districtID");

                entity.Property(e => e.DivisionType)
                    .HasColumnName("divisionType")
                    .HasMaxLength(151);

                entity.Property(e => e.WardName)
                    .HasColumnName("wardName")
                    .HasMaxLength(151);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("fk_Wards_District");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
