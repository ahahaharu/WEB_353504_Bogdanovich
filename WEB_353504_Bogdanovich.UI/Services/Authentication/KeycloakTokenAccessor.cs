using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Headers;
using System.Text.Json;
using WEB_353504_Bogdanovich.UI.HelperClasses;

namespace WEB_353504_Bogdanovich.UI.Services.Authentication
{
    public class KeycloakTokenAccessor : ITokenAccessor
    {
        private readonly HttpContext? _httpContext;
        private readonly KeycloakData _keycloakData;
        private readonly HttpClient _tokenClient;

        public KeycloakTokenAccessor(
            IHttpContextAccessor httpContextAccessor,
            IOptions<KeycloakData> keycloakOptions,
            IHttpClientFactory httpClientFactory)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _keycloakData = keycloakOptions.Value;
            _tokenClient = httpClientFactory.CreateClient("KeycloakTokenClient");
        }

        private async Task TryRefreshTokenAsync()
        {
            if (_httpContext == null || !_httpContext.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authenticateResult = await _httpContext.AuthenticateAsync();
            var expiresAtString = authenticateResult.Properties.GetTokenValue("expires_at");

            if (string.IsNullOrEmpty(expiresAtString) || !DateTimeOffset.TryParse(expiresAtString, out var expiresAt))
            {
                // Токен не найден или не имеет срока действия. Не можем обновить.
                return;
            }

            // Обновляем токен, если до истечения осталось меньше 60 секунд
            if (expiresAt.Subtract(TimeSpan.FromSeconds(60)) > DateTimeOffset.UtcNow)
            {
                return; // Токен еще действителен
            }

            var refreshToken = authenticateResult.Properties.GetTokenValue("refresh_token");

            if (string.IsNullOrEmpty(refreshToken))
            {
                // Не можем обновить, т.к. refresh_token отсутствует.
                return;
            }

            var tokenUrl = $"{_keycloakData.Host}/realms/{_keycloakData.Realm}/protocol/openid-connect/token";

            var refreshRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(OpenIdConnectParameterNames.ClientId, _keycloakData.ClientId),
                new KeyValuePair<string, string>(OpenIdConnectParameterNames.ClientSecret, _keycloakData.ClientSecret),
                new KeyValuePair<string, string>(OpenIdConnectParameterNames.GrantType, OpenIdConnectGrantTypes.RefreshToken),
                new KeyValuePair<string, string>(OpenIdConnectParameterNames.RefreshToken, refreshToken),
            });

            var response = await _tokenClient.PostAsync(tokenUrl, refreshRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"\n--- ОШИБКА ОБНОВЛЕНИЯ ТОКЕНА KEYCLOAK ({response.StatusCode}) ---");
                Console.WriteLine($"Запрос: {tokenUrl}");
                Console.WriteLine($"Ответ: {errorContent}");
                Console.WriteLine("-----------------------------------------------------------------\n");

                // *** ИЗМЕНЕНИЕ ***
                // Если Keycloak отклонил Refresh Token (400 Bad Request),
                // это означает, что Refresh Token недействителен ("Token is not active").
                // Принудительно выходим из системы, чтобы очистить истекшие куки
                // и заставить пользователя пройти повторную аутентификацию.
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var newAccessToken = tokenResponse.GetProperty(OpenIdConnectParameterNames.AccessToken).GetString();
            var newRefreshToken = tokenResponse.GetProperty(OpenIdConnectParameterNames.RefreshToken).GetString();

            // Если refresh_token не вернулся, используем старый.
            if (string.IsNullOrEmpty(newRefreshToken))
            {
                newRefreshToken = refreshToken;
            }

            var expiresIn = tokenResponse.GetProperty(OpenIdConnectParameterNames.ExpiresIn).GetInt32();

            var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o");

            // Обновляем токены в свойствах аутентификации
            authenticateResult.Properties.UpdateTokenValue("access_token", newAccessToken);
            authenticateResult.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
            authenticateResult.Properties.UpdateTokenValue("expires_at", newExpiresAt);

            // Переподписываем пользователя, чтобы сохранить новые токены в куках
            await _httpContext.SignInAsync(authenticateResult.Principal, authenticateResult.Properties);
        }

        public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient)
        {
            string? token = null;

            if (!isClient && _httpContext != null)
            {
                // 1. Попытка обновить токены перед извлечением
                await TryRefreshTokenAsync();

                // 2. Извлечение (возможно, нового) токена
                token = await _httpContext.GetTokenAsync("access_token");
            }
            else if (isClient)
            {
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _keycloakData.ClientId),
                    new KeyValuePair<string, string>("client_secret", _keycloakData.ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                });

                var discoveryUrl = $"{_keycloakData.Host}/realms/{_keycloakData.Realm}/protocol/openid-connect/token";
                var response = await _tokenClient.PostAsync(discoveryUrl, tokenRequest);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
                token = tokenResponse.GetProperty("access_token").GetString();
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Требуется токен для доступа к API. Пользователь не аутентифицирован или токен недоступен.");
            }

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

}
