﻿namespace Caliburn.Micro
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        void ShowDialog(object rootModel, object context = null);
    }

    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public class WindowManager : IWindowManager
    {
        static readonly DependencyProperty IsElementGeneratedProperty =
            DependencyProperty.RegisterAttached(
                "IsElementGenerated",
                typeof(bool),
                typeof(WindowManager),
                new PropertyMetadata(false, null)
                );

        bool actuallyClosing;

        /// <summary>
        /// Used to retrieve the root, non-framework-created view.
        /// </summary>
        /// <param name="view">The view to search.</param>
        /// <returns>The root element that was not created by the framework.</returns>
        /// <remarks>In certain instances the WindowManager creates UI elements in order to display windows.
        /// For example, if you ask the window manager to show a UserControl as a dialog, it creates a window to host the UserControl in.
        /// The WindowManager marks that element as a framework-created element so that it can determine what it created vs. what was intended by the developer.
        /// Calling GetSignificantView allows the framework to discover what the original element was. 
        /// </remarks>
        public static DependencyObject GetSignificantView(DependencyObject view)
        {
            if((bool)view.GetValue(IsElementGeneratedProperty))
                return (DependencyObject)((ContentControl)view).Content;

            return view;
        }

        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        public void ShowDialog(object rootModel, object context = null)
        {
            var view = EnsureWindow(rootModel, ViewLocator.LocateForModel(rootModel, null, context));
            ViewModelBinder.Bind(rootModel, view, context);

            var haveDisplayName = rootModel as IHaveDisplayName;
            if (haveDisplayName != null)
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                view.SetBinding(ChildWindow.TitleProperty, binding);
            }

            var activatable = rootModel as IActivate;
            if (activatable != null)
                activatable.Activate();

            var deactivatable = rootModel as IDeactivate;
            if (deactivatable != null)
                view.Closed += (s, e) => deactivatable.Deactivate(true);

            var guard = rootModel as IGuardClose;
            if (guard != null)
                view.Closing += (s, e) => OnShutdownAttempted(guard, view, e);

            view.Show();
        }

        static ChildWindow EnsureWindow(object model, object view)
        {
            var window = view as ChildWindow;

            if(window == null)
            {
                window = new ChildWindow { Content = view };
                window.SetValue(IsElementGeneratedProperty, true);
            }

            return window;
        }

        void OnShutdownAttempted(IGuardClose guard, ChildWindow view, CancelEventArgs e)
        {
            if (actuallyClosing)
            {
                actuallyClosing = false;
                return;
            }

            bool runningAsync = false, shouldEnd = false;

            guard.CanClose(canClose =>{
                if(runningAsync && canClose)
                {
                    actuallyClosing = true;
                    view.Close();
                }
                else e.Cancel = !canClose;

                shouldEnd = true;
            });

            if (shouldEnd)
                return;

            runningAsync = e.Cancel = true;
        }
    }
}