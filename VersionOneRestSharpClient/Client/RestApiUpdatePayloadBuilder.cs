using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace VersionOneRestSharpClient.Client
{
	public class RestApiUpdatePayloadBuilder
	{
		private readonly string _oidToken = string.Empty;
		private readonly object _attributes = new { };

		public RestApiUpdatePayloadBuilder(string oidToken)
		{
			if (string.IsNullOrWhiteSpace(oidToken))
			{
				throw new ArgumentNullException("oidToken");
			}
			_oidToken = oidToken;
		}

		public RestApiUpdatePayloadBuilder(string oidToken, object attributes)
			: this(oidToken)
		{
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
