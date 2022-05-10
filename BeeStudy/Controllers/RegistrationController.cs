using BeeStudy.Data.Services;
using BeeStudy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BeeStudy.Controllers
{
    public class RegistrationController : Controller
    {

        private readonly IRegistrationService _service;
        private readonly HttpClient _httpClient;

        public RegistrationController(IRegistrationService service, IEmailSender emailSender, EmailContent emailContent)
        {
            _service = service;
            _httpClient = new HttpClient();
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

            //check if learner is registered user
            var registeredUsers = _service.UserService.GetAll();
            foreach(IdentityUser user in registeredUsers)
            {
                if (user.Email.Equals(newLearner.Email))
                {
                    newLearner.IsRegisteredUser = true;
                }
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
            }
            else
            {
                await _service.CourseService.UpdateAsync(course.Id, newCourse);
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

            //send HTTPTrigger to SendOneCourseId 
            //Compose course object 
            var courseInfo = new
            {
                Name = learner.Name,
                Email = learner.Email,
                UdemyId = course.UdemyId,
                Url = course.Url,
                Title = course.Title,
                Headline = course.Headline,
                ImageUrl = course.ImageUrl
            };

            //Serialize object to json
            string jsonData = JsonConvert.SerializeObject(courseInfo);
            var data = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //Send HTTPTrigger to Azure Logic App
            var appUrl = "https://prod-09.canadacentral.logic.azure.com:443/workflows/4583d307ab304d97899d336426fbf20a/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=rlkjR4Eb24jmC_QOQJOm29TKEuHbxD-cVoU7iIWyBvQ";

            var result = await _httpClient.PostAsync(appUrl, data);



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
