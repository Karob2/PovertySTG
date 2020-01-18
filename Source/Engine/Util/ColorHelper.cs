using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PovertySTG.Engine.Util
{
    public static class ColorHelper
    {
        public static Color FromHex(string hexString)
        {
            int r = int.Parse(hexString.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(hexString.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(hexString.Substring(5, 2), NumberStyles.HexNumber);
            return new Color(r, g, b);
        }
    }
}
