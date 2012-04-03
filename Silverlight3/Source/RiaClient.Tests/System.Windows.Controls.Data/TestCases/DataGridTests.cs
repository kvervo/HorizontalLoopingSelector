﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls.Test;
using System.Windows.Media;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Controls.Data.Test
{
    /// <summary>
    /// Unit tests for DataGrid.
    /// </summary>
    public partial class DataGridUnitTest<TDataClass> : SilverlightTest 
        where TDataClass : new()
    {
        protected PropertyInfo[] properties;
        protected int numberOfProperties;

        public DataGridUnitTest()
        {
            properties = typeof(TDataClass).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            numberOfProperties = properties.Length;                
        }

        public DataGridColumn[] PickTwoColumns(DataGrid dataGrid)
        {
            DataGridColumn col0 = null;
            DataGridColumn col1 = null;

            // pick 2 random columns to sort on
            int columnCount = dataGrid.ColumnsItemsInternal.Count;

            while (col0 == null || col0 == col1)
            {
                col0 = dataGrid.ColumnsItemsInternal[Common.RandomInt32(0, columnCount - 1)];
            }

            while (col1 == null || col0 == col1)
            {
                col1 = dataGrid.ColumnsItemsInternal[Common.RandomInt32(0, columnCount - 1)];
            }

            return new[] { col0, col1 };
        }

        //protected void DataGrid_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        //{
        //    if (e.EditingUnit == DataGridEditingUnit.Cell)
        //    {
        //        FrameworkElement root = e.EditingElement;
        //        Assert.AreEqual(e.EditingElement.DataContext, e.Row.DataContext);
        //        //Assert.AreEqual(e.Cell.RowIndex, e.RowIndex);
        //        //Assert.AreEqual(e.Cell.ColumnIndex, e.ColumnIndex);
        //        //Assert.AreEqual(e.Column, e.Cell.OwningColumn);

        //        TextBox firstNameTextBox = root.FindName("editedFirstName") as TextBox;
        //        TextBox lastNameTextBox = root.FindName("editedLastName") as TextBox;

        //        if (firstNameTextBox != null && lastNameTextBox != null)
        //        {
        //            Customer customer = e.Row.DataContext as Customer;
        //            Assert.IsNotNull(customer);
        //            customer.FirstName = firstNameTextBox.Text;
        //            customer.LastName = lastNameTextBox.Text;
        //        }
        //    }
        //}

        protected void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            FrameworkElement root = e.EditingElement;

            TextBox firstNameTextBox = root.FindName("editedFirstName") as TextBox;
            TextBox lastNameTextBox = root.FindName("editedLastName") as TextBox;

            if (firstNameTextBox != null && lastNameTextBox != null)
            {
                firstNameTextBox.Text = "Maurizio";
                lastNameTextBox.Text = "Moroni";
            }
            else
            {
                TextBox aStringTextBox = root.FindName("anEditedString") as TextBox;
                TextBox aBooleanTextBox = root.FindName("anEditedBoolean") as TextBox;
                Assert.IsNotNull(aStringTextBox);
                Assert.IsNotNull(aBooleanTextBox);
                aStringTextBox.Text = "ABC";
                aBooleanTextBox.Text = "False";
            }
        }


        protected bool isLoaded = false;
        protected void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
        }

        protected void dataGrid_LoadingRowGetRow(object sender, DataGridRowEventArgs e)
        {
            if (rowLoaded == false)
            {
                rowLoaded = true;
                dataGridRow = e.Row;
            }
            numberOfRowsLoaded++;
        }

        protected bool lastRowLoaded = false;
        protected DataGridRow dataGridRow;
        protected int numberOfRowsLoaded = 0;
        protected bool rowLoaded = false;
        protected bool selectionChanged = false;
        protected int numberOfColumnsGenerated = 0;
        protected IEnumerable loadRowList;

        #region ValidateGridRow
        //Checks the dataGridRow that is passed in against the bound Items.
        protected void ValidateGridRow(DataGrid dataGrid, DataGridRow row, TDataClass boundItem)
        {
            ValidateGridRow(dataGrid, row, boundItem, true);
        }

        protected void ValidateGridRow(DataGrid dataGrid, DataGridRow row, TDataClass boundItem, bool onlyWritableColumns)
        {
            foreach (DataGridBoundColumn boundColumn in dataGrid.Columns)
            {
                //All columns are autogenerated so the column headers match the property definition.
                PropertyInfo pi = properties.Where(p => boundColumn.Header.ToString() == p.Name).First();
                if (((onlyWritableColumns && pi.CanWrite) || !onlyWritableColumns) && dataGrid.IsColumnDisplayed(boundColumn.Index))
                {
                    object value = pi.GetValue(boundItem, null);
                    FrameworkElement cellElement = boundColumn.GetCellContent(row);
                    if (cellElement is TextBlock || cellElement is TextBox)
                    {
                        if (value != null)
                        {
                            if (value is DateTime)
                            {
                                // For our tests to pass on all platforms, we have to compare universal times
                                DateTime dateTime = Convert.ToDateTime(cellElement is TextBlock ? ((TextBlock)cellElement).Text : ((TextBox)cellElement).Text);
                                Assert.AreEqual(((DateTime)value).ToUniversalTime().ToString(), dateTime.ToUniversalTime().ToString());
                            }
                            else
                            {
                                Assert.AreEqual(value.ToString(),
                                    cellElement is TextBlock ? ((TextBlock)cellElement).Text : ((TextBox)cellElement).Text,
                                    String.Format(CultureInfo.CurrentCulture, "Column {0} doesn't match", pi.Name));
                            }
                        }
                        else //null's show up as empty strings
                        {
                            Assert.AreEqual("",
                                cellElement is TextBlock ? ((TextBlock)cellElement).Text : ((TextBox)cellElement).Text,
                                String.Format(CultureInfo.CurrentCulture, "Column {0} doesn't match", pi.Name));
                        }
                    }
                    else if (cellElement is CheckBox)
                    {
                        Assert.AreEqual(value, ((CheckBox)cellElement).IsChecked, String.Format(CultureInfo.CurrentCulture, "Column {0} doesn't match", pi.Name));
                    }
                }
            }
        }
        #endregion

        #region Visual Tree Helpers

        protected int CountVisualTreeRecursively(DependencyObject root)
        {
            if (root == null)
            {
                return 0;
            }
            else
            {
                int count = 1;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                {
                    count += CountVisualTreeRecursively(VisualTreeHelper.GetChild(root, i));
                }
                return count;
            }
        }

        // 
        protected DependencyObject FindChild(DependencyObject root, string name)
        {
            if (root == null)
            {
                return null;
            }
            FrameworkElement element = root as FrameworkElement;
            if (element != null && element.Name == name)
            {
                return element;
            }
            else
            {
                int childCount = VisualTreeHelper.GetChildrenCount(root);
                for (int i = 0; i < childCount; i++)
                {
                    FrameworkElement child = FindChild(VisualTreeHelper.GetChild(root, i), name) as FrameworkElement;
                    if (child != null)
                    {
                        return child;
                    }
                }
                return null;
            }
        }

        #endregion Visual Tree Helpers
    }
}

