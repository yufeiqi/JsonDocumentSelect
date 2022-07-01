using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class ScanFilter : PathFilter
    {
        internal string Name;

        public ScanFilter(string name)
        {
            Name = name;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (JsonElementExt c in current)
            {
                foreach (var e in GetNextScanValue(c))
                {
                    if (e.Name == Name)
                    {
                        yield return e.Value;
                    }
                }
            }
        }
    }
}
