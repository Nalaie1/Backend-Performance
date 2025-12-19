using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.SeedData;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Server=nalaie\\MSSQLSERVER2022;Database=ConsoleApp2Db;Trusted_Connection=True;TrustServerCertificate=True;",
        sqlOptions => sqlOptions.CommandTimeout(600)));

// services.AddScoped<ICommentRepository, CommentRepository>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.SeedAsync(context);
}        