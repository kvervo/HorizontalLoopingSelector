﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;

namespace System.Windows.Controls.Samples
{
    /// <summary>
    /// Sample page demonstrating the ValidationSummary.
    /// </summary>
    [Sample("ValidationSummary", DifficultyLevel.Intermediate, "Validation")]
    public partial class ValidationSummarySample : UserControl
    {
        /// <summary>
        /// Initializes an ValidationSummarySample.
        /// </summary>
        public ValidationSummarySample()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the data after loading.
        /// </summary>
        /// <param name="sender">This control.</param>
        /// <param name="e">Event arguments.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by event defined in Xaml.")]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            contactGrid.DataContext = Contact.JohnDoe;
        }
    }
}
