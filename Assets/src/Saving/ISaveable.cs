using Game.Saving.Data;

namespace Game.Saving
{
    public interface ISaveable {
        public void StartSaving(ref ISaveData data);
        public float Save(ref ISaveData data);
        public void StartLoading(ISaveData data);
        public float Load(ISaveData data);
    }
}