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

        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupActivity> GroupActivities { get; set; }
        public DbSet<StudentGroupActivity> StudentGroupActivities { get; set; }
        public DbSet<StudentAcademicRecord> StudentAcademicRecords { get; set; }
        public DbSet<MessageLog> MessageLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<MassTiming> MassTimings { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<MigrationLog> MigrationLogs { get; set; }
        public DbSet<AssessmentSummary> AssessmentSummaries { get; set; }

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
            builder.Entity<MassTiming>()
                .HasQueryFilter(mt => !mt.IsDeleted);
            builder.Entity<Announcement>()
                .HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<Ward>()
                .HasQueryFilter(w => !w.IsDeleted);

            // Ward Configurations
            builder.Entity<Ward>()
                .HasIndex(w => w.Name)
                .IsUnique();
            builder.Entity<Ward>()
                .Property(w => w.Name)
                .HasMaxLength(100);
            builder.Entity<Ward>()
                .Property(w => w.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Ward>()
                .Property(w => w.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<Ward>()
                .Property(w => w.UpdatedBy)
                .HasMaxLength(150);
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
                .HasIndex(f => f.ChurchRegistrationNumber)
                .IsUnique()
                .HasFilter("[ChurchRegistrationNumber] IS NOT NULL");
            builder.Entity<Family>()
                .HasIndex(f => f.TemporaryID)
                .IsUnique()
                .HasFilter("[TemporaryID] IS NOT NULL");
            builder.Entity<Family>()
                .Property(f => f.FamilyName)
                .HasMaxLength(150);
            builder.Entity<Family>()
                .Property(f => f.ChurchRegistrationNumber)
                .HasMaxLength(8);
            builder.Entity<Family>()
                .Property(f => f.TemporaryID)
                .HasMaxLength(8);
            builder.Entity<Family>()
                .Property(f => f.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Family>()
                .Property(f => f.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<Family>()
                .Property(f => f.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<Family>()
                .HasMany(f => f.FamilyMembers)
                .WithOne(m => m.Family)
                .HasForeignKey(m => m.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Family>()
                .HasMany(f => f.MigrationLogs)
                .WithOne(ml => ml.Family)
                .HasForeignKey(ml => ml.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            // FamilyMember Configurations
            builder.Entity<FamilyMember>()
                .HasIndex(m => m.UserId)
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL");
            builder.Entity<FamilyMember>()
                .Property(m => m.FirstName)
                .HasMaxLength(100);
            builder.Entity<FamilyMember>()
                .Property(m => m.LastName)
                .HasMaxLength(100);
            builder.Entity<FamilyMember>()
                .Property(m => m.Contact)
                .HasMaxLength(20);
            builder.Entity<FamilyMember>()
                .Property(m => m.Email)
                .HasMaxLength(150);
            builder.Entity<FamilyMember>()
                .Property(m => m.Role)
                .HasMaxLength(50);
            builder.Entity<FamilyMember>()
                .Property(m => m.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<FamilyMember>()
                .Property(m => m.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<FamilyMember>()
                .HasOne(m => m.StudentProfile)
                .WithOne(s => s.FamilyMember)
                .HasForeignKey<Student>(s => s.FamilyMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student Configurations
            builder.Entity<Student>()
                .HasIndex(s => s.GroupId);
            builder.Entity<Student>()
                .Property(s => s.Grade)
                .HasMaxLength(20);
            builder.Entity<Student>()
                .Property(s => s.StudentOrganisation)
                .HasMaxLength(150);
            builder.Entity<Student>()
                .Property(s => s.MigratedTo)
                .HasMaxLength(150);
            builder.Entity<Student>()
                .Property(s => s.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<Student>()
                .Property(s => s.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<Student>()
                .Property(s => s.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
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
                .HasMany(s => s.StudentGroupActivities) // Fixed: Changed from GroupActivities to StudentGroupActivities
                .WithOne(sga => sga.Student)
                .HasForeignKey(sga => sga.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAcademicRecord>()
                .HasIndex(r => new { r.StudentId, r.AcademicYear })
                .IsUnique();

            builder.Entity<Student>()
                .HasMany(s => s.AcademicRecords)
                .WithOne(r => r.Student)
                .HasForeignKey(r => r.StudentId);

            // Group Configurations
            builder.Entity<Group>()
                .HasIndex(g => g.Name)
                .IsUnique();
            builder.Entity<Group>()
                .Property(g => g.Name)
                .HasMaxLength(100);
            builder.Entity<Group>()
                .Property(g => g.Description)
                .HasMaxLength(500);
            builder.Entity<Group>()
                .Property(g => g.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Group>()
                .Property(g => g.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<Group>()
                .Property(g => g.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<Group>()
                .HasMany(g => g.Students)
                .WithOne(s => s.Group)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Group>()
                .HasMany(g => g.GroupActivities)
                .WithOne(ga => ga.Group)
                .HasForeignKey(ga => ga.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupActivity Configurations
            builder.Entity<GroupActivity>()
                .HasIndex(ga => ga.GroupId);
            builder.Entity<GroupActivity>()
                .HasIndex(ga => ga.Date);
            builder.Entity<GroupActivity>()
                .Property(ga => ga.Name)
                .HasMaxLength(150);
            builder.Entity<GroupActivity>()
                .Property(ga => ga.Description)
                .HasMaxLength(500);
            builder.Entity<GroupActivity>()
                .Property(ga => ga.Date)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<GroupActivity>()
                .Property(ga => ga.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<GroupActivity>()
                .Property(ga => ga.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<GroupActivity>()
                .HasMany(ga => ga.Participants)
                .WithOne(sga => sga.GroupActivity)
                .HasForeignKey(sga => sga.GroupActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentGroupActivity Configurations
            builder.Entity<StudentGroupActivity>()
                .HasIndex(sga => sga.StudentId);
            builder.Entity<StudentGroupActivity>()
                .HasIndex(sga => sga.GroupActivityId);
            builder.Entity<StudentGroupActivity>()
                .Property(sga => sga.ParticipationDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<StudentGroupActivity>()
                .Property(sga => sga.Remarks)
                .HasMaxLength(250);
            builder.Entity<StudentGroupActivity>()
                .Property(sga => sga.RecordedBy)
                .HasMaxLength(150)
                .IsRequired();

            // MessageLog Configurations
            builder.Entity<MessageLog>()
                .HasIndex(ml => ml.SentAt);
            builder.Entity<MessageLog>()
                .Property(ml => ml.Recipient)
                .HasMaxLength(150);
            builder.Entity<MessageLog>()
                .Property(ml => ml.Message)
                .HasMaxLength(1000);
            builder.Entity<MessageLog>()
                .Property(ml => ml.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Sent");
            builder.Entity<MessageLog>()
                .Property(ml => ml.ResponseDetails)
                .HasMaxLength(250);
            builder.Entity<MessageLog>()
                .Property(ml => ml.SentBy)
                .HasMaxLength(150);
            builder.Entity<MessageLog>()
                .Property(ml => ml.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // AuditLog Configurations
            builder.Entity<AuditLog>()
                .HasIndex(al => al.Timestamp);
            builder.Entity<AuditLog>()
                .Property(al => al.UserId)
                .HasMaxLength(450);
            builder.Entity<AuditLog>()
                .Property(al => al.Action)
                .HasMaxLength(150);
            builder.Entity<AuditLog>()
                .Property(al => al.EntityName)
                .HasMaxLength(150);
            builder.Entity<AuditLog>()
                .Property(al => al.EntityId)
                .HasMaxLength(100);
            builder.Entity<AuditLog>()
                .Property(al => al.Details)
                .HasMaxLength(1000);
            builder.Entity<AuditLog>()
                .Property(al => al.IpAddress)
                .HasMaxLength(45);
            builder.Entity<AuditLog>()
                .Property(al => al.PerformedBy)
                .HasMaxLength(250);
            builder.Entity<AuditLog>()
                .Property(al => al.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            // MassTiming Configurations
            builder.Entity<MassTiming>()
                .HasIndex(mt => mt.WeekStartDate);
            builder.Entity<MassTiming>()
                .Property(mt => mt.Day)
                .HasMaxLength(20);
            builder.Entity<MassTiming>()
                .Property(mt => mt.Location)
                .HasMaxLength(100);
            builder.Entity<MassTiming>()
                .Property(mt => mt.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<MassTiming>()
                .Property(mt => mt.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<MassTiming>()
                .Property(mt => mt.UpdatedBy)
                .HasMaxLength(150);

            // Announcement Configurations
            builder.Entity<Announcement>()
                .HasIndex(a => a.PostedDate);
            builder.Entity<Announcement>()
                .Property(a => a.Title)
                .HasMaxLength(200);
            builder.Entity<Announcement>()
                .Property(a => a.Description)
                .HasMaxLength(2000);
            builder.Entity<Announcement>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Announcement>()
                .Property(a => a.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<Announcement>()
                .Property(a => a.UpdatedBy)
                .HasMaxLength(150);

            // MigrationLog Configurations
            builder.Entity<MigrationLog>()
                .HasIndex(ml => ml.FamilyId);
            builder.Entity<MigrationLog>()
                .Property(ml => ml.MigratedTo)
                .HasMaxLength(150);
            builder.Entity<MigrationLog>()
                .Property(ml => ml.MigrationDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<MigrationLog>()
                .Property(ml => ml.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<MigrationLog>()
                .Property(ml => ml.UpdatedBy)
                .HasMaxLength(150);

            // AssessmentSummary Configurations
            builder.Entity<AssessmentSummary>()
                .HasIndex(s => new { s.StudentId, s.AcademicYear })
                .IsUnique();
            builder.Entity<AssessmentSummary>()
                .Property(s => s.Grade)
                .HasMaxLength(20);
            builder.Entity<AssessmentSummary>()
                .Property(s => s.Remarks)
                .HasMaxLength(500);
            builder.Entity<AssessmentSummary>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<AssessmentSummary>()
                .Property(s => s.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();
            builder.Entity<AssessmentSummary>()
                .Property(s => s.UpdatedBy)
                .HasMaxLength(150);
            builder.Entity<AssessmentSummary>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser Configurations
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(150);
            builder.Entity<ApplicationUser>()
                .Property(u => u.ProfileImageUrl)
                .HasMaxLength(250);
            builder.Entity<ApplicationUser>()
                .Property(u => u.Designation)
                .HasMaxLength(50);
            builder.Entity<ApplicationUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}