using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class RootFilter : PathFilter
    {
        public static readonly RootFilter Instance = new RootFilter();

        private RootFilter()
        {
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            return new List<JsonElementExt>()
            {
                new JsonElementExt()
                {
                     Element = root
                }
            };
        }
    }
}
