﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace System.Windows.Controls.UnitTests
{
    public partial class NavigationCacheModeDisabledPage : Page
    {
        public NavigationCacheModeDisabledPage()
        {
            InitializeComponent();
        }

        public string GetTextBoxText()
        {
            return _textBox.Text;
        }

        public void SetTextBoxText(string newText)
        {
            _textBox.Text = newText;
        }

    }
}
