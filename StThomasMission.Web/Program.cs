using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Repositories;
using StThomasMission.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Data;
using StThomasMission.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(GlobalExceptionFilter));
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5002); // Allows binding to your network IP
});


// Add Razor Pages support
builder.Services.AddRazorPages(); // Added to enable Razor Pages services

// Add DbContext with SQL Server
builder.Services.AddDbContext<StThomasMissionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<StThomasMissionDbContext>()
.AddDefaultTokenProviders();

// Add Authorization
builder.Services.AddAuthorization();

// Register Repositories and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IFamilyMemberService, FamilyMemberService>();
builder.Services.AddScoped<ICatechismService, CatechismService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IWardService, WardService>();
builder.Services.AddScoped<IMassTimingService, MassTimingService>(); 
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IMigrationLogService, MigrationLogService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed Roles & Data
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedRolesAsync(scope.ServiceProvider);
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database.");
        throw;
    }
}

// Route Configuration
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Use(async (context, next) =>
{
    Console.WriteLine($"Path: {context.Request.Path}, User: {context.User.Identity?.Name}");
    await next();
});


app.Run();

// Seed Roles Method
static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "ParishPriest", "ParishAdmin", "HeadTeacher", "Teacher", "Admin", "Parent" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                Log.Error("Failed to create role {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                Log.Information("Created role: {Role}", role);
            }
        }
    }
}

// Global Exception Filter
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception occurred.");
        context.Result = new ViewResult
        {
            ViewName = "Error",
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                context.ModelState)
            {
                Model = new ErrorViewModel { Message = "An unexpected error occurred. Please try again later." }
            }
        };
        context.ExceptionHandled = true;
    }
}