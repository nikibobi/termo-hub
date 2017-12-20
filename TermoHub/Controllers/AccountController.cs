using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace TermoHub.Controllers
{
    using Authorization;
    using Models;
    using vm = ViewModels;

    [AllowAnonymous]
    public class AccountController : Controller
    {
        private const string defaultRedirect = "/";

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet("/login")]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost("/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(vm.Login model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.Username,
                    model.Password,
                    isPersistent: true,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl ?? defaultRedirect);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login failed.");
                }
            }
            return View(model);
        }

        [HttpGet("/register")]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost("/register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(vm.Register model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var anyUsers = userManager.Users.Any();

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    result = await userManager.AddToRoleAsync(user, anyUsers ? Role.User : Role.Admin);
                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: true);
                        return LocalRedirect(returnUrl ?? defaultRedirect);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost("/promote/{username}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> Promote([FromRoute] string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();

            var result = await userManager.AddToRoleAsync(user, Role.Admin);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("/logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(MainController.Index), "Main");
        }
    }
}