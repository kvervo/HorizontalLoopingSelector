﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Controls.Data.Test.DataClassSources;
using System.Windows.Controls.Test;
using System.Windows.Media;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Windows.Controls.Data.Test
{
    /// <summary>
    /// Tests that should have no restrictions on the data source.
    /// </summary>
    public partial class DataGridUnitTest_Unrestricted<TDataClass, TDataClassSource> : DataGridUnitTest<TDataClass>
        where TDataClass : new()
        where TDataClassSource : DataClassSource<TDataClass>, new()
    {
        [TestMethod]
        [Description("Create a DataGrid control.")]
        public virtual void CreateDataGrid()
        {
            DataGrid dg = new DataGrid();
            Assert.IsNotNull(dg);
        }

        #region ColumnReorderingByMouse
        bool onReorderingCalled;
        bool onReorderedCalled;

        // 
        [Ignore]
        [TestMethod]
        [Asynchronous]
        [Description("End-user column reorder via the mouse.")]
        public void ColumnReorderingByMouse()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.ColumnReordering += new EventHandler<DataGridColumnReorderingEventArgs>(dataGrid_ColumnReordering);
            dataGrid.ColumnReordered += new EventHandler<DataGridColumnEventArgs>(dataGrid_ColumnReordered);
            dataGrid.ItemsSource = boundList;
            this.onReorderingCalled = false;
            this.onReorderedCalled = false;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();

            DataGridColumn column0 = null;
            DataGridColumn column1 = null;
            DataGridColumn column2 = null;
            bool handled = false;

            EnqueueCallback(
                delegate
                {
                    Assert.IsTrue(dataGrid.CanUserReorderColumns, "datagrid doesn't allow column reordering");
                    Assert.IsTrue(dataGrid.Columns.Count > 2, "Not enough columns to test");

                    // get columns

                    column0 = dataGrid.ColumnsInternal.GetColumnAtDisplayIndex(0);
                    column1 = dataGrid.ColumnsInternal.GetColumnAtDisplayIndex(1);
                    column2 = dataGrid.ColumnsInternal.GetColumnAtDisplayIndex(2);

                    // verify display indices

                    Assert.AreEqual(0, column0.DisplayIndex);
                    Assert.AreEqual(1, column1.DisplayIndex);
                    Assert.AreEqual(2, column2.DisplayIndex);

                    // start rotating through columns

                    handled = false;
    
                    // mouseDown on col1

                    column1.HeaderCell.OnMouseLeftButtonDown(ref handled, new Point(20, 5));
                }
            );

            this.EnqueueYieldThread();

            EnqueueCallback(
                delegate
                {
                    Assert.IsTrue(handled, "MouseLeftButtonDown not handled");
                    handled = false;

                    Assert.IsFalse(onReorderingCalled, "onReordering called unexpectedly");
                    Assert.IsFalse(onReorderedCalled, "onReordered called unexpectedly");

                    // verify display indices

                    Assert.AreEqual(0, column1.DisplayIndex);
                    Assert.AreEqual(1, column0.DisplayIndex);
                    Assert.AreEqual(2, column2.DisplayIndex);

                    // mouseMove on col1
                    double distanceToMove = -(column0.HeaderCell.ActualWidth / 2 + 20);
                    Point mousePosition = new Point(distanceToMove, 0);
                    GeneralTransform transformToHeadersPresenter = column1.HeaderCell.TransformToVisual(dataGrid.ColumnHeaders);
                    column1.HeaderCell.OnMouseMove(ref handled, mousePosition, transformToHeadersPresenter.Transform(mousePosition));
                }
            );

            this.EnqueueYieldThread();

            EnqueueCallback(
                delegate
                {
                    Assert.IsTrue(handled, "MouseMove not handled");
                    handled = false;

                    Assert.IsTrue(onReorderingCalled, "onReordering not called");
                    Assert.IsFalse(onReorderedCalled, "onReordered called unexpectedly");

                    // verify display indices

                    Assert.AreEqual(0, column1.DisplayIndex);
                    Assert.AreEqual(1, column0.DisplayIndex);
                    Assert.AreEqual(2, column2.DisplayIndex);

                    // mouseUp on col1
                    double totalDistanceMoved = -(column0.HeaderCell.ActualWidth / 2 + 20);
                    Point mousePosition = new Point(totalDistanceMoved, 0);
                    GeneralTransform transformToHeadersPresenter = column1.HeaderCell.TransformToVisual(dataGrid.ColumnHeaders);
                    column1.HeaderCell.OnMouseLeftButtonUp(ref handled, mousePosition, transformToHeadersPresenter.Transform(mousePosition));
                }
            );

            this.EnqueueYieldThread();

            EnqueueCallback(
                delegate
                {
                    Assert.IsTrue(handled, "MouseLeftButtonUp not handled");
                    handled = false;

                    Assert.IsFalse(onReorderingCalled, "onReordering called unexpectedly");
                    Assert.IsTrue(onReorderedCalled, "onReordered not called");

                    // verify display indices

                    Assert.AreEqual(0, column1.DisplayIndex);
                    Assert.AreEqual(1, column0.DisplayIndex);
                    Assert.AreEqual(2, column2.DisplayIndex);
                }
            );

            EnqueueTestComplete();
        }

        void dataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            this.onReorderedCalled = true;
        }

        void dataGrid_ColumnReordering(object sender, DataGridColumnReorderingEventArgs e)
        {
            this.onReorderingCalled = true;
        }

        #endregion

        [TestMethod]
        [Asynchronous]
        [Description("Check Default Values.")]
        public virtual void DefaultValues()
        {
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            TestPanel.Children.Add(dataGrid);
            this.EnqueueYieldThread();

            this.EnqueueCallback(delegate
            {
                //Verify Default Grid Values
                Assert.AreEqual(true, dataGrid.CanUserResizeColumns, "CanUserResizeColumns wrong default");
                Assert.AreEqual(false, dataGrid.IsReadOnly, "IsReadOnly wrong default");
                Assert.AreEqual(DataGridLengthUnitType.Auto, dataGrid.ColumnWidth.UnitType, "ColumnWidth wrong default");
                Assert.AreEqual(double.NaN, dataGrid.RowHeight, "RowHeight wrong default");
                Assert.AreEqual(double.NaN, dataGrid.ColumnHeaderHeight, "ColumnHeaderHeight wrong default");
                Assert.AreEqual(double.NaN, dataGrid.RowHeaderWidth, "RowHeaderWidth wrong default");
                Assert.AreEqual(null, dataGrid.SelectedItem, "SelectedItem wrong default");
                Assert.AreEqual(((IList<TDataClass>)(new List<TDataClass>())).Count, dataGrid.SelectedItems.Count, "SelectedItems wrong default");
                Assert.AreEqual(DataGridSelectionMode.Extended, dataGrid.SelectionMode, "SelectionMode wrong default");
                Assert.AreEqual(null, dataGrid.CurrentColumn, "CurrentColumn wrong default");
                Assert.AreEqual(false, dataGrid.AreRowDetailsFrozen, "AreRowDetailsFrozen wrong default");
                Assert.AreEqual(true, dataGrid.AutoGenerateColumns, "AutoGenerateColumns wrong default");
                Assert.AreEqual(DataGridRowDetailsVisibilityMode.VisibleWhenSelected, dataGrid.RowDetailsVisibilityMode, "RowDetailsVisibilityMode wrong default");
            });
            EnqueueTestComplete();
        }

        #region AutoGenenerate
        [TestMethod]
        [Asynchronous]
        [Description("Test autogenerated columns.")]
        public virtual void AutogeneratedColumns()
        {
            IEnumerable emptyNonGeneric = new NonGenericEnumerable_0<TDataClass>();
            IEnumerable nonEmptyNonGeneric = new NonGenericEnumerable_1<TDataClass>();
            IEnumerable<TDataClass> emptyList = new GenericEnumerable_0<TDataClass>();

            IList emptyNonGenericList = new NonGenericList_0<TDataClass>();

            IList nonEmptyNonGenericList = new NonGenericList_1<TDataClass>();

            IList emptyNonGenericListINCC = new NonGenericListINCC_0<TDataClass>();

            IEnumerable<TDataClass> nonEmptyGeneric = new GenericEnumerable_25<TDataClass>();
            DataGrid dg = new DataGrid();
            Assert.IsNotNull(dg);
            TestPanel.Children.Add(dg);
            int i;

            // empty non-generics don't trigger AGC
            dg.ItemsSource = emptyNonGeneric;
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after assigning an empty non-generic sequence");

                // non-empty non-generics do
                dg.ItemsSource = nonEmptyNonGeneric;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after assigning a non-empty, non-generic sequence");

                // in order to get autogeneration to happen as a result of adding an item to the list, the list needs to be INCC
                dg.ItemsSource = emptyNonGenericList;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after assigning an empty non-generic list");

                 emptyNonGenericList.Add(new TDataClass());
                 Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after adding an item to an empty, non-generic list");

                 // test column count after removing from a non-empty, non-INCC
                 dg.ItemsSource = nonEmptyNonGenericList;

                 Assert.AreEqual(0, dg.Columns.Count, "Column updates were not delayed until the next layout when the ItemsSource was set");
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after assigning a non-empty, non-generic list");
                 Assert.AreEqual(1, dg.SlotCount, "Wrong number of rows in datagrid after assigning a non-empty, non-generic list.");

                 // we're non-INCC, so the grid won't be seeing this remove
                 nonEmptyNonGenericList.RemoveAt(0);
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after removing item from a non-empty, non-generic list");
                 Assert.AreEqual(1, dg.SlotCount, "Wrong number of rows in datagrid after removing item from a non-empty, non-generic list.");

                 // an INCC data source
                 dg.ItemsSource = emptyNonGenericListINCC;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after assigning an empty non-generic INCC list");

                 // ... adding an item triggers AGC
                 emptyNonGenericListINCC.Add(new TDataClass());
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after adding an item to an empty, non-generic INCC list");
                 Assert.AreEqual(1, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(emptyNonGenericListINCC.Count, dg.SlotCount, "Row count does not match item count");

                 // ... removing an item doesn't clear the columns
                 emptyNonGenericListINCC.RemoveAt(0);
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after removing the item from an empty, non-generic INCC list");
                 Assert.AreEqual(0, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(emptyNonGenericListINCC.Count, dg.SlotCount, "Row count does not match item count");

                 // ... clearing the columns manually does
                 dg.Columns.Clear();
                 Assert.AreEqual(0, dg.Columns.Count, "Wrong column count after clearing all columns");
                 Assert.AreEqual(0, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(emptyNonGenericListINCC.Count, dg.SlotCount, "Row count does not match item count");

                 // ... adding a new (different type) item to an ItemsSource for which columns have been generated regenerates the autogenerated columns
                 // ... also, the row doesn't show since the columns don't exist
                 emptyNonGenericListINCC.Add(this.GetType());
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(16, dg.Columns.Count, "Wrong column count after adding this an empty, non-generic INCC list which has already had columns generated and cleared");
                 Assert.AreEqual(1, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(1, emptyNonGenericListINCC.Count, "Wrong list count");

                 // ... setting AutogenerateColumns to false clears the AutogeneratedColumns
                 dg.AutoGenerateColumns = false;
                 Assert.AreEqual(0, dg.Columns.Count, "Wrong column count after adding this an empty, non-generic INCC list which has already had columns generated and cleared");
                 Assert.AreEqual(0, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(1, emptyNonGenericListINCC.Count, "Wrong list count");

                 // ... setting AutogenerateColumnsBack to true regenerates the columns
                 dg.AutoGenerateColumns = true;
                 Assert.AreEqual(16, dg.Columns.Count, "Wrong column count after adding this an empty, non-generic INCC list which has already had columns generated and cleared");
                 Assert.AreEqual(1, dg.SlotCount, "Wrong number of rows in datagrid.");
                 Assert.AreEqual(1, emptyNonGenericListINCC.Count, "Wrong list count");

                 // clear the dg
                 dg.ItemsSource = emptyNonGeneric;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after assigning an empty non-generic sequence");

                 // strongly-typed enumerables trigger AGC
                 dg.ItemsSource = emptyList;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 i = 0;
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after assigning an empty generic sequence");
                 foreach (PropertyInfo prop in properties)
                 {
                     DataGridColumn col = dg.Columns[i++];
                     Assert.AreEqual(prop.Name, col.Header);

                     PropertyTestExpectedResultsAttribute expectedResults = PropertyTestExpectedResultsAttribute.GetExpectedResults(prop, "AutogeneratedColumns");

                     Assert.AreEqual(expectedResults.IsReadOnly, col.IsReadOnly, "IsReadOnly wrong value: " + prop.Name);
                 }

                 // clear the dg
                 dg.ItemsSource = emptyNonGeneric;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 Assert.AreEqual(0, dg.Columns.Count, "Non-zero column count after assigning an empty non-generic sequence");

                 // another strongly-typed enumerable
                 dg.ItemsSource = nonEmptyGeneric;
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                 i = 0;
                 Assert.AreEqual(numberOfProperties, dg.Columns.Count, "Wrong column count after assigning a non-empty generic sequence");
                 foreach (PropertyInfo prop in properties)
                 {
                     DataGridColumn col = dg.Columns[i++];
                     Assert.AreEqual(prop.Name, col.Header);

                     PropertyTestExpectedResultsAttribute expectedResults = PropertyTestExpectedResultsAttribute.GetExpectedResults(prop, "AutogeneratedColumns");

                     Assert.AreEqual(expectedResults.IsReadOnly, col.IsReadOnly, "IsReadOnly wrong value: " + prop.Name);
                 }

                 dg.ItemsSource = new List<int?> { 1, 3 };
            });
            this.EnqueueYieldThread();
            this.EnqueueCallback(delegate
            {
                Assert.AreEqual(1, dg.Columns.Count, "Improper number of columns when binding to List<int?>");
            });

            EnqueueTestComplete();
        }

        #endregion

        #region CheckColumnHeaderVisibility
        [TestMethod]
        [Asynchronous]
        [Description("Test ColumnHeaderVisibilty for various ItemsSource conditions")]
        public virtual void CheckColumnHeaderVisibility()
        {
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            ObservableCollection<Person> people = new ObservableCollection<Person>();
            dataGrid.ItemsSource = people;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.Columns.Count > 0, "Columns were not generated for IEnumerable<T>");
                // Make sure the Headers are visible since we have columns even though the list is empty
                Assert.IsTrue(dataGrid.ColumnHeaders.Visibility == Visibility.Visible, "ColumnHeaders should be visible for an empty list");
                people.Add(new Person { Name = "Square guy" });
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.ColumnHeaders.Visibility == Visibility.Visible, "ColumnHeaders should be visible for a non-empty list");
                people.RemoveAt(0);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.ColumnHeaders.Visibility == Visibility.Visible, "ColumnHeaders should be visible even after deleting all items");
                people.Clear();
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.ColumnHeaders.Visibility == Visibility.Visible, "ColumnHeaders should be visible even after Clear (NotifyCollectionChangedAction.Reset)");
                dataGrid.ItemsSource = null;
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.Columns.Count == 0, "Columns were not cleared when ItemsSource was set to null");
            });
            EnqueueTestComplete();
        }
        #endregion CheckColumnHeaderVisibility

        #region CheckLoadRow
        List<TDataClass> gridList = new List<TDataClass>();
        int gridListItems = 0;

        [Asynchronous]
        [TestMethod]
        [Description("Test that all data in the list is also in the grid.")]
        public virtual void CheckLoadRow()
        {
            gridList.Clear();
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            dataGrid.Width = 300;
            dataGrid.Height = 250;
            Assert.IsNotNull(dataGrid);
            dataGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(dataGrid_LoadingRow);
            TestPanel.Children.Add(dataGrid);
            dataGrid.ItemsSource = boundList;

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                foreach (TDataClass st in boundList)
                {
                    dataGrid.ScrollIntoView(st, null);
                }
            });
            
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(boundList.Count() <= gridList.Count);

                foreach (TDataClass st in boundList)
                {
                    Assert.IsTrue(gridList.Contains(st));
                }
            });

            EnqueueTestComplete();
        }

        void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            gridListItems++;
            gridList.Add((TDataClass)e.Row.DataContext);
        }
        #endregion

        [TestMethod]
        [Asynchronous]
        [Description("Test adding items to underlying collection and make sure grid gets updated.")]
        public virtual void AddItemsToUnderlyingData()
        {
            IEnumerable listSomeType = new TDataClassSource();
            int sizeOfList = listSomeType.Count();
            DataGrid dataGrid1 = new DataGrid();
            dataGrid1.ItemsSource = listSomeType;
            TestPanel.Children.Add(dataGrid1);
            this.EnqueueYieldThread();

            //List should have sizeOfList items in it.
            int countOfItems = 0;
            EnqueueCallback(delegate
            {
                foreach (TDataClass i in dataGrid1.ItemsSource)
                {
                    countOfItems++;
                    //This will fail if the item isn't in the grid.
                    dataGrid1.SelectedItems.Add(i);
                }
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.AreEqual(sizeOfList, countOfItems);
                Assert.AreEqual(sizeOfList, dataGrid1.SelectedItems.Count,
                    String.Format("Should be {0} items in SelectedItems", sizeOfList));
                //Can only add items with IList
                if (listSomeType is IList)
                {
                    TDataClass newItem = new TDataClass();
                    ((IList)listSomeType).Add(newItem);

                    if (listSomeType is INotifyCollectionChanged)
                    {
                        dataGrid1.SelectedItems.Add(newItem);
                        Assert.AreEqual(sizeOfList + 1, dataGrid1.SelectedItems.Count,
                            String.Format("Should be {0} items in SelectedItems", sizeOfList + 1));


                        //
                    }
                }
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the HorizontalScrollBarVisibility property after the DataGrid was populated with rows.")]
        public virtual void SetHorizontalScrollBarVisibility()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Make sure default value is ScrollBarVisibility.Auto
                Assert.AreEqual(ScrollBarVisibility.Auto, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Disabled
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                Assert.AreEqual(ScrollBarVisibility.Disabled, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Visible
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                Assert.AreEqual(ScrollBarVisibility.Visible, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Hidden
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                Assert.AreEqual(ScrollBarVisibility.Hidden, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Auto
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                Assert.AreEqual(ScrollBarVisibility.Auto, dataGrid.HorizontalScrollBarVisibility);
            });
            EnqueueCallback(delegate
            {
                // Make the vertical scrollbar appear too
                boundList = new TDataClassSource();
                dataGrid.ItemsSource = boundList;
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Disabled
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                Assert.AreEqual(ScrollBarVisibility.Disabled, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Visible
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                Assert.AreEqual(ScrollBarVisibility.Visible, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Hidden
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                Assert.AreEqual(ScrollBarVisibility.Hidden, dataGrid.HorizontalScrollBarVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change value to ScrollBarVisibility.Auto
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                Assert.AreEqual(ScrollBarVisibility.Auto, dataGrid.HorizontalScrollBarVisibility);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the HorizontalGridLinesBrush property after the DataGrid was populated with rows.")]
        public virtual void SetHorizontalGridLinesBrush()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Make sure default GridLinesVisibility value is DataGridGridLinesDataGridGridLinesVisibility.Vertical
                Assert.AreEqual(DataGridGridLinesVisibility.Vertical, dataGrid.GridLinesVisibility);

                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;

                // Change HorizontalGridLinesBrush value to SolidColorBrush
                SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
                dataGrid.HorizontalGridLinesBrush = blueBrush;
                Assert.AreEqual(blueBrush, dataGrid.HorizontalGridLinesBrush);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility value to DataGridGridLinesVisibility.None
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;
                Assert.AreEqual(DataGridGridLinesVisibility.None, dataGrid.GridLinesVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility to DataGridGridLinesVisibility.Horizontal
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                Assert.AreEqual(DataGridGridLinesVisibility.Horizontal, dataGrid.GridLinesVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Reset HorizontalGridLinesBrush value
                dataGrid.HorizontalGridLinesBrush = null;
                Assert.IsNull(dataGrid.HorizontalGridLinesBrush);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility to DataGridGridLinesVisibility.All
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                Assert.AreEqual(DataGridGridLinesVisibility.All, dataGrid.GridLinesVisibility);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the VerticalGridLinesBrush property after the DataGrid was populated with rows.")]
        public virtual void SetVerticalGridLinesBrush()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Make sure default GridLinesVisibility value is DataGridGridLinesVisibility.Vertical
                Assert.AreEqual(DataGridGridLinesVisibility.Vertical, dataGrid.GridLinesVisibility);

                // Change VerticalGridLinesBrush value to SolidColorBrush
                SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
                dataGrid.VerticalGridLinesBrush = blueBrush;
                Assert.AreEqual(blueBrush, dataGrid.VerticalGridLinesBrush);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility value to DataGridGridLinesVisibility.None
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;
                Assert.AreEqual(DataGridGridLinesVisibility.None, dataGrid.GridLinesVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility to DataGridGridLinesVisibility.Vertical
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical;
                Assert.AreEqual(DataGridGridLinesVisibility.Vertical, dataGrid.GridLinesVisibility);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Reset VerticalGridLinesBrush value
                dataGrid.VerticalGridLinesBrush = null;
                Assert.IsNull(dataGrid.VerticalGridLinesBrush);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Change GridLinesVisibility to DataGridGridLinesVisibility.All
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                Assert.AreEqual(DataGridGridLinesVisibility.All, dataGrid.GridLinesVisibility);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the DataGridRowHeader's SeparatorBrush / SeparatorVisibility properties while populating the DataGrid.")]
        public virtual void SetRowHeaderSeparator()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);

            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                for (int i = dataGrid.DisplayData.FirstScrollingSlot; i <= dataGrid.DisplayData.LastScrollingSlot; i++)
                {
                    DataGridRow row = dataGrid.DisplayData.GetDisplayedRow(i);
                    row.HeaderCell.SeparatorBrush = new SolidColorBrush(Colors.Cyan);
                    if (row.Index % 2 == 0)
                    {
                        row.HeaderCell.SeparatorVisibility = Visibility.Collapsed;
                    }
                }
            });

            EnqueueCallback(delegate
            {
                //Reset datagrid
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid_Loaded);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the DataGridColumnHeader's SeparatorBrush / SeparatorVisibility properties while populating the DataGrid.")]
        public virtual void SetColumnHeaderSeparator()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.RowHeight = 18;
            dataGrid.ColumnWidth = new DataGridLength(30);
            dataGrid.Width = 350;
            dataGrid.Height = 350;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(DataGrid_LoadingRow_ColumnHeaderSeparatorBrush);
            dataGrid.AutoGenerateColumns = true;
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);

            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                //Reset datagrid
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid_Loaded);
                dataGrid.LoadingRow -= new EventHandler<DataGridRowEventArgs>(DataGrid_LoadingRow_ColumnHeaderSeparatorBrush);
            });
            EnqueueTestComplete();
        }

        private void DataGrid_LoadingRow_ColumnHeaderSeparatorBrush(object sender, DataGridRowEventArgs e)
        {
            foreach (DataGridColumn dataGridColumn in e.Row.OwningGrid.Columns)
            {
                if (e.Row.Index % 3 == 0)
                {
                    dataGridColumn.HeaderCell.SeparatorBrush = new SolidColorBrush(Colors.Green);
                }
                else if (e.Row.Index % 5 == 0)
                {
                    dataGridColumn.HeaderCell.SeparatorBrush = null;
                }
                if (dataGridColumn.DisplayIndex % 2 == (e.Row.Index % 2))
                {
                    dataGridColumn.HeaderCell.SeparatorVisibility = Visibility.Collapsed;
                }
                else
                {
                    dataGridColumn.HeaderCell.SeparatorVisibility = Visibility.Visible;
                }
            }
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that exercises the HeadersVisibility property to show/hide both column and row headers.")]
        public virtual void SetHeadersVisibility()
        {
            IEnumerable listSomeType = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 350;
            dataGrid.ColumnHeaderHeight = 25;
            dataGrid.RowHeaderWidth = 50;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.AutoGenerateColumns = true;
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
            dataGrid.ItemsSource = listSomeType;
            TestPanel.Children.Add(dataGrid);

            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();
            IList list = listSomeType as IList;
            if (list != null && list is INotifyCollectionChanged)
            {
                EnqueueCallback(delegate
                {
                    list.Add(new TDataClass());
                    list.Insert(1, new TDataClass());
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(25, dataGrid.ColumnHeaders.DesiredSize.Height);
                    Assert.AreEqual(50, dataGrid.ActualRowHeaderWidth);
                    list.Add(new TDataClass());
                    list.Insert(1, new TDataClass());
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(false, dataGrid.AreRowHeadersVisible);
                    list.RemoveAt(3);
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.Row;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(50, dataGrid.ActualRowHeaderWidth);
                    list.RemoveAt(0);
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(false, dataGrid.AreRowHeadersVisible);
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.Row;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(50, dataGrid.ActualRowHeaderWidth);
                    list.Add(new TDataClass());
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(25, dataGrid.ColumnHeaders.DesiredSize.Height);
                    Assert.AreEqual(false, dataGrid.AreRowHeadersVisible);
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(false, dataGrid.AreRowHeadersVisible);
                    list.Add(new TDataClass());
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(25, dataGrid.ColumnHeaders.DesiredSize.Height);
                    Assert.AreEqual(false, dataGrid.AreRowHeadersVisible);
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(25, dataGrid.ColumnHeaders.DesiredSize.Height);
                    Assert.AreEqual(50, dataGrid.ActualRowHeaderWidth);
                    Assert.AreEqual(true, dataGrid.AreRowHeadersVisible);
                    list.Add(new TDataClass());
                    dataGrid.HeadersVisibility = DataGridHeadersVisibility.Row;
                });
                this.EnqueueYieldThread();
                EnqueueCallback(delegate
                {
                    Assert.AreEqual(Visibility.Collapsed, dataGrid.ColumnHeaders.Visibility);
                    Assert.AreEqual(50, dataGrid.ActualRowHeaderWidth);
                    Assert.AreEqual(true, dataGrid.AreRowHeadersVisible);
                    list.Clear();
                });
            }
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                //Reset datagrid
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid_Loaded);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test scrolling rows into view.")]
        public virtual void ScrollRowsIntoView()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.AutoGenerateColumns = true;
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 99, false);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 0, false);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 99, false);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 0, false);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 25, false);
            });
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                dataGrid.UpdateStateOnMouseLeftButtonDown(null, 0, 5, false);
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                //Reset datagrid
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid_Loaded);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test changing column display index.")]
        public virtual void SetColumnDisplayIndex()
        {
            IEnumerable boundList = new TDataClassSource();
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.Width = 350;
            dataGrid.Height = 250;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            dataGrid.AutoGenerateColumns = true;
            dataGrid.ItemsSource = boundList;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Columns[2].DisplayIndex = 1;
                    Assert.AreEqual(0, dataGrid.Columns[0].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[2].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[1].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Columns[0].DisplayIndex = 2;
                    Assert.AreEqual(0, dataGrid.Columns[2].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[1].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[0].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Columns[0].DisplayIndex = 0;
                    dataGrid.Columns[1].DisplayIndex = 1;
                    Assert.AreEqual(0, dataGrid.Columns[0].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[1].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[2].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Focus();
                    dataGrid.BeginEdit();
                    dataGrid.Columns[2].DisplayIndex = 1;
                    Assert.AreEqual(0, dataGrid.Columns[0].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[2].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[1].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Focus();
                    dataGrid.BeginEdit();
                    dataGrid.Columns[0].DisplayIndex = 2;
                    Assert.AreEqual(0, dataGrid.Columns[2].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[1].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[0].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                if (dataGrid.Columns.Count >= 3)
                {
                    dataGrid.Focus();
                    dataGrid.BeginEdit();
                    dataGrid.Columns[0].DisplayIndex = 0;
                    dataGrid.Columns[1].DisplayIndex = 1;
                    Assert.AreEqual(0, dataGrid.Columns[0].DisplayIndex);
                    Assert.AreEqual(1, dataGrid.Columns[1].DisplayIndex);
                    Assert.AreEqual(2, dataGrid.Columns[2].DisplayIndex);
                }
            });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                //Reset datagrid
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid_Loaded);
            });
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        [Description("Test that checks scrollbar visibility and the filler column width")]
        public virtual void ScrollBarVisibilityTest()
        {
            DataGrid dataGrid = new DataGrid();
            isLoaded = false;
            dataGrid.Width = 150;
            dataGrid.ColumnHeaderHeight = 25;
            dataGrid.RowHeight = 50;
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;
            dataGrid.RowHeaderWidth = 25;
            dataGrid.Height = 150;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            ObservableCollection<Person> people = new ObservableCollection<Person>();
            people.Add(new Person { Name = "Person 1" });
            dataGrid.ItemsSource = people;

            TestPanel.Children.Add(dataGrid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();

            double fillerWidth = 0;
            DataGridFillerColumn fillerColumn = null;
            DataGridCell fillerCell = null;
            EnqueueCallback(delegate
            {
                // Check ScrollBar visibility
                Assert.IsNotNull(dataGrid.HorizontalScrollBar);
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Collapsed, "Horizontal ScrollBar should be Collapsed");
                Assert.IsNotNull(dataGrid.VerticalScrollBar);
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");

                // Check the filler column
                fillerColumn = dataGrid.ColumnsInternal.FillerColumn;
                Assert.IsNotNull(fillerColumn);
                fillerWidth = fillerColumn.FillerWidth;
                Assert.IsTrue(fillerColumn.IsActive, "FillerColumn should be active");
                Assert.IsTrue(fillerWidth > 0);

                // Check the width of a cell in the filler column
                DataGridRow row = dataGrid.GetRowFromItem(people[0]) as DataGridRow;
                Assert.IsNotNull(row);
                fillerCell = row.FillerCell;
                Assert.IsNotNull(fillerCell);
                Assert.IsTrue(fillerCell.ActualWidth == fillerWidth, "Filler cells should have the same width as the FillerColumn");

                dataGrid.Width = 200;
            });
            this.EnqueueYieldThread();
            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(fillerColumn.FillerWidth > fillerWidth, "FillerColumn Width should auto grow with DataGrid");
                Assert.IsTrue(fillerCell.ActualWidth == fillerColumn.FillerWidth, "Filler cells should update when FillerColumn width changes");

                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Collapsed, "Horizontal ScrollBar should be Collapsed");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");

                dataGrid.ColumnWidth = new DataGridLength(100);
                dataGrid.Width = 120;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Visible, "Horizontal ScrollBar should be Visible");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");
                dataGrid.Width = 150;
                dataGrid.RowHeaderWidth = 100;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Visible, "Horizontal ScrollBar should be Visible");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");
                dataGrid.RowHeaderWidth = 25;
                dataGrid.RowHeight = 150;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Collapsed, "Horizontal ScrollBar should be Collapsed");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Visible, "Vertical ScrollBar should be Visible");
                dataGrid.RowHeight = 120;
                dataGrid.RowHeaderWidth = 35;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Collapsed, "Horizontal ScrollBar should be Collapsed");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");
                dataGrid.RowHeight = 150;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Vertical ScrollBar causes us to have a Horizontal ScrollBar
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Visible, "Horizontal ScrollBar should be Visible");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Visible, "Vertical ScrollBar should be Visible");

                dataGrid.RowHeight = 120;
                dataGrid.RowHeaderWidth = 50;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                // Horizontal ScrollBar causes us to have a Vertical ScrollBar
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Visible, "Horizontal ScrollBar should be Visible");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Visible, "Vertical ScrollBar should be Visible");

                dataGrid.RowHeaderWidth = 25;
            });

            this.EnqueueYieldThread();
            EnqueueCallback(delegate
            {
                Assert.IsTrue(dataGrid.HorizontalScrollBar.Visibility == Visibility.Collapsed, "Horizontal ScrollBar should be Collapsed");
                Assert.IsTrue(dataGrid.VerticalScrollBar.Visibility == Visibility.Collapsed, "Vertical ScrollBar should be Collapsed");
            });

            EnqueueTestComplete();
        }

        // 
















        //        EnqueueConditional(delegate { return isLoaded; });
        //        EnqueueCallback(delegate
        //        {
        //            dataGrid.SelectedIndex = 0;
        //            ((IList)listSomeType).Insert(0, newItem);
        //        });
        //        this.EnqueueYieldThread();
        //        EnqueueCallback(delegate
        //        {
        //            Assert.AreEqual(1, dataGrid.SelectedIndex);
        //            ((IList)listSomeType).Remove(newItem);
        //        });
        //        this.EnqueueYieldThread();
        //        EnqueueCallback(delegate
        //        {
        //            Assert.AreEqual(0, dataGrid.SelectedIndex);
        //        });
        //    }
        //    EnqueueTestComplete();
        //}

        [TestMethod]
        [Asynchronous]
        [Description("Test that counts the number of elements in the visual tree for a 1 column 1 row datagrid")]
        public virtual void VisualTreeCount()
        {
            DataGrid dataGrid = new DataGrid();
            Assert.IsNotNull(dataGrid);
            isLoaded = false;
            dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            dataGrid.Loaded += new RoutedEventHandler(dataGrid_Loaded);
            List<Person> people = new List<Person>();
            people.Add(new Person { Name = "circle guy" });
            dataGrid.ItemsSource = people;

            Grid grid = new Grid();
            grid.Width = 200;
            grid.Height = 200;

            grid.Children.Add(dataGrid);
            TestPanel.Children.Add(grid);
            EnqueueConditional(delegate { return isLoaded; });
            this.EnqueueYieldThread();

            EnqueueCallback(delegate
            {
                // Make sure the DataGrid auto fills the space its given
                Assert.IsTrue(dataGrid.ActualWidth == grid.ActualWidth, "The DataGrid's width didn't fill");
                Assert.IsTrue(dataGrid.ActualHeight == grid.ActualHeight, "The DataGrid's height didn't fill");
                Assert.AreEqual(62, CountVisualTreeRecursively(dataGrid), "New number of elements in dataGrid visual tree");
            });

            EnqueueTestComplete();
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}

