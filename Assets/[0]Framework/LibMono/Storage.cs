/*===============================================================
Product:    EntityEngine
Developer:  Dimitry Pixeye - info@pixeye,games
Company:    Homebrew
Date:       7/25/2018 11:49 AM
================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Homebrew
{
    public abstract class Storage
    {
        public static List<Storage> all = new List<Storage>(40);
        public abstract void Remove(int index, bool addCallBacks = true);
    }

    public class Storage<T> : Storage where T : new()
    {
        public static readonly Storage<T> Instance = new Storage<T>();
        public T[] components = new T[EngineSettings.MinComponents];

        public bool[] entityHasComponent = new bool[EngineSettings.MinComponents];
        //  public int length = EngineSettings.MinComponents;


        public List<GroupBase> groups = new List<GroupBase>();

        public Storage()
        {
            all.Add(this);
        }


        public T Add(T component, int entityID)
        {
            if (entityID >= components.Length)
            {
                var l = entityID << 1;
            
                Array.Resize(ref components, l);
                Array.Resize(ref entityHasComponent, l);
            }

            components[entityID] = component;
            entityHasComponent[entityID] = true;


            int len = groups.Count;
            for (int i = 0; i < len; i++)
            {
                var gr = groups[i];

                if (gr.CheckTags(entityID))
                {
                    gr.TryAdd(entityID);
                }
            }


            return component;
        }

        public T Add(int entityID)
        {
            if (entityID >= components.Length)
            {
                var l = entityID << 1;
               
                Array.Resize(ref components, l);
                Array.Resize(ref entityHasComponent, l);
            }

            if (entityHasComponent[entityID]) return components[entityID];

            if (components[entityID] == null)
                components[entityID] = new T();


            entityHasComponent[entityID] = true;

            int len = groups.Count;
            for (int i = 0; i < len; i++)
            {
                var gr = groups[i];

                if (gr.CheckTags(entityID))
                {
                    gr.TryAdd(entityID);
                }
            }

            return components[entityID];
        }


        public override void Remove(int entityID, bool addCallBacks = true)
        {
            if (entityID >= entityHasComponent.Length) return;
            if (!entityHasComponent[entityID]) return;

            entityHasComponent[entityID] = false;

            int len = groups.Count;
            for (int i = 0; i < len; i++)
            {
                groups[i].Remove(entityID, addCallBacks);
            }
        }

        public bool HasComponent(int entityID)
        {
            return entityID >= components.Length ? false : entityHasComponent[entityID];
        }

        public T TryGet(int entityID)
        {
            return entityID >= components.Length || !entityHasComponent[entityID] ? default(T) : components[entityID];
        }
    }
}