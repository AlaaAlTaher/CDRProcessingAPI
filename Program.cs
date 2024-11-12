using CDRProcessingAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models; // for the auth
using Serilog;
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

builder.Services.AddSwaggerGen(options => // Add SwaggerGen service and the auth ui
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CDR Processing API", Version = "v1" });

    // Define API Key Authorization for Swagger
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme // Defines the API key as a required header parameter in Swagger
    {
        Description = "API Key needed to access the endpoints. API Key must be in the header with the key 'x-api-key'.",
        In = ParameterLocation.Header,
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // Add a global requirement for the API key in Swagger
    options.AddSecurityRequirement(new OpenApiSecurityRequirement // Ensures that the API key is required for each request in the Swagger UI.
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