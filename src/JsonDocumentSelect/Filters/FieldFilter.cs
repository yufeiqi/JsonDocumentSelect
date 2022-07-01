using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class FieldFilter : PathFilter
    {
        internal string Name;

        public FieldFilter(string name)
        {
            Name = name;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (var t in current)
            {
                if (t?.Element?.ValueKind == JsonValueKind.Object)
                {
                    if (Name != null)
                    {
                        foreach (var jsonElementExt in GetJsonElements(errorWhenNoMatch, t.Element.Value))
                        {
                            yield return jsonElementExt;
                        }
                    }
                    else
                    {
                        foreach (var p in t.Element.Value.ChildrenTokens())
                        {
                            yield return new JsonElementExt() {Element = p};
                        }
                    }
                }
                else if (errorWhenNoMatch)
                {
                    throw new JsonException($"Property '{Name ?? "*"}' not valid on {t?.GetType().Name}.");
                }
            }
        }
        
        private IEnumerable<JsonElementExt> GetJsonElements(bool errorWhenNoMatch, JsonElement t)
        {
            if (t.TryGetProperty(Name, out JsonElement v))
            {
                if (v.ValueKind != JsonValueKind.Null)
                {
                    yield return new JsonElementExt() {Element = v, Name = Name};
                }
                else if (errorWhenNoMatch)
                {
                    throw new JsonException($"Property '{Name}' does not exist on JObject.");
                }
            }
        }
    }
}
