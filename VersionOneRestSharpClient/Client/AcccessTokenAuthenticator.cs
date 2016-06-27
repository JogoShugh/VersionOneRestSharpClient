using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionOneRestSharpClient.Client
{
    public class AcccessTokenAuthenticator : RestSharp.Authenticators.IAuthenticator
    {
        private string _accessToken { get; set; }

        public AcccessTokenAuthenticator(string accessToken)
        {
            _accessToken = accessToken;
        }

        public void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request)
        {
            request.AddHeader("Authorization", "Bearer " + _accessToken);
        }
    }
}
