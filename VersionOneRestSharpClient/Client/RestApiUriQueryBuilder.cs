using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace VersionOneRestSharpClient.Client
{
    public class RestApiUriQueryBuilder
    {
        public readonly List<object> SelectFields = new List<object>();
        public readonly List<WhereCriterion> WhereCriteria = new List<WhereCriterion>();
        public int PageSize = -1;
        public int PageStart = -1;        
        private readonly string _assetType = string.Empty;
        private string _id = string.Empty;

        public RestApiUriQueryBuilder(string assetType)
        {
            if (string.IsNullOrWhiteSpace(assetType))
            {
                throw new ArgumentNullException("assetType");
            }
            _assetType = assetType;
        }

        public RestApiUriQueryBuilder Id(object id)
        {
            if (id == null) throw new ArgumentNullException("id");
            var val = id.ToString();

            if (string.IsNullOrWhiteSpace(val))
            {
                throw new ArgumentNullException("id", "id.ToString() must return a non-empty string");
            }
            _id = val;

            return this;
        }

        public RestApiUriQueryBuilder Select(params object[] fields)
        {
            SelectFields.AddRange(fields);

            return this;
        }

        public RestApiUriQueryBuilder Where(string attributeName, string matchValue)
        {
            WhereCriteria.Add(new WhereCriterion
            {
                AttributeName = attributeName,
                Operator = ComparisonOperator.Equals,
                MatchValue = matchValue
            });

            return this;
        }

        public RestApiUriQueryBuilder Filter(string attributeName, string op, object filterValue)
        {
            var oper = ComparisonOperator.GetOperator(op);

            return Filter(attributeName, oper, filterValue);
        }

        public RestApiUriQueryBuilder Filter(string attributeName, ComparisonOperator op, object filterValue)
        {
            WhereCriteria.Add(new WhereCriterion
            {
                AttributeName = attributeName,
                Operator = op,
                MatchValue = filterValue
            });

            return this;
        }

        public RestApiUriQueryBuilder Paging(int pageSize, int pageStart = 0)
        {
            PageSize = pageSize;
            PageStart = pageStart;

            return this;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var query = new StringBuilder();

            if (SelectFields.Count > 0)
            {
                var selectFragment = String.Join(",", SelectFields);
                query.Append("sel=" + Uri.EscapeDataString(selectFragment));
            }

            if (WhereCriteria.Count > 0)
            {
                var encodedCriteria = new List<string>(WhereCriteria.Count);

                foreach (var criterion in WhereCriteria)
                {
                    var encoded = criterion.AttributeName
                                  + criterion.Operator.Token
                                  + "'" + Uri.EscapeDataString(criterion.MatchValue.ToString()) + "'";
                    encodedCriteria.Add(encoded);
                }

                var whereFragment = String.Join(";", encodedCriteria);

                if (query.Length > 0)
                {
                    query.Append("&");
                }
                query.Append("where=" + whereFragment);
            }

            if (PageSize != -1 && PageStart != -1)
            {
                if (query.Length > 0)
                {
                    query.Append("&");
                }
                query.Append(string.Format("page={0},{1}", PageSize, PageStart));
            }

            builder.Append("/" + _assetType);

            if (!string.IsNullOrWhiteSpace(_id))
            {
                builder.Append("/" + _id);
            }

            if (query.Length > 0)
            {
                builder.Append("?" + query);
            }

            return builder.ToString();
        }
    }

    public class RestApiUriQueryBuilderTyped<T> : RestApiUriQueryBuilder
    {
        public RestApiUriQueryBuilderTyped()
            : base(typeof(T).Name)
        {
        }

        public new RestApiUriQueryBuilderTyped<T> Id(object id)
        {
            base.Id(id);
            return this;
        }

        public RestApiUriQueryBuilderTyped<T> Select(params Expression<Func<T, object>>[] fields)
        {
            foreach (var field in fields)
            {
                var name = (field.Body as MemberExpression ??
                            ((UnaryExpression)field.Body).Operand as MemberExpression).Member.Name;
                SelectFields.Add(GetMappedPathForAttribute(name));
            }

            return this;
        }
        
        public new RestApiUriQueryBuilderTyped<T> Where(string attributeName, string matchValue)
        {
            return base.Where(GetMappedPathForAttribute(attributeName),
                matchValue) as RestApiUriQueryBuilderTyped<T>;
        }

        private static string GetMappedPathForAttribute(string attributeName)
        {
            return MapAttribute.GetMappedPathForProperty<T>(attributeName);
        }

        public RestApiUriQueryBuilderTyped<T> Where(Expression<Func<T, object>> field, string matchValue)
        {
            var name = (field.Body as MemberExpression ??
                        ((UnaryExpression)field.Body).Operand as MemberExpression).Member.Name;

            return base.Where(GetMappedPathForAttribute(name), matchValue) as RestApiUriQueryBuilderTyped<T>;
        }

        public new RestApiUriQueryBuilderTyped<T> Filter(string attributeName, string op, object filterValue)
        {
            return base.Filter(GetMappedPathForAttribute(attributeName),
                op, filterValue) as RestApiUriQueryBuilderTyped<T>;
        }

        public RestApiUriQueryBuilderTyped<T> Filter(string attributeName, ComparisonOperator op, object filterValue)
        {
            return base.Filter(GetMappedPathForAttribute(attributeName), 
                op, filterValue) as RestApiUriQueryBuilderTyped<T>;
        }

        public RestApiUriQueryBuilderTyped<T> Filter(Expression<Func<T, object>> field, ComparisonOperator op, string filterValue)
        {
            var name = (field.Body as MemberExpression ??
                        ((UnaryExpression)field.Body).Operand as MemberExpression).Member.Name;

            return base.Filter(GetMappedPathForAttribute(name), 
                op, filterValue) as RestApiUriQueryBuilderTyped<T>;
        }
    }
}
