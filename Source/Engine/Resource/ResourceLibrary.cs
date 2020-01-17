using Engine.Util;
using System.Collections.Generic;
using System.IO;

namespace Engine.Resource
{
    // Required to call Load on a ResourceLibrary<string, T> of unknown type T.
    public abstract class ResourceLibrary
    {
        //public abstract string Directory { get; }
        public abstract string[] Extensions { get; }
        public abstract bool Load(string path);
        //public abstract bool Load(string groupName, string key);
    }

    /// <summary>
    /// Generic class for storing and retrieving resources by a string name.
    /// </summary>
    /// <typeparam name="T">The type of resource to store.</typeparam>
    public class ResourceLibrary<T> : ResourceLibrary
    {
        //public override string Directory { get => directory; }
        public override string[] Extensions => null;

        // List of loaded resources.
        protected Dictionary<string, T> list = new Dictionary<string, T>();
        public Dictionary<string, T> List => list;
        protected GameServices gs;
        //protected ResourceManager rm;
        //protected string directory;

        /*
        public ResourceLibrary(ResourceManager rm, string directory)
        {
            this.rm = rm;
            this.directory = directory;
        }
        */

        public ResourceLibrary(GameServices gs)
        {
            this.gs = gs;
        }

        /// <summary>
        /// Load resource from path and store in the library, returning true if successful
        /// and false if the resource already exists in the library.
        /// </summary>
        public override bool Load(string path)
        {
            return Load(path, out T item);
        }

        /*
        public override bool Load(string groupName, string key)
        {
            return Load(groupName, key, out T item);
        }
        */

        /// <summary>
        /// Load resource from path and store in the library, returning true if successful
        /// and false if the resource already exists in the library.
        /// </summary>
        /// <param name="item">The loaded resource.</param>
        public bool Load(string path, out T item)
        {
            // Items are identified by their extensionless filename.
            string key = Path.GetFileNameWithoutExtension(path);

            // If resource is already loaded, use the loaded copy.
            if (Get(key, out item))
            {
                return false;
            }

            // Otherwise, load the resource and store a reference in the library.
            item = InternalLoad(path);
            list.Add(key, item);
            return true;
        }

        /*
        public bool Load(string groupName, string key, out T item)
        {
            string path = Path.Combine(rm.ContentDirectory, groupName, key); //needs file extension too
            return Load(path, out item);
        }
        */

        /// <summary>
        /// Fetch resource from library using a key, returning the item reference.
        /// </summary>
        public T Get(string key)
        {
            // If asset is already loaded, use the loaded copy.
            if (Get(key, out T item))
            {
                return item;
            }

            // Otherwise, critically fail.
            gs.Error.LogErrorAndShutdown("Failed to find reference to asset '" + key.ToString() + "'. Did you forget to Register() it?");
            return item;
        }

        /// <summary>
        /// Fetch resource from library using a key, returning true if resource was found.
        /// </summary>
        public bool Get(string key, out T item)
        {
            // Search for resource in library and return if found.
            if (list.TryGetValue(key, out item)) return true;

            // Otherwise, fail.
            item = default(T);
            return false;
        }

        /// <summary>
        /// Override this with type-specific resource loading code.
        /// Use the key (usually a file name) to create and return an asset of type T.
        /// </summary>
        protected virtual T InternalLoad(string key) { return default(T); }

        /*
        // Removes the specified asset from the local library.
        // --Should only be used if absolutely certain the asset isn't being used locally and won't be used again soon.
        // --Should only be used when large amounts of data should be released prematurely for performance reasons.
        // Generally, garbage collection is sufficient to handle necessary removal whenever asset libraries are deleted.
        public void Remove(K key)
        {
            list.Remove(key);
        }
        // Slower alternative method.
        public void Remove(T item)
        {
            foreach (K key in list.Keys)
            {
                if (list[key].Equals(item))
                {
                    // Only delete the first matching instance, because multiple local instances should not exist.
                    list.Remove(key);
                    break;
                }
            }
        }
        // Individual item removal cannot work well for assets loaded by a ContentManager.
        // I guess I could create a ContentManger for each and every item... o_o
        */

        /// <summary>
        /// Forcibly unload all resources in the library.
        /// </summary>
        public virtual void Unload()
        {
            // TODO: Test this for bugs.
            foreach (string key in list.Keys)
            {
                InternalUnload(list[key]);
            }
            list.Clear();
        }

        /// <summary>
        /// Override this with type-specific resource unloading code.
        /// Unload the specified resource from memory and clean up as necessary.
        /// </summary>
        protected virtual void InternalUnload(T item) { }
        
        /*
        // DEBUG:
        public void List()
        {
            Engine.Util.Error.Log("Keys:");
            foreach (string key in list.Keys)
            {
                Engine.Util.Error.Log(key);
            }
        }
        */
    }
}
