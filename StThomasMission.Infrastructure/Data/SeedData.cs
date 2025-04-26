using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<StThomasMissionDbContext>();

            // Seed Roles
            string[] roles = new[] { UserRole.Admin, UserRole.ParishPriest, UserRole.ParishAdmin, UserRole.HeadTeacher, UserRole.Teacher, UserRole.Parent };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Wards
            if (!await context.Wards.AnyAsync())
            {
                context.Wards.AddRange(
                    new Ward { Name = "St. John Ward", CreatedDate = DateTime.UtcNow, CreatedBy = "System" },
                    new Ward { Name = "Teachers Ward", CreatedDate = DateTime.UtcNow, CreatedBy = "System" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Groups
            if (!await context.Groups.AnyAsync())
            {
                context.Groups.AddRange(
                    new Group { Name = "St. Peter", Description = "Catechism Group for Year 1-6", CreatedDate = DateTime.UtcNow, CreatedBy = "System" },
                    new Group { Name = "St. Paul", Description = "Catechism Group for Year 7-12", CreatedDate = DateTime.UtcNow, CreatedBy = "System" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            var adminUser = new ApplicationUser
            {
                UserName = "admin@stthomasmission.com",
                Email = "admin@stthomasmission.com",
                FullName = "System Admin",
                WardId = 1, // St. John Ward
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            string adminPassword = "Admin@123";
            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
            }

            // Seed Parish Priest User
            var priestUser = new ApplicationUser
            {
                UserName = "priest@stthomasmission.com",
                Email = "priest@stthomasmission.com",
                FullName = "Father John",
                WardId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            string priestPassword = "Priest@123";
            if (await userManager.FindByEmailAsync(priestUser.Email) == null)
            {
                await userManager.CreateAsync(priestUser, priestPassword);
                await userManager.AddToRoleAsync(priestUser, UserRole.ParishPriest);
            }

            // Seed HeadTeacher User
            var headTeacherUser = new ApplicationUser
            {
                UserName = "headteacher@stthomasmission.com",
                Email = "headteacher@stthomasmission.com",
                FullName = "Head Teacher",
                WardId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Designation = "Head Teacher"
            };
            string headTeacherPassword = "HeadTeacher@123";
            if (await userManager.FindByEmailAsync(headTeacherUser.Email) == null)
            {
                await userManager.CreateAsync(headTeacherUser, headTeacherPassword);
                await userManager.AddToRoleAsync(headTeacherUser, UserRole.HeadTeacher);
            }

            // Seed ParishAdmin User
            var parishAdminUser = new ApplicationUser
            {
                UserName = "parishadmin@stthomasmission.com",
                Email = "parishadmin@stthomasmission.com",
                FullName = "Parish Admin",
                WardId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Designation = "Parish Administrator"
            };
            string parishAdminPassword = "ParishAdmin@123";
            if (await userManager.FindByEmailAsync(parishAdminUser.Email) == null)
            {
                await userManager.CreateAsync(parishAdminUser, parishAdminPassword);
                await userManager.AddToRoleAsync(parishAdminUser, UserRole.ParishAdmin);
            }

            // Seed Sample Family, Parent, and Student
            if (!await context.Families.AnyAsync())
            {
                var family = new Family
                {
                    FamilyName = "Smith Family",
                    WardId = 1, // St. John Ward
                    IsRegistered = true,
                    ChurchRegistrationNumber = "108020001",
                    Status = FamilyStatus.Active,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Families.Add(family);
                await context.SaveChangesAsync();

                var parentUser = new ApplicationUser
                {
                    UserName = "parent@stthomasmission.com",
                    Email = "parent@stthomasmission.com",
                    FullName = "John Smith",
                    WardId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                string parentPassword = "Parent@123";
                if (await userManager.FindByEmailAsync(parentUser.Email) == null)
                {
                    await userManager.CreateAsync(parentUser, parentPassword);
                    await userManager.AddToRoleAsync(parentUser, UserRole.Parent);
                }

                var familyMember = new FamilyMember
                {
                    FamilyId = family.Id,
                    UserId = parentUser.Id,
                    FirstName = "John",
                    LastName = "Smith",
                    Relation = FamilyMemberRole.Parent,
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Contact = "+1234567890",
                    Email = "parent@stthomasmission.com",
                    Role = "Parent",
                    CreatedBy = "System"
                };
                context.FamilyMembers.Add(familyMember);
                await context.SaveChangesAsync();

                var student = new Student
                {
                    FamilyMemberId = familyMember.Id,
                    GroupId = 1, // St. Peter
                    AcademicYear = 2025,
                    Grade = "Year 1",
                    Status = StudentStatus.Active,
                    CreatedBy = "System"
                };
                context.Students.Add(student);
                await context.SaveChangesAsync();

                // Seed MigrationLog
                context.MigrationLogs.Add(new MigrationLog
                {
                    FamilyId = family.Id,
                    MigratedTo = "St. Mary Parish",
                    MigrationDate = DateTime.UtcNow,
                    CreatedBy = "System"
                });
                await context.SaveChangesAsync();
            }

            // Seed Group Activities
            if (!await context.GroupActivities.AnyAsync())
            {
                context.GroupActivities.AddRange(
                    new GroupActivity
                    {
                        GroupId = 1, // St. Peter
                        Name = "Charity Event",
                        Description = "Annual charity fundraiser",
                        Points = 10,
                        Date = DateTime.Today.AddDays(5),
                        Status = ActivityStatus.Active,
                        CreatedBy = "System"
                    },
                    new GroupActivity
                    {
                        GroupId = 1, // St. Peter
                        Name = "Community Prayer",
                        Description = "Weekly prayer meeting",
                        Points = 5,
                        Date = DateTime.Today.AddDays(15),
                        Status = ActivityStatus.Active,
                        CreatedBy = "System"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Mass Timings
            if (!await context.MassTimings.AnyAsync())
            {
                context.MassTimings.AddRange(
                    new MassTiming
                    {
                        Day = "Sunday",
                        Time = new TimeSpan(8, 0, 0),
                        Location = "Main Church",
                        Type = MassType.Regular,
                        WeekStartDate = DateTime.Today,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    },
                    new MassTiming
                    {
                        Day = "Sunday",
                        Time = new TimeSpan(10, 0, 0),
                        Location = "Main Church",
                        Type = MassType.Regular,
                        WeekStartDate = DateTime.Today,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Announcements
            if (!await context.Announcements.AnyAsync())
            {
                context.Announcements.AddRange(
                    new Announcement
                    {
                        Title = "Welcome to St. Thomas Mission",
                        Description = "Join us for our annual parish festival next month!",
                        PostedDate = DateTime.Today,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    },
                    new Announcement
                    {
                        Title = "Catechism Registration",
                        Description = "Registration for 2025 catechism classes is now open.",
                        PostedDate = DateTime.Today,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Teachers (Year 1 to 12)
            for (int year = 1; year <= 12; year++)
            {
                string grade = $"Year {year}";
                int groupId = year <= 6 ? 1 : 2; // St. Peter for Year 1-6, St. Paul for Year 7-12
                string teacherEmail = $"teacher{year}@stthomasmission.com";
                string teacherPassword = $"Teacher@{year}";

                var teacherUser = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FullName = $"Teacher Year {year}",
                    WardId = 2, // Teachers Ward
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Designation = "Catechism Teacher"
                };

                if (await userManager.FindByEmailAsync(teacherEmail) == null)
                {
                    await userManager.CreateAsync(teacherUser, teacherPassword);
                    await userManager.AddToRoleAsync(teacherUser, UserRole.Teacher);

                    // Add Family for Teacher
                    var teacherFamily = new Family
                    {
                        FamilyName = $"Teacher {year} Family",
                        WardId = 2, // Teachers Ward
                        IsRegistered = false,
                        TemporaryID = $"TMP-{1000 + year}",
                        Status = FamilyStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    context.Families.Add(teacherFamily);
                    await context.SaveChangesAsync();

                    // Add FamilyMember (Teacher)
                    var teacherMember = new FamilyMember
                    {
                        FamilyId = teacherFamily.Id,
                        UserId = teacherUser.Id,
                        FirstName = $"Teacher{year}",
                        LastName = "Mission",
                        Relation = FamilyMemberRole.Other,
                        DateOfBirth = new DateTime(1980, 1, 1),
                        Email = teacherEmail,
                        Role = "Teacher",
                        CreatedBy = "System"
                    };
                    context.FamilyMembers.Add(teacherMember);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}