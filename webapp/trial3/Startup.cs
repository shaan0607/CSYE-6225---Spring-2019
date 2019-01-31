using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
                var connection = @"Server=localhost;Database=CLOUD_CSYE;user= root;password=1234";
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            
            app.UseMvc();
        }
    }
}