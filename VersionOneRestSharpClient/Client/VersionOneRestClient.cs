using System;
using RestSharp;
using RestSharp.Validation;
using RestSharp.Authenticators;

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

        public VersionOneRestClient(string baseUrl, string accessToken) : base(baseUrl)
        {
            Require.Argument("baseUrl", baseUrl);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException("accessToken");
            }

            Authenticator = new AcccessTokenAuthenticator(accessToken);
            AddHandler("text/xml", new AssetDeserializer());
        }

		public IRestResponse Update(string oidToken, object attributes)
		{
			var update = new RestApiUpdatePayloadBuilder(oidToken, attributes);
			var payload = update.Build();
			var asset = oidToken.Replace(":", "/");
			var req = new RestRequest(asset, Method.POST);
			req.AddParameter("application/json", payload, ParameterType.RequestBody);
			return this.Post(req);
		}
    }
}