using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StThomasMission.Core.Entities;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = scope.ServiceProvider.GetRequiredService<StThomasMissionDbContext>();

            // Seed Roles
            string[] roles = new[] { "Admin", "ParishPriest", "ParishAdmin", "HeadTeacher", "Teacher", "Parent" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminUser = new IdentityUser { UserName = "admin@stthomasmission.com", Email = "admin@stthomasmission.com" };
            string adminPassword = "Admin@123";
            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Parish Priest User
            var priestUser = new IdentityUser { UserName = "priest@stthomasmission.com", Email = "priest@stthomasmission.com" };
            string priestPassword = "Priest@123";
            if (await userManager.FindByEmailAsync(priestUser.Email) == null)
            {
                await userManager.CreateAsync(priestUser, priestPassword);
                await userManager.AddToRoleAsync(priestUser, "ParishPriest");
            }

            // Seed Sample Family and Parent
            if (!context.Families.Any())
            {
                var family = new Family
                {
                    FamilyName = "Smith Family",
                    Ward = "St. John Ward",
                    IsRegistered = true,
                    ChurchRegistrationNumber = "108020001",
                    Status = Core.Enums.FamilyStatus.Active,
                    CreatedDate = DateTime.UtcNow
                };
                context.Families.Add(family);
                await context.SaveChangesAsync();

                var familyMember = new FamilyMember
                {
                    FamilyId = family.Id,
                    FirstName = "John",
                    LastName = "Smith",
                    Contact = "+1234567890",
                    Email = "parent@stthomasmission.com",
                    Role = "Parent"
                };
                context.FamilyMembers.Add(familyMember);
                await context.SaveChangesAsync();

                var parentUser = new IdentityUser { UserName = "parent@stthomasmission.com", Email = "parent@stthomasmission.com" };
                string parentPassword = "Parent@123";
                if (await userManager.FindByEmailAsync(parentUser.Email) == null)
                {
                    await userManager.CreateAsync(parentUser, parentPassword);
                    await userManager.AddToRoleAsync(parentUser, "Parent");

                    familyMember.UserId = parentUser.Id;
                    context.FamilyMembers.Update(familyMember);
                    await context.SaveChangesAsync();
                }

                var student = new Student
                {
                    FamilyMemberId = familyMember.Id,
                    Grade = "Year 1",
                    AcademicYear = 2025,
                    Group = "St. Peter Group",
                    Status = Core.Enums.StudentStatus.Active
                };
                context.Students.Add(student);
                await context.SaveChangesAsync();

                if (!context.GroupActivities.Any())
                {
                    context.GroupActivities.AddRange(
                        new GroupActivity
                        {
                            Group = "St. Peter Group",
                            Name = "Charity Event",
                            Points = 10,
                            Date = DateTime.Today.AddDays(5),
                            Status = Core.Enums.ActivityStatus.Active
                        },
                        new GroupActivity
                        {
                            Group = "St. Peter Group",
                            Name = "Community Prayer",
                            Points = 5,
                            Date = DateTime.Today.AddDays(15),
                            Status = Core.Enums.ActivityStatus.Active
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
