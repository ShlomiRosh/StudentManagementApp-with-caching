﻿using System;
using System.Threading.Tasks;
using DbAccess.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace DbAccess.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StudentManagementContext _context;
        private readonly ILogger _logger;
            /*private readonly ILessonRepository _lessonRepository;
            private readonly IStudentLessonRepository _studentLessonRepository;
            private readonly IMeasurementRepository _measurementRepository;
            private readonly ILogRepository _logRepository;*/

        public StudentRepository(StudentManagementContext context, ILogger<StudentRepository> logger)
        {
            _context = context;
            _logger = logger;
            /*_lessonRepository = lessonRepository;
            _studentLessonRepository = studentLessonRepository;
            _measurementRepository = measurementRepository;
            _logRepository = logRepository;*/
        }
        
        /// <summary>
        /// Get student by id from database
        /// </summary>
        /// <param name="id">student id</param>
        /// <returns>the student from the DB</returns>
        public async Task<Student> GetStudentById(int id)
        {
            try
            {
                return await _context.Students.Where(x => x.Id == id).Include(x => x.School).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get student from DB. With student id: {id}. due to: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// Add student to database
        /// </summary>
        /// <param name="student">student to add</param>
        /// <returns>the added student</returns>
        public async Task<Student> AddStudent(Student student)
        {
            try
            {
                Student studentFromDb = null;
                // check if student already exists
                studentFromDb = await GetStudentByStudent(student);
                if (studentFromDb == null)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return student;
                }
                _logger.LogInformation($"Student: {studentFromDb} already exist");
                return studentFromDb;
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot add student to DB. due to: {e}");
                return null;
            }
        }
        
        /// <summary> 
        /// Get student by student data from database, if student exist return it, else return null
        /// </summary>
        /// <param name="student">student data</param>
        /// <returns>the student from the DB</returns>
        private async Task<Student> GetStudentByStudent(Student student)
        {
            try
            {
                return await _context.Students.Where(
                    x => x.FirstName == student.FirstName && x.LastName == student.LastName && x.Gpa == student.Gpa)
                    .Include(x => x.School == student.School).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get student from DB. Due to: {e}");
                return null;
            }
            
        }
        
        /// <summary>
        /// Update student in database
        /// </summary>
        /// <param name="student">student to update</param>
        /// <returns>the updated student</returns>
        public async Task<Student> UpdateStudent(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return student;
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot update student in DB. student id: {student.Id}. due to: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// Delete student from database
        /// </summary>
        /// <param name="id">student id</param>
        /// <returns>true if student deleted, false if not</returns>
        public async Task<bool> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                _logger.LogInformation($"Student with id: {id} not found");
                return false;
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }
        
    }
}