using BeeStudy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public interface ILearnerService
    {
        Task<IEnumerable<Learner>> GetAllAsync();

        Task<Learner> GetByIdAsync(int id);

        Task<Learner> GetByEmailAsync(string email);

        Task<IEnumerable<Learner>> GetAllByCourseAsync(int courseId);

        Task AddAsync(Learner newLearner);

        Task<Learner> UpdateAsync(int id, Learner learner);

        Task DeleteAsync(int id);

        Task RemoveCourseAsync(int learnerId, int courseId);

    }
}
