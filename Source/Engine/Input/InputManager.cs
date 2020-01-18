using Engine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Input
{
    /// <summary>
    /// Manager for configurable keyboard, mouse, and gamepad inputs.
    /// </summary>
    public static class InputManager
    {
        const string inputConfigVersion = "c";

        static GameServices gs;

        static InputNode[] inputNodeList;
        static InputNode[] aliasNodeList;

        public static KeyboardState KeyboardState { get; private set; }
        public static GamePadState GamepadState { get; private set; }
        public static bool GamepadConnected { get; private set; }
        public static MouseState MouseState { get; private set; }
        public static bool MouseMoved { get; private set; }
        static int mouseOldX = 0, mouseOldY = 0;
        static int stickyMouse;
        static Point stickyPosition;

        static List<Tuple<GameCommand, GameCommand>> crossBindings;
        static string path;
        public static bool AskSetControls { get; set; }

        /// <summary>
        /// Creates default input bindings.
        /// </summary>
        public static void Initialize(GameServices gs)
        {
            InputManager.gs = gs;
            // TODO: GameCommand list should be set by the application, not the engine, so enum may not be good.
            inputNodeList = new InputNode[Enum.GetValues(typeof(GameCommand)).Length];
            aliasNodeList = new InputNode[Enum.GetValues(typeof(GameCommand)).Length];
            for (int i = 0; i < inputNodeList.Length; i++)
            {
                inputNodeList[i] = new InputNode((GameCommand)i);
                aliasNodeList[i] = new InputNode((GameCommand)i);
            }

            crossBindings = new List<Tuple<GameCommand, GameCommand>>();
            AddCrossBinding(GameCommand.Up, GameCommand.MenuUp);
            AddCrossBinding(GameCommand.Down, GameCommand.MenuDown);
            AddCrossBinding(GameCommand.Left, GameCommand.MenuLeft);
            AddCrossBinding(GameCommand.Right, GameCommand.MenuRight);
            AddCrossBinding(GameCommand.Action1, GameCommand.MenuConfirm);
            AddCrossBinding(GameCommand.Action2, GameCommand.MenuCancel);
            AddCrossBinding(GameCommand.Action3, GameCommand.MenuConfirm);
            AddCrossBinding(GameCommand.MenuUp, GameCommand.Up);
            AddCrossBinding(GameCommand.MenuDown, GameCommand.Down);
            AddCrossBinding(GameCommand.MenuLeft, GameCommand.Left);
            AddCrossBinding(GameCommand.MenuRight, GameCommand.Right);

            Default();
        }

        /// <summary>
        /// Initialize and load input bindings from a file.
        /// </summary>
        public static void Initialize(GameServices gs, string path)
        {
            Initialize(gs);
            LoadConfig(path);
        }

        static void AddCrossBinding(GameCommand gc1, GameCommand gc2)
        {
            crossBindings.Add(new Tuple<GameCommand, GameCommand>(gc1, gc2));
        }

        /// <summary>
        /// Restore input bindings to the defaults.
        /// </summary>
        public static void Default()
        {
            Set(GameCommand.Up, Keys.W, GamepadInput.Up, GamepadInput.LUp);
            Set(GameCommand.Down, Keys.S, GamepadInput.Down, GamepadInput.LDown);
            Set(GameCommand.Left, Keys.A, GamepadInput.Left, GamepadInput.LLeft);
            Set(GameCommand.Right, Keys.D, GamepadInput.Right, GamepadInput.LRight);
            Set(GameCommand.Action1, Keys.J, Keys.Z, GamepadInput.A);
            Set(GameCommand.Action2, Keys.K, Keys.X, GamepadInput.B);
            Set(GameCommand.Action3, Keys.L, Keys.C, GamepadInput.X);
            //Set(GameCommand.Action4, Keys.OemSemicolon, Keys.V, GamepadInput.Y);
            Set(GameCommand.Skip, Keys.Back, GamepadInput.LB, GamepadInput.RB);
            // Menu controls that shouldn't be configurable in-game (at least the first values):
            Set(GameCommand.MenuUp, Keys.Up, GamepadInput.Up);
            Set(GameCommand.MenuDown, Keys.Down, GamepadInput.Down);
            Set(GameCommand.MenuLeft, Keys.Left, GamepadInput.Left);
            Set(GameCommand.MenuRight, Keys.Right, GamepadInput.Right);
            Set(GameCommand.MenuConfirm, Keys.Enter, Keys.Space, GamepadInput.A, GamepadInput.X, GamepadInput.Start);
            Set(GameCommand.MenuCancel, Keys.Escape, GamepadInput.B, GamepadInput.Y, GamepadInput.Back);
            Set(GameCommand.Save, Keys.S);
            Set(GameCommand.Load, Keys.L);
            Set(GameCommand.New, Keys.N);
            Set(GameCommand.Edit, Keys.E);
            Set(GameCommand.Play, Keys.P);
            Set(GameCommand.Tab, Keys.Tab);
            Set(GameCommand.Pause, Keys.Escape, GamepadInput.Start);
            Set(GameCommand.SafeBack, Keys.Escape, GamepadInput.Back);
            Set(GameCommand.TextLeft, Keys.Left);
            Set(GameCommand.TextRight, Keys.Right);
            UpdateAliases();
            AskSetControls = true;
        }

        public static InputNode GetNode(int i)
        {
            return inputNodeList[i];
        }

        /// <summary>
        /// Bind a command to any number of inputs.
        /// </summary>
        static void Set(GameCommand command, params System.Enum[] keys)
        {
            inputNodeList[(int)command].Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].GetType() == typeof(Keys))
                {
                    inputNodeList[(int)command].AddCombo("Keyboard." + keys[i].ToString());
                }
                else if (keys[i].GetType() == typeof(MouseInput))
                {
                    inputNodeList[(int)command].AddCombo("Mouse." + keys[i].ToString());
                }
                else if (keys[i].GetType() == typeof(GamepadInput))
                {
                    inputNodeList[(int)command].AddCombo("Gamepad." + keys[i].ToString());
                }
                else
                {
                    gs.Error.LogErrorAndShutdown("Invalid input type: " + keys[i].GetType().Name);
                }
            }
            inputNodeList[(int)command].Reset();
        }

        public static void LoadConfig()
        {
            LoadConfig(path);
        }

        /// <summary>
        /// Loads input bindings from a file.
        /// </summary>
        public static void LoadConfig(string path)
        {
            InputManager.path = path;
            Default();

            // Create default bindings file if no file exists.
            if (!File.Exists(path))
            {
                // TODO: One downside of this method is that the config file will not automatically populate with new options when the game version is updated.
                SaveConfig(path);
                return;
            }

            // Else, load bindings from file.
            InputConfig inputConfig = JsonHelper<InputConfig>.Load(path);
            if (inputConfig.Version != inputConfigVersion)
            {
                SaveConfig(path);
                return;
            }
            AskSetControls = inputConfig.AskSetControls;

            foreach (InputNode inputNode in inputConfig.InputNodes)
            {
                inputNode.PostSet();
                inputNodeList[(int)inputNode.GameCommand] = inputNode;
            }

            UpdateAliases();
        }

        public static void SaveConfig()
        {
            SaveConfig(path);
        }

        /// <summary>
        /// Save input bindings to a file.
        /// </summary>
        public static void SaveConfig(string path)
        {
            InputConfig inputConfig = new InputConfig();
            inputConfig.Version = inputConfigVersion;
            inputConfig.AskSetControls = AskSetControls;
            foreach (GameCommand command in Enum.GetValues(typeof(GameCommand)))
            {
                // Hacky way to prevent certain keys from being written to the XML file (so users don't change them).
                if ((int)command >= (int)GameCommand.MenuUp) continue;

                inputNodeList[(int)command].PreGet();
                inputConfig.InputNodes.Add(inputNodeList[(int)command]);
            }
            JsonHelper<InputConfig>.Save(path, inputConfig);
        }

        public static void UpdateAliases()
        {
            //ClearAliases();
            
            // Copy commands to alias list.
            for (int i = 0; i < inputNodeList.Length; i++)
            {
                aliasNodeList[i].Clear();
                foreach (UniversalInputCombo ic in inputNodeList[i].InternalInputCombos)
                {
                    aliasNodeList[i].AddCombo(ic.ToString());
                }
            }

            // Create requested aliases as long as there are no conflicts.
            foreach (Tuple<GameCommand, GameCommand> cb in crossBindings)
            {
                //if ((int)cb.Item1 >= (int)GameCommand.MenuUp) continue;
                //if ((int)cb.Item2 < (int)GameCommand.MenuUp) continue;
                InputNode leftNode = inputNodeList[(int)cb.Item1];
                InputNode rightAliasNode = aliasNodeList[(int)cb.Item2];
                //foreach (UniversalInputCombo ic in leftNode.InternalInputCombos)
                for (int i = 0; i < leftNode.InternalInputCombos.Count; i++)
                {
                    UniversalInputCombo ic = leftNode.InternalInputCombos[i];
                    bool used = false;
                    int startJ, endJ;
                    if ((int)cb.Item1 < (int)GameCommand.MenuUp)
                    {
                        startJ = (int)GameCommand.MenuUp;
                        endJ = (int)GameCommand.Save;
                    }
                    else
                    {
                        startJ = (int)GameCommand.Up;
                        endJ = (int)GameCommand.MenuUp;
                    }
                    for (int j = startJ; j < endJ; j++)
                    {
                        InputNode otherNode = inputNodeList[j];
                        if (otherNode.Conflict(ic))
                        {
                            used = true;
                            break;
                        }
                    }
                    if (!used) rightAliasNode.AddCombo(leftNode.InternalInputCombos[i].ToString());
                }
            }

            Reset();
        }

        /*
        public static void ClearAliases()
        {
            foreach (InputNode node in inputNodeList)
            {
                List<UniversalInputCombo> comboList = node.InternalInputCombos;
                for (int i = comboList.Count - 1; i >= 0; i--)
                {
                    if (comboList[i].Alias) comboList.RemoveAt(i);
                }
            }
        }
        */

        /// <summary>
        /// Update the status of all game inputs.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            // Update keyboard
            KeyboardState = Keyboard.GetState();

            // Update gamepad
            if (GamePad.GetCapabilities(PlayerIndex.One).IsConnected)
            {
                GamepadConnected = true;
                GamepadState = GamePad.GetState(PlayerIndex.One);
            }
            else
            {
                GamepadConnected = false;
                GamepadState = default(GamePadState);
            }

            // Update mouse
            MouseState = Mouse.GetState();
            int mouseX = MouseState.X;
            int mouseY = MouseState.Y;
            if (mouseX != mouseOldX || mouseY != mouseOldY) MouseMoved = true;
            else MouseMoved = false;
            if (stickyMouse > 0)
            {
                Point rev = gs.DisplayManager.ReverseScaleCoordinates(stickyPosition);
                Mouse.SetPosition(rev.X, rev.Y);
                MouseState = Mouse.GetState();
                if (MouseMoved) stickyMouse++;
                MouseMoved = false;
                if (stickyMouse > 1) stickyMouse = 0; // TODO: I don't know if 1 is enough for all platforms.
            }
            mouseOldX = mouseX;
            mouseOldY = mouseY;

            // Update button inputs (keyboard, gamepad, mouse buttons)
            foreach (InputNode node in aliasNodeList)
            {
                node.Update();
                node.UpdateRepeater(gameTime);
            }
        }

        /// <summary>
        /// Reset the status of all inputs to avoid wrongly triggering keypresses
        /// when input bindings are changed.
        /// </summary>
        public static void Reset()
        {
            if (gs == null) return;

            foreach (InputNode node in aliasNodeList)
            {
                node.Reset();
            }
            MouseMoved = false;
            mouseOldX = Mouse.GetState().X;
            mouseOldY = Mouse.GetState().Y;
        }

        public static void Reset(GameCommand command)
        {
            aliasNodeList[(int)command].Reset();
        }

        /// <summary>
        /// Check if an input is being held down.
        /// </summary>
        public static bool Held(GameCommand command)
        {
            return aliasNodeList[(int)command].Held();
        }

        /// <summary>
        /// Check if an input just started being held down.
        /// </summary>
        public static bool JustPressed(GameCommand command)
        {
            return aliasNodeList[(int)command].JustPressed();
        }

        /// <summary>
        /// Check if an input just stopped being held down.
        /// </summary>
        public static bool JustReleased(GameCommand command)
        {
            return aliasNodeList[(int)command].JustReleased();
        }

        /// <summary>
        /// Check if an input is being "ticked". Ticks happen at a slow rate
        /// whenever the key is held.
        /// </summary>
        public static bool Ticked(GameCommand command)
        {
            return aliasNodeList[(int)command].Ticked();
        }

        public static Point MousePositionScaled()
        {
            return gs.DisplayManager.ScaleCoordinates(MouseState.Position);
        }

        public static Keys[] GetKeyboardInput()
        {
            return KeyboardState.GetPressedKeys();
        }

        public static List<MouseInput> GetMouseInput()
        {
            List<MouseInput> list = new List<MouseInput>();
            MouseState state = MouseState;
            if (state.LeftButton == ButtonState.Pressed) list.Add(MouseInput.LButton);
            if (state.RightButton == ButtonState.Pressed) list.Add(MouseInput.RButton);
            if (state.MiddleButton == ButtonState.Pressed) list.Add(MouseInput.MButton);
            if (state.XButton1 == ButtonState.Pressed) list.Add(MouseInput.X1Button);
            if (state.XButton2 == ButtonState.Pressed) list.Add(MouseInput.X2Button);
            return list;
        }

        public static List<Buttons> GetGamepadInput()
        {
            List<Buttons> list = new List<Buttons>();
            GamePadState currentState = GamepadState;
            foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
            {
                if (currentState.IsButtonDown(button)) list.Add(button);
            }
            return list;
        }

        public static void StickMouse()
        {
            stickyMouse = 1;
            stickyPosition = gs.DisplayManager.ScaleCoordinates(MouseState.Position);
        }
    }
}
