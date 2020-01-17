using Engine;
using Engine.ECS;
using Engine.Resource;
using Engine.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.ECS
{
    public class Scene
    {
        GameServices gs;
        //ResourceManager rm;
        public GameServices GS => gs;

        enum ChildrenMode
        {
            Default = 0,
            OnlyLast
        }

        Scene parent;
        List<Scene> children = new List<Scene>();
        Dictionary<string, Scene> childrenByName = new Dictionary<string, Scene>();
        string name;
        public string Name => name;
        bool enabled;
        bool active;
        bool visible;
        //bool focused; // the game can decide which actions still occur when a scene is active but not focused
        // how about the last scene in the list is the "focused" one
        /*
        public bool Enabled { get => enabled; set => enabled = value; }
        public bool Active { get => active; set => active = value; }
        public bool Visible { get => visible; set => visible = value; }
        public bool Focused { get => focused; set => focused = value; }
        */
        int childrenMode;
        bool onlyLastChildActive = true;
        bool onlyLastChildVisible = true;
        //bool channel;

        public float AnimationSpeed { get; set; } = 1;

        List<CSystem> updateSystems;
        List<CSystem> renderSystems;
        //public Entity Root { get; private set; }
        List<Entity> entities;
        public List<Entity> EntityList => entities; // for debug
        List<Entity> freeEntities;
        public List<Entity> FreeEntities => freeEntities; // for debug
        //Dictionary<string, EntityGroup> entityGroups;

        Dictionary<string, EntityGroup> entityGroups;

        Dictionary<Type, ComponentGroup> componentGroups = new Dictionary<Type, ComponentGroup>();

        /*
        Dictionary<Type, ResourceLibrary> libraries; // TODO: Do I really need this to be dictionary?
        bool hasLibraries;
        */

        public Scene(GameServices gs)
        {
            this.gs = gs;
            updateSystems = new List<CSystem>();
            renderSystems = new List<CSystem>();
            entities = new List<Entity>();
            freeEntities = new List<Entity>();
            entityGroups = new Dictionary<string, EntityGroup>();
        }

        public Scene(GameServices gs, string name) : this(gs)
        {
            this.name = name;
        }

        public void Enable() { this.enabled = true; }
        public void Disable() { this.enabled = false; }

        public Scene AddScene(string name)
        {
            Scene scene;
            if (childrenByName.TryGetValue(name, out scene)) return scene;
            scene = new Scene(gs, name);
            children.Add(scene);
            childrenByName.Add(name, scene);
            return scene;
        }

        public Scene AddScene(Scene scene)
        {
            children.Add(scene);
            childrenByName.Add(scene.name, scene);
            return scene;
        }

        public void RemoveScene(string name)
        {
            if (childrenByName.TryGetValue(name, out Scene scene))
            {
                children.Remove(scene);
                childrenByName.Remove(name);
            }
        }

        public void RemoveLastScene()
        {
            if (children.Count > 0)
            {
                string name = children[children.Count - 1].name;
                children.RemoveAt(children.Count - 1);
                childrenByName.Remove(name);
            }
        }

        public void FocusScene(string name)
        {
            if (childrenByName.TryGetValue(name, out Scene scene))
            {
                children.Remove(scene);
                children.Add(scene);
            }
        }

        public bool TryGetChild(string name, out Scene scene)
        {
            return childrenByName.TryGetValue(name, out scene);
        }

        /*
        public void Pop()
        {
            if (parent != null)
            {
                parent.RemoveScene(this.name);
            }
        }
        */

        public void AddUpdateSystem(CSystem system)
        {
            updateSystems.Add(system);
            system.Initialize(gs, this);
        }

        public void AddRenderSystem(CSystem system)
        {
            renderSystems.Add(system);
            system.Initialize(gs, this);
        }

        // TODO: Unused
        public void Start()
        {
            foreach (CSystem system in updateSystems)
            {
                system.Start();
            }
            foreach (CSystem system in renderSystems)
            {
                system.Start();
            }
            foreach (Scene scene in children)
            {
                scene.Start();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!enabled) return;

            foreach (CSystem system in updateSystems)
            {
                system.Update(gameTime);
                system.FirstRun = false;
            }

            // TODO: Make a delete queue instead of iterating over the whole entity list.
            // TODO: Move this to a separate function.
            foreach (Entity entity in entities)
            {
                // TODO: Re-use in many cases would be better than deleting. I could give components a Reset() method.
                if (!entity.Deleting) continue;
                entity.Free();
                freeEntities.Add(entity);
            }

            if (onlyLastChildActive && children.Count > 0)
            {
                children[children.Count - 1].Update(gameTime);
            }
            else
            {
                foreach (Scene scene in children)
                {
                    scene.Update(gameTime);
                }
            }
        }

        public void Render(GameTime gameTime)
        {
            if (!enabled) return;

            foreach (CSystem system in renderSystems)
            {
                system.Update(gameTime);
                system.FirstRun = false;
            }

            if (onlyLastChildVisible && children.Count > 0)
            {
                children[children.Count - 1].Render(gameTime);
            }
            else
            {
                foreach (Scene scene in children)
                {
                    scene.Render(gameTime);
                }
            }
        }

        public ComponentGroup<T> GetComponentGroup<T>() where T : Component
        {
            if (componentGroups.TryGetValue(typeof(T), out ComponentGroup group))
            {
                return (ComponentGroup<T>)group; //downcasting
            }
            ComponentGroup<T> group2 = new ComponentGroup<T>(this);
            componentGroups.Add(typeof(T), group2);
            return group2;
        }

        public Entity NewEntity()
        {
            Entity entity;
            if (freeEntities.Count > 0)
            {
                entity = freeEntities[freeEntities.Count - 1];
                entity.Deleted = false;
                entity.Scene = this;
                freeEntities.RemoveAt(freeEntities.Count - 1);
            }
            else
            {
                entity = new Entity(this);
                entities.Add(entity);
            }
            return entity;
        }


        // Creates a new group if necessary.
        // TODO: In some cases, wouldn't it be better to fail than to silently return null?
        //       Well, I guess it can't be helped since groups can exist before the scene even does.
        private EntityGroup GetEntityGroup(string groupName)
        {
            EntityGroup list;
            if (!entityGroups.TryGetValue(groupName, out list))
            {
                /*
                list = new EntityGroup();
                entityGroups.Add(groupName, list);
                */
                return null;
            }
            return list;
        }

        // TODO: Misleading name sounds like it's getting the list of groups instead of the list within the group.
        public List<Entity> GetGroup(string groupName)
        {
            EntityGroup group = GetEntityGroup(groupName);
            if (group == null) return null;
            return group.GetList();
        }

        public Entity GetEntity(string entityName)
        {
            List<Entity> group = GetGroup(entityName);
            if (group != null) return group.FirstOrDefault();
            return null;
        }

        public EntityGroup CreateGroup(string groupName, int maxSize = 0)
        {
            EntityGroup group;

            // If group already exists, just return the handle to it.
            // TODO: How to handle differing maxSize values?
            if (entityGroups.TryGetValue(groupName, out group)) return group;

            group = new EntityGroup(maxSize);
            entityGroups.Add(groupName, group);
            return group;
        }

        // Creates a new group if neccessary.
        public void AddToGroup(string groupName, Entity entity)
        {
            EntityGroup group;
            if (!entityGroups.TryGetValue(groupName, out group))
            {
                group = CreateGroup(groupName);
            }
            group.AppendEntity(entity);
            return;
        }

        public void RemoveFromGroup(string groupName, Entity entity)
        {
            EntityGroup group;
            if (!entityGroups.TryGetValue(groupName, out group)) return;
            group.RemoveEntity(entity);
        }
        
        /*
        public void CreateLibraries()
        {
            if (hasLibraries) return;
            foreach (LibraryType libraryType in rm.LibraryTypes)
            {
                libraries.Add(libraryType.Type, (ResourceLibrary)Activator.CreateInstance(libraryType.Type, new object[] { rm, libraryType.Subfolder}));
            }
            hasLibraries = true;
        }

        public void LoadAssetGroup(string groupName)
        {
            CreateLibraries();
            string groupPath = Path.Combine(rm.ContentDirectory, groupName);
            if (!Directory.Exists(groupPath)) Error.LogWarning("Asset path not found: " + groupPath);

            foreach (ResourceLibrary library in libraries.Values)
            {
                string path = Path.Combine(groupPath, library.subfolder);
                if (Directory.Exists(path))
                {
                    foreach (string file in Directory.GetFiles(path))
                    {
                        string ext = Path.GetExtension(file);

                        bool valid = false;
                        foreach (string validExt in fileTypes[collection])
                        {
                            if (ext.Equals(validExt)) valid = true;
                        }
                        if (!valid) continue;

                        Error.Log("Loading " + file);
                        library.Load(file);
                    }
                }
                //library.Load()
            }
        }
        */
    }

    public class EntityGroup
    {
        List<Entity> list = new List<Entity>();
        //List<Entity> addList;
        int maxSize;

        public EntityGroup(int maxSize = 0)
        {
            this.maxSize = maxSize;
        }

        // Only tries to add entity onto end of list.
        // Returns false if the list is full.
        public bool AppendEntity(Entity entity)
        {
            if (maxSize <= 0 || list.Count < maxSize)
            {
                list.Add(entity);
                return true;
            }
            return false;
        }

        public void RemoveEntity(Entity entity)
        {
            list.Remove(entity);
        }

        //public bool InsertEntity

        public List<Entity> GetList()
        {
            return list;
        }
    }

    public class ComponentGroup { }

    public class ComponentGroup<T> : ComponentGroup where T : Component
    {
        // TODO: Replace this with a re-usable smart collection?
        List<T> list = new List<T>();
        public List<T> List { get { return list; } }
        Dictionary<Entity, T> listByOwner = new Dictionary<Entity, T>();
        public EnabledComponentCollection<T> EnabledList { get; set; }
        Scene scene;

        public ComponentGroup(Scene scene)
        {
            this.scene = scene;
            EnabledList = new EnabledComponentCollection<T>(list);
        }

        public void Add(T component, out int id)
        {
            id = list.Count;
            list.Add(component);
            listByOwner.Add(component.Owner, component);
        }

        public void Remove(T component)
        {
            list.Remove(component);
            listByOwner.Remove(component.Owner);
        }

        public T GetByOwner(Entity owner)
        {
            T component = null;
            listByOwner.TryGetValue(owner, out component);
            return component;
        }

        public T GetByOwner(string ownerName)
        {
            Entity owner = scene.GetEntity(ownerName);
            T component = null;
            listByOwner.TryGetValue(owner, out component);
            return component;
        }

        public bool TryGetByOwner(Entity owner, out T component)
        {
            return listByOwner.TryGetValue(owner, out component);
        }

        public bool TryGetEnabled(Entity owner, out T component)
        {
            if (listByOwner.TryGetValue(owner, out component))
            {
                if (component.Enabled) return true;
            }
            return false;
        }

        public T First()
        {
            if (list.Count == 0) return null;
            return list[0];
        }

        public bool TryGetFirst(out T first)
        {
            if (list.Count == 0)
            {
                first = null;
                return false;
            }
            first = list[0];
            return true;
        }

        /// <summary>
        /// Returns false if the group has no components, or if the first component is disabled.
        /// </summary>
        public bool TryGetFirstEnabled(out T first)
        {
            // TODO: Would I ever need this to find the "first enabled component", instead of the "first component if enabled"?
            if (list.Count == 0)
            {
                first = null;
                return false;
            }
            first = list[0];
            if (!first.Enabled) return false;
            return true;
        }
    }

    public class EnabledComponentCollection<T> : IEnumerable<T> where T : Component
    {
        List<T> list;

        public EnabledComponentCollection(List<T> list)
        {
            this.list = list;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this.list);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this.list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this.list);
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private List<T> list;
            private int index;
            private T current;

            internal Enumerator(List<T> list)
            {
                this.list = list;
                index = 0;
                current = default(T);
            }

            public T Current { get { return current; } }

            Object IEnumerator.Current { get { return Current; } }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }

            public bool MoveNext()
            {
                while (index < list.Count)
                {
                    current = list[index];
                    index++;
                    if (current.Enabled) return true;
                }
                index = list.Count + 1;
                current = default(T);
                return false;
            }

            public void Dispose() { }
        }
    }
}
