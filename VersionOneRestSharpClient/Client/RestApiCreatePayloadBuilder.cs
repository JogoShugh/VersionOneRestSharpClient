using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace VersionOneRestSharpClient.Client
{
    public class RestApiCreatePayloadBuilder
    {
        private readonly string _assetType = string.Empty;
        private readonly object _attributes = new { };

        public RestApiCreatePayloadBuilder(string assetType, object attributes)
        {
            if (string.IsNullOrWhiteSpace(assetType))
            {
                throw new ArgumentNullException("assetType");
            }
            _assetType = assetType;

            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            _attributes = attributes;
        }

        public string Build()
        {
            return JObject.FromObject(new
            {
                Attributes = (from prop in
                                  (
                                      from token in JToken.FromObject(_attributes)
                                      where token.Type == JTokenType.Property
                                      select token as JProperty
                                  )
                              select new
                              {
                                  name = prop.Name,
                                  value = prop.Value,
                                  act = "set"
                              }
                            ).ToDictionary(obj => obj.name, obj => obj)
            }).ToString();
        }
    }
}
