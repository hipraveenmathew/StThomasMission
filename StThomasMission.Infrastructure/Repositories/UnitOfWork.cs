using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        public UnitOfWork(DbContext context)
        {
            _context = context;
            Students = new StudentRepository(_context);
            Families = new Repository<Family>(_context);
            FamilyMembers = new Repository<FamilyMember>(_context);
            Attendances = new Repository<Attendance>(_context);
            Assessments = new Repository<Assessment>(_context);
            GroupActivities = new Repository<GroupActivity>(_context);
            StudentGroupActivities = new Repository<StudentGroupActivity>(_context);
            MessageLogs = new Repository<MessageLog>(_context);
            AuditLogs = new Repository<AuditLog>(_context);
        }

        public IStudentRepository Students { get; }
        public IRepository<Family> Families { get; }
        public IRepository<FamilyMember> FamilyMembers { get; }
        public IRepository<Attendance> Attendances { get; }
        public IRepository<Assessment> Assessments { get; }
        public IRepository<GroupActivity> GroupActivities { get; }
        public IRepository<StudentGroupActivity> StudentGroupActivities { get; }
        public IRepository<MessageLog> MessageLogs { get; }
        public IRepository<AuditLog> AuditLogs { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}