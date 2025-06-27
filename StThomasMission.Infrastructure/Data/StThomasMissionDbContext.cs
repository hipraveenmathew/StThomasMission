using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;

namespace StThomasMission.Infrastructure.Data
{
    public class StThomasMissionDbContext : IdentityDbContext<ApplicationUser>
    {
        public StThomasMissionDbContext(DbContextOptions<StThomasMissionDbContext> options)
            : base(options)
        {
        }

        // Core Entities
        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }

        // Activity & Assessment Entities
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<GroupActivity> GroupActivities { get; set; }
        public DbSet<StudentGroupActivity> StudentGroupActivities { get; set; }
        public DbSet<StudentAcademicRecord> StudentAcademicRecords { get; set; }
        public DbSet<AssessmentSummary> AssessmentSummaries { get; set; }

        // Supporting & Logging Entities
        public DbSet<MessageLog> MessageLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MassTiming> MassTimings { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<MigrationLog> MigrationLogs { get; set; }
        public DbSet<CountStorage> CountStorages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Global Query Filters
            builder.Entity<Student>().HasQueryFilter(s => s.Status != StudentStatus.Deleted);
            builder.Entity<Family>().HasQueryFilter(f => f.Status != FamilyStatus.Deleted);
            builder.Entity<GroupActivity>().HasQueryFilter(ga => ga.Status != ActivityStatus.Inactive);
            builder.Entity<MassTiming>().HasQueryFilter(mt => !mt.IsDeleted);
            builder.Entity<Announcement>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<Ward>().HasQueryFilter(w => !w.IsDeleted);
            #endregion

            #region Entity Configurations

            // ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(150);
                entity.Property(u => u.ProfileImageUrl).HasMaxLength(250);
                entity.Property(u => u.Designation).HasMaxLength(50);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Ward
            builder.Entity<Ward>(entity =>
            {
                entity.HasIndex(w => w.Name).IsUnique();
                entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
                entity.HasMany(w => w.Families).WithOne(f => f.Ward).HasForeignKey(f => f.WardId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(w => w.Users).WithOne(u => u.Ward).HasForeignKey(u => u.WardId).OnDelete(DeleteBehavior.Restrict);
            });

            // Grade
            builder.Entity<Grade>(entity =>
            {
                entity.HasIndex(g => g.Name).IsUnique();
                entity.HasIndex(g => g.Order).IsUnique();
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            });

            // Group
            builder.Entity<Group>(entity =>
            {
                entity.HasIndex(g => g.Name).IsUnique();
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Description).HasMaxLength(500);
                entity.HasMany(g => g.Students).WithOne(s => s.Group).HasForeignKey(s => s.GroupId).OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(g => g.GroupActivities).WithOne(ga => ga.Group).HasForeignKey(ga => ga.GroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // TeacherAssignment
            builder.Entity<TeacherAssignment>(entity =>
            {
                entity.HasKey(ta => new { ta.UserId, ta.GradeId, ta.AcademicYear });
                entity.HasOne(ta => ta.User).WithMany().HasForeignKey(ta => ta.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ta => ta.Grade).WithMany().HasForeignKey(ta => ta.GradeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ta => ta.Group).WithMany().HasForeignKey(ta => ta.GroupId).OnDelete(DeleteBehavior.Restrict);
            });

            // Family & FamilyMember
            builder.Entity<Family>(entity =>
            {
                entity.HasIndex(f => f.ChurchRegistrationNumber).IsUnique().HasFilter("[ChurchRegistrationNumber] IS NOT NULL");
                entity.HasIndex(f => f.TemporaryID).IsUnique().HasFilter("[TemporaryID] IS NOT NULL");
                entity.HasMany(f => f.FamilyMembers).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(f => f.MigrationLogs).WithOne(ml => ml.Family).HasForeignKey(ml => ml.FamilyId).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<FamilyMember>(entity =>
            {
                entity.HasIndex(m => m.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
                entity.HasOne(m => m.StudentProfile).WithOne(s => s.FamilyMember).HasForeignKey<Student>(s => s.FamilyMemberId).OnDelete(DeleteBehavior.Cascade);
            });

            // Student
            builder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.GradeId);
                entity.HasIndex(s => s.GroupId);
                entity.HasOne(s => s.Grade).WithMany(g => g.Students).HasForeignKey(s => s.GradeId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(s => s.StudentOrganisation).HasMaxLength(150);
                entity.Property(s => s.MigratedTo).HasMaxLength(150);
                entity.HasMany(s => s.Attendances).WithOne(a => a.Student).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(s => s.Assessments).WithOne(a => a.Student).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(s => s.StudentGroupActivities).WithOne(sga => sga.Student).HasForeignKey(sga => sga.StudentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(s => s.AcademicRecords).WithOne(r => r.Student).HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            // StudentAcademicRecord
            builder.Entity<StudentAcademicRecord>(entity =>
            {
                entity.HasIndex(r => new { r.StudentId, r.AcademicYear, r.GradeId }).IsUnique();
                entity.HasOne(r => r.Grade).WithMany().HasForeignKey(r => r.GradeId).OnDelete(DeleteBehavior.Restrict);
            });

            // AssessmentSummary
            builder.Entity<AssessmentSummary>(entity =>
            {
                entity.HasKey(s => s.Id); // Ensure primary key is defined
                entity.HasIndex(s => new { s.StudentId, s.AcademicYear, s.GradeId }).IsUnique();
                entity.HasOne(s => s.Grade).WithMany().HasForeignKey(s => s.GradeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Student).WithMany().HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            // GroupActivity & StudentGroupActivity
            builder.Entity<GroupActivity>(entity =>
            {
                entity.HasMany(ga => ga.Participants).WithOne(sga => sga.GroupActivity).HasForeignKey(sga => sga.GroupActivityId).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<StudentGroupActivity>(entity =>
            {
                entity.HasKey(sga => sga.Id); // Ensure primary key is defined
                entity.HasIndex(sga => new { sga.StudentId, sga.GroupActivityId });
            });

            #endregion
        }
    }
}