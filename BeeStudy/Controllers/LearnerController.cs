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
    public class LearnerController : Controller
    {

        private readonly ILearnerService _service;

        public LearnerController(ILearnerService service)
        {
            _service = service;
        }


        //GET Learner/Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var learnerList = await _service.GetAllAsync();
            return View(learnerList);
        }


        //GET Learner/Add
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(string name, string email)
        {
            //Create new Learner 
            Learner newLearner = new Learner();

            //check if learner already in database 
            Learner learner = await _service.GetByEmailAsync(email);
            if (learner == null)
            {
                newLearner.Email = email;
                newLearner.Name = name;

                await _service.AddAsync(newLearner);

                return RedirectToAction("Details", new { id = newLearner.Id });
            }
            else
            {
                return View("DuplicateError", new { entity = "Learner" });
            }


        }

        //GET Learner/Details/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            Learner learner = await _service.GetByIdAsync(id);

            if (learner == null)
            {
                return View("NotFound");
            }
            return View(learner);
        }


        //GET Learner/Delete/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Learner learnerToDelete = await _service.GetByIdAsync(id);
            if (learnerToDelete == null)
            {
                return View("NotFound");
            }

            return View(learnerToDelete);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Learner learnerToDelete = await _service.GetByIdAsync(id);
            if (learnerToDelete == null)
            {
                return View("NotFound");
            }
            await _service.DeleteAsync(id);

            return RedirectToAction("Index");
        }


        //GET Learner/Edit/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            Learner learnerToEdit = await _service.GetByIdAsync(id);
            if (learnerToEdit == null)
            {
                return View("NotFound");
            }

            return View(learnerToEdit);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Learner learner)
        {
            Learner learnerToEdit = await _service.GetByIdAsync(id);
            if (learnerToEdit == null)
            {
                return View("NotFound");
            }

            learnerToEdit.Name = learner.Name;
            learnerToEdit.Email = learner.Email;
            learnerToEdit.IsRegisteredUser = learner.IsRegisteredUser;


            await _service.UpdateAsync(id, learnerToEdit);

            return RedirectToAction("Details", new { id = id });
        }


        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveCourse(int learnerId, int courseId)
        {
            var userEmail = User.Identity.Name;
            var learner = await _service.GetByEmailAsync(userEmail);
            if (learner.Id != learnerId)
            {
                return View("Unauthorized");
            }
            else
            {
                await _service.RemoveCourseAsync(learnerId, courseId);

                return RedirectToAction("LearnerRegistrations", "Registration", new { id = learnerId });
            }


        }


    }
}
