using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using trial3;
using trial3.Authentication;

namespace NoteApp_Production
{
    public class Startup
    {
        private static String[] arguments = Environment.GetCommandLineArgs();

        private string server = arguments[2];

        private string database = arguments[3];

        private string username = arguments[4];

        private string password = arguments[5];
        
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)

        {

            var connection =@"Server="+server+";Database="+database +";user="+username+";password="+password+"; port=3306";
            services.AddDbContext<CLOUD_CSYEContext>(options => options.UseMySql(connection));
   
            //   services.AddDbContextPool<USerContext>( // replace "YourDbContext" with the class name of your DbContext
           //     options => options.UseMySql("server=localhost; port=3306; database=CSYE;user=deosthale;password=NikonD%100")); // replace with your Connection Strin;
                      //  services.AddDbContext<UserContext>(options =>
               // options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));
                        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                        services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions,BasicAuthenticationHandler>("BasicAuthentication", null);

            // Register DI for user service
            services.AddScoped<IUSerServices, UserServices>();
            services.AddAWSService<IAmazonS3>();
            services.Configure<FormOptions>(
                options =>
                {
                    options.MultipartBodyLengthLimit = 80000000;
                    options.ValueLengthLimit = int.MaxValue;
                    options.MultipartHeadersLengthLimit = int.MaxValue;
                });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
                });
        }
        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<CLOUD_CSYEContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
                
            if (env.IsDevelopment())
            { 
            app.UseDeveloperExceptionPage();
            }   
            else if(env.IsProduction())
            {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            }
            UpdateDatabase(app);
             app.UseAuthentication();
       
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            
            app.UseMvc();
        }
    }
}
