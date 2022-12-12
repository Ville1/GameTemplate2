using System;
using System.Collections.Generic;

namespace Game.Saving.Data
{
    [Serializable]
    public class NameManagerData : ISaveData
    {
        public List<NameManagerUsedNameListData> UsedNames;
    }

    [Serializable]
    public class NameManagerUsedNameListData : ISaveData
    {
        public int Type;
        public List<NameManagerUsedNameData> Names;
    }

    [Serializable]
    public class NameManagerUsedNameData : ISaveData
    {
        public string Name;
        public long Times;
    }
}
