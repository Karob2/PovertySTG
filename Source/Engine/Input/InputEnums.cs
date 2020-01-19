using Microsoft.Xna.Framework.Input;

namespace Engine.Input
{
    // TODO: Switch from built-in enum to a system that allows games to have their own game commands.
    public enum GameCommand
    {
        Up,
        Down,
        Left,
        Right,
        Action1,
        Action2,
        Action3,
        //Action4,

        MenuUp,
        MenuDown,
        MenuLeft,
        MenuRight,
        MenuConfirm,
        MenuCancel,

        Skip,

        Save,
        Load,
        New,
        Edit,
        Play,
        Tab,
        Pause,
        SafeBack,

        TextLeft,
        TextRight
    }

    public enum UniversalInputType
    {
        Keyboard,
        Mouse, //TODO: Touch?
        Gamepad
    }

    /*
    public enum KeyModifiers
    {
        LeftControl = 1,
        RightControl = 2,
        LeftShift = 4,
        RightShift = 8,
        LeftAlt = 16,
        RightAlt = 32
    }
    */

    public enum GamepadInput
    {
        A = Buttons.A,
        B = Buttons.B,
        X = Buttons.X,
        Y = Buttons.Y,
        LB = Buttons.LeftShoulder,
        RB = Buttons.RightShoulder,
        LT = Buttons.LeftTrigger,
        RT = Buttons.RightTrigger,
        Start = Buttons.Start,
        Back = Buttons.Back,
        Big = Buttons.BigButton,
        Up = Buttons.DPadUp,
        Down = Buttons.DPadDown,
        Left = Buttons.DPadLeft,
        Right = Buttons.DPadRight,
        LStick = Buttons.LeftStick,
        RStick = Buttons.RightStick,
        LUp = Buttons.LeftThumbstickUp,
        LDown = Buttons.LeftThumbstickDown,
        LLeft = Buttons.LeftThumbstickLeft,
        LRight = Buttons.LeftThumbstickRight,
        RUp = Buttons.RightThumbstickUp,
        RDown = Buttons.RightThumbstickDown,
        RLeft = Buttons.RightThumbstickLeft,
        RRight = Buttons.RightThumbstickRight
    }

    public enum MouseInput
    {
        Up,
        Down,
        Left,
        Right,
        LButton,
        MButton,
        RButton,
        X1Button,
        X2Button,
        ScrollUp,
        ScrollDown
    }
}
