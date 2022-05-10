using BeeStudy.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class CourseService: ICourseService
    {

        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.


        public CourseService(ApplicationDbContext context, IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _context = context;
            Options = optionsAccessor.Value;

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://www.udemy.com/api-2.0/")
            };

        }



        public async Task AddAsync(Course newCourse)
        {
            _context.Add(newCourse);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (SqlException se)
            {

            }
        }

        public async Task DeleteAsync(int id)
        {
            var courseToDelete = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            _context.Courses.Remove(courseToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            var result = await _context.Courses.Include(re => re.Registrations).ThenInclude(l => l.Learner).ToListAsync();
            return result;
        }

        public async Task<Course> GetByIdAsync(int courseId)
        {
            var course = await _context.Courses.Include(re => re.Registrations).ThenInclude(l => l.Learner).FirstOrDefaultAsync(c => c.Id == courseId);
            return course;
        }

        public async Task<Course> GetByUdemyIdAsync(int UdemyId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.UdemyId == UdemyId);
            return course;
        }

        public async Task<Course> GetDetailsByUrlAsync(string courseUrl)
        {
            Course newCourse = new Course();

            //check if course already in database
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Url == courseUrl);

            if (course != null)
            {
                newCourse = course;
            }
            else
            {
                //Get general course details
                string courseNameFromUrl = courseUrl.Substring("https://www.udemy.com/course/".Length, courseUrl.Length - "https://www.udemy.com/course/".Length - 1);

                var requestUrl = _httpClient.BaseAddress + string.Format("courses/{0}/", courseNameFromUrl);

                _httpClient.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                newCourse.Url = courseUrl;

                if (response.IsSuccessStatusCode)
                {
                    var courseDetails = (JObject)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    newCourse.UdemyId = (int)courseDetails["id"];
                    newCourse.Title = (string)courseDetails["title"];
                    newCourse.ImageUrl = (string)courseDetails["image_480x270"];
                }

                //get headline
                var headlineRequestUrl = _httpClient.BaseAddress + string.Format("courses/{0}/?fields[course]=title,headline", newCourse.UdemyId);
                _httpClient.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage headlineResponse = await _httpClient.GetAsync(headlineRequestUrl);
                if (headlineResponse.IsSuccessStatusCode)
                {
                    var headline = (JObject)JsonConvert.DeserializeObject(await headlineResponse.Content.ReadAsStringAsync());

                    newCourse.Headline = (string)headline["headline"];

                }
            }

            //Compose course object 

            var courseInfo = new
            {
                UdemyId = newCourse.UdemyId,
                Url = newCourse.Url,
                Title = newCourse.Title,
                Headline = newCourse.Headline,
                ImageUrl = newCourse.ImageUrl
            };

            //Serialize object to json
            string jsonData = JsonConvert.SerializeObject(courseInfo);
            var data = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //Send HTTPTrigger to Azure Logic App 

            var appUrl = "https://prod-09.canadacentral.logic.azure.com:443/workflows/4583d307ab304d97899d336426fbf20a/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=rlkjR4Eb24jmC_QOQJOm29TKEuHbxD-cVoU7iIWyBvQ";

            var result = await _httpClient.PostAsync(appUrl, data);

            return newCourse;
        }


        public async Task<IEnumerable<Course>> GetAllByLearnerAsync(int learnerId)
        {
            var allCourses = await _context.Courses.ToListAsync();

            List<Course> coursesByLearner = new List<Course>();

            foreach (Course course in allCourses)
            {
                foreach (Registration re in course.Registrations)
                {
                    if (re.LearnerId == learnerId)
                    {
                        coursesByLearner.Add(course);
                    }
                }
            }

            return coursesByLearner;
        }


        public async Task<Course> UpdateAsync(int id, Course course)
        {
            var courseToUpdate = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (courseToUpdate != null)
            {
                courseToUpdate.ListPrice = course.ListPrice;
                courseToUpdate.LastUpdatedPrice = course.LastUpdatedPrice;
                courseToUpdate.LastUpdatedPriceDate = course.LastUpdatedPriceDate;
                courseToUpdate.CurrentPrice = course.CurrentPrice;
                courseToUpdate.CurrentPriceDate = course.CurrentPriceDate;
                courseToUpdate.Browser = course.Browser;
                courseToUpdate.DiscountExpiration = course.DiscountExpiration;
                courseToUpdate.Headline = course.Headline;
                courseToUpdate.ImageUrl = course.ImageUrl;
                courseToUpdate.Registrations.Add(course.Registrations[0]);
            }
            await _context.SaveChangesAsync();
            return courseToUpdate;
        }


        public async Task RemoveLearnerAsync(int courseId, int learnerId)
        {
            var course = await _context.Courses.Include(re => re.Registrations).ThenInclude(l => l.Learner).FirstOrDefaultAsync(c => c.Id == courseId);

            foreach (Registration re in course.Registrations)
            {
                if (re.Learner.Id == learnerId)
                {
                    _context.Remove(re);
                }
            }

            await _context.SaveChangesAsync();

        }       



    }
}
