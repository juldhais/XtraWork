using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;
using XtraWork.Repositories;
using XtraWork.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("XtraWork");
builder.Services.AddDbContext<XtraWorkContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<TitleRepository>();
builder.Services.AddScoped<EmployeeRepository>();

builder.Services.AddScoped<TitleService>();
builder.Services.AddScoped<EmployeeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<XtraWorkContext>();
context.Database.EnsureCreated();

// global error handler
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception != null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = exception.GetBaseException().Message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
