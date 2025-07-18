using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Data
{
    /// <summary>
    /// Handles the initial seeding of database with essential data.
    /// This process is idempotent and can be run safely multiple times.
    /// </summary>
    public static class SeedData
    {
        private const string SystemUser = "System";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StThomasMissionDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<StThomasMissionDbContext>>();

            try
            {
                // In a production environment, you would typically use migrations.
                // EnsureCreated is useful for development and testing.
                await context.Database.MigrateAsync();

                await SeedRolesAsync(roleManager, logger);
                await SeedLookupDataAsync(context, logger);
                await SeedCountersAsync(context, logger);
                await SeedProductionUsersAsync(context, userManager, configuration, logger);
                await SeedProductionTeachersAsync(context, userManager, configuration, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding.");
                // Depending on the policy, you might want to re-throw the exception
                // to halt application startup if the seed data is critical.
            }
        }

        /// <summary>
        /// Seeds the user roles from the UserRoles constants class.
        /// </summary>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roles = typeof(UserRoles).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation("Role '{RoleName}' created.", roleName);
                }
            }
        }

        /// <summary>
        /// Seeds essential lookup tables like Wards, Groups, and Grades.
        /// </summary>
        private static async Task SeedLookupDataAsync(StThomasMissionDbContext context, ILogger logger)
        {
            if (!await context.Wards.AnyAsync())
            {
                context.Wards.AddRange(
                    new Ward { Name = "St. John Ward", CreatedBy = SystemUser },
                    new Ward { Name = "St. Matthew Ward", CreatedBy = SystemUser },
                    new Ward { Name = "Unassigned", CreatedBy = SystemUser }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default Wards.");
            }

            if (!await context.Groups.AnyAsync())
            {
                context.Groups.AddRange(
                    new Group { Name = "St. Peter", CreatedBy = SystemUser, Description = "Students from Year 1 to Year 6" },
                    new Group { Name = "St. Paul", CreatedBy = SystemUser, Description = "Students from Year 7 to Year 12" }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default Groups.");
            }

            if (!await context.Grades.AnyAsync())
            {
                for (int i = 1; i <= 12; i++)
                {
                    context.Grades.Add(new Grade { Name = $"Year {i}", Order = i });
                }
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default Grades (Year 1-12).");
            }
        }

        /// <summary>
        /// Seeds the initial values for unique number generators.
        /// </summary>
        private static async Task SeedCountersAsync(StThomasMissionDbContext context, ILogger logger)
        {
            string churchRegCounterName = "ChurchRegistrationNumber";
            if (await context.CountStorages.FindAsync(churchRegCounterName) == null)
            {
                context.CountStorages.Add(new CountStorage { CounterName = churchRegCounterName, LastValue = 20250000 });
                logger.LogInformation("Initialized '{CounterName}' counter.", churchRegCounterName);
            }

            string tempIdCounterName = "TemporaryID";
            if (await context.CountStorages.FindAsync(tempIdCounterName) == null)
            {
                context.CountStorages.Add(new CountStorage { CounterName = tempIdCounterName, LastValue = 1000 });
                logger.LogInformation("Initialized '{CounterName}' counter.", tempIdCounterName);
            }
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds the main administrative users from configuration.
        /// </summary>
        private static async Task SeedProductionUsersAsync(StThomasMissionDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
        {
            var defaultWard = await context.Wards.FirstOrDefaultAsync(w => w.Name == "Unassigned");
            if (defaultWard == null)
            {
                logger.LogError("Seeding failed: 'Unassigned' ward not found. Cannot create default users.");
                return;
            }

            var userDefinitions = new[]
            {
                new { ConfigKey = "Admin", Role = UserRoles.Admin, FullName = "System Administrator" },
                new { ConfigKey = "ParishPriest", Role = UserRoles.ParishPriest, FullName = "Parish Priest" },
                new { ConfigKey = "HeadTeacher", Role = UserRoles.HeadTeacher, FullName = "Head Teacher" },
                new { ConfigKey = "ParishAdmin", Role = UserRoles.ParishAdmin, FullName = "Parish Administrator" }
            };

            foreach (var def in userDefinitions)
            {
                var email = configuration[$"ProductionUsers:{def.ConfigKey}:Email"];
                var password = configuration[$"ProductionUsers:{def.ConfigKey}:Password"];

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) continue;

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = def.FullName, WardId = defaultWard.Id, IsActive = true };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, def.Role);
                        logger.LogInformation("Created and assigned role for user '{Email}'.", email);
                    }
                }
            }
        }

        /// <summary>
        /// Seeds the 12 teacher accounts and their grade assignments from configuration.
        /// </summary>
        private static async Task SeedProductionTeachersAsync(StThomasMissionDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
        {
            var teacherEmailFormat = configuration["ProductionUsers:Teacher:EmailFormat"];
            var teacherPassword = configuration["ProductionUsers:Teacher:Password"];

            if (string.IsNullOrEmpty(teacherEmailFormat) || string.IsNullOrEmpty(teacherPassword)) return;

            var defaultWard = await context.Wards.FirstOrDefaultAsync(w => w.Name == "Unassigned");
            if (defaultWard == null)
            {
                logger.LogError("Seeding failed: 'Unassigned' ward not found. Cannot create teacher users.");
                return;
            }

            for (int year = 1; year <= 12; year++)
            {
                var email = teacherEmailFormat.Replace("{year}", year.ToString("D2"));
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var teacherUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = $"Teacher Year {year}",
                        WardId = defaultWard.Id,
                        IsActive = true,
                        Designation = "Catechism Teacher"
                    };

                    var grade = await context.Grades.SingleOrDefaultAsync(g => g.Order == year);
                    if (grade == null)
                    {
                        logger.LogWarning("Could not find Grade with Order {Year} to assign teacher.", year);
                        continue;
                    }

                    using var transaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        var result = await userManager.CreateAsync(teacherUser, teacherPassword);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(teacherUser, UserRoles.Teacher);

                            var assignment = new TeacherAssignment
                            {
                                UserId = teacherUser.Id,
                                GradeId = grade.Id,
                                AcademicYear = DateTime.UtcNow.Year
                            };
                            context.TeacherAssignments.Add(assignment);
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            logger.LogInformation("Created and assigned teacher for Year {Year}.", year);
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            logger.LogWarning("Failed to create teacher for Year {Year}.", year);
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        logger.LogError(ex, "Error creating teacher for Year {Year}.", year);
                    }
                }
            }
        }
    }
}