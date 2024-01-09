using IdentityModel.Client;
using IdentityServer4.Services;
using InnoClinic.Identity.Models.Entities;
using InnoClinic.Identity.Models.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Identity.Controllers
{
    public sealed class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IIdentityServerInteractionService _interactionService;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IIdentityServerInteractionService interactionService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interactionService = interactionService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _userManager.FindByNameAsync(viewModel.Username);
            var userRole = await _userManager.GetRolesAsync(user);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return View(viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, false, false);
            if (result.Succeeded)
            {
                var accessToken = await GetAccessToken(viewModel.Username, viewModel.Username);

                Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(30)
                });

                return Redirect("https://localhost:7187/swagger/index.html");
            }

            ModelState.AddModelError(string.Empty, "Login failed");
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            var viewModel = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = new AppUser
            {
                UserName = viewModel.UserName
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");

                await _signInManager.SignInAsync(user, false);
                return Redirect(viewModel.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Register failed");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();
            var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);
            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        private async Task<string> GetAccessToken(string username, string password)
        {
            using (var client = new HttpClient())
            {
                var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:7104");

                if (discovery.IsError)
                {
                    throw new Exception(discovery.Error);
                }

                var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = discovery.TokenEndpoint,
                    ClientId = "innoclinic-web-api",
                    ClientSecret = "inno-clinic-secret",
                    UserName = username,
                    Password = password,
                });

                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.Error);
                }

                return tokenResponse.AccessToken;
            }
        }
    }
}
