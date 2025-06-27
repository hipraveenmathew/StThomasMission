using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Services;
using StThomasMission.Web.Models;

// --- 1. BOOTSTRAPPING & CONFIGURATION ---

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. SERVICE REGISTRATION (Dependency Injection) ---

// Add MVC Controllers and Views, including a global exception filter
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddRazorPages();

// Configure the database context
builder.Services.AddDbContext<StThomasMissionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Configure ASP.NET Core Identity with strong password requirements for production
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true if you implement email confirmation

    // Production-ready password policy
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<StThomasMissionDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add Authorization policies if needed in the future
builder.Services.AddAuthorization();

#region Register Application Services and Repositories
// Register the Unit of Work and all repositories/services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register all application services
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
builder.Services.AddScoped<IGroupActivityService, GroupActivityService>();
#endregion

// --- 3. BUILD THE APPLICATION ---

var app = builder.Build();

// --- 4. CONFIGURE HTTP REQUEST PIPELINE (Middleware) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// --- 5. SEED THE DATABASE ---

// Use a scope to get services and seed the database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        // This single call now handles roles, users, and all other essential data
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while seeding the database. Application will terminate.");
        // In a containerized environment, you might want the app to fail fast if seeding fails.
        // For other environments, you might log and continue, depending on how critical seeding is.
        return;
    }
}

// --- 6. CONFIGURE ENDPOINTS & ROUTES ---

// Map area-based routes first
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Map default controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages if you are using them for Identity UI or other features
app.MapRazorPages();

// --- 7. RUN THE APPLICATION ---

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


// --- SUPPORTING CLASSES ---

/// <summary>
/// A custom filter to catch all unhandled exceptions globally, log them,
/// and display a user-friendly error page.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (!context.ExceptionHandled)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred in the application.");

            // Create a generic error model to pass to the view
            var errorModel = new ErrorViewModel
            {
                // In production, you might not want to expose the real message.
                // For dev, this can be useful.
                Message = context.Exception.Message,
                // You can add a unique request ID for correlation
                RequestId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
            };

            var result = new ViewResult
            {
                ViewName = "Error", // Points to /Views/Shared/Error.cshtml
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                    context.ModelState)
                {
                    Model = errorModel
                }
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
