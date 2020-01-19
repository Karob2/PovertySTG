using Engine;
using Engine.ECS;
using Engine.Input;
using PovertySTG.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class GuiSystem : CSystem
    {
        SystemReferences sys;
        public bool HoldFocus { get; set; } = false;
        public bool MouseEnabled { get; set; } = false;
        public bool KeyboardEnabled { get; set; } = true;
        ButtonComponent focusedButton;

        public GuiSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            if (FirstRun) ResetGui();

            if (MouseEnabled)
            {
                UpdateMouseSpritePosition();
                if (InputManager.MouseMoved) UpdateMouseFocusedButton();
            }
            if (KeyboardEnabled)
            {
                UpdateKeyboardFocusedButton();
            }

            UpdateButtonPressed();
        }

        void UpdateText()
        {
            foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
            {
                switch (buttonComponent.PressCommand)
                {
                    case "fs":
                        if (gs.DisplayManager.Fullscreen) buttonComponent.TextComponent.Text = "Fullscreen";
                        else buttonComponent.TextComponent.Text = "Windowed";
                        break;
                    case "zoom-up":
                        buttonComponent.TextComponent.Text = "Zoom: " + gs.DisplayManager.GameScale + "x";
                        break;
                }
                switch (buttonComponent.LeftCommand)
                {
                    case "m-dn":
                        buttonComponent.TextComponent.Text = "Music Volume: " + (int)(SoundManager.MusicVolume * 100) + "%";
                        break;
                    case "s-dn":
                        buttonComponent.TextComponent.Text = "Sound Volume: " + (int)(SoundManager.SoundVolume * 100) + "%";
                        break;
                }
            }
        }

        void UpdateKeyboardFocusedButton()
        {
            int direction = 0;
            if (InputManager.Ticked(GameCommand.MenuUp)) direction = 1;
            if (InputManager.Ticked(GameCommand.MenuDown)) direction = 2;
            if (InputManager.Ticked(GameCommand.MenuLeft)) direction = 3;
            if (InputManager.Ticked(GameCommand.MenuRight)) direction = 4;
            if (direction == 0) return;

            foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
            {
                if (focusedButton == null)
                {
                    Focus(buttonComponent);
                    return;
                }

                if (!buttonComponent.Focused) continue;
                switch (direction)
                {
                    case 1:
                        if (buttonComponent.Up != null)
                        {
                            Focus(buttonComponent.Up);
                            Defocus(buttonComponent);
                        }
                        break;
                    case 2:
                        if (buttonComponent.Down != null)
                        {
                            Focus(buttonComponent.Down);
                            Defocus(buttonComponent);
                        }
                        break;
                    case 3:
                        if (buttonComponent.Left != null)
                        {
                            Focus(buttonComponent.Left);
                            Defocus(buttonComponent);
                        }
                        break;
                    default:
                        if (buttonComponent.Right != null)
                        {
                            Focus(buttonComponent.Right);
                            Defocus(buttonComponent);
                        }
                        break;
                }
                break;
            }
        }

        void UpdateMouseSpritePosition()
        {
            Point mousePosition = InputManager.MousePositionScaled();
            if (sys.PointerComponents.TryGetFirstEnabled(out PointerComponent component))
            {
                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = mousePosition.X;
                    renderComponent.Y = mousePosition.Y;
                }
            }
        }

        void ResetGui()
        {
            bool first = true;
            foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
            {
                if (first)
                {
                    Focus(buttonComponent);
                    first = false;
                }
                else
                {
                    Defocus(buttonComponent);
                }
            }
            UpdateText();
        }

        void UpdateMouseFocusedButton()
        {
            Point mousePosition = InputManager.MousePositionScaled();
            ButtonComponent hoverButton = null;
            RenderComponent hoverButtonRC = null;

            // Check which button is being hovered over.
            foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
            {
                if (!sys.RenderComponents.TryGetEnabled(buttonComponent.Owner, out RenderComponent renderComponent)) continue;

                if (!buttonComponent.Initialized) InitButton(buttonComponent);

                int x = (int)renderComponent.X;
                int y = (int)renderComponent.Y;
                buttonComponent.X = x;
                buttonComponent.Y = y;
                int x2 = x + buttonComponent.Width;
                int y2 = y + buttonComponent.Height;

                if (mousePosition.X < x) continue;
                if (mousePosition.X >= x2) continue;
                if (mousePosition.Y < y) continue;
                if (mousePosition.Y >= y2) continue;

                if (hoverButton == null)
                {
                    hoverButton = buttonComponent;
                    hoverButtonRC = renderComponent;
                    continue;
                }

                if (hoverButtonRC.Layer < renderComponent.Layer) continue;
                if (hoverButtonRC.Layer == renderComponent.Layer)
                {
                    if (hoverButtonRC.Depth < renderComponent.Depth) continue;
                }

                hoverButton = buttonComponent;
                hoverButtonRC = renderComponent;
            }

            if (hoverButton != null)
            {
                foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
                {
                    if (buttonComponent.Focused) Defocus(buttonComponent);
                }
                Focus(hoverButton);
            }
            else if (!HoldFocus)
            {
                foreach (ButtonComponent buttonComponent in sys.ButtonComponents.EnabledList)
                {
                    if (buttonComponent.Focused) Defocus(buttonComponent);
                }
            }
        }

        void UpdateButtonPressed()
        {
            if (focusedButton == null) return;

            int commandType = 0;
            string command = null;
            if (InputManager.JustPressed(GameCommand.MenuConfirm) && focusedButton.PressCommand != null)
            {
                commandType = 1;
                command = focusedButton.PressCommand;
            }
            if (InputManager.JustPressed(GameCommand.MenuLeft) && focusedButton.LeftCommand != null)
            {
                commandType = 2;
                command = focusedButton.LeftCommand;
            }
            if (InputManager.JustPressed(GameCommand.MenuRight) && focusedButton.RightCommand != null)
            {
                commandType = 3;
                command = focusedButton.RightCommand;
            }
            if (commandType == 0) return;

            //SoundManager.PlaySound("tap2");
            GameStateComponent gameState = sys.GetGameState();
            if (scene.Name == "menu")
            {
                switch (command)
                {
                    case "start":
                        gameState.Request("danmaku");
                        break;
                    case "options":
                        gameState.Request("options");
                        break;
                    case "quit":
                        gameState.Request("quit");
                        break;
                }
            }
            else if (scene.Name == "options")
            {
                switch (command)
                {
                    case "m-dn":
                        SoundManager.MusicVolume = Math.Max(SoundManager.MusicVolume -= 0.1f, 0);
                        break;
                    case "m-up":
                        SoundManager.MusicVolume = Math.Min(SoundManager.MusicVolume += 0.1f, 1);
                        break;
                    case "s-dn":
                        SoundManager.SoundVolume = Math.Max(SoundManager.SoundVolume -= 0.1f, 0);
                        break;
                    case "s-up":
                        SoundManager.SoundVolume = Math.Min(SoundManager.SoundVolume += 0.1f, 1);
                        break;
                    case "fs":
                        InputManager.StickMouse();
                        if (gs.DisplayManager.Fullscreen) gs.DisplayManager.SetFullscreen(false);
                        else gs.DisplayManager.SetFullscreen(true);
                        break;
                    case "zoom-up":
                        InputManager.StickMouse();
                        if (Config.Zoom < Config.GameScales.Count - 1) Config.Zoom++;
                        else Config.Zoom = 0;
                        gs.DisplayManager.SetZoom(Config.GameScales[Config.Zoom]);
                        break;
                    case "zoom-dn":
                        InputManager.StickMouse();
                        if (Config.Zoom > 0) Config.Zoom--;
                        else Config.Zoom = Config.GameScales.Count - 1;
                        gs.DisplayManager.SetZoom(Config.GameScales[Config.Zoom]);
                        break;
                    case "controls":
                        gameState.Request("controls");
                        break;
                    case "controls-r":
                        InputManager.Default();
                        break;
                    case "return":
                        Config.SaveConfig();
                        gameState.Request("return");
                        break;
                }
                UpdateText();
            }
            InputManager.Reset(GameCommand.Action1);
        }

        void Focus(ButtonComponent buttonComponent)
        {
            buttonComponent.Focused = true;
            if (buttonComponent.TextComponent != null)
                buttonComponent.TextComponent.Color = buttonComponent.FocusedTextColor;
            if (buttonComponent.SpriteComponent != null)
                buttonComponent.SpriteComponent.CurrentAnimationObject = buttonComponent.FocusedAnimation;
            focusedButton = buttonComponent;
        }

        void Defocus(ButtonComponent buttonComponent)
        {
            buttonComponent.Focused = false;
            if (buttonComponent.TextComponent != null)
                buttonComponent.TextComponent.Color = buttonComponent.DefaultTextColor;
            if (buttonComponent.SpriteComponent != null)
                buttonComponent.SpriteComponent.CurrentAnimationObject = buttonComponent.DefaultAnimation;
            if (focusedButton == buttonComponent) focusedButton = null;
        }

        void InitButton(ButtonComponent buttonComponent)
        {
            buttonComponent.Initialized = true;
            Defocus(buttonComponent);
        }
    }
}
