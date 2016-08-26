using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Collections;

namespace VersionOneRestSharpClient.Client
{
	public class RestJArray : JArray
	{
		public RestJArray(JArray other)
		{
			foreach (var item in other)
			{
				Add(item);
			}
		}

		public override void Add(object content)
		{
			base.AddFirst(JToken.FromObject(content));
			//base.Add(JToken.FromObject(content));
		}
	}

	public class Asset : DynamicObject, IEnumerable<JProperty>
	{
		private enum AddOrRemove
		{
			original,
			add,
			remove
		}

		private dynamic _wrapped;
		private Dictionary<string, List<Tuple<string, AddOrRemove>>> _relationshipAssetReferencesMap =
			new Dictionary<string, List<Tuple<string, AddOrRemove>>>();
		private Dictionary<string, bool> _modifiedAttributes = new Dictionary<string, bool>();

		private bool _fromDynamic = false;

		public Asset(dynamic wrapped)
		{
			_wrapped = wrapped;
			_fromDynamic = true;
		}

		public Asset(string assetTypeName, object attributes = null)
		{
			if (attributes != null) _wrapped = JObject.FromObject(attributes);
			else _wrapped = new JObject();
			_wrapped["AssetType"] = assetTypeName;
		}

		[JsonIgnore]
		public string AssetType
		{
			get
			{
				var assetType = _wrapped["AssetType"];
				if (assetType != null && !string.IsNullOrWhiteSpace(assetType.ToString())) return assetType.ToString();
				else return string.Empty;
			}
		}

		[JsonIgnore]
		public string OidToken
		{
			get
			{
				return _wrapped["_links"].self.oidToken;
			}
		}

		public JObject GetChangesDto()
		{
			if (_fromDynamic) return _wrapped as JObject;

			var changesObj = new JObject();
			foreach (var key in _modifiedAttributes.Keys)
			{
				changesObj[key] = _wrapped[key];
			}

			foreach (var list in _relationshipAssetReferencesMap)
			{
				var items = new List<object>();
				foreach (var oidTokenReference in list.Value)
				{
					var act = oidTokenReference.Item2.ToString();
					items.Add(new
					{
						idref = oidTokenReference.Item1,
						act = act
					});
				}
				changesObj[list.Key] = JArray.FromObject(items);
			}

			return changesObj;
		}

		public string GetYamlPayload()
		{
			return QueryYamlPayloadBuilder.Build(this);
		}

		private JArray GetRelation(string relationName)
		{
			return _wrapped["_links"][relationName] as JArray;
		}

		private List<Tuple<string, AddOrRemove>> GetOrCreateRelationMap(string relationName)
		{
			if (!_relationshipAssetReferencesMap.ContainsKey(relationName))
			{
				var newMap = new List<Tuple<string, AddOrRemove>>();
				_relationshipAssetReferencesMap[relationName] = newMap;
			}
			var map = _relationshipAssetReferencesMap[relationName];
			return map;
		}

		public void CreateRelatedAsset(string relationName, Asset asset)
		{
			var relation = _wrapped[relationName];
			if (relation == null || !(relation is JArray))
			{
				relation = new JArray();
				Set(relationName, relation);
			}
			var array = relation as JArray;
			dynamic wrapped = asset.GetWrappedDynamic();
			var token = JToken.FromObject(wrapped);
			array.Add(token);
		}

		private void RegisterRemovedRelationshipAssetReference(string relationName, string oidToken)
			=> RegisterRelationshipAssetReference(relationName, oidToken, AddOrRemove.remove);

		private void RegisterAddedRelationshipAssetReference(string relationName, string oidToken)
			=> RegisterRelationshipAssetReference(relationName, oidToken, AddOrRemove.add);

		private void RegisterRelationshipAssetReference(string relationName, string oidToken, AddOrRemove direction)
		{
			var map = GetOrCreateRelationMap(relationName);
			var entry = map.FirstOrDefault(m => m.Item1 == oidToken);
			if (entry == null)
			{
				entry = new Tuple<string, AddOrRemove>(oidToken, direction);
				map.Add(entry);
			}
			else
			{
				var newEntry = new Tuple<string, AddOrRemove>(entry.Item1, direction);
				map.Remove(entry);
				map.Add(newEntry);
			}
		}

		public int RemoveRelatedAssets(string relationName, params object[] oidTokens)
		{
			int removed = 0;
			try
			{
				foreach (var oidToken in oidTokens)
				{
					var oid = oidToken.ToString();
					var relation = GetRelation(relationName);

					foreach (dynamic item in relation)
					{
						if (item.idref == oid)
						{
							relation.Remove(item);
							RegisterRemovedRelationshipAssetReference(relationName, oid);
							removed++;
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
			return removed;
		}

		public void AddRelatedAssets(string relationName, params object[] oidTokens)
		{
			// TODO check for duplicates first
			foreach (var oidToken in oidTokens)
			{
				var oid = oidToken.ToString();
				var relation = GetRelation(relationName);
				RegisterAddedRelationshipAssetReference(relationName, oid);
				relation.Add(JObject.FromObject(new
				{
					idref = oidToken
				}));
			}
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (binder.Name == "RemoveRelatedAssets")
			{
				result = this.RemoveRelatedAssets(args[0] as string, args[1] as object);
				return true;
			}
			else if (binder.Name == "AddRelatedAssets")
			{
				this.AddRelatedAssets(args[0] as string, args[1] as object);
				result = null;
				return true;
			}
			else if (binder.Name == "GetChangesDto")
			{
				result = this.GetChangesDto();
				return true;
			}

			return base.TryInvokeMember(binder, args, out result);
		}

		public object Get(string attributeName)
		{
			return _wrapped[attributeName];
		}

		public override bool TryGetMember(GetMemberBinder binder,
												 out object result)
		{
			bool success;
			try
			{
				result = _wrapped[binder.Name];
				success = true;
			}
			catch (Exception)
			{
				result = null;
				success = false;
			}
			return success;
		}

		public void Set(string atttributeName, object value)
		{
			_wrapped[atttributeName] = JToken.FromObject(value);
			_modifiedAttributes[atttributeName] = true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			bool success;
			try
			{
				var originalValue = _wrapped[binder.Name];
				if (originalValue != value)
				{
					Set(binder.Name, value);
				}
				success = true;
			}
			catch (Exception ex)
			{
				// TODO...
				success = false;
			}
			return success;
		}
		public object this[string attributeName]
		{
			get
			{
				return Get(attributeName);
			}
			set
			{
				Set(attributeName, value);
			}
		}

		internal dynamic GetWrappedDynamic()
		{
			return _wrapped;
		}

		public IEnumerator<JProperty> GetEnumerator()
		{
			var wrapped = GetWrappedDynamic();
			JObject obj = JObject.FromObject(wrapped);
			return obj.Properties().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
