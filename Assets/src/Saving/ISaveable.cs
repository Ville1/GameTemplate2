using Game.Saving.Data;

namespace Game.Saving
{
    public interface ISaveable {
        public bool Save(ref ISaveData data);
    }
}