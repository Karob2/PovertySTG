using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engine.Resource
{
    /// <summary>
    /// JSON-serializable story script class.
    /// </summary>
    [JsonObject]
    public class Story
    {
        [JsonProperty]
        public List<StoryLine> Lines { get; set; }

        public Story() { }
    }

    [JsonObject]
    public class StoryLine
    {
        [JsonProperty]
        public string LeftSpeaker { get; set; }
        [JsonProperty]
        public string LeftExpression { get; set; }
        [JsonProperty]
        public string RightSpeaker { get; set; }
        [JsonProperty]
        public string RightExpression { get; set; }
        [JsonProperty]
        public string ActiveSpeaker { get; set; }
        [JsonProperty]
        public string Message { get; set; }
        [JsonProperty]
        public List<StoryAction> Actions { get; set; }
        [JsonProperty]
        public string Command { get; set; }
        [JsonProperty]
        public string Tag { get; set; }
    }

    [JsonObject]
    public class StoryAction
    {
        [JsonProperty]
        public string Option { get; set; }
        [JsonProperty]
        public string Command { get; set; }
        [JsonProperty]
        public string Tag { get; set; }
    }
}
