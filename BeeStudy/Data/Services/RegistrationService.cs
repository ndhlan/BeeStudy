using Microsoft.Extensions.Options;
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

        public IOptions<AuthMessageSenderOptions> Options { get; } //Set with Secret Manager.


        public RegistrationService(ApplicationDbContext context, IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _context = context;
            Options = optionsAccessor;
        }

        public ICourseService CourseService
        {
            get
            {
                return _courseService = _courseService ?? new CourseService(_context, Options);
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
