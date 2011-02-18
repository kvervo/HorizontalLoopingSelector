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
using System.Reflection;
using System.Windows.Controls.Design.Common;
using System.Windows.Controls.Primitives;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using SSWC = Silverlight::System.Windows.Controls;
using SSWCP = Silverlight::System.Windows.Controls.Primitives;

[assembly: ProvideMetadata(typeof(System.Windows.Controls.Data.Design.MetadataRegistration))]

namespace System.Windows.Controls.Data.Design
{
    /// <summary>
    /// MetadataRegistration class.
    /// </summary>
    public partial class MetadataRegistration : MetadataRegistrationBase, IProvideAttributeTable
    {
        /// <summary>
        /// Design time metadata registration class.
        /// </summary>
        public MetadataRegistration()
            : base()
        {
            // Note:
            // The default constructor sets value of AssemblyFullName and 
            // XmlResourceName used by MetadataRegistrationBase.AddDescriptions().
            // The convention here is that the <RootNamespace> in .design.csproj
            // (or Default namespace in Project -> Properties -> Application tab)
            // must be the same as runtime assembly's main namespace (t.Namespace)
            // plus .Design.

            Type t = typeof(SSWC.DataGrid);
            AssemblyName an = t.Assembly.GetName();
            AssemblyFullName = ", " + an.FullName;
            XmlResourceName = t.Namespace + ".Data.Design." + an.Name + ".XML";
        }

        /// <summary>
        /// Gets the AttributeTable for design time metadata.
        /// </summary>
        public AttributeTable AttributeTable
        {
            get
            {
                return BuildAttributeTable();
            }
        }

        /// <summary>
        /// Provide a place to add custom attributes without creating a AttributeTableBuilder subclass.
        /// </summary>
        /// <param name="builder">The assembly attribute table builder.</param>
        protected override void AddAttributes(AttributeTableBuilder builder)
        {
            builder.AddCallback(typeof(SSWC.DataGrid), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data", true)));
            builder.AddCallback(typeof(SSWC.DataGridCell), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWC.DataGridRow), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWC.DataGridRowGroupHeader), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridCellsPresenter), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridColumnHeader), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridColumnHeadersPresenter), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridDetailsPresenter), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridFrozenGrid), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridRowHeader), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));
            builder.AddCallback(typeof(SSWCP.DataGridRowsPresenter), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data/Control Parts", false)));

            builder.AddCallback(typeof(SSWC.DataPager), b => b.AddCustomAttributes(new ToolboxCategoryAttribute("Data", true)));

            // Note: everything added here must be duplicated in VisualStudio.Design as well!
            builder.AddCallback(typeof(SSWC.DataPager), b => b.AddCustomAttributes(new DefaultBindingPropertyAttribute("Source")));
        }
    }
}
