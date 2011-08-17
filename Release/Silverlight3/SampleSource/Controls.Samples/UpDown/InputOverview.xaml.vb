﻿' (c) Copyright Microsoft Corporation.
' This source is subject to the Microsoft Public License (Ms-PL).
' Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
' All other rights reserved.

Imports Microsoft.VisualBasic
Imports System.ComponentModel
Imports System.Windows.Controls
''' <summary>
''' Overview Page is placed at the top of the Input root node in the Sample
''' TreeView and provide an overview of Input.
''' </summary>
<Sample("Overview", DifficultyLevel.None), Category("Input")> _
Partial Public Class InputOverview
    Inherits UserControl
    ''' <summary>
    ''' Class Constructor.
    ''' </summary>
    Public Sub New()
        InitializeComponent()
    End Sub
End Class