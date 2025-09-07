using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.data;
using WebApplication2.mappers.userMapper;
using WebApplication2.repositories;
using WebApplication2.repositories.repository;
using WebApplication2.service;
using WebApplication2.settings;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")  
);
builder.Services.AddTransient<EmailService>();

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<MappingProfile>();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
