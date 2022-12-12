using System;

namespace Game.Saving.Data
{
    [Serializable]
    public class SaveData
    {
        //Example map data
        [DataProperty("Map", "Map")]
        public Map Map;

        [DataProperty("NameManagerSaveHelper", 0.01f, "Names", "Game")]
        public NameManagerData Names;
    }
}
