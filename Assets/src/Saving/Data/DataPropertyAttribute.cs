using System;

namespace Game.Saving.Data
{
    public class DataPropertyAttribute : Attribute
    {
        public static readonly float DEFAULT_WEIGHT = 1.0f;

        public string SaveableName { get; private set; }
        public float Weight { get; private set; }
        public string Description { get; private set; }
        public string DescriptionLocalizationTable { get; private set; }

        public DataPropertyAttribute(string saveableName, float weight, string descriptionLocalizationKey, string descriptionLocalizationTable)
        {
            SaveableName = saveableName;
            Weight = weight;
            Description = descriptionLocalizationKey;
            DescriptionLocalizationTable = descriptionLocalizationTable;
        }

        public DataPropertyAttribute(string saveableName, string descriptionLocalizationKey, string descriptionLocalizationTable)
        {
            SaveableName = saveableName;
            Weight = DEFAULT_WEIGHT;
            Description = descriptionLocalizationKey;
            DescriptionLocalizationTable = descriptionLocalizationTable;
        }

        public DataPropertyAttribute(string saveableName, string description)
        {
            SaveableName = saveableName;
            Weight = DEFAULT_WEIGHT;
            Description = description;
            DescriptionLocalizationTable = null;
        }

        public LString DescriptionL
        {
            get {
                return string.IsNullOrEmpty(DescriptionLocalizationTable) ? Description : new LString(Description, DescriptionLocalizationTable);
            }
        }
    }
}