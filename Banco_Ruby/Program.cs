using BancoCenit.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

WebApplication app = builder.Build();

app.UseApplicationPipeline();
app.MapApplicationEndpoints();

app.Run();
