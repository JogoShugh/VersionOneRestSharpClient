using System;
using System.Collections.Generic;

namespace VersionOneRestSharpClient.Client
{
    public class ComparisonOperator
    {
        public string Token { get; set; }
        public string Name { get; set; }

        public ComparisonOperator(string name, string token)
        {
            Name = name;
            Token = token;
        }

        public new static readonly ComparisonOperator Equals =
            new ComparisonOperator(ComparisonOperatorConstants.Equals,
                                   ComparisonOperatorConstants.EqualsToken);
        public static readonly ComparisonOperator NotEquals =
            new ComparisonOperator(ComparisonOperatorConstants.NotEquals,
                                   ComparisonOperatorConstants.NotEqualsToken);
        public static readonly ComparisonOperator LessThan =
            new ComparisonOperator(ComparisonOperatorConstants.LessThan,
                                   ComparisonOperatorConstants.LessThanToken);
        public static readonly ComparisonOperator LessThanOrEqual =
            new ComparisonOperator(ComparisonOperatorConstants.LessThanOrEqual,
                                   ComparisonOperatorConstants.LessThanOrEqualToken);
        public static readonly ComparisonOperator GreaterThan =
            new ComparisonOperator(ComparisonOperatorConstants.GreaterThan,
                                   ComparisonOperatorConstants.GreaterThanToken);
        public static readonly ComparisonOperator GreaterThanOrEqual =
            new ComparisonOperator(ComparisonOperatorConstants.GreaterThanOrEqual,
                                   ComparisonOperatorConstants.GreaterThanOrEqualToken);

        private static Dictionary<string, ComparisonOperator> _operatorsMap =
            new Dictionary<string, ComparisonOperator>()
                {
                    {ComparisonOperatorConstants.Equals, Equals},
                    {ComparisonOperatorConstants.EqualsToken, Equals},
                    {ComparisonOperatorConstants.NotEquals, NotEquals},
                    {ComparisonOperatorConstants.NotEqualsToken, NotEquals},
                    {ComparisonOperatorConstants.LessThan, LessThan},
                    {ComparisonOperatorConstants.LessThanToken, LessThan},
                    {ComparisonOperatorConstants.LessThanOrEqual, LessThanOrEqual},
                    {ComparisonOperatorConstants.LessThanOrEqualToken, LessThanOrEqual},
                    {ComparisonOperatorConstants.GreaterThan, GreaterThan},
                    {ComparisonOperatorConstants.GreaterThanToken, GreaterThan},
                    {ComparisonOperatorConstants.GreaterThanOrEqual, GreaterThanOrEqual},
                    {ComparisonOperatorConstants.GreaterThanOrEqualToken, GreaterThanOrEqual}
                };

        public static ComparisonOperator GetOperator(string op)
        {
            if (_operatorsMap.ContainsKey(op))
            {
                return _operatorsMap[op];
            }

            throw new ArgumentException(string.Format("Could not find an operator by name or token for the supplied parameter: {0}", op));
        }
    }
}