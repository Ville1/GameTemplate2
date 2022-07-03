namespace Game
{
    public class SpriteData : IHasSprite
    {
        public string Sprite { get; set; }
        public TextureDirectory SpriteDirectory { get; set; }
        public int Order { get; set; }

        public SpriteData(string sprite, TextureDirectory spriteDirectory, int order = 0)
        {
            Sprite = sprite;
            SpriteDirectory = spriteDirectory;
            Order = order;
        }

        public SpriteData(SpriteData data)
        {
            Sprite = data.Sprite;
            SpriteDirectory = data.SpriteDirectory;
            Order = data.Order;
        }

        public SpriteData Copy
        {
            get{
                return new SpriteData(this);
            }
        }
    }
}
