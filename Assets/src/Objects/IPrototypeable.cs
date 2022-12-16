using System;


namespace Game.Objects
{
    public interface IPrototypeable
    {
        public string InternalName { get; }
        public IPrototypeable Clone { get; }
    }
}
