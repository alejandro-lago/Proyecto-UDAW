using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;
using TfgApi.Repositories;
using TfgApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Adds PostgreSQL Database Context with Identity
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

//  I register my services so ASP.NET can inject them into the controllers
//  AddHttpClient creates a HttpClient and inject it into the services
//  AddScoped creates one instance per HTTP request
builder.Services.AddHttpClient<IExerciseApiService, ExerciseApiService>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();

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
