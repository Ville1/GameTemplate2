using Game.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Game.Objects
{
    public class PrototypeManager<PrototypeType> where PrototypeType : IPrototypeable
    {

        protected List<PrototypeType> prototypes = new List<PrototypeType>();

        public PrototypeManager()
        { }

        public virtual bool Has(string internalName)
        {
            return prototypes.Any(prototype => prototype.InternalName == internalName);
        }

        public virtual PrototypeType Get(string internalName)
        {
            PrototypeType prototype = prototypes.FirstOrDefault(p => p.InternalName == internalName);
            if(prototype == null) {
                CustomLogger.Error("{PrototypeDoesNotExist}", typeof(PrototypeType).Name, internalName);
                return default(PrototypeType);
            }
            return prototype;
        }
    }
}
