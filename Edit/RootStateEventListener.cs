using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LibDescent.Edit
{
    using PropertyGetterDelegate = Func<object>;
    using PropertySetterDelegate = Action<object>;

    internal delegate object GetPropertyValueUncachedDelegate(object root, string property);

    /// <summary>
    /// An event listener for ChangeableStates that will automatically pass events
    /// to event handlers registered through it, and will also pass events down if an
    /// event handler is listening to a change in a state and the parent state itself
    /// changes.
    /// </summary>
    public class RootStateEventListener : IDisposable
    {
        private ChangeableState root;

        /// <summary>
        /// Defines a new RootStateEventListener with the given state being considered
        /// the "root" state, relative to which all properties will be resolved and which
        /// will be listened for.
        /// </summary>
        /// <param name="root">The root state. All events and properties will be relative
        /// to the root state.</param>
        /// <exception cref="ArgumentNullException">The passed state was null.</exception>
        public RootStateEventListener(ChangeableState root)
        {
            this.root = root;
            if (this.root == null)
                throw new ArgumentNullException(nameof(root));

            this.tree = new PropertyNameTree();
            this.root.PropertyChanged += OnRootPropertyChanged;
        }

        #region --- implementation

        private bool disposed = false;
        private readonly PropertyNameTree tree;
        private readonly object treeLock = new object();

        public void Dispose()
        {
            if (!disposed)
            {
                this.root.PropertyChanged -= OnRootPropertyChanged;
                this.tree.Dispose();
                disposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private void OnRootPropertyChanged(object sender, PropertyChangeEventArgs e)
        {
            tree.Call(e, GetPropertyValueUncached);
        }

        private static PropertyGetterDelegate MakeGetterGeneric<T, R>(MethodInfo getter, T parent) where T : class
        {
            Func<T, R> typedGetter = (Func<T, R>)getter.CreateDelegate(typeof(Func<T, R>));
            return () => typedGetter(parent);
        }

        private static PropertySetterDelegate MakeSetterGeneric<T, V>(MethodInfo setter, T parent) where T : class
        {
            Action<T, V> typedSetter = (Action<T, V>)setter.CreateDelegate(typeof(Action<T, V>));
            return (object v) => typedSetter(parent, (V)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PropertyGetterDelegate MakeGetter(object parent, PropertyInfo prop)
        {
            MethodInfo getterMethod = prop.GetGetMethod();
            if (getterMethod == null)
                return () => throw new InvalidOperationException("The property " + prop.Name + " has no public getter");

            MethodInfo genericHelper = typeof(RootStateEventListener).GetMethod(nameof(MakeGetterGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo genericizedHelper = genericHelper.MakeGenericMethod(parent.GetType(), prop.PropertyType);
            return (PropertyGetterDelegate)genericizedHelper.Invoke(null, new object[] { getterMethod, parent });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PropertySetterDelegate MakeSetter(object parent, PropertyInfo prop)
        {
            MethodInfo setterMethod = prop.GetSetMethod();
            if (setterMethod == null)
                return (object v) => throw new InvalidOperationException("The property " + prop.Name + " has no public setter");

            MethodInfo genericHelper = typeof(RootStateEventListener).GetMethod(nameof(MakeSetterGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo genericizedHelper = genericHelper.MakeGenericMethod(parent.GetType(), prop.PropertyType);
            return (PropertySetterDelegate)genericizedHelper.Invoke(null, new object[] { setterMethod, parent });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PropertyCacheEntry ResolvePropertyUncached(object root, string property)
        {
            object self = root;
            PropertyInfo tmp = null;
            PropertyCacheEntry pce = new PropertyCacheEntry();
            /* pce.PropertyType = null; */

            foreach (string tok in property.Split('.'))
            {
                if (tok == "") continue;
                if (tmp != null) self = tmp.GetValue(self);
                if (self == null) break;
                tmp = self.GetType().GetProperty(tok);
                if (tmp == null) break;
            }

            if (self != null && tmp != null)
                pce = new PropertyCacheEntry(tmp.PropertyType, MakeGetter(self, tmp), MakeSetter(self, tmp));

            lock (treeLock)
                if (Object.Equals(root, root))
                    tree.Cache(property, pce);
            return pce;
        }

        private PropertyCacheEntry ResolveProperty(object root, string property)
        {
            if (Object.Equals(root, root))
            {
                // maybe lookup cache
                lock (treeLock)
                {
                    if (tree.GetCachedEntry(property, out PropertyCacheEntry entry))
                    {
                        return entry;
                    }
                }
            }
            return ResolvePropertyUncached(root, property);
        }

        private object GetPropertyValueUncached(object root, string property)
        {
            PropertyCacheEntry pce = ResolvePropertyUncached(root, property);
            return pce.Getter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetPropertyValue(object root, string property)
        {
            PropertyCacheEntry pce = ResolveProperty(root, property);
            if (pce.PropertyType == null) throw new ArgumentException("The property '" + property + "' could not be found.");
            return pce.Getter();
        }

        /// <summary>
        /// Returns the type of a property resolved relative to the root state,
        /// or null if the property could not be found.
        /// </summary>
        /// <param name="property">The full name of the property, relative to the root state.
        /// Dots are allowed for properties within properties.</param>
        /// <returns>The type of the property, or null if it could not be resolved.</returns>
        public Type GetPropertyType(string property)
        {
            CheckNotDisposed();
            PropertyCacheEntry pce = ResolveProperty(root, property);
            return pce.PropertyType;
        }

        /// <summary>
        /// Returns the current value of a property resolved relative to the root state.
        /// </summary>
        /// <param name="property">The full name of the property, relative to the root state.
        /// Dots are allowed for properties within properties.</param>
        /// <returns>The value of the property.</returns>
        /// <exception cref="ArgumentException">The property with the name could not be found.</exception>
        public object GetPropertyValue(string property)
        {
            CheckNotDisposed();
            return GetPropertyValue(root, property);
        }

        /// <summary>
        /// Assigns the current value of a property resolved relative to the root state and
        /// returns whether the assignment was successful.
        /// </summary>
        /// <param name="property">The full name of the property, relative to the root state.
        /// Dots are allowed for properties within properties.</param>
        /// <param name="value">The new value to assign.</param>
        /// <returns>Whether the value was successfully assigned.</returns>
#if DEBUG
        public bool SetPropertyValue(string property, object value)
        {
            CheckNotDisposed();
            try
            {
                SetPropertyValue_i(property, value);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(new UnableToSetPropertyWarningException(property, value, e));
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPropertyValue_i(string property, object value)
        {
            PropertyCacheEntry pce = ResolveProperty(root, property);
            if (pce.PropertyType == null)
            {
                throw new ArgumentException("Property '" + property + "' could not be resolved.");
            }
            try
            {
                pce.Setter(value);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
#else
        public bool SetPropertyValue(string property, object value)
        {
            CheckNotDisposed();
            PropertyCacheEntry pce = ResolveProperty(root, property);
            if (pce.PropertyType == null)
                return false;
            try
            {
                pce.Setter(value);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
#endif

        /// <summary>
        /// Applies a function to a property value, such as incrementing or negating it.
        /// </summary>
        /// <param name="property">The full name of the property, relative to the root state.
        /// Dots are allowed for properties within properties.</param>
        /// <param name="func">The function to apply.</param>
        /// <returns>Whether the new value was successfully assigned.</returns>
        public bool ApplyToPropertyValue(string property, Func<object, object> func)
        {
            return SetPropertyValue(property, func(GetPropertyValue(root, property)));
        }

        /// <summary>
        /// Registers a new event handler for this event listener.
        /// </summary>
        /// <param name="property">The property the handler should listen to. This should
        /// be the full name of the property, relative to the root state. Dots are allowed
        /// for properties within properties.</param>
        /// <param name="handler">The event handler that should be called when the property
        /// or any of its ancestors change.</param>
        public void Register(string property, PropertyChangeEventHandler handler)
        {
            CheckNotDisposed();
            tree.Add(property, handler);
        }

        /// <summary>
        /// Registers a new event handler for this event listener and calls it immediately
        /// as if the property had changed the moment the handler was registered.
        /// </summary>
        /// <param name="property">The property the handler should listen to. This should
        /// be the full name of the property, relative to the root state. Dots are allowed
        /// for properties within properties.</param>
        /// <param name="handler">The event handler that should be called when the property
        /// or any of its ancestors change.</param>
        public void RegisterAndCall(string property, PropertyChangeEventHandler handler)
        {
            CheckNotDisposed();
            Register(property, handler);
            handler(this, new PropertyChangeEventArgs(property, GetPropertyValueUncached(root, property)));
        }

        #endregion
    }

    public struct PropertyCacheEntry
    {
        public Type PropertyType { get; }
        public PropertyGetterDelegate Getter { get; }
        public PropertySetterDelegate Setter { get; }

        public PropertyCacheEntry(Type type, PropertyGetterDelegate getter, PropertySetterDelegate setter)
        {
            PropertyType = type;
            Getter = getter;
            Setter = setter;
        }
    }

    [Serializable]
    public class UnableToSetPropertyWarningException : System.ComponentModel.WarningException
    {
        public UnableToSetPropertyWarningException(string property, object value, Exception innerException) : base("Unable to assign new value (" + (value?.ToString() ?? "null") + ") to property '" + property + "'.", innerException)
        {
        }
    }
}
