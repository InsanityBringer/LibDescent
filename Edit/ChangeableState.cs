using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LibDescent.Edit
{
    using PausedStateDirtySet = Dictionary<string, object>;

    public delegate void PropertyChangeEventHandler(object sender, PropertyChangeEventArgs e);
    public delegate void BeforePropertyChangeEventHandler(object sender, BeforePropertyChangeEventArgs e);

    /// <summary>
    /// Represents a state class containing properties that cause an event whenever
    /// their value is changed. It is expected that properties implemented in subclasses
    /// will appropriately use AssignChanged or AssignAlways instead of plain assignment
    /// to make sure events can be raised when a property is changed.
    /// </summary>
    public abstract class ChangeableState
    {
        private object _stateLock = new object();
        private readonly Dictionary<string, bool> _propSubscribeCache = new Dictionary<string, bool>();
        private readonly Dictionary<string, SubstateListener> _substates = new Dictionary<string, SubstateListener>();
        private PausedStateDirtySet _pausedStateDirtySet = null;
        private bool reentrant = false;

        /// <summary>
        /// Called after a property has been changed. The new value is provided as a convenience.
        /// </summary>
        public event PropertyChangeEventHandler PropertyChanged;

        /// <summary>
        /// Called before a property has been changed. The new value is provided as a convenience. This
        /// event might not be called for read-only events. The old value may be equal by value or refrence
        /// to the new value.
        /// </summary>
        public event BeforePropertyChangeEventHandler BeforePropertyChanged;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldUpdatePropertyAsSubstate(PropertyInfo prop)
        {
            return prop != null && !Attribute.IsDefined(prop, typeof(NoSubstateAutoSubscribeAttribute));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldUpdatePropertyAsSubstate(string propertyName)
        {
            if (propertyName.Contains("."))
                return false; // never subscribe to sub-sub-states, as the events will be passed up anyway
            return _propSubscribeCache.ContainsKey(propertyName)
                ? _propSubscribeCache[propertyName]
                : _propSubscribeCache[propertyName] = ShouldUpdatePropertyAsSubstate(this.GetType().GetProperty(propertyName));
        }

        private void OnPropertyChanged(string propertyName, object newValue)
        {
            bool lockTaken = false;
            bool iAmEntrant = false;
            try
            {
                Monitor.Enter(_stateLock, ref lockTaken);

                if (reentrant) return; // prevent re-entry from substate update => prevents infinite loops
                reentrant = iAmEntrant = true;

                bool shouldUpdateSubstate = newValue is ChangeableState && ShouldUpdatePropertyAsSubstate(propertyName);
                if (_substates.ContainsKey(propertyName)) // discard old substate regardless
                {
                    SubstateListener substate = _substates[propertyName];
                    substate.State.PropertyChanged -= substate.Handler;
                    _substates.Remove(propertyName);
                }

                if (_pausedStateDirtySet != null) // paused?
                {
                    _pausedStateDirtySet[propertyName] = newValue;
                    return;
                }

                if (shouldUpdateSubstate)
                {
                    ChangeableState newSubstate = (ChangeableState)newValue;
                    PropertyChangeEventHandler newHandler = (object sender, PropertyChangeEventArgs e) =>
                    {
                        this.OnPropertyChanged(propertyName + "." + e.PropertyName, e.NewValue);
                    };
                    _substates[propertyName] = new SubstateListener(newSubstate, newHandler);
                    newSubstate.PropertyChanged += newHandler;
                }
                PropertyChanged?.Invoke(this, new PropertyChangeEventArgs(propertyName, newValue));
            }
            finally
            {
                reentrant &= !iAmEntrant;
                if (lockTaken) Monitor.Exit(_stateLock);
            }
        }

        /// <summary>
        /// Raises PropertyChanged without raising BeforePropertyChanged. Use this, if the value of an otherwise
        /// read-only property is updated because of a change in value in another property. Use AssignChanged
        /// instead of read/write properties.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="newValue">The new value assigned, and the value that will be given to event handlers.</param>
        protected void OnReadOnlyPropertyChanged(string propertyName, object newValue)
        {
            OnPropertyChanged(propertyName, newValue);
        }

        /// <summary>
        /// Assigns a new value to a variable. Returns whether the value was changed
        /// (i.e. whether the old value was different from the new value), and if
        /// so, will also automatically call OnPropertyChanged with the name of 
        /// the property that this was called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable to assign to.</param>
        /// <param name="newValue">The new value to assign.</param>
        /// <param name="property">The property name. This is filled in automatically;
        /// if you want to enter another value, please use AssignChanged.</param>
        /// <returns></returns>
        protected bool AssignChanged<T>(ref T variable, T newValue, [CallerMemberName] string property = null)
        {
            T oldValue = variable;
            bool changed = !EqualityComparer<T>.Default.Equals(oldValue, newValue);
            if (changed && _pausedStateDirtySet != null) this.BeforePropertyChanged?.Invoke(this, new BeforePropertyChangeEventArgs(property, oldValue, newValue));
            variable = newValue;
            if (changed) OnPropertyChanged(property, newValue);
            return changed;
        }

        /// <summary>
        /// Assigns a new value to a variable. Returns whether the value was changed
        /// (i.e. whether the old value was different from the new value), and if
        /// so, will also automatically call OnPropertyChanged. Use this if the
        /// property name that is reported for events needs to be different from the
        /// actual name of the property this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable to assign to.</param>
        /// <param name="newValue">The new value to assign.</param>
        /// <param name="property">The property name. This should be the
        /// plain property name (i.e. no nesting or dots) and must be a valid
        /// property name for the object (a property with the name must exist).</param>
        /// <returns></returns>
        protected bool AssignChangedRename<T>(ref T variable, T newValue, string property)
        {
            T oldValue = variable;
            bool changed = !EqualityComparer<T>.Default.Equals(oldValue, newValue);
            if (changed && _pausedStateDirtySet != null) this.BeforePropertyChanged?.Invoke(this, new BeforePropertyChangeEventArgs(property, oldValue, newValue));
            variable = newValue;
            if (changed) OnPropertyChanged(property, newValue);
            return changed;
        }

        /// <summary>
        /// Assigns a new value to a variable and calls OnPropertyChanged even if
        /// the value was not changed with the name of the property that this was 
        /// called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable to assign to.</param>
        /// <param name="newValue">The new value to assign.</param>
        /// <param name="property">The property name. This is filled in automatically;
        /// if you want to enter another value, please use AssignAlwaysRename.</param>
        /// <returns></returns>
        protected void AssignAlways<T>(ref T variable, T newValue, [CallerMemberName] string property = null)
        {
            if (_pausedStateDirtySet != null) this.BeforePropertyChanged?.Invoke(this, new BeforePropertyChangeEventArgs(property, variable, newValue));
            variable = newValue;
            OnPropertyChanged(property, newValue);
        }

        /// <summary>
        /// Assigns a new value to a variable and calls OnPropertyChanged even if
        /// the value was not changed. Use this if the  property name that is reported
        /// for events needs to be different from the actual name of the property
        /// this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">The variable to assign to.</param>
        /// <param name="newValue">The new value to assign.</param>
        /// <param name="property">The property name. This should be the
        /// plain property name (i.e. no nesting or dots) and must be a valid
        /// property name for the object (a property with the name must exist).</param>
        /// <returns></returns>
        protected void AssignAlwaysRename<T>(ref T variable, T newValue, string property)
        {
            if (_pausedStateDirtySet != null) this.BeforePropertyChanged?.Invoke(this, new BeforePropertyChangeEventArgs(property, variable, newValue));
            variable = newValue;
            OnPropertyChanged(property, newValue);
        }

        /// <summary>
        /// Causes all property change events to be suppressed until the next
        /// ResumeStateEvents call.
        /// </summary>
        protected void PauseStateEvents()
        {
            lock (_stateLock)
            {
                if (_pausedStateDirtySet != null)
                    throw new InvalidOperationException("The state is already paused.");
                _pausedStateDirtySet = new PausedStateDirtySet();
            }
        }

        /// <summary>
        /// Unpauses property change events paused by the previous call to
        /// PauseStateEvents, and raises events for all properties that have
        /// been changed during the time the events were paused.
        /// </summary>
        protected void ResumeStateEvents()
        {
            lock (_stateLock)
            {
                PausedStateDirtySet set = _pausedStateDirtySet;
                if (set == null)
                    throw new InvalidOperationException("The state was not paused.");
                _pausedStateDirtySet = null;
                foreach ((string property, object data) in set.Select(x => (x.Key, x.Value)))
                {
                    OnPropertyChanged(property, data);
                }
            }
        }
    }

    /// <summary>
    /// If added to a property with a data type that inherits from ChangeableState,
    /// the substate will not be automatically subscribed to from the parent object,
    /// which prevents property change events from automatically passing from the
    /// property to the parent. This should be used in case there are cyclic
    /// references to prevent an infinite loop.
    /// </summary>
    public class NoSubstateAutoSubscribeAttribute : Attribute
    {
    }

    // used to handle substates
    internal struct SubstateListener
    {
        internal readonly ChangeableState State;
        internal readonly PropertyChangeEventHandler Handler;

        public SubstateListener(ChangeableState state, PropertyChangeEventHandler handler)
        {
            State = state;
            Handler = handler;
        }
    }

    /// <summary>
    /// The event arguments for a PropertyChange event.
    /// </summary>
    public class PropertyChangeEventArgs : EventArgs
    {
        private string propertyName;
        private object newValue;

        public PropertyChangeEventArgs(string propertyName, object newValue)
        {
            this.propertyName = propertyName;
            this.newValue = newValue;
        }

        /// <summary>
        /// The full name of the property that changed, possibly including dots
        /// for properties that changed in substates.
        /// </summary>
        public string PropertyName
        {
            get => propertyName;
        }

        /// <summary>
        /// The new value of the property.
        /// </summary>
        public object NewValue
        {
            get => newValue;
        }
    }

    /// <summary>
    /// The event arguments for a BeforePropertyChange event.
    /// </summary>
    public class BeforePropertyChangeEventArgs : EventArgs
    {
        private string propertyName;
        private object oldValue;
        private object newValue;

        public BeforePropertyChangeEventArgs(string propertyName, object oldValue, object newValue)
        {
            this.propertyName = propertyName;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// The full name of the property that changed, possibly including dots
        /// for properties that changed in substates.
        /// </summary>
        public string PropertyName
        {
            get => propertyName;
        }

        /// <summary>
        /// The old value of the property.
        /// </summary>
        public object OldValue
        {
            get => oldValue;
        }

        /// <summary>
        /// The new value of the property.
        /// </summary>
        public object NewValue
        {
            get => newValue;
        }
    }
}