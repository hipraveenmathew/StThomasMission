﻿using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class CatechismService : ICatechismService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CatechismService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));
            return student;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Students.GetByGradeAsync(grade);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group)
        {
            if (string.IsNullOrEmpty(group)) throw new ArgumentException("Group is required.", nameof(group));
            return await _unitOfWork.Students.GetByGroupAsync(group);
        }

        public async Task AddStudentAsync(int familyMemberId, int academicYear, string grade, string? group, string? studentOrganisation)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (familyMember == null)
                throw new ArgumentException("Family member not found.", nameof(familyMemberId));

            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                AcademicYear = academicYear,
                Grade = grade,
                Group = group,
                StudentOrganisation = studentOrganisation,
                Status = StudentStatus.Active
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, StudentStatus status, string? migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId);

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            student.Grade = grade;
            student.Group = group;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = migratedTo;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkPassFailAsync(int studentId, StudentStatus status)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (!Regex.IsMatch(student.Grade, @"^Year \d{1,2}$")) throw new InvalidOperationException("Student grade is in invalid format.");

            if (status == StudentStatus.Graduated)
            {
                if (student.Grade != "Year 12")
                    throw new InvalidOperationException("Only Year 12 students can graduate.");
                student.Status = StudentStatus.Graduated;
            }
            else if (status == StudentStatus.Active)
            {
                if (student.Grade == "Year 12")
                {
                    student.Status = StudentStatus.Graduated;
                }
                else
                {
                    int currentYear = int.Parse(student.Grade.Replace("Year ", ""));
                    student.Grade = $"Year {currentYear + 1}";
                    student.AcademicYear += 1;
                    student.Status = StudentStatus.Active;
                }
            }
            else
            {
                throw new ArgumentException("Invalid status for pass/fail operation.", nameof(status));
            }

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            await _unitOfWork.Students.DeleteAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            var students = await GetStudentsByGradeAsync(grade);
            foreach (var student in students.Where(s => s.AcademicYear == academicYear))
            {
                await MarkPassFailAsync(student.Id, StudentStatus.Active);
            }
        }
    }
}