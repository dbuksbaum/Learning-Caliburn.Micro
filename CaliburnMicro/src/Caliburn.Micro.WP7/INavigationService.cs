﻿namespace Caliburn.Micro
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    /// <summary>
    /// Implemented by services that provide <see cref="Uri"/> based navigation.
    /// </summary>
    public interface INavigationService : INavigate
    {
        /// <summary>
        /// The <see cref="Uri"/> source.
        /// </summary>
        Uri Source { get; set; }

        /// <summary>
        /// Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// The current <see cref="Uri"/> source.
        /// </summary>
        Uri CurrentSource { get; }

        /// <summary>
        /// Stops the loading process.
        /// </summary>
        void StopLoading();

        /// <summary>
        /// Navigates back.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Navigates forward.
        /// </summary>
        void GoForward();

        /// <summary>
        /// Raised after navigation.
        /// </summary>
        event NavigatedEventHandler Navigated;

        /// <summary>
        /// Raised prior to navigation.
        /// </summary>
        event NavigatingCancelEventHandler Navigating;

        /// <summary>
        /// Raised when navigation fails.
        /// </summary>
        event NavigationFailedEventHandler NavigationFailed;

        /// <summary>
        /// Raised when navigation is stopped.
        /// </summary>
        event NavigationStoppedEventHandler NavigationStopped;

        /// <summary>
        /// Raised when a fragment navigation occurs.
        /// </summary>
        event FragmentNavigationEventHandler FragmentNavigation;
    }

    /// <summary>
    /// A basic implementation of <see cref="INavigationService"/> designed to adapt the <see cref="Frame"/> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {
        readonly Frame frame;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter"/>
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService"/>.</param>
        public FrameAdapter(Frame frame)
        {
            this.frame = frame;
            this.frame.Navigated += OnNavigated;
            this.frame.Navigating += OnNavigating;
        }

        /// <summary>
        /// Occurs before navigation
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var fe = frame.Content as FrameworkElement;
            if (fe == null)
                return;

            var guard = fe.DataContext as IGuardClose;
            if(guard != null && !e.Uri.IsAbsoluteUri)
            {
                bool shouldCancel = false;
                guard.CanClose(result =>{
                    shouldCancel = !result;
                });

                if(shouldCancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            var deactivator = fe.DataContext as IDeactivate;
            if (deactivator != null)
                deactivator.Deactivate(e.Uri.IsAbsoluteUri);
        }

        /// <summary>
        /// Occurs after navigation
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.IsAbsoluteUri)
                return;

            var viewModel = ViewModelLocator.LocateForView(e.Content);
            if (viewModel == null)
                return;

            ViewModelBinder.Bind(viewModel, (DependencyObject)e.Content, null);

            TryInjectQueryString(viewModel, e.Content);

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();
        }

        /// <summary>
        /// Attempts to inject query string parameters from the view into the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="view">The view.</param>
        protected virtual void TryInjectQueryString(object viewModel, object view) 
        {
            var page = view as Page;
            if (page == null)
                return;

            var viewModelType = viewModel.GetType();

            foreach(var pair in page.NavigationContext.QueryString)
            {
                var property = viewModelType.GetProperty(pair.Key);
                if(property == null)
                    continue;

                property.SetValue(viewModel, MessageBinder.CoerceValue(property.PropertyType, pair.Value), null);
            }
        }

        /// <summary>
        /// The <see cref="Uri"/> source.
        /// </summary>
        public Uri Source
        {
            get { return frame.Source; }
            set { frame.Source = value; }
        }

        /// <summary>
        /// Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return frame.CanGoBack; }
        }

        /// <summary>
        /// Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return frame.CanGoForward; }
        }

        /// <summary>
        /// The current <see cref="Uri"/> source.
        /// </summary>
        public Uri CurrentSource
        {
            get { return frame.CurrentSource; }
        }

        /// <summary>
        /// Stops the loading process.
        /// </summary>
        public void StopLoading()
        {
            frame.StopLoading();
        }

        /// <summary>
        /// Navigates back.
        /// </summary>
        public void GoBack()
        {
            frame.GoBack();
        }

        /// <summary>
        /// Navigates forward.
        /// </summary>
        public void GoForward()
        {
            frame.GoForward();
        }

        /// <summary>
        /// Navigates to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> to navigate to.</param>
        /// <returns>Whether or not navigation succeeded.</returns>
        public bool Navigate(Uri source)
        {
            return frame.Navigate(source);
        }

        /// <summary>
        /// Raised after navigation.
        /// </summary>
        public event NavigatedEventHandler Navigated
        {
            add { frame.Navigated += value; }
            remove { frame.Navigated -= value; }
        }

        /// <summary>
        /// Raised prior to navigation.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating
        {
            add { frame.Navigating += value; }
            remove { frame.Navigating -= value; }
        }

        /// <summary>
        /// Raised when navigation fails.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed
        {
            add { frame.NavigationFailed += value; }
            remove { frame.NavigationFailed -= value; }
        }

        /// <summary>
        /// Raised when navigation is stopped.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped
        {
            add { frame.NavigationStopped += value; }
            remove { frame.NavigationStopped -= value; }
        }

        /// <summary>
        /// Raised when a fragment navigation occurs.
        /// </summary>
        public event FragmentNavigationEventHandler FragmentNavigation
        {
            add { frame.FragmentNavigation += value; }
            remove { frame.FragmentNavigation -= value; }
        }
    }
}