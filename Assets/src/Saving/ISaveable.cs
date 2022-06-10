using Game.Saving.Data;

namespace Game.Saving
{
    public interface ISaveable {
        public void Save(ref ISaveData data);
    }
}