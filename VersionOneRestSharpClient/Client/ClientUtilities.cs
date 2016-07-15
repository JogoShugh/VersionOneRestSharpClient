using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace VersionOneRestSharpClient.Client
{
    internal class Update
    {
        public string name { get; set; }
        public object value { get; set; }
    }

    internal class UpdateAttribute : Update
    {
        public string act { get; set; }
    }

    internal class AssetReference
    {
        public string idref { get; set; }
        public string act { get; set; }
    }

    public static class ClientUtilities
    {
        public static Asset Asset(string assetTypeName, object attributes=null) => new Asset(assetTypeName, attributes);
        public static JArray Assets() => new JArray();
        public static Array Relations(params object[] relations) => relations;
        public static Array Relation(object relation) => new[] { relation };
        public static object Add(string oidToken) => new AssetReference { idref = oidToken, act = "add" };
        public static object Add(string assetType, int id) => new AssetReference { idref = $"{assetType}:{id}", act = "add" };
        public static object Remove(string oidToken) => new AssetReference { idref = oidToken, act = "remove" };
        public static object Remove(string assetType, int id) => new AssetReference { idref = $"{assetType}:{id}", act = "remove" };
        public static QueryApiQueryBuilder From(string assetSelectionExpression) => new QueryApiQueryBuilder(assetSelectionExpression);
    }
}
