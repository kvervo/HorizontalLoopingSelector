﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
using VS = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio
{
    /// <summary>
    /// A provider wrapper for a test method.
    /// </summary>
    public class TestMethod : ITestMethod
    {
        /// <summary>
        /// Property name for the TestContext.
        /// </summary>
        private const string ContextPropertyName = "TestContext";

        /// <summary>
        /// Default value for methods when no priority attribute is defined.
        /// </summary>
        private const int DefaultPriority = 3;

        /// <summary>
        /// An empty object array.
        /// </summary>
        private static readonly object[] None = { };

        /// <summary>
        /// Method reflection object.
        /// </summary>
        private MethodInfo _methodInfo;

        /// <summary>
        /// Private constructor, the constructor requires the method reflection object.
        /// </summary>
        private TestMethod() { }

        /// <summary>
        /// Creates a new test method wrapper object.
        /// </summary>
        /// <param name="methodInfo">The reflected method.</param>
        public TestMethod(MethodInfo methodInfo) : this()
        {
            _methodInfo = methodInfo;
        }

        /// <summary>
        /// Allows the test to perform a string WriteLine.
        /// </summary>
        public event EventHandler<StringEventArgs> WriteLine;

        /// <summary>
        /// Call the WriteLine method.
        /// </summary>
        /// <param name="s">String to WriteLine.</param>
        internal void OnWriteLine(string s)
        {
            EventHandler<StringEventArgs> handler = WriteLine;
            if (handler != null)
            {
                handler(this, new StringEventArgs(s));
            }
        }

        /// <summary>
        /// Decorates a test class instance with the unit test framework's 
        /// specific test context capability, if supported.
        /// </summary>
        /// <param name="instance">Instance to decorate.</param>
        public void DecorateInstance(object instance)
        {
            if (instance == null)
            {
                return;
            }

            Type t = instance.GetType();
            PropertyInfo pi = t.GetProperty(ContextPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && pi.CanWrite)
            {
                try
                {
                    UnitTestContext utc = new UnitTestContext(this);
                    pi.SetValue(instance, utc, null);
                }
                finally
                {
                }
            }
        }

        /// <summary>
        /// Gets the underlying reflected method.
        /// </summary>
        public MethodInfo Method 
        { 
            get { return _methodInfo; }
        }

        /// <summary>
        /// Gets a value indicating whether there is an Ignore attribute.
        /// </summary>
        public bool Ignore
        {
            get { return ReflectionUtility.HasAttribute(this, ProviderAttributes.IgnoreAttribute); }
        }

        /// <summary>
        /// Gets any description marked on the test method.
        /// </summary>
        public string Description
        {
            get
            {
                VS.DescriptionAttribute description = ReflectionUtility.GetAttribute(
                    this,
                    ProviderAttributes.DescriptionAttribute,
                    true) as VS.DescriptionAttribute;
                return description != null ? description.Description : null;
            }
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public virtual string Name
        {
            get { return _methodInfo.Name; }
        }

        /// <summary>
        /// Gets the Category.
        /// </summary>
        public string Category
        {
            get { return null; }
        } 

        /// <summary>
        /// Gets the owner name of the test.
        /// </summary>
        public string Owner
        {
            get
            {
                VS.OwnerAttribute owner = ReflectionUtility.GetAttribute(
                    this,
                    ProviderAttributes.OwnerAttribute) as
                    VS.OwnerAttribute;
                return owner == null ? null : owner.Owner;
            }
        }

        /// <summary>
        /// Gets any expected exception attribute information for the test method.
        /// </summary>
        public IExpectedException ExpectedException
        {
            get
            {
                VS.ExpectedExceptionAttribute exp = ReflectionUtility.GetAttribute(
                    this,
                    ProviderAttributes.ExpectedExceptionAttribute) as
                    VS.ExpectedExceptionAttribute;
                return exp != null ?
                    new ExpectedException(exp) : null;
            }
        }

        /// <summary>
        /// Gets any timeout.  A Nullable property.
        /// </summary>
        public int? Timeout
        {
            get
            {
                VS.TimeoutAttribute timeout = ReflectionUtility.GetAttribute(
                    this,
                    ProviderAttributes.TimeoutAttribute,
                    true) as VS.TimeoutAttribute;
                return timeout != null ? (int?)timeout.Timeout : null;
            }
        }

        /// <summary>
        /// Gets a Collection of test properties.
        /// </summary>
        public ICollection<ITestProperty> Properties
        {
            get
            {
                List<ITestProperty> properties = new List<ITestProperty>();
                ICollection<Attribute> attributes = ReflectionUtility.GetAttributes(
                    this,
                    ProviderAttributes.TestProperty,
                    true);
                if (attributes != null)
                {
                    foreach (Attribute a in attributes)
                    {
                        VS.TestPropertyAttribute tpa = a as VS.TestPropertyAttribute;
                        if (tpa != null)
                        {
                            properties.Add(new TestProperty(tpa.Name, tpa.Value));
                        }
                    }
                }

                return properties;
            }
        }

        /// <summary>
        /// Gets a collection of test work items.
        /// </summary>
        public ICollection<IWorkItemMetadata> WorkItems
        {
            get { return null; }
        }

        /// <summary>
        /// Gets Priority information.
        /// </summary>
        public IPriority Priority
        {
            get 
            {
                VS.PriorityAttribute pri = ReflectionUtility.GetAttribute(this, ProviderAttributes.Priority, true) as VS.PriorityAttribute;
                return new Priority(pri == null ? DefaultPriority : pri.Priority);
            }
        }

        /// <summary>
        /// Get any attribute on the test method that are provided dynamically.
        /// </summary>
        /// <returns>
        /// Dynamically provided attributes on the test method.
        /// </returns>
        public virtual IEnumerable<Attribute> GetDynamicAttributes()
        {
            return new Attribute[] { };
        }

        /// <summary>
        /// Invoke the test method.
        /// </summary>
        /// <param name="instance">Instance of the test class.</param>
        public virtual void Invoke(object instance)
        {
            _methodInfo.Invoke(instance, None);
        }

        /// <summary>
        /// Exposes the name of the test method as a string.
        /// </summary>
        /// <returns>Returns the name of the test method.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}