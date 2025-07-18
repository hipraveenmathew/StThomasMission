using StThomasMission.Core.Interfaces;
using StThomasMission.Core.Models.Settings;
using StThomasMission.Infrastructure;
using StThomasMission.Infrastructure.Repositories;
using StThomasMission.Services.Interfaces;
using StThomasMission.Services.Messaging;
using StThomasMission.Services.Reporting;
using StThomasMission.Services.Services;

namespace StThomasMission.Web
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Framework Services
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            // Register strongly-typed configuration options
            services.Configure<TwilioSettings>(configuration.GetSection("Twilio"));
            services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));

            // Register the Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register all application services
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<ICatechismService, CatechismService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IFamilyService, FamilyService>();
            services.AddScoped<IFamilyMemberService, FamilyMemberService>();
            services.AddScoped<IFamilyRegistrationService, FamilyRegistrationService>();
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IGroupActivityService, GroupActivityService>();
            services.AddScoped<IMassTimingService, MassTimingService>();
            services.AddScoped<IMigrationLogService, MigrationLogService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWardService, WardService>();

            // Register abstracted external service senders
            services.AddTransient<IEmailSender, SendGridEmailSender>();
            services.AddTransient<ISmsSender, TwilioSmsSender>();

            return services;
        }
    }
}