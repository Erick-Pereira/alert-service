<<<<<<< HEAD
﻿using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Infrastructure.Persistence.Repositories;
using Simcag.AlertService.Application.UseCases.GetAlerts;
using Simcag.AlertService.Application.UseCases.GetAlertById;
using Simcag.AlertService.Application.UseCases.MarkAlertAsRead;
using Simcag.AlertService.Application.UseCases.GetAlertStats;

var builder = WebApplication.CreateBuilder(args);

// 🔥 DB
=======
var builder = WebApplication.CreateBuilder(args);

>>>>>>> 23a5c09dab3fb6f834f5f4642538e5640262907f
builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

<<<<<<< HEAD
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
=======
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
>>>>>>> 23a5c09dab3fb6f834f5f4642538e5640262907f
