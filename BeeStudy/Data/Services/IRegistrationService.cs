using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public interface IRegistrationService
    {
        ICourseService CourseService { get; }

        ILearnerService LearnerService { get; }

        IIdentityUserService UserService { get;  }

        void Save();

    }
}
