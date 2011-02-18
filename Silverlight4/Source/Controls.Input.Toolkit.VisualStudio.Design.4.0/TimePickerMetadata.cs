﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

extern alias Silverlight;
using System.ComponentModel;
using System.Windows.Controls.Design.Common;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Features;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using SSWC = Silverlight::System.Windows.Controls;

namespace System.Windows.Controls.Input.Design
{
    /// <summary>
    /// Metadata for the SSWC.TimePicker control.
    /// </summary>
    internal class TimePickerMetadata : AttributeTableBuilder
    {
        /// <summary>
        /// To register design time metadata for SSWC.TimePicker.
        /// </summary>
        public TimePickerMetadata()
            : base()
        {
            AddCallback(
                typeof(SSWC.TimePicker),
                b =>
                {
                    b.AddCustomAttributes(new DefaultPropertyAttribute(Extensions.GetMemberName<SSWC.TimePicker>(x => x.Value)));
                    b.AddCustomAttributes(new DefaultEventAttribute("ValueChanged"));

                    b.AddCustomAttributes(
                        Extensions.GetMemberName<SSWC.TimePicker>(x => x.TimeParsers),
                        new NewItemTypesAttribute(typeof(SSWC.CatchallTimeParser)));
                });
        }
    }
}
