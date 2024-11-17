// <copyright file="BaseViewModel.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Codaris.Common;

namespace Codaris.Wpf
{
    /// <summary>
    /// Base for all view models.
    /// </summary>
    /// <seealso cref="Codaris.Common.NotifyObject" />
    public abstract class BaseViewModel : NotifyObject
    {
        /// <summary>
        /// The propagated properties.
        /// </summary>
        private readonly ConditionalWeakTable<INotifyPropertyChanged, Dictionary<string, List<string>>> propagatedProperties = [];

        /// <summary>
        /// Runs the on UI thread.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void RunOnUIThread(Action action)
        {
            Assert.IsNotNull(action); 
            if (Application.Current?.Dispatcher?.CheckAccess() ?? true) action();
            else Application.Current.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Runs the action on UI thread async.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RunOnUIThreadAsync(Action action)
        {
            Assert.IsNotNull(action);
            if (Application.Current?.Dispatcher?.CheckAccess() ?? true) action();
            else await Application.Current.Dispatcher.InvokeAsync(action);
        }

        /// <summary>
        /// Propagates the property changed event of another object.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instancePropertyName">Name of the instance property.</param>
        /// <param name="localProperties">The local properties.</param>
        internal protected void PropagatePropertyChanged(INotifyPropertyChanged instance, string instancePropertyName, params string[] localProperties)
        {
            Assert.IsNotNull(instance);
            var instanceProperties = propagatedProperties.GetOrCreateValue(instance);
            if (!instanceProperties.TryGetValue(instancePropertyName, out var thisProperties))
            {
                thisProperties = [];
                instanceProperties.Add(instancePropertyName, thisProperties);
            }

            thisProperties.AddRange(localProperties);
            instance.PropertyChanged -= PropagatePropertyChangeEventHandler;
            instance.PropertyChanged += PropagatePropertyChangeEventHandler;
        }

        /// <summary>
        /// Propagating property change event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void PropagatePropertyChangeEventHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (!propagatedProperties.TryGetValue((INotifyPropertyChanged)sender!, out var fields)) return;
            if (e.PropertyName == null || !fields.TryGetValue(e.PropertyName, out var value)) return;
            foreach (var propertyName in value) OnPropertyChanged(propertyName);
        }
    }
}
