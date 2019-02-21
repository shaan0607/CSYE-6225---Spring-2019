using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using trial3.Authentication;
// /using WebApplication2.Authentication;

namespace trial3
{
    //dotnet ef dbcontext scaffold "Server=localhost;Database=CLOUD_CSYE;User=deosthale;Password=NikonD%100;" "Pomelo.EntityFrameworkCore.MySql"
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

//        MySqlConnection connection = new MySqlConnection("server=xxx-rds-dev.cri8oe6mntib.us-east-1.rds.amazonaws.com;user id=xxx;password=xxx;database=xxx;port=3306;"))

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
                var connection = @"Server=localhost;Database=CLOUD_CSYE;user= deosthale;password=NikonD%100";
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
            services.Configure<ForwardedHeadersOptions>(options =>
            {
    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
                });
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

            app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

app.UseAuthentication();
 
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            
            app.UseMvc();
        }
    }
}
