using CDRProcessingAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models; // for the auth
using Serilog;

using System.Reflection; // Added for XML comments
using System.IO; // Added for XML comments



var builder = WebApplication.CreateBuilder(args);






// Add Logging Services (Replace the placeholder with your specific configuration)
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .MinimumLevel.Debug() // Adjust logging level as needed (e.g., Information, Warning)
        .Enrich.FromLogContext() // Optionally include additional logging context information
        .WriteTo.Console() // Write logs to the console
        .WriteTo.File("cdrprocessingapi.log", rollingInterval: RollingInterval.Day) // Write rolling logs to a file
        .CreateLogger());
});

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<CDRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); // Add SwaggerGen service

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CDR Processing API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Add it in the 'x-api-key' header.",
        In = ParameterLocation.Header,
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                Scheme = "ApiKeyScheme",
                Name = "x-api-key",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDR API V1"));
}

app.UseMiddleware<ApiKeyMiddleware>();  // this is for implimenting the middle ware.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();