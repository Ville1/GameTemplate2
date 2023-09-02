using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public enum DamageType {//Int values are used in sorting
        Physical = 0,
        Magical = 1,
        Fire = 10,
        Water = 11
    }

    public class Damage
    {
        public enum DeltaType {
            /// <summary>
            /// Creates new segments. Exactly same segments get added together. Examples:
            /// (10 physical/fire) + (2 physical) = (10 physical/fire, 2 physical)
            /// (10 physical/fire) + (2 physical/fire) = (12 physical/fire)
            /// </summary>
            NewSegments,
            /// <summary>
            /// Adds full amount to segments. Examples:
            /// (5 physical/fire, 2 magical) + (1 physical) = (6 physical/fire, 2 magical)
            /// </summary>
            Full,
            /// <summary>
            /// Adds relative amount to segments. Example: (9 physical/fire/water) + (3 physical/fire) = (11 physical/fire/water)
            /// </summary>
            Relative
        }

        public enum MultiplicationType {
            /// <summary>
            /// Multiplies segment amounts relative to type's portion and creates new segments. Examples:
            /// (5 magical/fire) * (2 fire) = (10 fire, 5 magical)
            /// NOT IMPLEMENTED
            /// </summary>
            NewSegments,
            /// <summary>
            /// Multiplies segment amounts relative to type's portion. Examples:
            /// (5 magical/fire) * (2 fire) = (7.5 magical/fire) Fire damage component is half of the damage is 2.5, so doubling that increases total amount by 2.5
            /// (5 magical/fire) * (0 fire) = (2.5 magical/fire)
            /// (5 magical/fire) * (-1 fire) = (0 magical/fire)
            /// </summary>
            Relative,
            /// <summary>
            /// Multiplies all component containing type. This one is recomended as it is propably the most intuitive for player. Examples:
            /// (5 magical/fire) * (2 fire) = (10 magical/fire) (Takes twice the damage from fire, this being magical fire does not change anything)
            /// (5 magical/fire) * (0 magical) = (0 magical/fire) (Magic immunity)
            /// </summary>
            Full
        }

        public float Amount { get { return segments.Select(segment => segment.Amount).Sum(); } }
        public Dictionary<DamageType, float> RelativeAmounts { get; private set; } = new Dictionary<DamageType, float>();

        private List<DamageSegment> segments = new List<DamageSegment>();

        public Damage() { }

        public Damage(Damage damage) {
            segments = damage.segments.Select(segment => new DamageSegment() {
                Amount = segment.Amount,
                Types = segment.Types.Copy()
            }).ToList();
            RelativeAmounts = DictionaryHelper.Copy(damage.RelativeAmounts);
        }

        public Damage(float amount, DamageType type)
        {
            segments.Add(new DamageSegment() {
                Amount = amount,
                Types = new List<DamageType> { type }
            });
            SortCalculateRelativeAmounts();
        }

        public Damage(float amount, List<DamageType> types)
        {
            segments.Add(new DamageSegment() {
                Amount = amount,
                Types = types == null ? new List<DamageType>() : types.Copy()
            });
            SortCalculateRelativeAmounts();
        }

        public Damage(float amount, params DamageType[] types)
        {
            segments.Add(new DamageSegment() {
                Amount = amount,
                Types = types.ToList()
            });
            SortCalculateRelativeAmounts();
        }

        public Damage(Dictionary<List<DamageType>, float> damage)
        {
            segments = damage.Select(pair => new DamageSegment() {
                Amount = pair.Value,
                Types = pair.Key.Copy()
            }).ToList();
            SortCalculateRelativeAmounts();
        }

        public Damage Add(float delta)
        {
            //Note: We don't need Sort() here, since this operation should not change any orders
            if(segments.Count == 0) {
                if(delta == 0.0f) {
                    return this;
                }
                segments.Add(new DamageSegment() { Amount = delta, Types = new List<DamageType>() });
                CalculateRelativeAmounts();
                return this;
            }
            if(segments.Count == 1) {
                segments[0].Amount += delta;
                CalculateRelativeAmounts();
                return this;
            }
            float amount = Amount;
            if(amount == 0.0f) {
                segments[0].Amount += delta;
                CalculateRelativeAmounts();
                return this;
            }
            float multiplier = (delta + amount) / amount;
            foreach(DamageSegment segment in segments) {
                segment.Amount *= multiplier;
            }
            return this;
        }

        public Damage Subtract(float delta)
        {
            return Add(-1.0f * delta);
        }

        public Damage Add(Damage damage, DeltaType deltaType = DeltaType.Full)
        {
            switch (deltaType) {
                case DeltaType.NewSegments:
                    foreach (DamageSegment newSegment in damage.segments) {
                        DamageSegment identicalSegment = segments.FirstOrDefault(s => s.TypeString == newSegment.TypeString);
                        if (identicalSegment != null) {
                            identicalSegment.Amount += newSegment.Amount;
                        } else {
                            segments.Add(new DamageSegment() {
                                Amount = newSegment.Amount,
                                Types = newSegment.Types.Copy()
                            });
                        }
                    }
                    break;
                case DeltaType.Full:
                    foreach(DamageSegment segment in segments) {
                        foreach(DamageSegment deltaSegment in damage.segments) {
                            if(segment.Types.Any(type => deltaSegment.Types.Contains(type))) {
                                segment.Amount += deltaSegment.Amount;
                            }
                        }
                    }
                    break;
                case DeltaType.Relative:
                    throw new NotImplementedException();
                    //break;
                default:
                    throw new NotImplementedException(deltaType.ToString());
            }
            SortCalculateRelativeAmounts();
            return this;
        }

        public Damage Reduce(Damage damage, DeltaType deltaType = DeltaType.NewSegments)
        {
            damage.Multiply(-1.0f);
            return Add(damage, deltaType);
        }

        public Damage Combine(Damage damage)
        {
            return Add(damage, DeltaType.NewSegments);
        }

        public Damage Multiply(float multiplier)
        {
            //Note: We don't need Sort() here, since this operation should not change any orders
            foreach (DamageSegment segment in segments) {
                segment.Amount *= multiplier;
            }
            return this;
        }

        public Damage Divide(float divisor)
        {
            //Note: We don't need Sort() here, since this operation should not change any orders
            foreach (DamageSegment segment in segments) {
                segment.Amount /= divisor;
            }
            return this;
        }

        public Damage Multiply(DamageType type, float multiplier, MultiplicationType multiplicationType = MultiplicationType.Full)
        {
            if (!RelativeAmounts.ContainsKey(type)) {
                return this;
            }

            foreach(DamageSegment segment in segments) {
                if (segment.Types.Contains(type)) {
                    switch (multiplicationType) {
                        case MultiplicationType.Relative:
                            segment.Amount += (multiplier - 1.0f) * (segment.Amount / segment.Types.Count);
                            break;
                        case MultiplicationType.Full:
                            segment.Amount *= multiplier;
                            break;
                        case MultiplicationType.NewSegments:
                            throw new NotImplementedException(multiplicationType.ToString());
                        default:
                            throw new NotImplementedException(multiplicationType.ToString());
                    }
                }
            }

            SortCalculateRelativeAmounts();
            return this;
        }

        public Damage Copy
        {
            get {
                return new Damage(this);
            }
        }

        public void ClampSegments(float min)
        {
            ClampSegments(min, float.MaxValue);
        }

        public void ClampSegments(float min, float max)
        {
            foreach (DamageSegment segment in segments) {
                segment.Amount = Math.Clamp(segment.Amount, min, max);
            }
            SortCalculateRelativeAmounts();
        }

        private void Sort()
        {
            //Maybe remove 0 damage segments?
            //Well, they propably should stay so if damage gets reduced to 0 so we can see what types it originally had

            //Remove duplicated types from DamageSegment.Types lists and order the remaining types
            segments = segments.Select(segment => {
                segment.Types = segment.Types.Distinct().OrderBy(type => (int)type).ToList();
                return segment;
            }).ToList();

            //Add together segments with same Types
            segments = segments.GroupBy(segment => string.Join(",", segment.Types.Select(type => (int)type))).Select(grouping => {
                DamageSegment segment = grouping.ElementAt(0);
                if (grouping.Count() == 1) {
                    return segment;
                }
                segment.Amount += grouping.Skip(1).Select(segment => segment.Amount).Sum();
                return segment;
            }).ToList();

            //Order by amount
            segments = segments.OrderByDescending(segment => segment.Amount).ToList();
        }

        private void CalculateRelativeAmounts()
        {
            Dictionary<DamageType, float> totals = new Dictionary<DamageType, float>();
            float total = 0.0f;
            foreach(DamageSegment segment in segments) {
                foreach(DamageType type in segment.Types) {
                    float amount = segment.Amount / segment.Types.Count;
                    total += amount;
                    if (totals.ContainsKey(type)) {
                        totals[type] += amount;
                    } else {
                        totals.Add(type, amount);
                    }
                }
            }
            RelativeAmounts = totals.Select(pair => new Tuple<DamageType, float>(pair.Key, pair.Value / total)).ToDictionary(pair => pair.Item1, pair => pair.Item2);
        }

        private void SortCalculateRelativeAmounts()
        {
            Sort();
            CalculateRelativeAmounts();
        }

        public string ParseString(LString typelessText, LString totalText)
        {
            return segments.Count == 0 ? "0 " + typelessText : string.Join(", ", segments.Select(segment => segment.ParseString(typelessText))) +
                (segments.Count != 1 ? string.Format(" ({0}: {1})", totalText, Amount) : string.Empty);
        }

        public override string ToString()
        {
            return ParseString("Typeless", "Total");
        }
    }

    public class DamageSegment
    {
        public float Amount { get; set; }
        public List<DamageType> Types { get; set; }

        public string TypeString
        {
            get {
                return string.Join(",", Types.Select(t => (int)t).OrderBy(t => t));
            }
        }

        public string ParseString(LString typelessText)
        {
            return string.Format("{0} {1}", Amount, Types.Count == 0 ? typelessText : string.Join("/", Types.Select(type => type.ToString())));
        }
    }

    public class DamageModification
    {
        public enum CalculationOrder { FlatFirst, MultipliersFirst }

        public Dictionary<DamageType, float> Flat { get; set; } = new Dictionary<DamageType, float>();
        public Dictionary<DamageType, float> Multipliers { get; set; } = new Dictionary<DamageType, float>();
        public bool AllowNegativeResults { get; set; } = true;
        public CalculationOrder Order { get; set; } = CalculationOrder.FlatFirst;

        public DamageModification() { }

        public DamageModification(Dictionary<DamageType, float> flat, Dictionary<DamageType, float> multipliers, bool allowNegativeResults = true, CalculationOrder order = CalculationOrder.FlatFirst)
        {
            Flat = flat == null ? new Dictionary<DamageType, float>() : DictionaryHelper.Copy(flat);
            Multipliers = multipliers == null ? new Dictionary<DamageType, float>() : DictionaryHelper.Copy(multipliers);
            AllowNegativeResults = allowNegativeResults;
            Order = order;
        }

        public DamageModification(DamageModification modification)
        {
            Flat = DictionaryHelper.Copy(modification.Flat);
            Multipliers = DictionaryHelper.Copy(modification.Multipliers);
            AllowNegativeResults = modification.AllowNegativeResults;
            Order = modification.Order;
        }

        public void Apply(Damage damage)
        {
            switch (Order) {
                case CalculationOrder.FlatFirst:
                    ApplyFlat(damage);
                    ApplyMultipliers(damage);
                    break;
                case CalculationOrder.MultipliersFirst:
                    ApplyMultipliers(damage);
                    ApplyFlat(damage);
                    break;
                default:
                    throw new NotImplementedException(Order.ToString());
            }
        }

        private void ApplyFlat(Damage damage)
        {
            foreach (KeyValuePair<DamageType, float> pair in Flat) {
                damage.Add(new Damage(pair.Value, pair.Key));
            }
            if (!AllowNegativeResults) {
                damage.ClampSegments(0.0f);
            }
        }

        private void ApplyMultipliers(Damage damage)
        {
            foreach(KeyValuePair<DamageType, float> pair in Multipliers) {
                damage.Multiply(pair.Key, pair.Value);
            }
        }
    }

    public class DamageReduction : DamageModification
    {
        public DamageReduction(Dictionary<DamageType, float> flat, Dictionary<DamageType, float> multipliers) : base(
            flat == null ? new Dictionary<DamageType, float>() :
            flat.Select(pair => {
                return new Tuple<DamageType, float>(pair.Key, pair.Value * -1.0f);
            }).ToDictionary(pair => pair.Item1, pair => pair.Item2),
            multipliers == null ? new Dictionary<DamageType, float>() :
            multipliers.Select(pair => {
                return new Tuple<DamageType, float>(pair.Key, 1.0f - pair.Value);
            }).ToDictionary(pair => pair.Item1, pair => pair.Item2),
            false)
        { }

        public DamageReduction(DamageModification modification)
        { }
    }
}
