using System.Net;

namespace Logic.Domain.GoogleSheetsManagement.InternalContract
{
    internal interface IOAuth2Listener
    {
        void Start(string consentUrl);
        Task<HttpListenerRequest> AwaitRequest();
    }
}
