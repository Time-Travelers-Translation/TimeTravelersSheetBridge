namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class UserConsentData
    {
        public bool IsConsent { get; set; }
        public string? Code { get; set; }
        public string? Error { get; set; }
    }
}
