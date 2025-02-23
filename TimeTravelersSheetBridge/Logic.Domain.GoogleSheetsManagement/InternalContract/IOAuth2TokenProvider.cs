namespace Logic.Domain.GoogleSheetsManagement.InternalContract
{
    public interface IOAuth2TokenProvider
    {
        Task<string?> GetAccessToken();
    }
}
