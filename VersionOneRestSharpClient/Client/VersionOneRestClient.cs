using System;
using RestSharp;
using RestSharp.Validation;

namespace VersionOneRestSharpClient.Client
{
    public class VersionOneRestClient : RestClient
    {
        public VersionOneRestClient(string baseUrl, string userName, string password) : base(baseUrl)
        {
            Require.Argument("baseUrl", baseUrl);

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }

            Authenticator = new HttpBasicAuthenticator(userName, password);
            AddHandler("text/xml", new AssetDeserializer());
        }
    }
}