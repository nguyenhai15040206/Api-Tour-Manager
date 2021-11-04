using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        // OK
        //Scaffold-DbContext "Server=DESKTOP-4MGR8RB\SQLEXPRESS;Database=HUFI_09DHTH_TourManager;User ID=sa;Password=tanhai123;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
    }
}
