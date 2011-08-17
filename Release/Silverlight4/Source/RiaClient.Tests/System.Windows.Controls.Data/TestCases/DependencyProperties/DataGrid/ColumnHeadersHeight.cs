﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Controls.Data.Test
{
    public partial class DataGrid_DependencyProperties_TestClass
    {
        [TestMethod]
        [Description("Verify Dependency Property: (double) DataGrid.ColumnHeaderHeight.")]
        public void ColumnHeaderHeight()
        {
            Type propertyType = typeof(double);
            bool expectGet = true;
            bool expectSet = true;
            bool hasSideEffects = true;

            DataGrid control = new DataGrid();
            Assert.IsNotNull(control);

            // Verify dependency property member
            FieldInfo fieldInfo = typeof(DataGrid).GetField("ColumnHeaderHeightProperty", BindingFlags.Static | BindingFlags.Public);
            Assert.AreEqual(typeof(DependencyProperty), fieldInfo.FieldType, "DataGrid.ColumnHeaderHeightProperty not expected type 'DependencyProperty'.");

            // Verify dependency property's value type
            DependencyProperty property = fieldInfo.GetValue(null) as DependencyProperty;

            Assert.IsNotNull(property);

            // 


            // Verify dependency property CLR property member
            PropertyInfo propertyInfo = typeof(DataGrid).GetProperty("ColumnHeaderHeight", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(propertyInfo, "Expected CLR property DataGrid.ColumnHeaderHeight does not exist.");
            Assert.AreEqual(propertyType, propertyInfo.PropertyType, "DataGrid.ColumnHeaderHeight not expected type 'double'.");

            // Verify getter/setter access
            Assert.AreEqual(expectGet, propertyInfo.CanRead, "Unexpected value for propertyInfo.CanRead.");
            Assert.AreEqual(expectSet, propertyInfo.CanWrite, "Unexpected value for propertyInfo.CanWrite.");

            // Verify that we set what we get
            if (expectSet) // if expectSet == false, this block can be removed
            {
                control.ColumnHeaderHeight = 20;

                Assert.AreEqual(20, control.ColumnHeaderHeight);

                control.ColumnHeaderHeight = 25;

                Assert.AreEqual(25, control.ColumnHeaderHeight);

                control.ColumnHeaderHeight = 30;

                Assert.AreEqual(30, control.ColumnHeaderHeight);
            }

            // Verify dependency property callback
            if (hasSideEffects)
            {
                MethodInfo methodInfo = typeof(DataGrid).GetMethod("OnColumnHeaderHeightPropertyChanged", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(methodInfo, "Expected DataGrid.ColumnHeaderHeight to have static, non-public side-effect callback 'OnColumnHeaderHeightPropertyChanged'.");

                // 
            }
            else
            {
                MethodInfo methodInfo = typeof(DataGrid).GetMethod("OnColumnHeaderHeightPropertyChanged", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNull(methodInfo, "Expected DataGrid.ColumnHeaderHeight NOT to have static side-effect callback 'OnColumnHeaderHeightPropertyChanged'.");
            }
        }
    }
}
