namespace Game
{
    public enum State { MainMenu, Running, Saving, Loading, GeneratingMap }
    public enum LogLevel { Debug, Warning, Error }
    public enum TextureDirectory { Sprites, Terrain }
    public enum KeyEventTag { IgnoreUI, Camera }
    public enum MouseEventTag { IgnoreUI, Map }
    public enum WindowEvent { Close, Accept }
    public enum MouseButton { Left = 0, Middle = 2, Right = 1 }//Int value should match parameter of Input.GetMouseButton - methods
}
