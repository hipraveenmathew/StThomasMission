using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;

namespace StThomasMission.Infrastructure.Data
{
    public class StThomasMissionDbContext : IdentityDbContext<ApplicationUser>
    {
        public StThomasMissionDbContext(DbContextOptions<StThomasMissionDbContext> options)
            : base(options)
        {
        }

        // Core Entities
        public DbSet<Family> Families => Set<Family>();
        public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Ward> Wards => Set<Ward>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();

        // Activity & Assessment Entities
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<GroupActivity> GroupActivities => Set<GroupActivity>();
        public DbSet<StudentAcademicRecord> StudentAcademicRecords => Set<StudentAcademicRecord>();

        // Supporting & Logging Entities
        public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<CountStorage> CountStorages => Set<CountStorage>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Global Query Filters for Soft Deletes
            // Apply a global filter to automatically exclude soft-deleted records from all queries.
            builder.Entity<Ward>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Family>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<FamilyMember>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Group>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Assessment>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<GroupActivity>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Announcement>().HasQueryFilter(e => !e.IsDeleted);
            #endregion

            #region Entity Configurations

            // ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers"); // Standard table name
                entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
                entity.HasOne(u => u.Ward).WithMany(w => w.Users).HasForeignKey(u => u.WardId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            });

            // Ward
            builder.Entity<Ward>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(w => w.Name).IsUnique();
            });

            // Grade
            builder.Entity<Grade>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(g => g.Name).IsUnique();
                entity.HasIndex(g => g.Order).IsUnique();
            });

            // Group
            builder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(g => g.Name).IsUnique();
                entity.HasMany(g => g.GroupActivities).WithOne(ga => ga.Group).HasForeignKey(ga => ga.GroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // Family
            builder.Entity<Family>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.FamilyName).IsRequired().HasMaxLength(150);
                entity.Property(f => f.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.HasIndex(f => f.ChurchRegistrationNumber).IsUnique().HasFilter($"[{nameof(Family.ChurchRegistrationNumber)}] IS NOT NULL");
                entity.HasIndex(f => f.TemporaryID).IsUnique().HasFilter($"[{nameof(Family.TemporaryID)}] IS NOT NULL");
                entity.HasOne(f => f.Ward).WithMany(w => w.Families).HasForeignKey(f => f.WardId).OnDelete(DeleteBehavior.Restrict);
            });

            // FamilyMember
            builder.Entity<FamilyMember>(entity =>
            {
                entity.HasKey(fm => fm.Id);
                entity.Property(fm => fm.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(fm => fm.LastName).IsRequired().HasMaxLength(100);
                entity.Property(fm => fm.Relation).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.HasIndex(m => m.UserId).IsUnique().HasFilter($"[{nameof(FamilyMember.UserId)}] IS NOT NULL");
                entity.HasOne(m => m.Family).WithMany(f => f.FamilyMembers).HasForeignKey(m => m.FamilyId).OnDelete(DeleteBehavior.Cascade);
            });

            // Student
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.HasOne(s => s.FamilyMember).WithOne(fm => fm.StudentProfile).HasForeignKey<Student>(s => s.FamilyMemberId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.Grade).WithMany(g => g.Students).HasForeignKey(s => s.GradeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Group).WithMany(g => g.Students).HasForeignKey(s => s.GroupId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            });

            // TeacherAssignment
            builder.Entity<TeacherAssignment>(entity =>
            {
                entity.HasKey(ta => new { ta.UserId, ta.GradeId, ta.AcademicYear });
                entity.HasOne(ta => ta.User).WithMany().HasForeignKey(ta => ta.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ta => ta.Grade).WithMany(g => g.TeacherAssignments).HasForeignKey(ta => ta.GradeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ta => ta.Group).WithMany().HasForeignKey(ta => ta.GroupId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            });

            // Assessment
            builder.Entity<Assessment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(150);
                entity.Property(a => a.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
                // Define precision for decimal types to avoid SQL server warnings
                entity.Property(a => a.Marks).HasColumnType("decimal(5, 2)");
                entity.Property(a => a.TotalMarks).HasColumnType("decimal(5, 2)");
                entity.Ignore(a => a.Percentage); // Do not map this calculated property to the DB
                entity.HasOne(a => a.Student).WithMany(s => s.Assessments).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            // Attendance
            builder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.HasOne(a => a.Student).WithMany(s => s.Attendances).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            // StudentAcademicRecord
            builder.Entity<StudentAcademicRecord>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.StudentId, r.AcademicYear, r.GradeId }).IsUnique();
                entity.HasOne(r => r.Student).WithMany(s => s.AcademicRecords).HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.Grade).WithMany().HasForeignKey(r => r.GradeId).OnDelete(DeleteBehavior.Restrict);
            });

            // GroupActivity
            builder.Entity<GroupActivity>(entity =>
            {
                entity.HasKey(ga => ga.Id);
                entity.Property(ga => ga.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            });

            // CountStorage
            builder.Entity<CountStorage>(entity =>
            {
                entity.HasKey(cs => cs.CounterName);
                entity.Property(cs => cs.CounterName).HasMaxLength(100);
            });

            // Enum conversions for other entities
            builder.Entity<MessageLog>().Property(ml => ml.Method).HasConversion<string>().HasMaxLength(50);
            builder.Entity<MessageLog>().Property(ml => ml.MessageType).HasConversion<string>().HasMaxLength(50);

            #endregion
        }
    }
}