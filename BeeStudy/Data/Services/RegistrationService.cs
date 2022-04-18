using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class RegistrationService: IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private CourseService _courseService;
        private LearnerService _learnerService;
        private IdentityUserService _userService;

        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICourseService CourseService
        {
            get
            {
                return _courseService = _courseService ?? new CourseService(_context);
            }
            set
            {

            }
        }

        public ILearnerService LearnerService
        {
            get
            {
                return _learnerService = _learnerService ?? new LearnerService(_context);
            }
            set
            {

            }
        }

        public IIdentityUserService UserService
        {
            get
            {
                return _userService = _userService ?? new IdentityUserService(_context);
            }
            set
            {

            }
        }


        public void Save()
        {
            _context.SaveChanges();
        }


    }
}
