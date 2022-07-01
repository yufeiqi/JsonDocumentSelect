using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class ArrayIndexFilter : PathFilter
    {
        public int? Index { get; set; }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (JsonElementExt t in current)
            {
                if (Index != null && t.Element != null)
                {
                    yield return GetTokenIndex(t.Element.Value, errorWhenNoMatch, Index.GetValueOrDefault());
                }
                else
                {
                    if (t.Element?.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement v in t.Element.Value.EnumerateArray())
                        {
                            yield return new JsonElementExt{ Element = v };
                        }
                    }
                    else if (errorWhenNoMatch)
                    {
                        throw new JsonException($"Index * not valid on {t.GetType().Name}.");
                    }
                }
            }
        }
    }
}
