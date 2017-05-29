using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGB
{
    public enum EntityType
    {
        Player,
        Attack,
        Misc,
        Boundary,
    }

    /// <summary>
    /// An Entity holds some data about what categories it falls into
    /// and holds all of the components that make it up.
    /// </summary>
    public class GEntity
    {
        public string Name { get; private set; }
        public bool Dead { get; private set; }
        public EntityType EntType { get; private set; }
        public int Faction { get; private set; }

        private Dictionary<Type, GComponent> components = new Dictionary<Type, GComponent>();
        private Dictionary<Type, Object> sharedData = new Dictionary<Type, Object>();

        public GEntity()
        {
            this.Name = "";
            Dead = false;
            this.EntType = EntityType.Misc;
        }

        public GEntity(string name)
        {
            this.Name = name;
            this.EntType = EntityType.Misc;
            this.Faction = -1;
            this.Dead = false;
        }

        public GEntity(string name, EntityType type, int faction)
        {
            this.Name = name;
            this.EntType = type;
            this.Faction = faction;
            this.Dead = false;
        }


        public void AddSharedData(Type t, Object o)
        {
            sharedData[t] = o;
        }

        public Object GetSharedData(Type t)
        {
            Object retData = null;
            sharedData.TryGetValue(t, out retData);
            return retData;
        }

        public void AddComponent(Type t, GComponent o)
        {
            components[t] = o;
        }

        public bool HasComponent(Type t)
        {
            return components.ContainsKey(t);
        }

        public bool HasSharedData(Type t)
        {
            return sharedData.ContainsKey(t);
        }

        public void KillEntity()
        {
            foreach (KeyValuePair<Type, GComponent> pair in components)
            {
                pair.Value.KillComponent();
            }

            Dead = true;
        }

        public void RemoveComponent(Type t)
        {
            if (components.ContainsKey(t))
            {
                components[t].KillComponent();
                components.Remove(t);
            }
        }

        public GComponent GetComponent(Type t)
        {
            GComponent retComponent = null;
            components.TryGetValue(t, out retComponent);
            //will be null if key is not found
            return retComponent;
        }

        public void KillComponent(Type t)
        {
            GComponent retComponent = null;
            if (components.TryGetValue(t, out retComponent))
            {
                retComponent.KillComponent();
            }
        }
    }
}
