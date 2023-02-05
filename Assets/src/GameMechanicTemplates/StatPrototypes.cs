using Game.Utils;
using System.Collections.Generic;

namespace Game
{
    public partial class Stat
    {
        private static void InitializePrototypes()
        {
            if (prototypes.Count != 0) {
                return;
            }

            //Base stats
            prototypes = new Dictionary<long, Stat>();

            SetPrototype(0, "Strength", "str", null, 0, null, null);
            SetPrototype(1, "Dexterity", "dex", null, 1, null, null);

            //Resource stats
            //Movement
            SetPrototype(100, "Movement", "move", null, 0, null, null, null, ResourceRecalculateType.Clamp);

            //HP
            SetPrototype(1001, "Health regen", "HP regen", "HP regen", 1, null, null, new Dictionary<Stat, float>() {
                { Strength, 0.05f }
            });
            SetPrototype(1000, "Health", "HP", null, 0, null, null, new Dictionary<Stat, float>() {
                { Strength, 1.5f }
            }, ResourceRecalculateType.Relative, HPRegen);
        }

        private static void SetPrototype(long id, LString name, LString abbreviation, LString uiShortText, long uiOrder, SubCategory uiCategory, string sprite,
            Dictionary<Stat, float> scaling = null, ResourceRecalculateType? resourceRecalculateType = null, Stat regen = null)
        {
            if (!prototypes.ContainsKey(id)) {
                SpriteData spriteData = string.IsNullOrEmpty(sprite) ? new SpriteData() : new SpriteData(sprite, TextureDirectory.UI);
                if (scaling == null) {
                    prototypes.Add(id, new Stat(id, name, abbreviation, uiShortText == null ? name : uiShortText, uiOrder, uiCategory, spriteData, resourceRecalculateType, regen));
                } else {
                    prototypes.Add(id, new Stat(id, name, abbreviation, uiShortText == null ? name : uiShortText, uiOrder, uiCategory, spriteData, scaling, resourceRecalculateType, regen));
                }
            } else {
                CustomLogger.Error("{StatPrototypeAlreadyCreated}", id);
            }
        }
    }
}
