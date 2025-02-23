using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class TokenData
    {
        public Scope? Scope { get; set; }
        public DateTime Expiration { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
