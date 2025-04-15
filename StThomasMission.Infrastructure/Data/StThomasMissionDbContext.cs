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

        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<GroupActivity> GroupActivities { get; set; }
        public DbSet<StudentGroupActivity> StudentGroupActivities { get; set; }
        public DbSet<MessageLog> MessageLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Family>()
                .HasMany(f => f.Members)
                .WithOne(m => m.Family)
                .HasForeignKey(m => m.FamilyId);

            builder.Entity<FamilyMember>()
                .HasOne(m => m.StudentProfile)
                .WithOne(s => s.FamilyMember)
                .HasForeignKey<Student>(s => s.FamilyMemberId);

            builder.Entity<Student>()
                .HasMany(s => s.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId);

            builder.Entity<Student>()
                .HasMany(s => s.Assessments)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId);
        }
    }
}