using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Engine.Util
{
    /// <summary>
    /// Helper to serialize and deserialize JSON files to classes marked with attributes like [JsonObject] and [JsonProperty].
    /// </summary>
    /// <typeparam name="T">Class type to serialize/deserialize.</typeparam>
    public static class JsonHelper<T> where T : class
    {
        /// <summary>
        /// Deserialize JSON data from a JSON file to a class of type T.
        /// </summary>
        public static T Load(string path)
        {
            // Load JSON file to string.
            string json;
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                json = streamReader.ReadToEnd();

            //var jsonSerializerSettings = new JsonSerializerSettings();
            //jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // Deserialize string to T.
            T item = JsonConvert.DeserializeObject<T>(json); //, jsonSerializerSettings);
            return item;
        }

        /// <summary>
        /// Serialize JSON data from a class of type T to a JSON file.
        /// </summary>
        public static void SaveCompact(string path, T item)
        {
            // Serialize T to JSON string.
            string json;
            json = JsonConvert.SerializeObject(item);

            // Write string to file.
            using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                streamWriter.Write(json);
        }

        /// <summary>
        /// Serialize JSON data from a class of type T to a JSON file.
        /// </summary>
        public static void Save(string path, T item)
        {
            // Serialize T to JSON string.
            string json;
            json = JsonConvert.SerializeObject(item, Formatting.Indented);

            // Write string to file.
            using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                streamWriter.Write(json);
        }

        /*
        static void RemoveComments(byte[] source, MemoryStream target)
        {
            int count = 0;
            bool commenting = false;
            while (count < source.Length)
            {
                if (commenting == true)
                {
                    if (source[count] == '\r' || source[count] == '\n')
                    {
                        commenting = false; // NOTE: We don't even need to seek past the newlines since the json parser will ignore them.
                    }
                    else
                    {
                        count++;
                    }
                }
                if (commenting == false)
                {
                    if (count + 1 < source.Length && source[count] == '/' && source[count + 1] == '/')
                    {
                        commenting = true;
                        count += 2;
                    }
                    else
                    {
                        target.WriteByte(source[count++]);
                    }
                }
            }
        }
        */
    }
}
