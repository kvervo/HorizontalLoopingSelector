﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
    internal static class Extensions
    {
        private static bool _areHandlersSuspended;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "obj")]
        public static bool AreHandlersSuspended(this DependencyObject obj)
        {
            return _areHandlersSuspended;
        }

        internal static bool ContainsFocusedElement(this FrameworkElement element)
        {
            if (element != null)
            {
                DependencyObject focusedDependencyObject = FocusManager.GetFocusedElement() as DependencyObject;
                while (focusedDependencyObject != null)
                {
                    if (focusedDependencyObject == element)
                    {
                        return true;
                    }

                    // Walk up the visual tree.  If we hit the root, try using the framework element's
                    // parent.  We do this because Popups behave differently with respect to the visual tree,
                    // and it could have a parent even if the VisualTreeHelper doesn't find it.
                    DependencyObject parent = VisualTreeHelper.GetParent(focusedDependencyObject);
                    if (parent == null)
                    {
                        FrameworkElement focusedElement = focusedDependencyObject as FrameworkElement;
                        if (focusedElement != null)
                        {
                            parent = focusedElement.Parent;
                        }
                    }
                    focusedDependencyObject = parent;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks a MemberInfo object (e.g. a Type or PropertyInfo) for the ReadOnly attribute
        /// and returns the value of IsReadOnly if it exists.
        /// </summary>
        /// <param name="memberInfo">MemberInfo to check</param>
        /// <returns>true if MemberInfo is read-only, false otherwise</returns>
        internal static bool GetIsReadOnly(this MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                // Check if ReadOnlyAttribute is defined on the member
                object[] attributes = memberInfo.GetCustomAttributes(typeof(ReadOnlyAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    ReadOnlyAttribute readOnlyAttribute = attributes[0] as ReadOnlyAttribute;
                    Debug.Assert(readOnlyAttribute != null);
                    return readOnlyAttribute.IsReadOnly;
                }
            }
            return false;
        }

        internal static Type GetItemType(this IEnumerable list)
        {
            Type listType = list.GetType();
            Type itemType = null;

            // if it's a generic enumerable, we get the generic type

            // Unfortunately, if data source is fed from a bare IEnumerable, TypeHelper will report an element type of object,
            // which is not particularly interesting.  We deal with it further on.
            if (listType.IsEnumerableType())
            {
                itemType = listType.GetEnumerableItemType();
            }

            // Bare IEnumerables mean that result type will be object.  In that case, we try to get something more interesting
            if (itemType == null || itemType == typeof(object))
            {
                // We haven't located a type yet.. try a different approach.
                // Does the list have anything in it?

                IEnumerator en = list.GetEnumerator();
                if (en.MoveNext() && en.Current != null) 
                {
                    return en.Current.GetType();
                }
            }

            // if we're null at this point, give up
            return itemType;
        }

        public static void SetStyleWithType(this FrameworkElement element, Style style)
        {
            if (element.Style != style && (style == null || style.TargetType != null))
            {
                element.Style = style;
            }
        }

        public static void SetValueNoCallback(this DependencyObject obj, DependencyProperty property, object value)
        {
            _areHandlersSuspended = true;
            try
            {
                obj.SetValue(property, value);
            }
            finally
            {
                _areHandlersSuspended = false;
            }
        }

        internal static Point Translate(this UIElement fromElement, UIElement toElement, Point fromPoint)
        {
            if (fromElement == toElement)
            {
                return fromPoint;
            }
            else
            {
                return fromElement.TransformToVisual(toElement).Transform(fromPoint);
            }
        }

        internal static bool Within(this Point referencePoint, UIElement referenceElement, FrameworkElement targetElement, bool ignoreVertical)
        {
            Point position = referenceElement.Translate(targetElement, referencePoint);

            return position.X > 0 && position.X < targetElement.ActualWidth
                && (ignoreVertical
                    || (position.Y > 0 && position.Y < targetElement.ActualHeight)
                );
        }
    }
}
