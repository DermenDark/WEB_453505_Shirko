namespace LRs.Services.Authentication
{
    public interface ITokenAccessor
    {
        /// <summary> 
        /// Добавление заголовка Authorizition : bearer 
        /// </summary> 
        /// <param name="httpClient">HttpLient, в который добавляется заголовок</param>
        /// <param name="isClient">если true - получить токен клиента; если false - получить токен пользователя</param>
        /// <returns></returns> 
        Task SetAuthorizationHeaderAsync(HttpClient httpClient,
        bool isClient);
    }
}
