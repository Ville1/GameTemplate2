using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Utils
{
    public class MathHelper
    {
        public static float WeightedAverage(List<Tuple<float, float>> values)
        {
            return WeightedAverage(new WeightedFloats(values));
        }

        //Can be used with collection initializer: MathHelper.WeightedAverage(new WeightedFloats() { { 10.0f, 1.0f }, { 3.0f, 2.0f } })
        public static float WeightedAverage(WeightedFloats values)
        {
            //Remove values with weight of 0
            values.Values = values.Values.Where(tuple => tuple.Item2 != 0.0f).ToList();

            //Disallow weights that are < 0
            if (values.Values.Any(tuple => tuple.Item2 < 0.0f)) {
                throw new ArgumentException("Weights can't be less than zero");
            }

            if (values.Count() == 0) {
                //Empty list
                return 0.0f;
            }
            if (values.Count() == 1) {
                //Only 1 value
                return values.Values[0].Item1;
            }

            float sum = 0.0f;
            float divider = 0.0f;
            foreach (Tuple<float, float> tuple in values.Values) {
                sum += tuple.Item1 * tuple.Item2;
                divider += tuple.Item2;
            }

            return sum / divider;
        }
    }

    public class WeightedFloats : IEnumerable<Tuple<float, float>>
    {
        public List<Tuple<float, float>> Values { get; set; }

        public WeightedFloats()
        {
            Values = new List<Tuple<float, float>>();
        }

        public WeightedFloats(List<Tuple<float, float>> values)
        {
            Values = values == null ? new List<Tuple<float, float>>() : values.Select(tuple => new Tuple<float, float>(tuple.Item1, tuple.Item2)).ToList();
        }

        public void Add(float value, float weight)
        {
            Values.Add(new Tuple<float, float>(value, weight));
        }

        public IEnumerator<Tuple<float, float>> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}
