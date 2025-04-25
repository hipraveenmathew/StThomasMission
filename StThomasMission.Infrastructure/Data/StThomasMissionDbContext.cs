using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StThomasMission.Infrastructure.Data
{
    public class StThomasMissionDbContext : IdentityDbContext<ApplicationUser>
    {
        public StThomasMissionDbContext(DbContextOptions<StThomasMissionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<GroupActivity> GroupActivities { get; set; }
        public DbSet<StudentGroupActivity> StudentGroupActivities { get; set; }
        public DbSet<MessageLog> MessageLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Ward> Wards { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Global Query Filters
            builder.Entity<Student>()
                .HasQueryFilter(s => s.Status != StudentStatus.Deleted);
            builder.Entity<Family>()
                .HasQueryFilter(f => f.Status != FamilyStatus.Deleted);
            builder.Entity<GroupActivity>()
                .HasQueryFilter(ga => ga.Status != ActivityStatus.Inactive);

            // Ward Configurations
            builder.Entity<Ward>()
                .HasIndex(w => w.Name)
                .IsUnique();
            builder.Entity<Ward>()
                .Property(w => w.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Ward>()
                .HasMany(w => w.Families)
                .WithOne(f => f.Ward)
                .HasForeignKey(f => f.WardId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Ward>()
                .HasMany(w => w.Users)
                .WithOne(u => u.Ward)
                .HasForeignKey(u => u.WardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Family Configurations
            builder.Entity<Family>()
                .HasIndex(f => f.WardId);
            builder.Entity<Family>()
                .Property(f => f.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Family>()
                .HasMany(f => f.Members)
                .WithOne(m => m.Family)
                .HasForeignKey(m => m.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            // FamilyMember Configurations
            builder.Entity<FamilyMember>()
                .HasOne(m => m.StudentProfile)
                .WithOne(s => s.FamilyMember)
                .HasForeignKey<Student>(s => s.FamilyMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student Configurations
            builder.Entity<Student>()
                .HasIndex(s => s.Grade);
            builder.Entity<Student>()
                .HasMany(s => s.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Student>()
                .HasMany(s => s.Assessments)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Student>()
                .HasMany(s => s.GroupActivities)
                .WithOne(sga => sga.Student)
                .HasForeignKey(sga => sga.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupActivity Configurations
            builder.Entity<GroupActivity>()
                .HasMany(ga => ga.Participants)
                .WithOne(sga => sga.GroupActivity)
                .HasForeignKey(sga => sga.GroupActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentGroupActivity Configurations
            builder.Entity<StudentGroupActivity>()
                .HasKey(sga => new { sga.StudentId, sga.GroupActivityId });
            builder.Entity<StudentGroupActivity>()
                .HasIndex(sga => sga.StudentId);
            builder.Entity<StudentGroupActivity>()
                .HasIndex(sga => sga.GroupActivityId);
            builder.Entity<StudentGroupActivity>()
                .Property(sga => sga.ParticipationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // MessageLog Configurations
            builder.Entity<MessageLog>()
                .Property(ml => ml.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<MessageLog>()
                .Property(ml => ml.Status)
                .HasDefaultValue("Sent");

            // AuditLog Configurations
            builder.Entity<AuditLog>()
                .Property(al => al.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            // Property Configurations
            builder.Entity<Family>()
                .Property(f => f.FamilyName)
                .HasMaxLength(150);
            builder.Entity<Ward>()
                .Property(w => w.Name)
                .HasMaxLength(100);
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(150);
        }
    }
}