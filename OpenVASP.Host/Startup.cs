using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenVASP.Host.Modules;
using OpenVASP.Host.Services;

namespace OpenVASP.Host
{
    public class Startup
    {
        public ILifetimeScope ApplicationContainer { get; private set; }

        private AppSettings _appSettings;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddNewtonsoftJson();

            _appSettings = new AppSettings
            {
                InstanceName = Configuration["AppSettings:InstanceName"],
                EthereumRpcUri = Configuration["AppSettings:EthereumRpcUri"],
                HandshakePrivateKeyHex = Configuration["AppSettings:HandshakePrivateKeyHex"],
                SignaturePrivateKeyHex = Configuration["AppSettings:SignaturePrivateKeyHex"],
                VaspSmartContractAddress = Configuration["AppSettings:VaspSmartContractAddress"],
                WhisperRpcUri = Configuration["AppSettings:WhisperRpcUri"]
            };
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"OpenVASP Demo ({_appSettings.InstanceName})", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServiceModule(_appSettings));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ApplicationContainer = app.ApplicationServices.GetAutofacRoot();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseStaticFiles();
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
            
            app.UseSwagger();
            app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenVASP Demo"));
        }
    }
}
