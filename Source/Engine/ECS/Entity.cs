using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.ECS
{
    public class Entity
    {
        private Scene scene;
        public Scene Scene { get => scene; set => scene = value; }
        public bool Enabled { get; set; }
        public bool Deleting { get; set; }
        public bool Deleted { get; set; }
        HashSet<string> groups = new HashSet<string>();
        HashSet<string> tags = new HashSet<string>();
        List<Component> componentReferences = new List<Component>();
        public List<Component> ComponentReferences => componentReferences; // debug

        public Entity(Scene scene)
        {
            this.scene = scene;
        }

        public void Enable()
        {
            Enabled = true;
            foreach (Component component in componentReferences)
            {
                component.Enabled = true;
            }
        }

        public void Disable()
        {
            Enabled = false;
            foreach (Component component in componentReferences)
            {
                component.Enabled = false;
            }
        }

        public void Delete()
        {
            Deleting = true;
            Disable();
        }

        public void Free()
        {
            while (componentReferences.Count > 0)
            {
                componentReferences[0].Remove();
            }
            foreach (string group in groups)
            {
                scene.RemoveFromGroup(group, this);
            }
            tags.Clear();
            groups.Clear();
            Deleting = false;
            Deleted = true;
            scene = null;
        }

        public Entity AddComponent<T>(T component) where T : Component
        {
            component.Owner = this;
            component.Enabled = Enabled;
            componentReferences.Add(component);
            if (scene != null)
            {
                ComponentGroup<T> group = scene.GetComponentGroup<T>();
                group.Add(component, out int componentId);
                //component.FilterComponent(); // This leads to downcasting via GetComponentFilter<> whenever an item is successfully filtered. (At the time of this writing, that's once per attach of each RenderComponent inheritor.)
                //component.OnAttach();
            }
            return this;
        }

        public void RemoveComponent<T>(T component) where T : Component
        {
            if (scene != null)
            {
                ComponentGroup<T> group = scene.GetComponentGroup<T>(); // downcasting within
                group.Remove(component);
            }
            component.Owner = null;
            componentReferences.Remove(component);
        }

        public T GetComponent<T>() where T : Component
        {
            return scene.GetComponentGroup<T>().GetByOwner(this);
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            return scene.GetComponentGroup<T>().TryGetByOwner(this, out component);
        }

        public Entity AddToGroup(string groupName)
        {
            if (groups.Contains(groupName)) return this;
            groups.Add(groupName);

            if (scene != null) scene.AddToGroup(groupName, this);
            return this;
        }

        private void RebindGroups()
        {
            if (scene == null) return;
            foreach (string group in groups)
            {
                scene.AddToGroup(group, this);
            }
            List<Component> refs = componentReferences;
            componentReferences = new List<Component>();
            foreach (Component component in refs)
            {
                component.AttachTo(this);
            }
        }

        public bool HasGroup(string groupName)
        {
            return groups.Contains(groupName);
        }

        public Entity AddTag(string tagName)
        {
            //if (tags == null) tags = new HashSet<string>();
            tags.Add(tagName);
            return this;
        }

        public Entity RemoveTag(string tagName)
        {
            //if (tags == null) return this;
            tags.Remove(tagName);
            return this;
        }

        public bool HasTag(string tagName)
        {
            //if (tags == null) return false;
            return tags.Contains(tagName);
        }

        public Entity Clone()
        {
            Entity entity = new Entity(scene);
            entity.CopyPropertiesFrom(this);
            return entity;
        }

        void CopyPropertiesFrom(Entity entity)
        {
            foreach (string groupName in entity.groups)
            {
                AddToGroup(groupName);
            }

            foreach (string tagName in entity.tags)
            {
                AddTag(tagName);
            }

            foreach (Component component in entity.componentReferences)
            {
                component.Clone().AttachTo(this);
            }
        }
    }
}
