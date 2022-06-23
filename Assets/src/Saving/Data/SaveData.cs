using System;

namespace Game.Saving.Data
{
    [Serializable]
    public class SaveData
    {
        [DataProperty("Map", "Map", false)]
        public Map Map;
    }
}
