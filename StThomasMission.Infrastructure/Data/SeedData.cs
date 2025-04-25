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
                // Seed HeadTeacher User
                var headTeacherEmail = "headteacher@stthomasmission.com";
                var headTeacherPassword = "HeadTeacher@123";
                var headTeacherUser = new IdentityUser { UserName = headTeacherEmail, Email = headTeacherEmail };

                if (await userManager.FindByEmailAsync(headTeacherEmail) == null)
                {
                    await userManager.CreateAsync(headTeacherUser, headTeacherPassword);
                    await userManager.AddToRoleAsync(headTeacherUser, "HeadTeacher");
                }

                // Seed ParishAdmin User
                var parishAdminEmail = "parishadmin@stthomasmission.com";
                var parishAdminPassword = "ParishAdmin@123";
                var parishAdminUser = new IdentityUser { UserName = parishAdminEmail, Email = parishAdminEmail };

                if (await userManager.FindByEmailAsync(parishAdminEmail) == null)
                {
                    await userManager.CreateAsync(parishAdminUser, parishAdminPassword);
                    await userManager.AddToRoleAsync(parishAdminUser, "ParishAdmin");
                }

                // Seed 12 Teachers (Year 1 to Year 12)
                for (int year = 1; year <= 12; year++)
                {
                    string grade = $"Year {year}";
                    string group = $"Group {year}";
                    string teacherEmail = $"teacher{year}@stthomasmission.com";
                    string teacherPassword = $"Teacher@{year}";

                    var teacherUser = new IdentityUser { UserName = teacherEmail, Email = teacherEmail };

                    if (await userManager.FindByEmailAsync(teacherEmail) == null)
                    {
                        await userManager.CreateAsync(teacherUser, teacherPassword);
                        await userManager.AddToRoleAsync(teacherUser, "Teacher");

                        // Add Family for Teacher
                        var teacherFamily = new Family
                        {
                            FamilyName = $"Teacher {year} Family",
                            Ward = "Teachers Ward",
                            IsRegistered = false,
                            TemporaryID = $"TMP-{1000 + year}",
                            Status = Core.Enums.FamilyStatus.Active,
                            CreatedDate = DateTime.UtcNow
                        };
                        context.Families.Add(teacherFamily);
                        await context.SaveChangesAsync();

                        // Add FamilyMember (Teacher)
                        var teacherMember = new FamilyMember
                        {
                            FamilyId = teacherFamily.Id,
                            FirstName = $"Teacher{year}",
                            LastName = "Mission",
                            Email = teacherEmail,
                            Role = "Teacher",
                            UserId = teacherUser.Id
                        };
                        context.FamilyMembers.Add(teacherMember);
                        await context.SaveChangesAsync();

                        // Optionally create a default GroupActivity entry for the Group
                        if (!context.GroupActivities.Any(ga => ga.Group == group))
                        {
                            context.GroupActivities.Add(new GroupActivity
                            {
                                Group = group,
                                Name = "Initial Group Setup",
                                Points = 0,
                                Date = DateTime.Today
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                }

            }
        }
    }
}
