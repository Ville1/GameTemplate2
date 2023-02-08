using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{

    public class Stats
    {
        public IHasStats Parent { get; private set; } = null;
        public bool IsPrototype { get { return Parent == null; } }
        public float MinValue { get; private set; } = float.MinValue;
        public List<Stat> List { get { return stats; } }

        public Stat Strength { get { return Get(Stat.Strength); } set { Set(Stat.Strength, value); } }
        public Stat Dexterity { get { return Get(Stat.Dexterity); } set { Set(Stat.Dexterity, value); } }

        public Stat HP { get { return Get(Stat.HP); } set { Set(Stat.HP, value); } }
        public Stat HPRegen { get { return Get(Stat.HPRegen); } set { Set(Stat.HPRegen, value); } }

        private List<Stat> stats;

        public Stats(Stat stat)
        {
            Initialize(new List<Stat>() { stat });
        }

        public Stats(List<Stat> stats)
        {
            Initialize(stats);
        }

        public Stats(Stat stat, float value)
        {
            Initialize(new List<Stat>() { stat }, value);
        }

        public Stats(List<Stat> stats, float baseValue)
        {
            Initialize(stats, baseValue);
        }

        public Stats(Dictionary<Stat, float> stats)
        {
            Initialize(stats);
        }

        public Stats(IHasStats parent, List<Stat> stats, float? minValue = 1.0f)
        {
            Parent = parent;
            Initialize(stats, minValue);
        }

        public Stats(IHasStats parent, List<Stat> stats, float baseValue, float? minValue = 1.0f)
        {
            Parent = parent;
            Initialize(stats, baseValue, minValue);
        }

        public Stats(IHasStats parent, Dictionary<Stat, float> stats, float? minValue = 1.0f)
        {
            Parent = parent;
            Initialize(stats, minValue);
        }

        public Stats(IHasStats parent, Stats stats)
        {
            Parent = parent;
            this.stats = stats.stats.Select(stat => new Stat(stat, stat.BaseValue, this)).ToList();
        }

        public Stats(Stats stats)
        {
            this.stats = stats.stats.Select(stat => new Stat(stat, stat.BaseValue, this)).ToList();
        }

        private void Initialize(List<Stat> stats, float? minValue = null)
        {
            this.stats = stats == null ? new List<Stat>() : stats.Select(stat => new Stat(stat, 0.0f, this)).ToList();
            MinValue = minValue.HasValue ? minValue.Value : float.MinValue;
            Update();
        }

        private void Initialize(List<Stat> stats, float baseValue, float? minValue = null)
        {
            this.stats = stats == null ? new List<Stat>() : stats.Select(stat => new Stat(stat, baseValue, this)).ToList();
            MinValue = minValue.HasValue ? minValue.Value : float.MinValue;
            Update();
        }

        private void Initialize(Dictionary<Stat, float> stats, float? minValue = null)
        {
            this.stats = stats == null ? new List<Stat>() : stats.Select(pair => new Stat(pair.Key, pair.Value, this)).ToList();
            MinValue = minValue.HasValue ? minValue.Value : float.MinValue;
            Update();
        }

        public bool Has(Stat searchStat)
        {
            return stats.Any(stat => stat.TypeId == searchStat.TypeId);
        }

        public Stat Get(Stat searchStat)
        {
            Stat returnStat = stats.FirstOrDefault(stat => stat.TypeId == searchStat.TypeId);
            return returnStat != null ? returnStat : new Stat(searchStat, 0.0f, this);
        }

        public void Set(Stat searchStat, Stat valueStat)
        {
            Set(searchStat, valueStat.IsResource ? valueStat.Amount : valueStat.BaseValue);
        }

        public void Set(Stat searchStat, float value)
        {
            Stat statData = stats.FirstOrDefault(stat => stat.TypeId == searchStat.TypeId);
            if (statData != null) {
                if (statData.IsResource) {
                    statData.Amount = value;
                } else {
                    statData.Value = value;
                }
            } else {
                stats.Add(new Stat(searchStat, value, this));
            }
        }

        public void Add(Stats stats)
        {
            foreach (Stat stat in stats.stats) {
                Set(stat, Get(stat).BaseValue + stat.BaseValue);
            }
        }

        public List<StatModifier> ToModifiers(LString name, LString description)
        {
            return stats.Select(stat => new StatModifier(stat, stat.BaseValue, 0.0f, name, description)).ToList();
        }

        public void Update()
        {
            if (IsPrototype || !(Parent is IHasStatModifiers)) {
                return;
            }
            IHasStatModifiers parent = Parent as IHasStatModifiers;
            List<StatModifier> modifiers = parent.GetStatModifiers();
            foreach (Stat stat in stats) {
                stat.Modifiers.Clear();
                stat.Recalculate();
            }
            foreach (StatModifier modifier in modifiers) {
                Stat stat = stats.FirstOrDefault(s => s.TypeId == modifier.Stat.TypeId);
                if (stat != null) {
                    stat.Modifiers.Add(modifier);
                }
            }
        }

        public float Scale(Dictionary<Stat, float> multipliers)
        {
            float sum = 0.0f;
            foreach (KeyValuePair<Stat, float> pair in multipliers) {
                sum += pair.Value * Get(pair.Key).Value;
            }
            return sum;
        }

        public void Refill()
        {
            foreach (Stat stat in stats) {
                if (stat.IsResource) {
                    stat.Refill();
                }
            }
        }

        public override string ToString()
        {
            return stats.Count == 0 ? "None" : string.Join(", ", stats.Select(stat => stat.ToString()));
        }
    }

    public partial class Stat
    {
        public enum Type { Stat, DerivedStat, ResourceStat, DerivedResourceStat }
        public enum Category { BaseStat, DerivedStat, ResourceStat, RegenStat }
        /// <summary>
        /// Determines how resource stat's Amount is changed when stat gets Recalculate():ed
        /// </summary>
        public enum ResourceRecalculateType
        {
            /// <summary>
            /// Amount does not change
            /// </summary>
            NoChange,
            /// <summary>
            /// Amount / Value - ratio stays same
            /// </summary>
            Relative,
            /// <summary>
            /// Amount is clamped to be between 0 and Amount
            /// </summary>
            Clamp
        }

        private static Dictionary<long, Stat> prototypes = new Dictionary<long, Stat>();

        public static List<Stat> Prototypes { get { InitializePrototypes(); return prototypes.Select(pair => pair.Value).ToList(); } }

        //Base stats
        public static Stat Strength { get { return GetPrototype(0); } }
        public static Stat Dexterity { get { return GetPrototype(1); } }

        //Resource stats
        public static Stat Movement { get { return GetPrototype(100); } }

        public static Stat HP { get { return GetPrototype(1000); } }
        public static Stat HPRegen { get { return GetPrototype(1001); } }

        public long TypeId { get; private set; }
        public Type StatType { get { return IsDerived ? (IsResource ? Type.DerivedResourceStat : Type.DerivedStat) : (IsResource ? Type.ResourceStat : Type.Stat); } }
        public LString Name { get; private set; }
        public LString UIShortText { get; private set; }
        public LString Abbreviation { get; private set; }
        public long UIOrder { get; private set; }
        public SpriteData Sprite { get; private set; }
        public Stats Collection { get; private set; } = null;
        public bool IsPrototype { get { return Collection == null; } }
        public List<StatModifier> Modifiers { get; set; } = new List<StatModifier>();
        public Dictionary<Stat, float> Scaling { get; private set; } = new Dictionary<Stat, float>();
        public bool IsDerived { get { return Scaling != null && Scaling.Count != 0; } }
        public bool IsResource { get { return ResourceRecalculate.HasValue; } }
        public ResourceRecalculateType? ResourceRecalculate { get; private set; } = null;
        public SubCategory UISubCategory { get; private set; } = null;

        private float baseValue = 0.0f;
        private float? oldValue = null;
        private float? value = null;
        private float amount = 0.0f;
        private Stat regenStat = null;
        private Category? category = null;

        /// <summary>
        /// Prototype constructor
        /// </summary>
        private Stat(long id, LString name, LString abbreviation, LString uiShortText, long uiOrder, SubCategory uiCategory, SpriteData sprite, ResourceRecalculateType? resourceRecalculateType = null, Stat regen = null)
        {
            TypeId = id;
            Name = name;
            Abbreviation = abbreviation;
            UIShortText = uiShortText;
            UIOrder = uiOrder;
            UISubCategory = uiCategory;
            Sprite = sprite == null ? new SpriteData() : sprite.Copy;
            ResourceRecalculate = resourceRecalculateType;
            Regen = regen;
        }


        /// <summary>
        /// Prototype constructor
        /// </summary>
        private Stat(long id, LString name, LString abbreviated, LString uiShortText, long uiOrder, SubCategory uiCategory, SpriteData sprite, Dictionary<Stat, float> scaling, ResourceRecalculateType? resourceRecalculateType = null, Stat regen = null)
        {
            TypeId = id;
            Name = name;
            Abbreviation = abbreviated;
            UIShortText = uiShortText;
            UIOrder = uiOrder;
            UISubCategory = uiCategory;
            Sprite = sprite == null ? new SpriteData() : sprite.Copy;
            Scaling = new Dictionary<Stat, float>();
            foreach (KeyValuePair<Stat, float> pair in scaling) {
                Scaling.Add(pair.Key, pair.Value);
            }
            ResourceRecalculate = resourceRecalculateType;
            Regen = regen;
        }

        public Stat(Stat stat, float value, Stats collection)
        {
            TypeId = stat.TypeId;
            Name = stat.Name;
            Abbreviation = stat.Abbreviation;
            UIShortText = stat.UIShortText;
            UIOrder = stat.UIOrder;
            UISubCategory = stat.UISubCategory;
            Sprite = stat.Sprite.Copy;
            ResourceRecalculate = stat.ResourceRecalculate;
            Regen = stat.Regen;
            Value = value;
            if (IsResource) {
                Amount = value;
            }
            Collection = collection;
            if (stat.Scaling != null) {
                Scaling = new Dictionary<Stat, float>();
                foreach (KeyValuePair<Stat, float> pair in stat.Scaling) {
                    Scaling.Add(pair.Key, pair.Value);
                }
            } else {
                Scaling = null;
            }
        }

        public float BaseValue
        {
            get {
                return baseValue;
            }
            set {
                baseValue = value;
                this.value = null;
                if (!oldValue.HasValue) {
                    oldValue = value;
                }
            }
        }

        /// <summary>
        /// NOTE: If you use setter for with this property with += (for example Stats.Strength.Value += 1.0f;) or other operator like it, it will use current value as part of operation
        /// instead of base value. This can lead to an unexpected new base value. Using these operations to target the Stat-object itself will use base value instead. (for example Stats.Strength += 1.0f;)
        /// Alternatively you can also use BaseValue-property. (for example Stats.Strength.BaseValue += 1.0f;)
        /// </summary>
        public float Value
        {
            get {
                if (value.HasValue) {
                    return value.Value;
                }
                if (IsPrototype) {
                    value = BaseValue;
                    return value.Value;
                }

                //Recalculate
                //Start with base
                float newValue = BaseValue;

                //Add derived stat scaling
                if (IsDerived) {
                    foreach (KeyValuePair<Stat, float> pair in Scaling) {
                        newValue += pair.Value * Collection.Get(pair.Key).Value;
                    }
                }

                //Modifiers
                //Flat first
                newValue += Modifiers.Where(modifier => modifier.Order == StatModifier.ModificationOrder.FlatFirst).Select(modifier => modifier.FlatValue).Sum();

                //Multiply
                float multiplier = 1.0f + Modifiers.Select(modifier => modifier.Multiplier - 1.0f).Sum();
                newValue *= multiplier;

                //Multiply first
                newValue += Modifiers.Where(modifier => modifier.Order == StatModifier.ModificationOrder.MultiplierFirst).Select(modifier => modifier.FlatValue).Sum();

                //Limit with min value
                value = Math.Max(newValue, Collection.MinValue);

                if (IsResource) {
                    //Resource amount
                    switch (ResourceRecalculate) {
                        case ResourceRecalculateType.Relative:
                            if (oldValue.Value == 0.0f) {
                                amount = 0.0f;
                            } else {
                                amount = (amount / oldValue.Value) * value.Value;
                            }
                            break;
                        case ResourceRecalculateType.Clamp:
                            Amount = Math.Clamp(amount, 0.0f, value.Value);
                            break;
                    }
                }

                return value.Value;
            }
            set {
                BaseValue = value;
            }
        }

        public void Recalculate()
        {
            if (value.HasValue) {
                oldValue = value.Value;
                value = null;
            }
        }

        public float Amount
        {
            get {
                return IsResource ? amount : 0.0f;
            }
            set {
                if (IsResource) {
                    amount = value;
                } else {
                    throw new InvalidOperationException("Can't set Amount for a non resource stat");
                }
            }
        }

        public void Refill()
        {
            Amount = Value;
        }

        public Stat Regen
        {
            get {
                return regenStat != null && Collection != null ? Collection.Get(regenStat) : regenStat;
            }
            set {
                regenStat = value;
            }
        }

        public Category StatCategory
        {
            get {
                if (category.HasValue) {
                    return category.Value;
                }
                return StatType == Type.ResourceStat || StatType == Type.DerivedResourceStat ? Category.ResourceStat :
                    (StatType == Type.DerivedStat ? (prototypes.Any(stat => stat.Value.Regen != null && stat.Value.Regen.TypeId == TypeId) ? Category.RegenStat : Category.DerivedStat)
                    : Category.BaseStat);
            }
            private set {
                category = value;
            }
        }

        public override string ToString()
        {
            return IsPrototype ? Name : (IsResource ? string.Format("{0} = {1}/{2}", Name, Amount, Value) : string.Format("{0} = {1}", Name, Value));
        }

        public static Stat operator +(Stat stat, float delta)
        {
            if (stat.IsResource) {
                stat.Amount += delta;
            } else {
                stat.Value = stat.BaseValue + delta;
            }
            return stat;
        }

        public static Stat operator -(Stat stat, float delta)
        {
            if (stat.IsResource) {
                stat.Amount -= delta;
            } else {
                stat.Value = stat.BaseValue - delta;
            }
            return stat;
        }

        public static Stat operator *(Stat stat, float multiplier)
        {
            if (stat.IsResource) {
                stat.Amount *= multiplier;
            } else {
                stat.Value = stat.BaseValue * multiplier;
            }
            return stat;
        }

        public static Stat operator /(Stat stat, float divider)
        {
            if (stat.IsResource) {
                stat.Amount /= divider;
            } else {
                stat.Value = stat.BaseValue / divider;
            }
            return stat;
        }

        public static implicit operator float(Stat stat)
        {
            return stat == null ? 0.0f : (stat.IsResource ? stat.Amount : stat.Value);
        }

        private static Stat GetPrototype(long id)
        {
            InitializePrototypes();
            return prototypes[id];
        }

        public class SubCategory
        {
            public long Id { get; private set; }
            public LString Name { get; private set; }

            public SubCategory(long id, LString name)
            {
                Id = id;
                Name = name;
            }

            public static SubCategory Defence { get { return new SubCategory(0, "test"); } }
            public static SubCategory Accuracy { get { return new SubCategory(1, "test 2"); } }
            public static SubCategory Evasion { get { return new SubCategory(2, "test 3"); } }
        }
    }

    public class StatModifier
    {
        public enum ModificationOrder { FlatFirst, MultiplierFirst }
        public enum StatModifierCategory { Equipment, StatusEffects }

        public Stat Stat { get; private set; }
        public float FlatValue { get; private set; }
        /// <summary>
        /// 1 = no change
        /// </summary>
        public float Multiplier { get; private set; }
        public ModificationOrder Order { get; private set; }
        public LString Name { get; set; } = null;
        public LString Description { get; set; } = null;
        public StatModifierCategory? Category { get; set; } = null;

        public StatModifier(Stat stat, float flatValue, float multiplier = 1.0f, ModificationOrder order = ModificationOrder.FlatFirst, StatModifierCategory? category = null)
        {
            Stat = stat;
            FlatValue = flatValue;
            Multiplier = multiplier;
            Order = order;
            Category = category;
        }

        public StatModifier(Stat stat, float flatValue, float multiplier, LString name, LString description, ModificationOrder order = ModificationOrder.FlatFirst)
        {
            Stat = stat;
            FlatValue = flatValue;
            Multiplier = multiplier;
            Name = name;
            Description = description;
            Order = order;
        }
    }

    public interface IHasStats
    {
        Stats Stats { get; }
    }

    public interface IHasStatModifiers
    {
        List<StatModifier> GetStatModifiers();
    }
}
