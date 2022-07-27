﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configutarion)
        {
            //limpiar el mapeo por defecto de cleim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Configutarion = configutarion;
        }

        public IConfiguration Configutarion { get; }
        public ReferenceHandler ReferenceHa { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
            }).AddJsonOptions(x =>
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configutarion.GetConnectionString("defaultConnection")));


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    //configurar validacion de nuestro token
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //configurar la firma
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configutarion["llavejwt"])),
                    ClockSkew = TimeSpan.Zero

                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiAutores", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

            });

            services.AddAutoMapper(typeof(Startup));
            
            //Configuración servicio Identity
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Configuraciones de autorización

            //configuración basada en claims
            services.AddAuthorization(opciones => {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            });

            //Configurar protección de datos (con esto tenemos acceso a los servicios de protección de datos)
            services.AddDataProtection();

            //Configurar servicio par Hash
            services.AddTransient<HashService>();

            //Configurar servicio CORS (Cross-Origin Resource Sharing o Intercambio de Recursos de Origen Cruzado) 
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    //WithOrigins son las webs que van a poder tener acceso a nuestra webAPI
                    //AllowAnyMethod se refiere a metodos HTTP como get, post, delete, etc
                    //AllowAnyHeader para exponer cabeceras que necesitas devolver desde la webApi (no usamos en este caso)
                    builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod();
                });
            }); 


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseLoguearRespuestaHTTP();

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
                
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiAutores v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
