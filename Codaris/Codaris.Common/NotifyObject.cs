// <copyright file="NotifyObject.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Codaris.Common
{
    /// <summary>
    /// Abstract base class for classes that implement INotifyPropertyChanged.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property value and raises the changed event.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="onChanged">The action to execute if the value changes.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// True if property was changed.
        /// </returns>
        protected bool SetProperty<T>(ref T field, T value, Action<T> onChanged = null, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            onChanged?.Invoke(value);
            OnPropertyChanged(name);
            return true;
        }

        /// <summary>
        /// Called when the property changed.
        /// </summary>
        /// <param name="name">The property name.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
