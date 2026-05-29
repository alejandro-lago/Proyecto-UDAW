using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Adds PostgreSQL Database Context with Identity
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//  This part it's optional. It configures the Identity options (password requirements, etc.)
builder.Services.Configure<IdentityOptions>(options =>
    {
        //  Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        //  Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map my controllers
app.MapControllers();

app.Run();
