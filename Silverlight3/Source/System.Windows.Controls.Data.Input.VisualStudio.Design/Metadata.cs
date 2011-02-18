﻿//-----------------------------------------------------------------------
// <copyright company="Microsoft">
//      (c) Copyright Microsoft Corporation.
//      This source is subject to the Microsoft Public License (Ms-PL).
//      Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//      All other rights reserved.
// </copyright>
//-----------------------------------------------------------------------

extern alias Silverlight;
using System.ComponentModel;
using System.Windows.Controls.Design.Common;
using Microsoft.Windows.Design.Metadata;
using SSWC = Silverlight::System.Windows.Controls;

namespace System.Windows.Controls.Data.Input.VisualStudio.Design
{
    /// <summary>
    /// MetadataRegistration class.
    /// </summary>
    public partial class MetadataRegistration : MetadataRegistrationBase, IRegisterMetadata
    {
        /// <summary>
        /// Borrowed from System.Windows.Controls.Toolbox.Design.MetadataRegistration:
        /// use a static flag to ensure metadata is registered only once.
        /// </summary>
        private static bool _initialized;

        /// <summary>
        /// Called by tools to register design time metadata.
        /// </summary>
        public void Register()
        {
            if (!_initialized)
            {
                MetadataStore.AddAttributeTable(BuildAttributeTable());
                _initialized = true;
            }
        }

        /// <summary>
        /// Provide a place to add custom attributes without creating a AttributeTableBuilder subclass.
        /// </summary>
        /// <param name="builder">The assembly attribute table builder.</param>
        protected override void AddAttributes(AttributeTableBuilder builder)
        {
            // duplicated from .Design

            string labelName = "System.Windows.Controls.Label" + this.AssemblyFullName;

            builder.AddCallback(Type.GetType(labelName), b => b.AddCustomAttributes(new DefaultBindingPropertyAttribute("Target")));
            builder.AddCallback(typeof(SSWC.DescriptionViewer), b => b.AddCustomAttributes(new DefaultBindingPropertyAttribute(Extensions.GetMemberName<SSWC.DescriptionViewer>(x => x.Target))));
            builder.AddCallback(typeof(SSWC.ValidationSummary), b => b.AddCustomAttributes(new DefaultBindingPropertyAttribute(Extensions.GetMemberName<SSWC.ValidationSummary>(x => x.Target))));
        }
    }
}
