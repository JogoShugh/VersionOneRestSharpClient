namespace VersionOneRestSharpClient.Client
{
    public class WhereCriterion
    {
        public string AttributeName { get; set; }
        public ComparisonOperator Operator { get; set; }
        public object MatchValue { get; set; }
    }
}