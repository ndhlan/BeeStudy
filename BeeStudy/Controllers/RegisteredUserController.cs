using BeeStudy.Data;
using BeeStudy.Data.Services;
using BeeStudy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Controllers
{
    [Authorize(Roles ="Admin")]
    public class RegisteredUserController : Controller
    {
        private readonly IIdentityUserService _userService;
        private readonly ILearnerService _learnerService;

        public RegisteredUserController(IIdentityUserService userService, ILearnerService learnerService)
        {
            _userService = userService;
            _learnerService = learnerService;
        }

        public async Task<IActionResult> Index()
        {            
            List<RegisteredUserViewModel> result = new List<RegisteredUserViewModel>();
            var registeredUsers = _userService.GetAll();

            foreach (IdentityUser user in registeredUsers)
            {
                Learner learner = await _learnerService.GetByEmailAsync(user.Email);
                result.Add(new RegisteredUserViewModel
                {
                    User = user,
                    Learner = learner
                });
            }

            return View(result);
        }


        public IActionResult Delete(string id)
        {
             _userService.Delete(id);

            return RedirectToAction("Index");
        }


    }
}
