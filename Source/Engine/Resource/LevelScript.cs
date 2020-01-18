using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engine.Resource
{
    /// <summary>
    /// JSON-serializable level script class.
    /// </summary>
    [JsonObject]
    public class LevelScript
    {
        [JsonProperty]
        public List<string> Lines { get; set; }

        public LevelScript() { }
    }
}
