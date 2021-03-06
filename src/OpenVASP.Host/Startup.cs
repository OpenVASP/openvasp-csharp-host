using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenVASP.Host.Modules;
using OpenVASP.Host.Validation;
using OpenVASP.Messaging.Messages.Entities;

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
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.UseMemberCasing();
                })
                .AddFluentValidation(x =>
                {
                    x.RegisterValidatorsFromAssemblyContaining<CreateOutgoingTransactionRequestModelValidator>();
                });

            services.AddAutoMapper(typeof(AutoMapperProfile));

            _appSettings = new AppSettings
            {
                InstanceName = Configuration["AppSettings:InstanceName"],
                EthereumRpcUri = Configuration["AppSettings:EthereumRpcUri"],
                HandshakePrivateKeyHex = Configuration["AppSettings:HandshakePrivateKeyHex"],
                SignaturePrivateKeyHex = Configuration["AppSettings:SignaturePrivateKeyHex"],
                VaspSmartContractAddress = Configuration["AppSettings:VaspSmartContractAddress"],
                WhisperRpcUri = Configuration["AppSettings:WhisperRpcUri"],
                VaspBic = string.IsNullOrWhiteSpace(Configuration.GetSection("AppSettings:VaspBic").Value)
                    ? null
                    : Configuration.GetSection("AppSettings:VaspBic").Value,
                VaspJuridicalIds = Configuration.GetSection("AppSettings:VaspJuridicalIds").GetChildren().Count() != 0
                    ? Configuration.GetSection("AppSettings:VaspJuridicalIds")
                        .GetChildren()
                        .Select(x =>
                            new JuridicalPersonId(
                                x.GetValue<string>("Id"),
                                Enum.Parse<JuridicalIdentificationType>(x.GetValue<string>("Type")),
                                Country.List[x.GetValue<string>("CountryCode")]))
                        .ToArray()
                    : null,
                VaspNaturalIds = Configuration.GetSection("AppSettings:VaspNaturalIds").GetChildren().Count() != 0
                    ? Configuration.GetSection("AppSettings:VaspNaturalIds")
                        .GetChildren()
                        .Select(x =>
                            new NaturalPersonId(
                                x.GetValue<string>("Id"),
                                Enum.Parse<NaturalIdentificationType>(x.GetValue<string>("Type")),
                                Country.List[x.GetValue<string>("CountryCode")]))
                        .ToArray()
                    : null,
                VaspPlaceOfBirth = string.IsNullOrWhiteSpace(Configuration.GetSection("AppSettings:VaspPlaceOfBirth:Date").Value)
                    ? null
                    : new PlaceOfBirth(
                        DateTime.Parse(Configuration["AppSettings:VaspPlaceOfBirth:Date"]),
                        Configuration["AppSettings:VaspPlaceOfBirth:City"],
                        Country.List[Configuration["AppSettings:VaspPlaceOfBirth:CountryCode"]]),
                AutoConfirmedVaspCodes = Configuration.GetSection("AppSettings:AutoConfirmedVaspCodes").GetChildren().Count() != 0
                    ? Configuration.GetSection("AppSettings:AutoConfirmedVaspCodes")
                        .GetChildren()
                        .Select(x => x.Value)
                        .ToArray()
                    : null,
            };

            if (string.IsNullOrWhiteSpace(_appSettings.VaspBic))
            {
                _appSettings.VaspBic = null;
            }

            if (_appSettings.VaspPlaceOfBirth == null && _appSettings.VaspNaturalIds == null &&
                _appSettings.VaspJuridicalIds == null && _appSettings.VaspBic == null)
            {
                throw new ArgumentException("Invalid configuration.");
            }

            if ((_appSettings.VaspPlaceOfBirth != null || _appSettings.VaspNaturalIds != null) &&
                (_appSettings.VaspJuridicalIds != null || _appSettings.VaspBic != null))
            {
                throw new ArgumentException("Invalid configuration.");
            }

            if (_appSettings.VaspJuridicalIds != null && ( _appSettings.VaspNaturalIds != null ||
                _appSettings.VaspPlaceOfBirth != null || _appSettings.VaspBic != null))
            {
                throw new ArgumentException("Invalid configuration.");
            }

            if (_appSettings.VaspBic != null && ( _appSettings.VaspNaturalIds != null ||
                _appSettings.VaspPlaceOfBirth != null || _appSettings.VaspJuridicalIds != null))
            {
                throw new ArgumentException("Invalid configuration.");
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"OpenVASP Demo ({_appSettings.InstanceName})", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServiceModule(_appSettings));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ApplicationContainer = app.ApplicationServices.GetAutofacRoot();

            app.UseCors(builder => builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(host => true)
                    .AllowCredentials()
                );

            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("");
                }
                else
                {
                    await next.Invoke();
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseStaticFiles();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(a =>
            {
                a.RoutePrefix = "swagger/ui";
                a.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenVASP Demo");
                a.DocumentTitle = $"OpenVASP Demo ({_appSettings.InstanceName})";
            });
        }
    }
}
