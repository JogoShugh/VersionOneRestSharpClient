namespace VersionOneRestSharpClient.Client
{
    public class WhereCriterion
    {
        public string AttributeName { get; set; }
        public ComparisonOperator Operator { get; set; }
        public object MatchValue { get; set; }

        public class MatchNotApplicableMatch
        {
            // Marker class to indicate that this is a unary criterion
        }

        public static MatchNotApplicableMatch MatchNotApplicable = new WhereCriterion.MatchNotApplicableMatch();

        public bool IsUnary
        {
            get
            {
                return MatchValue == MatchNotApplicable;
            }
        }
    }

}