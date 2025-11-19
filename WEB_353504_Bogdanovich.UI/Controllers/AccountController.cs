using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using WEB_353504_Bogdanovich.UI.HelperClasses;
using WEB_353504_Bogdanovich.UI.Models;
using WEB_353504_Bogdanovich.UI.Services.Authentication;
using WEB_353504_Bogdanovich.UI.Services.FileService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace WEB_353504_Bogdanovich.UI.Controllers
{
    internal class UserCredentials
    {
        public string Type { get; set; } = "password";
        public bool Temporary { get; set; } = false;
        public string Value { get; set; } = string.Empty;
    }

    internal class CreateUserModel
    {
        public Dictionary<string, string> Attributes { get; set; } = new();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public bool EmailVerified { get; set; } = true;
        public List<UserCredentials> Credentials { get; set; } = new();
    }

    public class AccountController(
        IHttpContextAccessor contextAccessor,
        HttpClient httpClient,
        ITokenAccessor tokenAccessor,
        IOptions<KeycloakData> options,
        IFileService fileService
    ) : Controller
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ITokenAccessor _tokenAccessor = tokenAccessor;
        private readonly KeycloakData _keycloakData = options.Value;
        private readonly IFileService _fileService = fileService;


        [HttpGet]
        public async Task Login(string returnUrl = null)
        {
            await HttpContext.ChallengeAsync(
                "keycloak", 
                new AuthenticationProperties { RedirectUri = returnUrl ?? Url.Action("Index", "Home") }
            );
        }

        [HttpPost]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignOutAsync(
                "keycloak",
                new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") }
            );
        }

        public IActionResult Register()
        {
            return View(new RegisterUserViewModel());
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (user == null)
            {
                return BadRequest();
            }

            try
            {
                await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения токена клиента Keycloak: {ex.Message}");
                ModelState.AddModelError("", "Не удалось получить токен клиента Keycloak. Регистрация невозможна.");
                return View(user);
            }

            var avatarUrl = "/images/default-profile-picture.png";

            if (user.Avatar != null)
            {
                avatarUrl = await _fileService.SaveFileAsync(user.Avatar);
            }

            var newUser = new CreateUserModel
            {
                Email = user.Email,
                Username = user.Email,
            };
            newUser.Attributes.Add("avatar", avatarUrl);
            newUser.Credentials.Add(new UserCredentials { Value = user.Password });

            var requestUri =
                $"{_keycloakData.Host}/admin/realms/{_keycloakData.Realm}/users";

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var userData = JsonSerializer.Serialize(newUser, serializerOptions);
            HttpContent content = new StringContent(userData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Ошибка регистрации на Keycloak. Код: {response.StatusCode}. Возможно, пользователь с таким Email уже существует. Детали: {errorDetails}");
                return View(user);
            }
        }
    }
}
