using Engine;
using Engine.ECS;
using Engine.Input;
using PovertySTG.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class KeyConfigSystem : CSystem
    {
        SystemReferences sys;
        TextComponent press, error;

        int currentKey;
        double conflictTimer;
        bool keysHeld = true;
        bool success;
        static InputNode[] draftNodes;

        public KeyConfigSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            if (FirstRun)
            {
                press = sys.TextComponents.GetByOwner("press");
                error = sys.TextComponents.GetByOwner("error");

                draftNodes = new InputNode[Enum.GetValues(typeof(GameCommand)).Length];
                for (int i = 0; i < draftNodes.Length; i++)
                {
                    draftNodes[i] = new InputNode((GameCommand)i);
                }
            }

            UpdateInput(gameTime);

            UpdateText();
        }

        void UpdateText()
        {
            //conflictWarning.Color = new Color((float)conflictTimer / 3, 0, 0, (float)conflictTimer / 3);
            if (conflictTimer > 0) error.Enabled = true;
            else error.Enabled = false;

            press.Text = "Press key for <" + Config.GetCommandName((GameCommand)currentKey) + ">";
            //press.Text = "Press key for <" + ((GameCommand)currentKey).ToString() + ">";
        }

        void UpdateInput(GameTime gameTime)
        {
            GameStateComponent gameState = sys.GetGameState();

            if (conflictTimer > 0)
            {
                conflictTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (InputManager.JustPressed(GameCommand.SafeBack))
            {
                //SoundManager.PlaySound("Throw2");
                currentKey--;
                if (currentKey < 0)
                {
                    scene.Disable();
                    gameState.Request("return");
                }
                return;
            }

            if (InputManager.Held(GameCommand.SafeBack))
            {
                return;
            }

            Keys[] pressedKeys = InputManager.GetKeyboardInput();
            List<MouseInput> pressedMouseKeys = InputManager.GetMouseInput();
            List<Buttons> pressedGamepadKeys = InputManager.GetGamepadInput();
            if (pressedKeys.Length == 0 && pressedMouseKeys.Count == 0 && pressedGamepadKeys.Count == 0)
            {
                keysHeld = false;
                return;
            }

            if (!keysHeld)
            {
                // Bind keyboard input.
                if (pressedKeys.Length > 0)
                {
                    keysHeld = true;
                    Keys key = pressedKeys[0];
                    if (key != Keys.LeftAlt && key != Keys.RightAlt)
                        SetBinding(key, UniversalInputType.Keyboard);
                }

                // Bind mouse input.
                else if (pressedMouseKeys.Count > 0)
                {
                    keysHeld = true;
                    MouseInput key = pressedMouseKeys[0];
                    SetBinding(key, UniversalInputType.Mouse);
                }

                // Bind gamepad input.
                else if (pressedGamepadKeys.Count > 0)
                {
                    keysHeld = true;
                    GamepadInput key = (GamepadInput)pressedGamepadKeys[0];
                    SetBinding(key, UniversalInputType.Gamepad);
                }
            }

            // If all customizable bindings are set, then leave this scene.
            if (currentKey >= (int)GameCommand.MenuUp)
            {
                success = true; // TODO: I don't think this is needed anymore.
                FinalizeKeybinds();
                scene.Disable();
                gameState.Request("return");
            }
        }

        bool SetBinding(Enum key, UniversalInputType type)
        {
            //InputNode node = InputManager.GetNode(currentKey);
            InputNode node = draftNodes[currentKey];
            string keyString = type.ToString() + "." + key.ToString();

            // Check for conflicts with previous bindings.
            UniversalInputCombo ic = new UniversalInputCombo(keyString);
            for (int i = 0; i < currentKey; i++)
            {
                if (draftNodes[i].Conflict(ic))
                {
                    //SoundManager.PlaySound("hit2");
                    conflictTimer = 3;
                    return false;
                }
            }

            // Write the new binding.
            node.SetCombo(keyString);
            InputManager.UpdateAliases();

            currentKey++;
            //SoundManager.PlaySound("tap2");
            conflictTimer = 0;
            return true;
        }

        public void FinalizeKeybinds()
        {
            if (!success) return;

            // Remove all conflicting old bindings.
            for (int i = 0; i < (int)GameCommand.MenuUp; i++)
            {
                InputNode draftNode = draftNodes[i];
                UniversalInputCombo draftCombo = draftNode.InternalInputCombos[0];
                for (int j = 0; j < (int)GameCommand.MenuUp; j++)
                {
                    InputNode node = InputManager.GetNode(j);
                    for (int k = 0; k < node.InternalInputCombos.Count; k++)
                    {
                        UniversalInputCombo combo = node.InternalInputCombos[k];
                        if (i == j && combo.GetInputType() == draftCombo.GetInputType())
                        {
                            node.InternalInputCombos.RemoveAt(k);
                            k--;
                            continue;
                        }
                        if (combo.ToString() == draftCombo.ToString())
                        {
                            node.InternalInputCombos.RemoveAt(k);
                            k--;
                            continue;
                        }
                    }
                }
            }

            // Add in new bindings.
            for (int i = 0; i < (int)GameCommand.MenuUp; i++)
            {
                InputManager.GetNode(i).AddCombo(draftNodes[i].InternalInputCombos[0].ToString());
            }

            InputManager.AskSetControls = false;
            InputManager.SaveConfig();
            InputManager.UpdateAliases();
        }
    }
}
