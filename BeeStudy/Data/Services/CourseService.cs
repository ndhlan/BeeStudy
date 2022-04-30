using BeeStudy.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class CourseService: ICourseService
    {

        private static readonly string _userAgentFirefox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0 Viewer/96.9.6838.39";
        private static readonly string _userAgentChrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.4977.0 Safari/537.36";
        private static readonly string _userAgentEdge = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.60 Safari/537.36 Edg/100.0.1185.29 Trailer/93.3.1282.83";
        private readonly string[] userAgents = { _userAgentChrome, _userAgentFirefox, _userAgentEdge };

        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;


        public CourseService(ApplicationDbContext context)
        {
            _context = context;
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


            //Get or update course prices

            //Move CurrentPrice and Date to LastUpdatedPrice and Date
            newCourse.LastUpdatedPrice = newCourse.CurrentPrice;
            newCourse.LastUpdatedPriceDate = newCourse.CurrentPriceDate;

            //Check for new CurrentPrice
            var requestPriceUrl = _httpClient.BaseAddress + string.Format("course-landing-components/{0}/me/?components=buy_button,discount_expiration,purchase", newCourse.UdemyId);

            foreach (string userAgent in userAgents)
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);


                HttpResponseMessage priceResponse = await _httpClient.GetAsync(requestPriceUrl);

                if (priceResponse.IsSuccessStatusCode)
                {
                    var priceDetails = (JObject)JsonConvert.DeserializeObject(await priceResponse.Content.ReadAsStringAsync());
                    var tempCurrentPrice = (decimal)priceDetails["buy_button"]["button"]["payment_data"]["purchasePrice"]["amount"];

                    if (newCourse.CurrentPrice == 0 || (newCourse.CurrentPrice != 0 && tempCurrentPrice < newCourse.CurrentPrice))
                    {
                        newCourse.ListPrice = (decimal)priceDetails["purchase"]["data"]["pricing_result"]["list_price"]["amount"]; ;
                        newCourse.CurrentPrice = (decimal)priceDetails["buy_button"]["button"]["payment_data"]["purchasePrice"]["amount"];
                        newCourse.DiscountExpiration = (string)priceDetails["discount_expiration"]["data"]["discount_deadline_text"];

                        newCourse.CurrentPriceDate = DateTime.Now;
                        if (userAgent.Equals(_userAgentFirefox))
                        {
                            newCourse.Browser = "Firefox";
                        }
                        else if (userAgent.Equals(_userAgentChrome))
                        {
                            newCourse.Browser = "Chrome";
                        }
                        else
                        {
                            newCourse.Browser = "Edge";
                        }
                    }

                }
            }


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
