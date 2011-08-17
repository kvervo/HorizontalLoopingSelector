﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Linq;

namespace Microsoft.Windows
{
    /// <summary>
    /// Provides helper methods and fields for initiating drag-and-drop operations,
    /// including a method to begin a drag-and-drop operation, and facilities for
    /// adding and removing drag-and-drop related event handlers.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    public static class DragDrop
    {
        /// <summary>
        /// Identifies the System.Windows.DragDrop.DragEnter attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")]
        public static readonly ExtendedRoutedEvent DragEnterEvent = new ExtendedRoutedEvent();

        /// <summary>
        /// Identifies the System.Windows.UIElement.DragLeave attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")] 
        public static readonly ExtendedRoutedEvent DragLeaveEvent = new ExtendedRoutedEvent();

        /// <summary>
        /// Identifies the System.Windows.UIElement.DragOver attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")]
        public static readonly ExtendedRoutedEvent DragOverEvent = new ExtendedRoutedEvent();

        /// <summary>
        /// Identifies the System.Windows.UIElement.Drop attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")]
        public static readonly ExtendedRoutedEvent DropEvent = new ExtendedRoutedEvent();

        /// <summary>
        /// Identifies the System.Windows.UIElement.GiveFeedback attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")]
        public static readonly ExtendedRoutedEvent GiveFeedbackEvent = new ExtendedRoutedEvent();

        /// <summary>
        /// Identifies the System.Windows.UIElement.QueryContinueDrag attached event.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Object is immutable.")]
        public static readonly ExtendedRoutedEvent QueryContinueDragEvent = new ExtendedRoutedEvent();

        #region public attached bool AllowDrop
        /// <summary>
        /// Gets the value of the AllowDrop attached property for a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement from which the property value is read.</param>
        /// <returns>The AllowDrop property value for the UIElement.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This attached property belongs to UIElement.")]
        public static bool GetAllowDrop(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            DependencyObject valueSetAncestor =
                element
                    .GetVisualAncestors()
                    .Prepend(element)
                    .FirstOrDefault(ancestor => ancestor.ReadLocalValue(DragDrop.AllowDropProperty) != DependencyProperty.UnsetValue);
            
            if (valueSetAncestor != null)
            {
                return (bool) valueSetAncestor.ReadLocalValue(DragDrop.AllowDropProperty);
            }

            return false;
        }

        /// <summary>
        /// Sets the value of the AllowDrop attached property to a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement to which the attached property is written.</param>
        /// <param name="value">The needed AllowDrop value.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This attached property belongs to UIElement.")]
        public static void SetAllowDrop(UIElement element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(AllowDropProperty, value);
        }

        /// <summary>
        /// Identifies the AllowDrop dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDropProperty =
            DependencyProperty.RegisterAttached(
                "AllowDrop",
                typeof(bool),
                typeof(DragDrop),
                new PropertyMetadata(false));
        #endregion public attached bool AllowDrop

        /// <summary>
        /// Gets a value indicating whether a drag is in progress.
        /// </summary>
        public static bool IsDragInProgress { get { return _dragOperationInProgress != null; } }

        /// <summary>
        /// The drag operation in progress.
        /// </summary>
        private static DragOperation _dragOperationInProgress;

        /// <summary>
        /// An event that is raised when a drag operation is completed.
        /// </summary>
        public static event EventHandler<DragDropCompletedEventArgs> DragDropCompleted;

        /// <summary>
        /// Raises the DragCompleted event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        private static void OnDragCompleted(DragDropCompletedEventArgs args)
        {
            _dragOperationInProgress.Dispose();
            _dragOperationInProgress = null;
            EventHandler<DragDropCompletedEventArgs> handler = DragDropCompleted;
            if (handler != null)
            {
                handler(null, args);
            }
        }

        /// <summary>
        /// Returns an observable that wraps the DragCompleted event.
        /// </summary>
        /// <returns>An observable that wraps the DragCompleted event.</returns>
        internal static IObservable<Event<DragDropCompletedEventArgs>> GetDragCompleted()
        {
            return new AnonymousObservable<Event<DragDropCompletedEventArgs>>(
                observer =>
                {
                    EventHandler<DragDropCompletedEventArgs> handler = (_, args) => observer.OnNext(new Event<DragDropCompletedEventArgs>(null, args));
                    DragDropCompleted += handler;
                    return new AnonymousDisposable(() => DragDropCompleted -= handler);
                });
        }

        #region public attached DragEventHandler DragEnterHandler
        /// <summary>
        /// Removes a handler from the attached DragEnter event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveDragEnterHandler(DependencyObject element, DragEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = element.GetDragEnterHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragEnter event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the handler if the event is handled.</param>
        internal static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> originalHandlers = element.GetDragEnterHandlers();
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>((h, a) => h(element, a)); 
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(DragEnterHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragEnter event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler)
        {
            AddDragEnterHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the drag enter handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> GetDragEnterHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>)element.GetValue(DragEnterHandlerProperty);
        }

