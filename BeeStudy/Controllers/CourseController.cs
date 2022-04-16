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
    public class CourseController : Controller
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        //GET Course/Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var courseList = await _service.GetAllAsync();

            return View(courseList);
        }

        //GET Course/Add
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(string courseUrl)
        {
            //Get course infor from Udemy
            Course newCourse = await _service.GetDetailsByUrlAsync(courseUrl);

            //check if course already in database
            Course course = await _service.GetByUdemyIdAsync(newCourse.UdemyId);
            if (course == null)
            {
                await _service.AddAsync(newCourse);

                return RedirectToAction("Details", new { id = newCourse.Id });
            }
            else
            {
                return View("DuplicateError", new { entity = "Course" });
            }

        }

        [Authorize(Roles = "Admin, User")]
        //GET Course/Details/1
        public async Task<IActionResult> Details(int id)
        {
            Course course = await _service.GetByIdAsync(id);

            if (course == null)
            {
                return View("NotFound");
            }

            return View(course);
        }


        [Authorize(Roles = "Admin")]
        //GET Course/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            Course courseToDelete = await _service.GetByIdAsync(id);
            if (courseToDelete == null)
            {
                return View("NotFound");
            }

            return View(courseToDelete);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Course courseToDelete = await _service.GetByIdAsync(id);
            if (courseToDelete == null)
            {
                return View("NotFound");
            }
            await _service.DeleteAsync(id);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveLearner(int courseId, int learnerId)
        {
            await _service.RemoveLearnerAsync(courseId, learnerId);

            return RedirectToAction("Details", new { id = courseId });
        }

    }
}
