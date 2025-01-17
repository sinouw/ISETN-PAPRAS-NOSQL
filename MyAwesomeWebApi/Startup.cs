﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MyAwesomeWebApi.Helpers;
using MyAwesomeWebApi.Models.Auth.Identity.Roles;
using MyAwesomeWebApi.Models.Auth.Settings;
using MyAwesomeWebApi.Models.Identity;

namespace MyAwesomeWebApi
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
            BsonClassMap.RegisterClassMap<DepartmentHead>(); // do it before you access DB
            BsonClassMap.RegisterClassMap<Student>(); // do it before you access DB
            BsonClassMap.RegisterClassMap<Teacher>(); // do it before you access DB
            BsonClassMap.RegisterClassMap<Staff>(); // do it before you access DB
            BsonClassMap.RegisterClassMap<Director>(); // do it before you access DB

            //Inject AppSettings
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

            services.Configure<Settings>(options =>
            {
                options.ConnectionString = Configuration.GetSection("MongoConnection:MongoDbDatabase").Value;
                options.Database = Configuration.GetSection("MongoConnection:DatabaseName").Value;
            });

            // Configure Identity MongoDB
            services.AddMongoIdentityProvider<ApplicationUser, ApplicationRole>
            (Configuration.GetConnectionString("MongoDbDatabase"), options =>             {
                //options.Password.RequiredLength = 6;
                //options.Password.RequireLowercase = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireDigit = true;

                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;             });


            // Add Jwt Authentication
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims             services.AddAuthentication(options =>             {
                //Set default Authentication Schema as Bearer                 options.DefaultAuthenticateScheme =
                           JwtBearerDefaults.AuthenticationScheme;                 options.DefaultScheme =
                           JwtBearerDefaults.AuthenticationScheme;                 options.DefaultChallengeScheme =
                           JwtBearerDefaults.AuthenticationScheme;             }).AddJwtBearer(cfg =>             {                 cfg.RequireHttpsMetadata = false;                 cfg.SaveToken = true;                 cfg.TokenValidationParameters =
                       new TokenValidationParameters
                {                     ValidIssuer = Configuration["JwtIssuer"],                     ValidAudience = Configuration["JwtIssuer"],                     IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),                     ClockSkew = TimeSpan.Zero // remove delay of token when expire
                };             });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors();
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

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseCors(builder =>
           //builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString())
           builder.WithOrigins("http://localhost:4200")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()

           );

        }
    }
}
