using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContentService;
using System.Net.Http;
using Microsoft.IdentityModel.Logging;

namespace APIGateway
{
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
            IdentityModelEventSource.ShowPII = true;
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIGateway", Version = "v1" });
            });
            services.AddGrpcClient<Content.ContentClient>(o =>
            {
                o.Address = new Uri(Configuration["Services:ContentService:Address"]);
            }).ConfigurePrimaryHttpMessageHandler(() => 
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                return handler;
            });
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            if (!string.IsNullOrWhiteSpace(Configuration["Services:AuthService:Address"]))
            {
                services.AddAuthentication("Bearer")
                    .AddJwtBearer(options =>
                    {
                        options.Authority = Configuration["Services:AuthService:Address"];
                        options.RequireHttpsMetadata = false;

                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateAudience = false
                        };
                    });
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("ApiScope", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("scope", "api-gateway");
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIGateway v1"));
            }

            app.UseHttpsRedirection();
            app.UseCors("default");

            app.UseRouting();


            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
