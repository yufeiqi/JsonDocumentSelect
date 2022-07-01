using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonDocumentSelect.Filters
{
    internal class ArraySliceFilter : PathFilter
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int? Step { get; set; }

        public override IEnumerable<JsonElementExt> ExecuteFilter(JsonElement root, IEnumerable<JsonElementExt> current, bool errorWhenNoMatch)
        {
            if (Step == 0)
            {
                throw new JsonException("Step cannot be zero.");
            }

            foreach (JsonElementExt t in current)
            {
                if (t.Element?.ValueKind == JsonValueKind.Array)
                {
                    var stepCount = StepCountIndex(t.Element.Value, out var startIndex, out var stopIndex, out var positiveStep);

                    if (IsValid(startIndex, stopIndex, positiveStep))
                    {
                        for (int i = startIndex; IsValid(i, stopIndex, positiveStep); i += stepCount)
                        {
                            yield return new JsonElementExt(){ Element = t.Element.Value[i] };
                        }
                    }
                    else if (errorWhenNoMatch)
                    {
                        throw new JsonException(
                            $"Array slice of {(Start != null ? Start.GetValueOrDefault().ToString() : "*")} to {(End != null ? End.GetValueOrDefault().ToString() : "*")} returned no results.");

                    }
                }
                else if (errorWhenNoMatch)
                {
                    throw new JsonException($"Array slice is not valid on {t.GetType().Name}.");
                }
            }
        }

        private int StepCountIndex(JsonElement t, out int startIndex, out int stopIndex, out bool positiveStep)
        {
            var aCount = t.GetArrayLength();
            // set defaults for null arguments
            int stepCount = Step ?? 1;
            startIndex = Start ?? ((stepCount > 0) ? 0 : aCount - 1);
            stopIndex = End ?? ((stepCount > 0) ? aCount : -1);

            // start from the end of the list if start is negative
            if (Start < 0)
            {
                startIndex = aCount + startIndex;
            }

            // end from the start of the list if stop is negative
            if (End < 0)
            {
                stopIndex = aCount + stopIndex;
            }

            // ensure indexes keep within collection bounds
            startIndex = Math.Max(startIndex, (stepCount > 0) ? 0 : int.MinValue);
            startIndex = Math.Min(startIndex, (stepCount > 0) ? aCount : aCount - 1);
            stopIndex = Math.Max(stopIndex, -1);
            stopIndex = Math.Min(stopIndex, aCount);

            positiveStep = (stepCount > 0);
            return stepCount;
        }

        private static bool IsValid(int index, int stopIndex, bool positiveStep)
        {
            if (positiveStep)
            {
                return (index < stopIndex);
            }

            return (index > stopIndex);
        }
    }
}
