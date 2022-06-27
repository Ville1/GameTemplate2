using System;

namespace Game.Saving.Data
{
    [Serializable]
    public class SaveData
    {
        [DataProperty("Map", "Map")]
        public Map Map;
    }
}
