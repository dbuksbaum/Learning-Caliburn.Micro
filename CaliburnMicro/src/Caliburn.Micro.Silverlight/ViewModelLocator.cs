﻿namespace Caliburn.Micro
{
    using System;
    using System.Windows;

    /// <summary>
    /// A strategy for determining which view model to use for a given view.
    /// </summary>
    public static class ViewModelLocator
    {
        /// <summary>
        /// Locates the view model for the specified view type.
        /// </summary>
        /// <returns>The view model.</returns>
        /// <remarks>Pass the view type as a parameter and recieve a view model instance.</remarks>
        public static Func<Type, object> LocateForViewType = viewType =>{
            var typeName = viewType.FullName;

            if(!typeName.EndsWith("View"))
                typeName += "View";

            var viewModelName = typeName.Replace("View", "ViewModel");
            var key = viewModelName.Substring(viewModelName.LastIndexOf(".") + 1);

            return IoC.GetInstance(null, key);
        };

        /// <summary>
        /// Locates the view model for the specified view instance.
        /// </summary>
        /// <returns>The view model.</returns>
        /// <remarks>Pass the view instance as a parameters and receive a view model instance.</remarks>
        public static Func<object, object> LocateForView = view =>{
            if(view == null)
                return null;

            var frameworkElement = view as FrameworkElement;
            if(frameworkElement != null && frameworkElement.DataContext != null)
                return frameworkElement.DataContext;

            return LocateForViewType(view.GetType());
        };
    }
}