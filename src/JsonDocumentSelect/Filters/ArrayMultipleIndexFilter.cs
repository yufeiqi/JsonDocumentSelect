using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class ArrayMultipleIndexFilter : PathFilter
    {
        internal List<int> Indexes;

        public ArrayMultipleIndexFilter(List<int> indexes)
        {
            Indexes = indexes;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current,
            bool errorWhenNoMatch)
        {
            return from t in current
                from i in Indexes
                let jsonElement = t.Element
                where jsonElement != null
                select GetTokenIndex(jsonElement.Value, errorWhenNoMatch, i)
                into v
                where v != null
                select v;
        }
    }
}
