using System;

namespace Game.Saving.Data
{
    public class DataPropertyAttribute : Attribute
    {
        public string SaveableName { get; private set; }
        public string Description { get; private set; }
        public bool LocalizeDescription { get; private set; }

        public DataPropertyAttribute(string saveableName, string description, bool localizeDescription)
        {
            SaveableName = saveableName;
            Description = description;
            LocalizeDescription = localizeDescription;
        }

        public DataPropertyAttribute(string saveableName, string description)
        {
            SaveableName = saveableName;
            Description = description;
            LocalizeDescription = true;
        }
    }
}