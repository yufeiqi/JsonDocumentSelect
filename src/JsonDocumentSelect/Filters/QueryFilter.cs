using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class QueryFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (JsonElementExt el in current)
            {
                if (el.Element?.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in el.Element.Value.EnumerateArray().Where(v => Expression.IsMatch(root, v)))
                    {
                        yield return new JsonElementExt(){ Element = v };
                    }
                }
                else if (el.Element?.ValueKind == JsonValueKind.Object)
                {
                    foreach (var v in el.Element.Value.EnumerateObject().Where(v => Expression.IsMatch(root, v.Value)))
                    {
                        yield return new JsonElementExt() {Element = v.Value, Name = v.Name};
                    }
                }
            }
        }
    }
}
