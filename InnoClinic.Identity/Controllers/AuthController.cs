using Duende.IdentityServer.Services;
using IdentityModel.Client;
using InnoClinic.Identity.Models.Entities;
using InnoClinic.Identity.Models.Views;
using InnoClinic.Identity.RabbitMQ.Interfaces;
using InnoClinic.Identity.RabbitMQ.Models.Send;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Identity.Controllers
{
    public sealed class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IMessageProducer _messageProducer;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IIdentityServerInteractionService interactionService, IMessageProducer messageProducer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interactionService = interactionService;
            _messageProducer = messageProducer;
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
            if (!user.IsPasswordConfirmed)
            {
                return Redirect("https://localhost:7104/auth/resetpassword");
            }

            var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, false, false);
            if (result.Succeeded)
            {
                var accessToken = await GetAccessToken(viewModel.Username, viewModel.Password);

                Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(30),
                });

                return Redirect("http://localhost:4200/receptionists");
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
                UserName = viewModel.UserName,
                IsPasswordConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");

                await _signInManager.SignInAsync(user, false);

                _messageProducer.SendMessage(new PatientCreatedModel
                {
                    Username = viewModel.UserName,
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    MiddleName = viewModel.MiddleName,
                    PhoneNumber = viewModel.PhoneNumber,
                    BirthDate = viewModel.BirthDate
                });

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

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _userManager.FindByNameAsync(viewModel.Username);
            if (user is not null)
            {
                var result = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

                if (result.Succeeded)
                {
                    user.IsPasswordConfirmed = true;
                    result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return Redirect("https://localhost:7104/auth/login");
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Reset password failed");
            return View(viewModel);
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
