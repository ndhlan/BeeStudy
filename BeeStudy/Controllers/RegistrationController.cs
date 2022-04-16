using BeeStudy.Data.Services;
using BeeStudy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Controllers
{
    public class RegistrationController : Controller
    {

        private readonly IRegistrationService _service;

        public RegistrationController(IRegistrationService service)
        {
            _service = service;
        }


        [AllowAnonymous]
        //GET Registration/Add
        public IActionResult Add()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Add(RegistrationViewModel newReg)
        {
            if (!ModelState.IsValid)
            {
                return View(newReg);
            }

            //Get new course from url
            Course newCourse = await _service.CourseService.GetDetailsByUrlAsync(newReg.Course.Url);

            //Create learner 
            Learner newLearner = new Learner();

            //check if learner already in database 
            Learner learner = await _service.LearnerService.GetByEmailAsync(newReg.Learner.Email);
            if (learner == null)
            {
                newLearner.Email = newReg.Learner.Email;
                newLearner.Name = newReg.Learner.Name;

                await _service.LearnerService.AddAsync(newLearner);
            }
            else
            {
                newLearner = learner;
            }

            //Add new course-learner relationship
            newCourse.Registrations = new List<Registration>
                {
                    new Registration
                    {
                    Course = newCourse,
                    Learner = newLearner
                    }
                };

            //check if course already in database
            Course course = await _service.CourseService.GetByUdemyIdAsync(newCourse.UdemyId);
            if (course == null)
            {
                await _service.CourseService.AddAsync(newCourse);

                //System.Diagnostics.Debug.WriteLine("Relationship " + newCourse.Registrations.Count());
            }
            else
            {
                await _service.CourseService.UpdateAsync(course.Id, newCourse);

                //System.Diagnostics.Debug.WriteLine("Relationship " + newCourse.Registrations.Count());
            }
            return RedirectToAction("Confirm", new { courseId = newCourse.Id, learnerId = newLearner.Id });


        }

        [AllowAnonymous]
        public async Task<IActionResult> Confirm(int courseId, int learnerId)
        {
            var course = await _service.CourseService.GetByIdAsync(courseId);
            var learner = await _service.LearnerService.GetByIdAsync(learnerId);
            RegistrationViewModel newRegistration = new RegistrationViewModel()
            {
                Course = course,
                Learner = learner
            };

            return View(newRegistration);
        }


        //GET Registration/LearnerRegistrations/
        [Authorize(Roles = "User")]
        public async Task<IActionResult> LearnerRegistrations()
        {
            var userEmail = User.Identity.Name;
            var learner = await _service.LearnerService.GetByEmailAsync(userEmail);

            if (learner == null)
            {
                return View("Unauthorized");
            }

            return View(learner.Registrations);
        }



        //GET Registration/Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var courseList = await _service.CourseService.GetAllAsync();

            List<RegistrationViewModel> registrationList = new List<RegistrationViewModel>();

            foreach (Course course in courseList)
            {

                foreach (Registration re in course.Registrations)
                {
                    registrationList.Add(new RegistrationViewModel
                    {
                        Course = re.Course,
                        Learner = re.Learner
                    });
                }
            }

            return View(registrationList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int courseId, int learnerId)
        {
            await _service.CourseService.RemoveLearnerAsync(courseId, learnerId);

            return RedirectToAction("Index");
        }


    }
}