        /// <summary>
        /// Identifies the DragEnterHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty DragEnterHandlerProperty =
            DependencyProperty.RegisterAttached(
                "DragEnterHandler",
                typeof(ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached DragEventHandler DragEnterHandler

        #region public attached DragEventHandler DragOverHandler
        /// <summary>
        /// Removes a handler from the attached DragOver event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveDragOverHandler(DependencyObject element, DragEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = element.GetDragOverHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragOver event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the handler if the event is handled.</param>
        internal static void AddDragOverHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> originalHandlers = element.GetDragOverHandlers();
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>((h, a) => h(element, a)); 
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(DragOverHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragOver event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddDragOverHandler(DependencyObject element, DragEventHandler handler)
        {
            AddDragOverHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the drag Over handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> GetDragOverHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>)element.GetValue(DragOverHandlerProperty);
        }

        /// <summary>
        /// Identifies the DragOverHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty DragOverHandlerProperty =
            DependencyProperty.RegisterAttached(
                "DragOverHandler",
                typeof(ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached DragEventHandler DragOverHandler

        #region public attached DragEventHandler DragLeaveHandler
        /// <summary>
        /// Removes a handler from the attached DragLeave event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveDragLeaveHandler(DependencyObject element, DragEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = element.GetDragLeaveHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragLeave event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the handler if the event is handled.</param>
        internal static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> originalHandlers = element.GetDragLeaveHandlers();
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>((h, a) => h(element, a)); 
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(DragLeaveHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached DragLeave event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler)
        {
            AddDragLeaveHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the drag Leave handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> GetDragLeaveHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>)element.GetValue(DragLeaveHandlerProperty);
        }

        /// <summary>
        /// Identifies the DragLeaveHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty DragLeaveHandlerProperty =
            DependencyProperty.RegisterAttached(
                "DragLeaveHandler",
                typeof(ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached DragEventHandler DragLeaveHandler

        #region public attached DragEventHandler DropHandler
        /// <summary>
        /// Removes a handler from the attached Drop event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveDropHandler(DependencyObject element, DragEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = element.GetDropHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached Drop event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the handler if the event is handled.</param>
        internal static void AddDropHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> originalHandlers = element.GetDropHandlers();
            ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>((h, a) => h(element, a)); 
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(DropHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached Drop event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddDropHandler(DependencyObject element, DragEventHandler handler)
        {
            AddDropHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the drag Leave handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs> GetDropHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>)element.GetValue(DropHandlerProperty);
        }

        /// <summary>
        /// Identifies the DropHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.RegisterAttached(
                "DropHandler",
                typeof(ExtendedRoutedEventHandlerCollection<DragEventHandler, DragEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached DragEventHandler DropHandler

        #region public attached GiveFeedbackEventHandler GiveFeedbackHandler
        /// <summary>
        /// Removes a handler from the attached GiveFeedback event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs> handlers = element.GetGiveFeedbackHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached GiveFeedback event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the 
        /// handler if the event has been handled.</param>
        internal static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs> originalHandlers = element.GetGiveFeedbackHandlers();
            ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs>((h, a) => h(element, a));
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(GiveFeedbackHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached GiveFeedback event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
        {
            AddGiveFeedbackHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the GiveFeedback Leave handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs> GetGiveFeedbackHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs>)element.GetValue(GiveFeedbackHandlerProperty);
        }

        /// <summary>
        /// Identifies the GiveFeedbackHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty GiveFeedbackHandlerProperty =
            DependencyProperty.RegisterAttached(
                "GiveFeedbackHandler",
                typeof(ExtendedRoutedEventHandlerCollection<GiveFeedbackEventHandler, GiveFeedbackEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached GiveFeedbackEventHandler GiveFeedbackHandler

        #region public attached QueryContinueDragEventHandler QueryContinueDragHandler
        /// <summary>
        /// Removes a handler from the attached QueryContinueDrag event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void RemoveQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs> handlers = element.GetQueryContinueDragHandlers();
            if (handlers != null)
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Adds a handler to the attached QueryContinueDrag event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="handledEventsToo">A value Indicating whether to invoke the 
        /// handler if the event has been handled.</param>
        internal static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler, bool handledEventsToo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs> originalHandlers = element.GetQueryContinueDragHandlers();
            ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs> handlers = originalHandlers ?? new ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs>((h, a) => h(element, a)); 
            handlers.Add(handler, handledEventsToo);
            if (handlers != originalHandlers)
            {
                element.SetValue(QueryContinueDragHandlerProperty, handlers);
            }
        }

        /// <summary>
        /// Adds a handler to the attached QueryContinueDrag event.
        /// </summary>
        /// <param name="element">The DependencyObject to attach an event handler for.</param>
        /// <param name="handler">The event handler.</param>
        internal static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
        {
            AddQueryContinueDragHandler(element, handler, false);
        }

        /// <summary>
        /// Gets the QueryContinueDrag Leave handler.
        /// </summary>
        /// <param name="element">The element to attach the event handler to.</param>
        /// <returns>The event handler.</returns>
        internal static ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs> GetQueryContinueDragHandlers(this DependencyObject element)
        {
            return (ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs>)element.GetValue(QueryContinueDragHandlerProperty);
        }

        /// <summary>
        /// Identifies the QueryContinueDragHandler dependency property.
        /// </summary>
        internal static readonly DependencyProperty QueryContinueDragHandlerProperty =
            DependencyProperty.RegisterAttached(
                "QueryContinueDragHandler",
                typeof(ExtendedRoutedEventHandlerCollection<QueryContinueDragEventHandler, QueryContinueDragEventArgs>),
                typeof(DragDrop),
                new PropertyMetadata(null));
        #endregion public attached QueryContinueDragEventHandler QueryContinueDragHandler

        /// <summary>
        /// Initiates a drag-and-drop operation.
        /// </summary>
        /// <param name="dragSource">A reference to the dependency object that is the source of the data being
        /// dragged.</param>
        /// <param name="data">A data object that contains the data being dragged.</param>
        /// <param name="allowedEffects">One of the System.Windows.DragDropEffects values that specifies permitted
        /// effects of the drag-and-drop operation.</param>
        /// <param name="initialKeyState">The initial key state when the drag operation begins.</param>
        public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects allowedEffects, DragDropKeyStates initialKeyState)
        {
            // TODO: Throw if IsDragDropInProgress
            _dragOperationInProgress = 
                new DragOperation(dragSource, data, allowedEffects, initialKeyState);

            _dragOperationInProgress
                    .Subscribe(effects => OnDragCompleted(new DragDropCompletedEventArgs { Effects = effects }));
        }
    }
}