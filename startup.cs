using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WEB_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.2
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
          
            services.AddCors(options =>
            {
               /* options.AddPolicy("MyPolicy",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:5001",
                                            "http://localhost:5000",
                                            "https://localhost:44305")
                                .WithMethods("PUT", "DELETE", "GET");
                    });*/

                    /*options.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins("https://localhost:5001",
                                            "http://localhost:5000",
                                            "https://localhost:44382",
                                            "http://localhost:3707")
                                .WithMethods("PUT", "DELETE", "GET");
                    });*/
            });

            services.AddCors(setup =>
            {
               
                setup.AddDefaultPolicy( policy =>
                 {
                     policy.AllowAnyOrigin();
                     policy.AllowAnyMethod();
                     policy.AllowAnyHeader();
                     //policy.WithHeaders("Access-Cotrol-Allow-Origin", "Access-Cotrol-Allow-Headers");
                 });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

             app.UseCors();
             app.UseCors(builder => {builder.WithOrigins("http://localhost:5000");});
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}