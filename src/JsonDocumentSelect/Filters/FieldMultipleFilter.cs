using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            foreach (JsonElementExt t in current)
            {
                if (t.Element?.ValueKind == JsonValueKind.Object)
                {
                    foreach (string name in Names)
                    {
                        if (!t.Element.Value.TryGetProperty(name, out JsonElement v)) 
                            continue;

                        if (v.ValueKind != JsonValueKind.Null)
                        {
                            yield return new JsonElementExt(){ Element = v, Name = name };
                        }
                        else if (errorWhenNoMatch)
                        {
                            throw new JsonException($"Property '{name}' does not exist on JObject.");
                        }
                    }
                }
                else if (errorWhenNoMatch)
                {
                    throw new JsonException(
                        $"Properties {string.Join(", ", Names.Select(n => "'" + n + "'").ToArray())} not valid on {t.GetType().Name}.");

                }
            }
        }
    }
}
