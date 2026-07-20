using Microsoft.AspNetCore.Builder;

namespace BancoCenit.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // In development keep default developer exceptions if present; nothing required here.
        }

        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        return app;
    }
}
