// PluginItemBase.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using dEngine.Instances;

namespace dEditor.Framework.Plugins
{
    public class PluginItemBase : Instance, INotifyPropertyChangedEx
    {
        /// <summary>
        /// Enables/Disables property change notification.
        /// Virtualized in order to help with document oriented view models.
        /// </summary>
        public virtual bool IsNotifying { get; set; }

        /// <summary>Occurs when a property value changes.</summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates an instance of <see cref="T:Caliburn.Micro.PropertyChangedBase" />.
        /// </summary>
        public PluginItemBase()
        {
            IsNotifying = true;
        }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public virtual void Refresh()
        {
            this.NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>Notifies subscribers of the property change.</summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            // ISSUE: reference to a compiler-generated field
            if (!this.IsNotifying || this.PropertyChanged == null)
                return;
            ((System.Action)(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)))).OnUIThread();
        }

        /// <summary>Notifies subscribers of the property change.</summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property expression.</param>
        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            NotifyOfPropertyChange(property.GetMemberInfo().Name);
        }

        /// <summary>
        /// Raises the <see cref="E:Caliburn.Micro.PropertyChangedBase.PropertyChanged" /> event directly.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler changedEventHandler = PropertyChanged;
            changedEventHandler?.Invoke(this, e);
        }
    }
}