using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Engine.Input
{
    // This is all sloppy and unfinished.

    [JsonObject]
    public class InputConfig
    {
        [JsonProperty]
        public string Version { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool AskSetControls { get; set; }

        [JsonProperty]
        public List<InputNode> InputNodes { get; set; }

        public InputConfig()
        {
            InputNodes = new List<InputNode>();
        }
    }

    public class UniversalInputCombo
    {
        List<UniversalInput> inputList;
        bool invalidPress;
        //public bool Alias { get; private set; }

        public string Value { get { return ToString(); } set { FromString(value); } }

        public UniversalInputCombo()
        {
            inputList = new List<UniversalInput>();
        }

        public UniversalInputCombo(string str) : this()
        {
            FromString(str);
            //Alias = alias;
        }

        public bool Conflict(UniversalInputCombo combo)
        {
            return inputList.Last().Conflict(combo.inputList.Last());
        }

        public UniversalInputType GetInputType()
        {
            return inputList[0].GetInputType();
        }

        public void FromString(string str)
        {
            //if (inputList == null) inputList = new List<UniversalInput>();

            inputList.Clear();
            string[] strList = str.Split('+');
            for (int i = 0; i < strList.Length; i++)
            {
                //TODO: Catch errors?
                inputList.Add(new UniversalInput(strList[i]));
            }
        }

        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool firstItem = true;
            foreach (UniversalInput item in inputList)
            {
                if (!firstItem)
                    sb.Append('+');
                else
                    firstItem = false;
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }

        public bool GetPressedState()
        {
            if (inputList.Count == 0) return false;
            if (inputList.Count == 1)
            {
                return inputList.Last().GetPressedState();
            }
            foreach (UniversalInput item in inputList)
            {
                // Modifier keys.
                if (item != inputList.Last())
                {
                    if (!item.GetPressedState()) break;
                }
                // Main key.
                else
                {
                    if (item.GetPressedState())
                    {
                        // Only return true if all modifier keys were pressed before the main key was pressed.
                        return !invalidPress;
                    }
                    else
                    {
                        invalidPress = false;
                        return false;
                    }
                }
            }
            // If modifier keys were not held, then main key press shouldn't count.
            invalidPress = inputList.Last().GetPressedState();
            return false;
        }

        public void Reset()
        {
            invalidPress = false;
        }
    }

    public class UniversalInput
    {
        UniversalInputType universalInputType;
        Keys keyInput;
        GamepadInput gamepadInput;
        MouseInput mouseInput;
        //KeyModifiers keyModifiers;

        public UniversalInput() { }

        public UniversalInput(string str)
        {
            FromString(str);
        }

        public bool Conflict(UniversalInput input)
        {
            if (universalInputType != input.universalInputType) return false;
            if (universalInputType == UniversalInputType.Keyboard)
            {
                return keyInput == input.keyInput;
            }
            else if (universalInputType == UniversalInputType.Mouse)
            {
                return mouseInput == input.mouseInput;
            }
            else if (universalInputType == UniversalInputType.Gamepad)
            {
                return gamepadInput == input.gamepadInput;
            }
            return false;
        }

        public UniversalInputType GetInputType()
        {
            return universalInputType;
        }

        public void FromString(string str)
        {
            string[] strItem = str.Split('.');
            if (strItem.Length != 2)
            {
                //TODO: throw exception
                return;
            }
            //TODO: catch for invalid strings in these enum conversions
            universalInputType = (UniversalInputType)Enum.Parse(typeof(UniversalInputType), strItem[0]);
            if (universalInputType == UniversalInputType.Keyboard)
            {
                keyInput = (Keys)Enum.Parse(typeof(Keys), strItem[1]);
            }
            else if (universalInputType == UniversalInputType.Mouse)
            {
                mouseInput = (MouseInput)Enum.Parse(typeof(MouseInput), strItem[1]);
            }
            else if (universalInputType == UniversalInputType.Gamepad)
            {
                gamepadInput = (GamepadInput)Enum.Parse(typeof(GamepadInput), strItem[1]);
            }
        }

        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(universalInputType.ToString());
            sb.Append(".");
            if (universalInputType == UniversalInputType.Keyboard)
            {
                sb.Append(keyInput.ToString());
            }
            else if (universalInputType == UniversalInputType.Mouse)
            {
                sb.Append(mouseInput.ToString());
            }
            else if (universalInputType == UniversalInputType.Gamepad)
            {
                sb.Append(gamepadInput.ToString());
            }

            return sb.ToString();
        }

        public bool GetPressedState()
        {
            if (universalInputType == UniversalInputType.Keyboard)
            {
                return InputManager.KeyboardState.IsKeyDown(keyInput);
            }
            else if (universalInputType == UniversalInputType.Mouse)
            {
                MouseState state = InputManager.MouseState;
                switch (mouseInput)
                {
                    case MouseInput.LButton:
                        return (state.LeftButton == ButtonState.Pressed);
                    case MouseInput.MButton:
                        return (state.MiddleButton == ButtonState.Pressed);
                    case MouseInput.RButton:
                        return (state.RightButton == ButtonState.Pressed);
                    case MouseInput.X1Button:
                        return (state.XButton1 == ButtonState.Pressed);
                    case MouseInput.X2Button:
                        return (state.XButton2 == ButtonState.Pressed);
                    default:
                        return false;
                }
            }
            else if (universalInputType == UniversalInputType.Gamepad)
            {
                if (!InputManager.GamepadConnected) return false;
                GamePadState state = InputManager.GamepadState;
                switch (gamepadInput)
                {
                    case GamepadInput.A:
                        return state.IsButtonDown(Buttons.A);
                    case GamepadInput.B:
                        return state.IsButtonDown(Buttons.B);
                    case GamepadInput.X:
                        return state.IsButtonDown(Buttons.X);
                    case GamepadInput.Y:
                        return state.IsButtonDown(Buttons.Y);
                    case GamepadInput.Start:
                        return state.IsButtonDown(Buttons.Start);
                    case GamepadInput.Back:
                        return state.IsButtonDown(Buttons.Back);
                    case GamepadInput.Big:
                        return state.IsButtonDown(Buttons.BigButton);
                    case GamepadInput.Up:
                        return state.IsButtonDown(Buttons.DPadUp);
                    case GamepadInput.Down:
                        return state.IsButtonDown(Buttons.DPadDown);
                    case GamepadInput.Left:
                        return state.IsButtonDown(Buttons.DPadLeft);
                    case GamepadInput.Right:
                        return state.IsButtonDown(Buttons.DPadRight);
                    case GamepadInput.LB:
                        return state.IsButtonDown(Buttons.LeftShoulder);
                    case GamepadInput.LT:
                        return state.IsButtonDown(Buttons.LeftTrigger);
                    case GamepadInput.RB:
                        return state.IsButtonDown(Buttons.RightShoulder);
                    case GamepadInput.RT:
                        return state.IsButtonDown(Buttons.RightTrigger);
                    case GamepadInput.LStick:
                        return state.IsButtonDown(Buttons.LeftStick);
                    case GamepadInput.RStick:
                        return state.IsButtonDown(Buttons.RightStick);
                    case GamepadInput.LUp:
                        return state.IsButtonDown(Buttons.LeftThumbstickUp);
                    case GamepadInput.LDown:
                        return state.IsButtonDown(Buttons.LeftThumbstickDown);
                    case GamepadInput.LLeft:
                        return state.IsButtonDown(Buttons.LeftThumbstickLeft);
                    case GamepadInput.LRight:
                        return state.IsButtonDown(Buttons.LeftThumbstickRight);
                    case GamepadInput.RUp:
                        return state.IsButtonDown(Buttons.RightThumbstickUp);
                    case GamepadInput.RDown:
                        return state.IsButtonDown(Buttons.RightThumbstickDown);
                    case GamepadInput.RLeft:
                        return state.IsButtonDown(Buttons.RightThumbstickLeft);
                    case GamepadInput.RRight:
                        return state.IsButtonDown(Buttons.RightThumbstickRight);
                    default:
                        return false;
                }
            }
            return false;
        }
    }

    [JsonObject]
    public class InputNode
    {
        GameCommand gameCommand;
        List<UniversalInputCombo> inputCombos;
        [JsonIgnore]
        public List<UniversalInputCombo> InternalInputCombos { get => inputCombos; }
        //bool ranged;
        bool pressed;
        bool toggled;
        bool ticked;
        int tickCount;
        float heldTime;

        [JsonIgnore]
        public GameCommand GameCommand { get { return gameCommand; } set { gameCommand = value; } }

        [JsonProperty]
        public string Command
        {
            get
            {
                return gameCommand.ToString();
            }
            set
            {
                //Enum.TryParse<KeyType>(value, out type);
                gameCommand = (GameCommand)Enum.Parse(typeof(GameCommand), value);
                // TODO: Catch exception here when invalid keytype is in config file.
            }
        }

        //public List<UniversalInputCombo> InputCombos { get { return inputCombos; } set { inputCombos = value; } }

        [JsonProperty]
        public List<string> InputCombos { get; set; }
        /*
        public List<string> InputCombos {
            get
            {
                List<string> ic = new List<string>();
                foreach (UniversalInputCombo uic in inputCombos)
                {
                    ic.Add(uic.Value);
                }
                return ic;
            }
            set
            {
                inputCombos = new List<UniversalInputCombo>();
                foreach (string ic in value)
                {
                    inputCombos.Add(new UniversalInputCombo() { Value = ic });
                }
            }
        }
        */

        public void PreGet()
        {
            List<string> ic = new List<string>();
            foreach (UniversalInputCombo uic in inputCombos)
            {
                ic.Add(uic.Value);
            }
            InputCombos = ic;
        }

        public void PostSet()
        {
            inputCombos = new List<UniversalInputCombo>();
            foreach (string ic in InputCombos)
            {
                inputCombos.Add(new UniversalInputCombo() { Value = ic });
            }
        }

        /*
        [XmlAttribute]
        public string Key1 { get { return key1.ToString(); } set { key1 = (Keys)Enum.Parse(typeof(Keys), value); } }
        [XmlAttribute]
        public string Key2 { get { return key2.ToString(); } set { key2 = (Keys)Enum.Parse(typeof(Keys), value); } }
        */

        public InputNode()
        {
            Clear();
        }

        public InputNode(GameCommand gameCommand) : this()
        {
            this.gameCommand = gameCommand;
        }

        public bool Conflict(UniversalInputCombo combo)
        {
            foreach (UniversalInputCombo ic in inputCombos)
            {
                if (combo.Conflict(ic)) return true;
            }
            return false;
        }

        public void Clear()
        {
            inputCombos = new List<UniversalInputCombo>();
        }

        //NOTE: What if the player has W and Shift+W for two different commands? Normally, Shift+W would execute Shift
        //command and W command, but in this case it needs to escape those and only execute Shift+W command.
        //Basically, if one scheme is a subset of another, then the larger one dominates.
        //I guess I could prebuild a list of dominators, for quick disabling of subordinates.
        //Even if I only allowed Shift, Ctrl, and Alt modifiers, I'd need to do something similar.
        //Though I could simplify to just checking the main (last) key.
        //Between Shift+C+F, Alt+F, and F, the longest would take priority.
        //What if the player releases Shift before F? Should it then count as if F is now held? Yes.

        //Cross-checking nodes sounds painful. Yes, let's pre-build. That needs to occur in InputManager.

        /*
        public void SetCombo(GameCommand gameCommand, String inputCombo)
        {
            this.gameCommand = gameCommand;
            inputCombos = new List<UniversalInputCombo> { new UniversalInputCombo(inputCombo) };
            Reset();
        }
        */

        public void SetCombo(String inputCombo)
        {
            inputCombos = new List<UniversalInputCombo> { new UniversalInputCombo(inputCombo) };
        }

        public void AddCombo(String inputCombo)
        {
            inputCombos.Add(new UniversalInputCombo(inputCombo));
            // TODO: Automatically sort input by type? (Keyboard, Touch, GamePad)
        }

        /*
        public void AddAliasCombo(String inputCombo)
        {
            inputCombos.Add(new UniversalInputCombo(inputCombo, true));
            // TODO: Automatically sort input by type? (Keyboard, Touch, GamePad)
        }
        */

        public void Reset()
        {
            foreach (UniversalInputCombo inputCombo in inputCombos)
            {
                inputCombo.Reset();
            }
            //pressed = Keyboard.GetState().IsKeyDown(key1) || Keyboard.GetState().IsKeyDown(key2);
            pressed = GetPressedState();
            toggled = false;
            ticked = false;
            tickCount = 0;
            heldTime = 0f;
        }

        public void Update()
        {
            toggled = false;
            ticked = false;
            //bool newState = Keyboard.GetState().IsKeyDown(key1) || Keyboard.GetState().IsKeyDown(key2);
            bool newState = GetPressedState();
            if (newState != pressed)
            {
                toggled = true;
            }
            pressed = newState;
        }

        bool GetPressedState()
        {
            foreach (UniversalInputCombo inputCombo in inputCombos)
            {
                if (inputCombo.GetPressedState()) return true;
            }
            return false;
        }

        public void UpdateRepeater(GameTime gameTime)
        {
            // TODO: Replace hard-coded delay values with values loaded from config.xml
            ticked = false;
            if (pressed)
            {
                if (tickCount == 0)
                {
                    tickCount = 1;
                    heldTime = 0f;
                    ticked = true;
                }
                else
                {
                    float newHeldTime = heldTime + (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (tickCount == 1 && newHeldTime >= 0.5f)
                    {
                        ticked = true;
                        tickCount = 2;
                        heldTime = Math.Min(newHeldTime - heldTime, 0.2f); //prevent overflow ticking from fps lag
                    }
                    else if (tickCount > 1 && newHeldTime >= 0.08f)
                    {
                        ticked = true;
                        tickCount++;
                        heldTime = Math.Min(newHeldTime - heldTime, 0.08f);
                    }
                    else
                    {
                        heldTime = newHeldTime;
                    }
                }
            }
            else
            {
                tickCount = 0;
            }
        }

        public bool Held()
        {
            return pressed;
        }

        public bool JustPressed()
        {
            return pressed && toggled;
        }
        public bool JustReleased()
        {
            return !pressed && toggled;
        }

        public bool Ticked()
        {
            return ticked;
        }
    }
}
