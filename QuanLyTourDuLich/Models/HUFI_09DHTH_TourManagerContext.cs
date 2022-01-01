using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QuanLyTourDuLich.Models
{
    public partial class HUFI_09DHTH_TourManagerContext : DbContext
    {

        public HUFI_09DHTH_TourManagerContext(DbContextOptions<HUFI_09DHTH_TourManagerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BookingTour> BookingTour { get; set; }
        public virtual DbSet<CatEnumeration> CatEnumeration { get; set; }
        public virtual DbSet<CatScreen> CatScreen { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<District> District { get; set; }
        public virtual DbSet<EmpUserGroup> EmpUserGroup { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Promotion> Promotion { get; set; }
        public virtual DbSet<PromotionalTour> PromotionalTour { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<Tour> Tour { get; set; }
        public virtual DbSet<TourDetails> TourDetails { get; set; }
        public virtual DbSet<TourGuide> TourGuide { get; set; }
        public virtual DbSet<TouristAttraction> TouristAttraction { get; set; }
        public virtual DbSet<TravelCompanyTransport> TravelCompanyTransport { get; set; }
        public virtual DbSet<UserGroup> UserGroup { get; set; }
        public virtual DbSet<Wards> Wards { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingTour>(entity =>
            {
                entity.Property(e => e.BookingTourId)
                    .HasColumnName("bookingTourID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AdultUnitPrice)
                    .HasColumnName("adultUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.BabyUnitPrice)
                    .HasColumnName("babyUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.BookingDate)
                    .HasColumnName("bookingDate")
                    .HasColumnType("date");

                entity.Property(e => e.ChildrenUnitPrice)
                    .HasColumnName("childrenUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.CustomerId).HasColumnName("customerID");

                entity.Property(e => e.DateConfirm)
                    .HasColumnName("dateConfirm")
                    .HasColumnType("date");

                entity.Property(e => e.Discount)
                    .HasColumnName("discount")
                    .HasColumnType("money");

                entity.Property(e => e.EmpIdconfirm).HasColumnName("empIDConfirm");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(500);

                entity.Property(e => e.OptionsNote)
                    .HasColumnName("optionsNote")
                    .HasMaxLength(500);

                entity.Property(e => e.Qrcode)
                    .HasColumnName("QRCode")
                    .HasColumnType("image");

                entity.Property(e => e.QuanityAdult).HasColumnName("quanityAdult");

                entity.Property(e => e.QuanityBaby).HasColumnName("quanityBaby");

                entity.Property(e => e.QuanityChildren).HasColumnName("quanityChildren");

                entity.Property(e => e.QuanityInfant).HasColumnName("quanityInfant");

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

                entity.Property(e => e.TypePayment).HasColumnName("typePayment");

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

            modelBuilder.Entity<CatEnumeration>(entity =>
            {
                entity.HasKey(e => e.EnumerationId)
                    .HasName("PK__Cat_Enum__0A87095AA30B4475");

                entity.ToTable("Cat_Enumeration");

                entity.Property(e => e.EnumerationId)
                    .HasColumnName("enumerationID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.EnumerationName)
                    .HasColumnName("enumerationName")
                    .HasMaxLength(500);

                entity.Property(e => e.EnumerationTranslate)
                    .HasColumnName("enumerationTranslate")
                    .HasMaxLength(500);

                entity.Property(e => e.EnumerationType)
                    .HasColumnName("enumerationType")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.CatEnumerationEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Cat_Enumeration_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.CatEnumerationEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_Cat_Enumeration_Employee_Update");
            });

            modelBuilder.Entity<CatScreen>(entity =>
            {
                entity.HasKey(e => e.ScreenId)
                    .HasName("PK__Cat_Scre__19F2D7EDB1BFD4AC");

                entity.ToTable("Cat_Screen");

                entity.Property(e => e.ScreenId)
                    .HasColumnName("screenID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.ScreenName)
                    .HasColumnName("screenName")
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Comments>(entity =>
            {
                entity.HasKey(e => e.CommentId)
                    .HasName("PK__Comments__CDDE91BD17058285");

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
                    .HasMaxLength(500);

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

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(37)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.CustomerEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Cat_Customer_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.CustomerEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_Cat_Customer_Employee_Update");
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

            modelBuilder.Entity<EmpUserGroup>(entity =>
            {
                entity.HasKey(e => new { e.EmpId, e.UserGroupId })
                    .HasName("PK__Emp_User__5EC07C2BBA878F0B");

                entity.ToTable("Emp_UserGroup");

                entity.Property(e => e.EmpId).HasColumnName("empID");

                entity.Property(e => e.UserGroupId).HasColumnName("userGroupID");

                entity.HasOne(d => d.Emp)
                    .WithMany(p => p.EmpUserGroup)
                    .HasForeignKey(d => d.EmpId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Emp_UserGroup_Employee");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.EmpUserGroup)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Emp_UserGroup_UserGroup");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmpId)
                    .HasName("PK__Employee__AFB3EC6DD143339C");

                entity.Property(e => e.EmpId)
                    .HasColumnName("empID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(500);

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
                    .HasMaxLength(20)
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

                entity.Property(e => e.EnumerationId).HasColumnName("enumerationID");

                entity.Property(e => e.ImagesList).HasColumnName("imagesList");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

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

                entity.HasOne(d => d.Enumeration)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.EnumerationId)
                    .HasConstraintName("fk_News_Cat_Enumeration");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => new { e.UserGroupId, e.ScreenId })
                    .HasName("PK__Permissi__D6A629105DA3972F");

                entity.Property(e => e.UserGroupId).HasColumnName("userGroupID");

                entity.Property(e => e.ScreenId).HasColumnName("screenID");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.Screen)
                    .WithMany(p => p.Permission)
                    .HasForeignKey(d => d.ScreenId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Permission_Screen");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.Permission)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Permission_UserGroup");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.Property(e => e.PromotionId)
                    .HasColumnName("promotionID")
                    .HasDefaultValueSql("(newid())");

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

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsApplyAll).HasColumnName("isApplyAll");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.PromotionName)
                    .HasColumnName("promotionName")
                    .HasMaxLength(500);

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.PromotionEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_Promotion_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.PromotionEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_Promotion_Employee_Update");
            });

            modelBuilder.Entity<PromotionalTour>(entity =>
            {
                entity.HasKey(e => new { e.TourId, e.PromotionId })
                    .HasName("PK__Promotio__A803AB93C147EC8F");

                entity.Property(e => e.TourId).HasColumnName("tourID");

                entity.Property(e => e.PromotionId).HasColumnName("promotionID");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.PromotionalTourEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_PromotionalTour_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.PromotionalTourEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_PromotionalTour_Employee_Update");

                entity.HasOne(d => d.Promotion)
                    .WithMany(p => p.PromotionalTour)
                    .HasForeignKey(d => d.PromotionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PromotionalTour_Promotion");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.PromotionalTour)
                    .HasForeignKey(d => d.TourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PromotionalTour_Tour");
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

                entity.Property(e => e.AdultUnitPrice)
                    .HasColumnName("adultUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.BabyUnitPrice)
                    .HasColumnName("babyUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.ChildrenUnitPrice)
                    .HasColumnName("childrenUnitPrice")
                    .HasColumnType("money");

                entity.Property(e => e.CompanyTransportInTourId).HasColumnName("companyTransportInTourID");

                entity.Property(e => e.CompanyTransportStartId).HasColumnName("companyTransportStartID");

                entity.Property(e => e.ConditionByTour).HasColumnName("conditionByTour");

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

                entity.Property(e => e.DeparturePlaceFrom).HasColumnName("departurePlaceFrom");

                entity.Property(e => e.DeparturePlaceTo).HasColumnName("departurePlaceTo");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.GroupNumber).HasColumnName("groupNumber");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.NoteByMyTour).HasColumnName("noteByMyTour");

                entity.Property(e => e.NoteByTour).HasColumnName("noteByTour");

                entity.Property(e => e.QuanityMax).HasColumnName("quanityMax");

                entity.Property(e => e.QuanityMin).HasColumnName("quanityMin");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.Schedule).HasColumnName("schedule");

                entity.Property(e => e.Suggest).HasColumnName("suggest");

                entity.Property(e => e.Surcharge)
                    .HasColumnName("surcharge")
                    .HasColumnType("money");

                entity.Property(e => e.TourGuideId).HasColumnName("tourGuideID");

                entity.Property(e => e.TourImg).HasColumnName("tourImg");

                entity.Property(e => e.TourName).HasColumnName("tourName");

                entity.Property(e => e.TravelTypeId).HasColumnName("travelTypeID");

                entity.HasOne(d => d.CompanyTransportInTour)
                    .WithMany(p => p.TourCompanyTransportInTour)
                    .HasForeignKey(d => d.CompanyTransportInTourId)
                    .HasConstraintName("fk_Tour_TransportV02");

                entity.HasOne(d => d.CompanyTransportStart)
                    .WithMany(p => p.TourCompanyTransportStart)
                    .HasForeignKey(d => d.CompanyTransportStartId)
                    .HasConstraintName("fk_Tour_TransportV01");

                entity.HasOne(d => d.DeparturePlaceFromNavigation)
                    .WithMany(p => p.TourDeparturePlaceFromNavigation)
                    .HasForeignKey(d => d.DeparturePlaceFrom)
                    .HasConstraintName("fk_Tour_Province");

                entity.HasOne(d => d.DeparturePlaceToNavigation)
                    .WithMany(p => p.TourDeparturePlaceToNavigation)
                    .HasForeignKey(d => d.DeparturePlaceTo)
                    .HasConstraintName("fk_Tour_ProvinceV01");

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
                    .HasName("PK__TourDeta__ED339C40A0CF02E4");

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

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TourDetailsEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TourDetails_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TourDetailsEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TourDetails_Employee_Update");

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
                    .HasMaxLength(500);

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
                    .HasMaxLength(20)
                    .IsUnicode(false);

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
                    .HasName("PK__TouristA__CAE8143A303386E8");

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

            modelBuilder.Entity<TravelCompanyTransport>(entity =>
            {
                entity.HasKey(e => e.CompanyId)
                    .HasName("PK__TravelCo__AD5459B0579099F5");

                entity.Property(e => e.CompanyId)
                    .HasColumnName("companyID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(500);

                entity.Property(e => e.CompanyImage)
                    .HasColumnName("companyImage")
                    .HasMaxLength(500);

                entity.Property(e => e.CompanyName)
                    .HasColumnName("companyName")
                    .HasMaxLength(500);

                entity.Property(e => e.DateInsert)
                    .HasColumnName("dateInsert")
                    .HasColumnType("date");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("dateUpdate")
                    .HasColumnType("date");

                entity.Property(e => e.EmpIdinsert).HasColumnName("empIDInsert");

                entity.Property(e => e.EmpIdupdate).HasColumnName("empIDUpdate");

                entity.Property(e => e.EnumerationId).HasColumnName("enumerationID");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phoneNumber")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("provinceID");

                entity.HasOne(d => d.EmpIdinsertNavigation)
                    .WithMany(p => p.TravelCompanyTransportEmpIdinsertNavigation)
                    .HasForeignKey(d => d.EmpIdinsert)
                    .HasConstraintName("fk_TravelCompanyTransport_Employee_Insert");

                entity.HasOne(d => d.EmpIdupdateNavigation)
                    .WithMany(p => p.TravelCompanyTransportEmpIdupdateNavigation)
                    .HasForeignKey(d => d.EmpIdupdate)
                    .HasConstraintName("fk_TravelCompanyTransport_Employee_Update");

                entity.HasOne(d => d.Enumeration)
                    .WithMany(p => p.TravelCompanyTransport)
                    .HasForeignKey(d => d.EnumerationId)
                    .HasConstraintName("fk_TravelCompanyTransport_Cat_Enumeration");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.TravelCompanyTransport)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("fk_TravelCompanyTransport_Province");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.Property(e => e.UserGroupId)
                    .HasColumnName("userGroupID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.UserGroupName)
                    .HasColumnName("userGroupName")
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Wards>(entity =>
            {
                entity.HasKey(e => e.WardId)
                    .HasName("PK__Wards__A14E2C70BF72F0C5");

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
