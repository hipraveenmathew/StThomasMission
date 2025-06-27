using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StThomasMission.Core.Entities;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StThomasMissionDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await context.Database.EnsureCreatedAsync();

            // 1. Seed Essential Roles
            string[] roles = { "Admin", "ParishPriest", "ParishAdmin", "HeadTeacher", "Teacher", "Parent" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Essential Lookup Tables
            if (!await context.Wards.AnyAsync())
            {
                context.Wards.AddRange(
                    new Ward { Name = "St. John Ward", CreatedBy = "System" },
                    new Ward { Name = "St. Matthew Ward", CreatedBy = "System" },
                    new Ward { Name = "Unassigned", CreatedBy = "System" }
                );
                await context.SaveChangesAsync();
            }
            if (!await context.Groups.AnyAsync())
            {
                context.Groups.AddRange(
                    new Group { Name = "St. Peter", CreatedBy = "System" },
                    new Group { Name = "St. Paul", CreatedBy = "System" }
                );
                await context.SaveChangesAsync();
            }
            if (!await context.Grades.AnyAsync())
            {
                for (int i = 1; i <= 12; i++)
                {
                    context.Grades.Add(new Grade { Name = $"Year {i}", Order = i });
                }
                await context.SaveChangesAsync();
            }

            // 3. Seed Key Users from Secure Configuration
            var defaultWard = await context.Wards.FirstAsync(w => w.Name == "Unassigned");

            // Helper function to create a user
            async Task CreateUserAsync(string configKey, string roleName, string defaultFullName)
            {
                var email = configuration[$"ProductionUsers:{configKey}:Email"];
                var password = configuration[$"ProductionUsers:{configKey}:Password"];

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return; // Skip if not configured

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = defaultFullName, WardId = defaultWard.Id, IsActive = true };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }

            // Create Admin, Priest, HeadTeacher, ParishAdmin
            await CreateUserAsync("Admin", "Admin", "System Administrator");
            await CreateUserAsync("ParishPriest", "ParishPriest", "Parish Priest");
            await CreateUserAsync("HeadTeacher", "HeadTeacher", "Head Teacher");
            await CreateUserAsync("ParishAdmin", "ParishAdmin", "Parish Administrator");

            // 4. Seed 12 Teachers from Secure Configuration
            var teacherEmailFormat = configuration["ProductionUsers:Teacher:EmailFormat"];
            var teacherPasswordFormat = configuration["ProductionUsers:Teacher:PasswordFormat"];

            if (!string.IsNullOrEmpty(teacherEmailFormat) && !string.IsNullOrEmpty(teacherPasswordFormat))
            {
                var stPeterGroup = await context.Groups.FirstAsync(g => g.Name == "St. Peter");
                var stPaulGroup = await context.Groups.FirstAsync(g => g.Name == "St. Paul");

                for (int year = 1; year <= 12; year++)
                {
                    var email = teacherEmailFormat.Replace("{year}", year.ToString());
                    var password = teacherPasswordFormat.Replace("{year}", year.ToString());

                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var teacherUser = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = $"Teacher Year {year}", WardId = defaultWard.Id, IsActive = true, Designation = "Catechism Teacher" };
                        var result = await userManager.CreateAsync(teacherUser, password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(teacherUser, "Teacher");

                            // Create the assignment linking the teacher to the grade
                            var currentGrade = await context.Grades.SingleAsync(g => g.Order == year);
                            var currentGroup = year <= 6 ? stPeterGroup : stPaulGroup;
                            var assignment = new TeacherAssignment
                            {
                                UserId = teacherUser.Id,
                                GradeId = currentGrade.Id,
                                GroupId = currentGroup.Id,
                                AcademicYear = DateTime.UtcNow.Year
                            };
                            context.TeacherAssignments.Add(assignment);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}