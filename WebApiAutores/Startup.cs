using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configutarion)
        {
            Configutarion = configutarion;
        }

        public IConfiguration Configutarion { get; }
        public ReferenceHandler ReferenceHa { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddJsonOptions(x => 
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configutarion.GetConnectionString("defaultConnection")));

            services.AddTransient<IServicio, ServicioA>();

            services.AddTransient<ServicioTransient>();
            services.AddScoped<ServicioScoped>();
            services.AddSingleton<ServicioSingleton>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            //app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
            app.UseLoguearRespuestaHTTP();

            app.Map("/ruta1", app =>
            {
                app.Run(async contexto =>
                {
                    await contexto.Response.WriteAsync("Estoy interceptando la tubería");
                });
            });

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();   

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
