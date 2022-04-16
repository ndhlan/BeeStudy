using BeeStudy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync();

        Task<Course> GetByIdAsync(int id);

        Task<Course> GetByUdemyIdAsync(int UdemyId);

        Task<Course> GetDetailsByUrlAsync(string url);

        Task<IEnumerable<Course>> GetAllByLearnerAsync(int learnerId);

        Task AddAsync(Course newCourse);

        Task<Course> UpdateAsync(int id, Course newCourse);

        Task DeleteAsync(int id);

        Task RemoveLearnerAsync(int courseId, int learnerId);

    }
}
