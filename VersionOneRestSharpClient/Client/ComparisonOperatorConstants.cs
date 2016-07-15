namespace VersionOneRestSharpClient.Client
{
    public class ComparisonOperatorConstants
    {
        public new static readonly string Equals = "Equals";
        public static readonly string EqualsToken = "=";
        public static readonly string NotEquals = "NotEquals";
        public static readonly string NotEqualsToken = "!=";
        public static readonly string LessThan = "LessThan";
        public static readonly string LessThanToken = "<";
        public static readonly string LessThanOrEqual = "LessThanOrEqual";
        public static readonly string LessThanOrEqualToken = "<=";
        public static readonly string GreaterThan = "GreaterThan";
        public static readonly string GreaterThanToken = ">";
        public static readonly string GreaterThanOrEqual = "GreaterThanOrEqual";
        public static readonly string GreaterThanOrEqualToken = ">=";
        public static readonly string Exists = "Exists";
        public static readonly string ExistsToken = "%2B"; // + character, encoded
        public static readonly string NotExists = "NotExists";
        public static readonly string NotExistsToken = "-";
    }
}