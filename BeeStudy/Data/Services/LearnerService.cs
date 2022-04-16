using BeeStudy.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class LearnerService: ILearnerService
    {
        private readonly ApplicationDbContext _context;

        public LearnerService(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task AddAsync(Learner newLearner)
        {
            _context.Add(newLearner);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var leanerToDelete = await _context.Learners.FirstOrDefaultAsync(l => l.Id == id);
            _context.Learners.Remove(leanerToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Learner>> GetAllAsync()
        {
            var result = await _context.Learners.Include(re => re.Registrations).ThenInclude(c => c.Course).ToListAsync();

            return result;
        }

        public async Task<Learner> GetByEmailAsync(string email)
        {
            var learner = await _context.Learners.Include(re => re.Registrations).ThenInclude(c => c.Course).FirstOrDefaultAsync(l => l.Email.Equals(email));
            return learner;
        }

        public async Task<Learner> GetByIdAsync(int id)
        {
            var learner = await _context.Learners.Include(re => re.Registrations).ThenInclude(c => c.Course).FirstOrDefaultAsync(l => l.Id == id);
            return learner;
        }

        public async Task<IEnumerable<Learner>> GetAllByCourseAsync(int courseId)
        {
            var allLearners = await _context.Learners.Include(re => re.Registrations).ThenInclude(c => c.Course).ToListAsync();

            List<Learner> learnersByCourse = new List<Learner>();

            foreach (Learner learner in allLearners)
            {
                foreach (Registration re in learner.Registrations)
                {
                    if (re.CourseId == courseId)
                    {
                        learnersByCourse.Add(learner);
                    }
                }
            }
            return learnersByCourse;
        }


        public async Task<Learner> UpdateAsync(int id, Learner learner)
        {
            var learnerToUpdate = await _context.Learners.FirstOrDefaultAsync(l => l.Id == id);
            learnerToUpdate.Name = learner.Name;

            await _context.SaveChangesAsync();
            return learnerToUpdate;
        }


        public async Task RemoveCourseAsync(int learnerId, int courseId)
        {
            var learner = await _context.Learners.Include(re => re.Registrations).ThenInclude(c => c.Course).FirstOrDefaultAsync(l => l.Id == learnerId);

            foreach (Registration re in learner.Registrations)
            {
                if (re.Course.Id == courseId)
                {
                    _context.Remove(re);
                }
            }

            await _context.SaveChangesAsync();

        }



    }
}
