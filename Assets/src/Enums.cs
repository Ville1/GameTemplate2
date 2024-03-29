namespace Game
{
    public enum State { MainMenu, Running, Saving, Loading, GeneratingMap }
    public enum LogLevel { Debug, Warning, Error }
    public enum TextureDirectory { Sprites, Terrain, Effects, UI }
    public enum KeyEventTag {
        /// <summary>
        /// Event can't be blocked by ui
        /// </summary>
        IgnoreUI,
        Camera
    }
    public enum MouseEventTag {
        /// <summary>
        /// Event can't be blocked by an open window. (Note: this checks for open windows, while MouseEventData.IsBlockedByUI checks for any ui element) (TODO: rename this to IgnoreWindows?)
        /// </summary>
        IgnoreUI,
        Map
    }
    public enum WindowEvent { Close, Accept, Cancel }
    public enum MouseButton { Left = 0, Middle = 2, Right = 1 }//Int value should match parameter of Input.GetMouseButton - methods
    public enum MouseDragEventType { Start, Move, End }
    public enum MouseOverEventType { Enter, Over, Exit }

    public enum AnimationQueue
    {
        /// <summary>
        /// Stops current animation and starts playing the new animation
        /// </summary>
        StopCurrent,
        /// <summary>
        /// Skips the new animation
        /// </summary>
        Skip,
        /// <summary>
        /// Sets the new animation to be played after the current one. If there is already an animation in queue, it gets replaced.
        /// </summary>
        QueueOne,
        /// <summary>
        /// Sets the new animation to be played after the current one. Allows multiple animations to be queued.
        /// </summary>
        QueueUnlimited
    }

    public enum NameType { City, Village, Test }//City names are used as a example for this template project

    public enum NotificationType { TestType, TestType2 }

    public enum SoundEffectType { None, UI, Ambient }

    public enum InputType { Number, Text, Slider, Toggle }

    public enum ConfigCategory { General = 0, Audio = 1 }
}
