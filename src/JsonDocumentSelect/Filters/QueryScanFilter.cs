using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class QueryScanFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (JsonElementExt t in current)
            {
                foreach (var (name, v) in GetNextScanValue(t))
                {
                    if (v.Element != null && Expression.IsMatch(root, v.Element.Value))
                    {
                        yield return new JsonElementExt(){ Element = v.Element, Name = name };
                    }
                }
            }
        }
    }
}
