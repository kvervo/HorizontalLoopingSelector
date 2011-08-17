﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Silverlight.Testing.Harness
{
    /// <summary>
    /// TestWorkItem which can contain sub-tasks; the underlying work item is 
    /// not marked complete until the Children have completed or an Exception 
    /// is thrown.
    /// </summary>
    public class CompositeWorkItem : WorkItem
    {
        /// <summary>
        /// Store the underlying tasks.
        /// </summary>
        private Queue<WorkItem> _children;

        /// <summary>
        /// Whether the TestTaskContainer::IsComplete should be set to true 
        /// after all children have been de-queued.
        /// </summary>
        private bool _finishWhenEmpty;

        /// <summary>
        /// Whether the first invoke has happened yet or not.
        /// </summary>
        private bool _invoked;

        /// <summary>
        /// Constructor for the TestTaskContainer type.
        /// </summary>
        public CompositeWorkItem()
        {
            _children = new Queue<WorkItem>();
            _finishWhenEmpty = true;
        }

        /// <summary>
        /// Gets a value indicating whether the container is marked Complete 
        /// when all children have executed or not.
        /// </summary>
        public bool FinishWhenEmpty
        {
            get { return _finishWhenEmpty; }

            protected set { _finishWhenEmpty = value; }
        }

        /// <summary>
        /// Optional method to call on the first invoke.
        /// </summary>
        protected virtual void FirstInvoke()
        {
        }

        /// <summary>
        /// Invoke the test container; in turn will execute child work items 
        /// as needed. Supports executing multiple items immediately for
        /// performance reasons.
        /// </summary>
        /// <returns>True if additional work remains, False once IsComplete 
        /// is set to true.</returns>
        public override bool Invoke() // return false when complete + event
        {
            WorkItem workItem;
            bool additionalWork;
            bool canContinueImmediateExecution = false;

            do
            {
                additionalWork = Invoke(out workItem);
                if (workItem != null)
                {
                    canContinueImmediateExecution = workItem.CanExecuteImmediately;
                }
            }
            while (additionalWork && canContinueImmediateExecution);
            
            return additionalWork;
        }

        /// <summary>
        /// Invoke the test container; in turn will execute child work items 
        /// as needed.
        /// </summary>
        /// <param name="usedWorkItem">The work item used for the invoke.</param>
        /// <returns>True if additional work remains, False once IsComplete 
        /// is set to true.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Key part of the unit test engine")]
        private bool Invoke(out WorkItem usedWorkItem)
        {
            usedWorkItem = null;

            // On the first invoke, run the optional method
            if (!_invoked)
            {
                _invoked = true;
                FirstInvoke();
            }

            // If this container is complete
            if (IsComplete)
            {
                ClearChildren();
                return false;
            }

            // If no remaining tasks
            if (! RemainingWork && _finishWhenEmpty) 
            {
                ClearChildren();
                WorkItemComplete();
                return false;
            }

            // The completion comes from an external event, background task, or 
            // other delegate outside of the empty complete flag
            if (! RemainingWork) 
            {
                return true;
            }
            
            // Examine the status of child work items
            WorkItem tt = Peek();
            if (tt == null) 
            {
                throw new InvalidOperationException(Properties.UnitTestMessage.CompositeWorkItem_Invoke_NoRemainingWorkItems);
                // Should understand if this is a possible condition!
            }

            usedWorkItem = tt;

            // Assumption: The task is running by default
            bool taskStillRunning = true;

            try
            {
                taskStillRunning = tt.Invoke(); // tt.IsComplete ? tt.IsComplete : tt.Invoke();
            }
            catch (Exception exp)
            {
                taskStillRunning = false;
                OnUnhandledException(exp);
            }
            finally
            {
                if (!taskStillRunning) 
                {
                    if (RemainingWork)
                    {
                        // Remove the child
                        Dequeue();
                    }
                    else
                    {
                        WorkItemComplete();
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether sub tasks remain.
        /// </summary>
        public bool RemainingWork
        {
            get { return _children.Count > 0; }
        }

        /// <summary>
        /// Clear the children.
        /// </summary>
        protected void ClearChildren()
        {
            _children.Clear();
        }

        /// <summary>
        /// Dequeue a work item.
        /// </summary>
        /// <returns>A work item.</returns>
        public WorkItem Dequeue()
        {
            return _children.Dequeue();
        }

        /// <summary>
        /// Add a new work item to the container to schedule it for invocation.
        /// </summary>
        /// <param name="item">New test work item to enqueue.</param>
        public void Enqueue(WorkItem item)
        {
            _children.Enqueue(item);
        }

        /// <summary>
        /// Adds a new work item that will execute more quickly.
        /// </summary>
        /// <param name="item">The item of work.</param>
        public void EnqueueQuick(WorkItem item)
        {
            item.CanExecuteImmediately = true;
            Enqueue(item);
        }

        /// <summary>
        /// Enqueues a callback or action that will quickly execute.
        /// </summary>
        /// <param name="action">The action or method.</param>
        public void EnqueueQuick(Action action)
        {
            EnqueueQuick(new CallbackWorkItem(action));
        }

        /// <summary>
        /// Add a new callback action and schedule it for invocation.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Enqueue(Action action)
        {
            Enqueue(new CallbackWorkItem(action));
        }

        /// <summary>
        /// Return the top work item, if any, from this container.
        /// </summary>
        /// <returns>Peek into any test work item.</returns>
        public WorkItem Peek()
        {
            return _children.Peek();
        }

        /// <summary>
        /// Work items must call this method to indicate completion of the work 
        /// item; in turn fires the Complete event delegates.
        /// </summary>
        protected override void WorkItemComplete()
        {
            base.WorkItemComplete();
            OnComplete(EventArgs.Empty);
        }

        /// <summary>
        /// Internal-only version which can be called during a test completion 
        /// through the relation - not necessarily the best design; events 
        /// may make more sense long-term.
        /// </summary>
        internal virtual void WorkItemCompleteInternal()
        {
            WorkItemComplete();
        }

        /// <summary>
        /// Call when an exception occurs inside a work item.
        /// </summary>
        /// <param name="e">Exception object.</param>
        internal virtual void WorkItemException(Exception e)
        {
            // REVIEW: DESIGN REVIEW OF EXCEPTION HANDLING NEEDED
            OnUnhandledException(e);
            WorkItemComplete();
        }

        /// <summary>
        /// Complete event is fired when the underlying WorkItemComplete method 
        /// is called by the work item.
        /// </summary>
        public event EventHandler Complete;

        /// <summary>
        /// Event fired when an exception is thrown and unhandled within the 
        /// underlying Invoke sequence.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        /// <summary>
        /// Fire the Complete event.
        /// </summary>
        /// <param name="e">Empty event arguments.</param>
        protected virtual void OnComplete(EventArgs e)
        {
            var handler = Complete;
            if (handler != null) 
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Fire the unhandled exception event.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        protected virtual void OnUnhandledException(Exception exception)
        {
            // REVIEW: Should this method take the actual Exception, or the actual EventArgs for containing it?
            var handler = UnhandledException;
            if (handler != null)
            {
                handler(this, new UnhandledExceptionEventArgs(exception, false));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the task can execute
        /// immediately.
        /// </summary>
        public override bool CanExecuteImmediately
        {
            get
            {
                return base.CanExecuteImmediately;
            }
            set
            {
                // Silently ignore this operation as it could cause unreliable
                // results.
            }
        }
    }
}