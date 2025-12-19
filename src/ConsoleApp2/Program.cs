using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=nalaie\\MSSQLSERVER2022;Database=ConsoleApp2Db;Trusted_Connection=True;TrustServerCertificate=True;"));

services.AddScoped<ICommentRepository, CommentRepository>();

var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var repo = scope.ServiceProvider.GetRequiredService<ICommentRepository>();