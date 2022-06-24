using System;

namespace Game.Saving.Data
{
    public class DataPropertyAttribute : Attribute
    {
        public static readonly float DEFAULT_WEIGHT = 1.0f;

        public string SaveableName { get; private set; }
        public float Weight { get; private set; }
        public string Description { get; private set; }
        public bool LocalizeDescription { get; private set; }

        public DataPropertyAttribute(string saveableName, float weight, string description, bool localizeDescription)
        {
            SaveableName = saveableName;
            Weight = weight;
            Description = description;
            LocalizeDescription = localizeDescription;
        }

        public DataPropertyAttribute(string saveableName, string description, bool localizeDescription)
        {
            SaveableName = saveableName;
            Weight = DEFAULT_WEIGHT;
            Description = description;
            LocalizeDescription = localizeDescription;
        }

        public DataPropertyAttribute(string saveableName, float weight, string description)
        {
            SaveableName = saveableName;
            Weight = weight;
            Description = description;
            LocalizeDescription = true;
        }

        public DataPropertyAttribute(string saveableName, string description)
        {
            SaveableName = saveableName;
            Weight = DEFAULT_WEIGHT;
            Description = description;
            LocalizeDescription = true;
        }
    }
}