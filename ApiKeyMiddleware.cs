using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next; //Holds reference to the next middleware in the pipeline
    private const string API_KEY_HEADER = "x-api-key"; //Constant defining the expected header name for the API key

    public ApiKeyMiddleware(RequestDelegate next) //accepts and stores the next middleware delegate
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    { /*
       checks If the API key header is missing (TryGetValue returns false)
        OR if the provided key doesn't match the configured key
       */
        // Uses pattern matching with out var apiKey to get the header value
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKey) ||
            apiKey != config["ApiSettings:ApiKey"])
        {
            context.Response.StatusCode = 401; // Unauthorized err when fails
            await context.Response.WriteAsync("Unauthorized: API Key is missing or invalid.");
            return; //Returns immediately, stopping the request pipeline
        }

        await _next(context); //If validation succeeds continue
    }
}
