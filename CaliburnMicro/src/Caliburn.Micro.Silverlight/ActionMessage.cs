﻿namespace Caliburn.Micro
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;
    using System.Windows.Markup;
    using System.Windows.Media;

    /// <summary>
    /// Used to send a message from the UI to a presentation model class, indicating that a particular Action should be invoked.
    /// </summary>
    [DefaultTrigger(typeof(FrameworkElement), typeof(System.Windows.Interactivity.EventTrigger), "MouseLeftButtonDown")]
    [DefaultTrigger(typeof(ButtonBase), typeof(System.Windows.Interactivity.EventTrigger), "Click")] 
    [ContentProperty("Parameters")]
    [TypeConstraint(typeof(FrameworkElement))]
    public class ActionMessage : TriggerAction<FrameworkElement>
    {
        static readonly ILog Log = LogManager.GetLog(typeof(ActionMessage));

        /// <summary>
        /// Represents the method name of an action message.
        /// </summary>
        public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register(
                "MethodName",
                typeof(string),
                typeof(ActionMessage),
                null
                );

        /// <summary>
        /// Represents the parameters of an action message.
        /// </summary>
        public static readonly DependencyProperty ParametersProperty = 
            DependencyProperty.Register(
            "Parameters",
            typeof(AttachedCollection<Parameter>),
            typeof(ActionMessage), 
            null
            );

        ActionExecutionContext context;

        /// <summary>
        /// Creates an instance of <see cref="ActionMessage"/>.
        /// </summary>
        public ActionMessage()
        {
            SetValue(ParametersProperty, new AttachedCollection<Parameter>());
        }

        /// <summary>
        /// Gets or sets the name of the method to be invoked on the presentation model class.
        /// </summary>
        /// <value>The name of the method.</value>
        [Category("Common Properties")]
        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        /// <summary>
        /// Gets the parameters to pass as part of the method invocation.
        /// </summary>
        /// <value>The parameters.</value>
        [Category("Common Properties")]
        public AttachedCollection<Parameter> Parameters
        {
            get { return (AttachedCollection<Parameter>)GetValue(ParametersProperty); }
        }

        /// <summary>
        /// Occurs before the message detaches from the associated object.
        /// </summary>
        public event EventHandler Detaching = delegate { };

        protected override void OnAttached()
        {
            if (!Bootstrapper.IsInDesignMode)
            {
                Parameters.Attach(AssociatedObject);
                Parameters.Apply(x => x.MakeAwareOf(this));

                if((bool)AssociatedObject.GetValue(View.IsLoadedProperty))
                    ElementLoaded(null, null);
                else AssociatedObject.Loaded += ElementLoaded;
            }

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (!Bootstrapper.IsInDesignMode)
            {
                Detaching(this, EventArgs.Empty);
                AssociatedObject.Loaded -= ElementLoaded;
                Parameters.Detach();
            }

            base.OnDetaching();
        }

        void ElementLoaded(object sender, RoutedEventArgs e)
        {
            context = new ActionExecutionContext {
                Message = this, 
                Source = AssociatedObject
            };

            PrepareContext(context);
            UpdateAvailability();
        }

        protected override void Invoke(object eventArgs)
        {
            Log.Info("Invoking {0}.", this);
            context.EventArgs = eventArgs;
            InvokeAction(context);
        }

        /// <summary>
        /// Forces an update of the UI's Enabled/Disabled state based on the the preconditions associated with the method.
        /// </summary>
        public void UpdateAvailability()
        {
            if (context == null)
                return;

            Log.Info("{0} availability update.", this);
            ApplyAvailabilityEffect(context);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "Action: " + MethodName;
        }

        /// <summary>
        /// Invokes the action using the specified <see cref="ActionExecutionContext"/>
        /// </summary>
        public static Action<ActionExecutionContext> InvokeAction = (context) =>{
            var values = MessageBinder.DetermineParameters(context, context.Method.GetParameters());
            var outcome = context.Method.Invoke(context.Target, values);
            var result = MessageBinder.CreateResult(outcome);
            if(result != null)
                result.Execute(context);
        };

        /// <summary>
        /// Prepares the action execution context for use.
        /// </summary>
        public static Action<ActionExecutionContext> PrepareContext = (context) =>{
            SetMethodBinding(context);
            if (context.Target == null || context.Method == null)
            {
                var ex = new Exception(string.Format("No target found for method {0}.", context.Message.MethodName));
                Log.Error(ex);
                throw ex;
            }

            var guardName = "Can" + context.Method.Name;
            var targetType = context.Target.GetType();
            var guard = targetType.GetMethod(guardName);

            if(guard == null)
            {
                var inpc = context.Target as INotifyPropertyChanged;
                if(inpc == null)
                    return;

                guard = targetType.GetMethod("get_" + guardName);
                if(guard == null)
                    return;

                PropertyChangedEventHandler handler = (s, e) =>{
                    if(e.PropertyName == guardName)
                        context.Message.UpdateAvailability();
                };

                inpc.PropertyChanged += handler;
                context.Message.Detaching += delegate { inpc.PropertyChanged -= handler; };
            }

            context.CanExecute = () => (bool)guard.Invoke(
                context.Target,
                MessageBinder.DetermineParameters(context, guard.GetParameters())
                );
        };

        /// <summary>
        /// Applies an availability effect, such as IsEnabled, to an element.
        /// </summary>
        public static Action<ActionExecutionContext> ApplyAvailabilityEffect = (context) =>{
#if SILVERLIGHT
            if(!(context.Source is Control))
                return;
#endif

#if SILVERLIGHT
            ((Control)context.Source).IsEnabled = context.CanExecute();
#else
            context.Source.IsEnabled = context.CanExecute();
#endif
        };

        /// <summary>
        /// Sets the target, method and view on the context using a bubbling strategy.
        /// </summary>
        /// <param name="context">The action invocation context.</param>
        public static void SetMethodBinding(ActionExecutionContext context)
        {
            DependencyObject currentElement = context.Source;
            MethodInfo actionMethod = null;
            object currentTarget = null;

            while (currentElement != null && actionMethod == null)
            {
                currentTarget = currentElement.GetValue(Message.HandlerProperty);

                if (currentTarget != null)
                    actionMethod = currentTarget.GetType().GetMethod(context.Message.MethodName);

                if (actionMethod == null)
                    currentElement = VisualTreeHelper.GetParent(currentElement);
            }

            if (actionMethod == null && context.Source.DataContext != null)
            {
                currentTarget = context.Source.DataContext;
                actionMethod = context.Source.DataContext.GetType().GetMethod(context.Message.MethodName);
                currentElement = context.Source;
            }

            context.Target = currentTarget;
            context.Method = actionMethod;
            context.View = currentElement;
        }
    }
}