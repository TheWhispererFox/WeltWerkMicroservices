// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using System;
using System.Net.Http;

namespace AuthService
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment environment, IConfiguration config)
        {
            Environment = environment;
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();


            //TODO: Add Grpc Database to services for DI
            services.AddGrpcClient<DatabaseManagementService.DatabaseManagement.DatabaseManagementClient>(o =>
            {
                o.Address = new Uri("http://host.docker.internal:5005");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                return handler;
            });

            services.AddIdentityCore<DatabaseProvider.WeltWerkUser>()
                .AddUserStore<DatabaseProvider.WeltWerkUserStore>();

            //services.AddTransient<IUserStore<WeltWerkUser>

            IdentityModelEventSource.ShowPII = true;

            var builder = services.AddIdentityServer(options =>
            {
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.IssuerUri = "http://localhost:5009";
            })
                //.AddAspNetIdentity<DatabaseProvider.WeltWerkUser>()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users);
            builder.Services.AddGrpcClient<DatabaseManagementService.DatabaseManagement.DatabaseManagementClient>();
            //builder.AddProfileService<DatabaseProvider.DatabaseProfileService>();
            //builder.AddResourceOwnerValidator<DatabaseProvider.DatabaseResourceOwnerPasswordValidator>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
            services.AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
                options.DefaultChallengeScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
            })
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "548228761022-6i2it3fkdkfpll42s00ct7vn7ca7krk3.apps.googleusercontent.com";
                    options.ClientSecret = "gB0VPOM0AmtEyqCpeTCv7Ipm";
                });

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                    policy.WithOrigins("http://localhost:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("default");

            app.UseIdentityServer();


            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
