using System;
using RestSharp;
using RestSharp.Validation;
using RestSharp.Authenticators;
using System.Collections.Generic;

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
			ClearHandlers();
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
			ClearHandlers();
            AddHandler("text/xml", new AssetDeserializer());
        }

        public IRestResponse<dynamic> Create(string assetType, object attributes)
        {
            var payload = RestApiPayloadBuilder.Build(attributes);
            var req = new RestRequest(assetType, Method.POST);
            req.AddParameter("application/json", payload, ParameterType.RequestBody);
            return this.Post<dynamic>(req);
        }

		public IRestResponse<dynamic> Update(string oidToken, object attributes)
		{
			var payload = RestApiPayloadBuilder.Build(attributes);
			var asset = oidToken.Replace(":", "/");
			var req = new RestRequest(asset, Method.POST);
			req.AddParameter("application/json", payload, ParameterType.RequestBody);
			return this.Post<dynamic>(req);
		}

		public RestApiUriQueryBuilder Query(string assetType)
		{
			Func<string, IList<dynamic>> execute = (string query) => {
				var req = new RestRequest(query);
				var response = this.Get<List<dynamic>>(req);
				var results = response.Data as IList<dynamic>;
				return results;
			};
			return new RestApiUriQueryBuilder(assetType, execute);
		}

        public RestApiUriQueryBuilderTyped<T> Query<T>() 
        {
            Func<string, IList<T>> execute = (string query) => {
                var req = new RestRequest(query);
                var response = this.Get<List<T>>(req);
                var results = response.Data as IList<T>;
                return results;
            };
            return new RestApiUriQueryBuilderTyped<T>(execute);
        }
    }
}