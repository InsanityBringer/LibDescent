using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LibDescent.Edit
{
    /// <summary>
    /// A prefix tree or trie implemented for storing property change event listeners. The
    /// end purpose is use in the RootStateEventHandler to ensure that property change events
    /// are passed down to listeners that are listening to either a change in that property
    /// or any property contained within; that is, if the changed property is itself a
    /// ChangeableState and a property listener is listening for an event within that state,
    /// it should also receive an event if the state itself changes.
    /// </summary>
    internal class PropertyNameTree : IDisposable
    {
        PropertyNameNode root;

        public PropertyNameTree()
        {
            root = new PropertyNameNode("");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PropertyNameNode Traverse(string key)
        {
            PropertyNameNode node = root;
            foreach (string tok in key.Split('.'))
            {
                if (tok == "") continue;
                node = node[tok];
                if (node == null) break;
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PropertyNameNode TraverseOrCreate(string key)
        {
            PropertyNameNode node = root;
            foreach (string tok in key.Split('.'))
            {
                if (tok == "") continue;
                node = node.GetOrCreate(tok);
            }
            return node;
        }

        internal void Add(string property, PropertyChangeEventHandler callback)
        {
            TraverseOrCreate(property).Leaves.Add(callback);
        }

        internal void Call(PropertyChangeEventArgs e, GetPropertyValueUncachedDelegate GetPropertyValueUncached)
        {
            Traverse(e.PropertyName)?.CallAllRecursive(e.PropertyName, e.NewValue, GetPropertyValueUncached, true);
        }

        internal bool GetCachedEntry(string key, out PropertyCacheEntry entry)
        {
            entry = new PropertyCacheEntry();
            PropertyNameNode node = Traverse(key);
            if (node != null)
            {
                entry = node.Cache;
                return entry.PropertyType != null;
            }
            return false;
        }

        internal void Cache(string key, PropertyCacheEntry entry)
        {
            TraverseOrCreate(key).Cache = entry;
        }

        public void Dispose()
        {
            root?.Dispose();
            root = null;
        }
    }

    internal class PropertyNameNode : IDisposable
    {
        private readonly string word;
        private readonly Dictionary<string, PropertyNameNode> children;
        private readonly List<PropertyChangeEventHandler> leaves;
        private PropertyCacheEntry propertyCache;

        internal PropertyNameNode(string word)
        {
            this.word = word;
            this.children = new Dictionary<string, PropertyNameNode>();
            this.leaves = new List<PropertyChangeEventHandler>();
            this.propertyCache = new PropertyCacheEntry();
        }

        internal string Name
        {
            get => word;
        }

        internal PropertyNameNode this[string word]
        {
            get
            {
                bool gotIt = children.TryGetValue(word, out PropertyNameNode value);
                return gotIt ? value : null;
            }
        }

        internal PropertyNameNode GetOrCreate(string word)
        {
            bool gotIt = children.TryGetValue(word, out PropertyNameNode value);
            if (!gotIt)
            {
                value = new PropertyNameNode(word);
                children[word] = value;
            }
            return value;
        }

        internal IEnumerable<PropertyNameNode> Children
        {
            get
            {
                return this.children.Values;
            }
        }

        internal List<PropertyChangeEventHandler> Leaves
        {
            get
            {
                return this.leaves;
            }
        }

        internal PropertyCacheEntry Cache
        {
            get
            {
                return this.propertyCache;
            }
            set
            {
                this.propertyCache = value;
            }
        }

        internal void CallAllRecursive(string basePath, object rootValue, GetPropertyValueUncachedDelegate GetPropertyValueUncached, bool top)
        {
            PropertyChangeEventArgs e = new PropertyChangeEventArgs(top ? basePath : basePath + "." + this.Name, top ? rootValue : GetPropertyValueUncached(rootValue, this.Name));
            if (!top) // since the root changed, we must invalidate cache
                this.propertyCache = new PropertyCacheEntry();
            foreach (PropertyChangeEventHandler EventHandler in this.Leaves) // call listeners
                EventHandler(this, e);
            foreach (PropertyNameNode n in this.Children)
                n?.CallAllRecursive(e.PropertyName, e.NewValue, GetPropertyValueUncached, false);
        }

        public void Dispose()
        {
            foreach (PropertyNameNode n in this.Children)
                n.Dispose();
            this.children.Clear();
            this.leaves.Clear();
            this.propertyCache = new PropertyCacheEntry();
        }
    }
}
