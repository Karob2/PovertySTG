using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input
{
    public static class TextHandler
    {
        /*
        EventHandler<TextInputEventArgs> onTextEntered;
        KeyboardState _prevKeyState;
        */
        static char[] textBuffer;
        static int bufferSize;
        const int maxBufferSize = 32;
        static List<char> textBufferClone;
        static bool running = false;

        public static List<char> TextBuffer
        {
            get
            {
                return textBufferClone;
            }
        }

        public static void Start(GameServices gs)
        {
            if (running) return;
            /*
            #if OpenGL
                        Window.TextInput += TextEntered;
                        onTextEntered += HandleInput;
            #else
                        GlobalServices.Game.Window.TextInput += HandleInput;
            #endif
            */
            textBuffer = new char[maxBufferSize];
            textBufferClone = new List<char>();
            gs.Game.Window.TextInput += HandleInput;
            textBufferClone.Clear();
            bufferSize = 0;
            running = true;
        }

        public static void Reset()
        {
            bufferSize = 0;
        }

        public static void Stop(GameServices gs)
        {
            if (!running) return;
            gs.Game.Window.TextInput -= HandleInput;
            textBufferClone.Clear();
            running = false;
        }

        /*
        private void TextEntered(object sender, TextInputEventArgs e)
        {
            if (onTextEntered != null)
                onTextEntered.Invoke(sender, e);
        }
        */

        private static void HandleInput(object sender, TextInputEventArgs e)
        {
            if (bufferSize < maxBufferSize)
            {
                textBuffer[bufferSize] = e.Character;
                bufferSize++;
            }
        }

        /*
        public void Update()
        {
            KeyboardState keyState = Keyboard.GetState();

#if OpenGL
            if (keyState.IsKeyDown(Keys.Back) && _prevKeyState.IsKeyUp(Keys.Back))
            {
                onTextEntered.Invoke(this, new TextInputEventArgs('\b'));
            }
            if (keyState.IsKeyDown(Keys.Enter) && _prevKeyState.IsKeyUp(Keys.Enter))
            {
                onTextEntered.Invoke(this, new TextInputEventArgs('\r'));
            }

            // Handle other special characters here (such as tab )
#endif

            _prevKeyState = keyState;
        }
        */

        public static void DumpBuffer()
        {
            textBufferClone.Clear();
            for (int i = 0; i < bufferSize; i++)
            {
                textBufferClone.Add(textBuffer[i]);
            }
            bufferSize = 0;
        }
    }
}
