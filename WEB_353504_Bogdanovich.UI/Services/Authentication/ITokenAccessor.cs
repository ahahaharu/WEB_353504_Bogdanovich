namespace WEB_353504_Bogdanovich.UI.Services.Authentication
{
    public interface ITokenAccessor
    {
        /// <summary>
        /// Добавление заголовка Authorization : bearer к HttpClient.
        /// </summary>
        /// <param name="httpClient">HttpClient, в который добавляется заголовок</param>
        /// <param name="isClient">Если true - получить токен клиента; если false - получить токен пользователя.</param>
        Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient);
    }
}
