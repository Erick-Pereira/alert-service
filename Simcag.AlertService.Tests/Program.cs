using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Infrastructure.Persistence.Repositories;
using Simcag.AlertService.Application.UseCases.GetAlerts;
using Simcag.AlertService.Application.UseCases.GetAlertById;
using Simcag.AlertService.Application.UseCases.MarkAlertAsRead;
using Simcag.AlertService.Application.UseCases.GetAlertStats;

var builder = WebApplication.CreateBuilder(args);

// 🔥 DB
builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// 🔥 Repository
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// 🔥 UseCases
builder.Services.AddScoped<GetAlertsHandler>();
builder.Services.AddScoped<GetAlertByIdHandler>();
builder.Services.AddScoped<MarkAlertAsReadHandler>();
builder.Services.AddScoped<GetAlertStatsHandler>();

builder.Services.AddControllers();

// 🔥 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();