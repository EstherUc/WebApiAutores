using WebApiAutores;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

// Add services to the container.
// Paso el código "Add services to the container" a la Clase Startup.cs al método ConfigureServices


var app = builder.Build();

var servicioLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment, servicioLogger
    );

// Paso el código de "Configure the HTTP request pipeline" a la Clase Startup.cs al método Configure

app.Run();
