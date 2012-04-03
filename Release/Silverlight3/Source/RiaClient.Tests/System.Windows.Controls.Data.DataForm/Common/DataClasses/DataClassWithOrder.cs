﻿//-----------------------------------------------------------------------
// <copyright company="Microsoft" file="DataClass.cs">
//      (c) Copyright Microsoft Corporation.
//      This source is subject to the Microsoft Public License (Ms-PL).
//      Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//      All other rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace System.Windows.Controls.UnitTests
{
    /// <summary>
    /// A data class for testing the <see cref="DataForm"/>.
    /// </summary>
    public class DataClassWithOrder : IEditableObject, INotifyPropertyChanged, IRevertibleChangeTracking
    {
        /// <summary>
        /// Private accessor to BoolProperty.
        /// </summary>
        private bool boolProperty;

        /// <summary>
        /// Private accessor to DateTimeProperty.
        /// </summary>
        private DateTime dateTimeProperty;

        /// <summary>
        /// Private accessor to IntProperty.
        /// </summary>
        private int intProperty;

        /// <summary>
        /// Private accessor to IntPropertyWithoutAutoGenerateField.
        /// </summary>
        private int intPropertyWithoutAutoGenerateField;

        /// <summary>
        /// Holds whether or not this data class has been changed.
        /// </summary>
        private bool isChanged;

        /// <summary>
        /// Private accessor to NonGeneratedIntProperty.
        /// </summary>
        private int nonGeneratedIntProperty;

        /// <summary>
        /// Private accessor to StringProperty.
        /// </summary>
        private string stringProperty;

        /// <summary>
        /// The unchaged data class for use in IRevertibleChangeTracking.
        /// </summary>
        private DataClassWithOrder unchangedDataClass;

        /// <summary>
        /// The old data class for use in IEditableObject implementation.
        /// </summary>
        private DataClassWithOrder oldDataClass;

        /// <summary>
        /// Gets or sets a value indicating whether or not the bool property is true.
        /// </summary>
        [DataMember()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool", Justification = "This property is only used in unit tests.")]
        [Display(AutoGenerateField = true, ShortName = "Bool Property", Order = 1)]
        public bool BoolProperty
        {
            get
            {
                return this.boolProperty;
            }

            set
            {
                if ((this.boolProperty != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("BoolProperty", value);
                    this.boolProperty = value;
                    this.RaiseDataMemberChanged("BoolProperty");
                }
            }
        }

        /// <summary>
        /// Gets or sets a DateTime value.
        /// </summary>
        [DataMember()]
        [Display(AutoGenerateField = true, ShortName = "Date Time Property")]
        public DateTime DateTimeProperty
        {
            get
            {
                return this.dateTimeProperty;
            }

            set
            {
                if ((this.dateTimeProperty != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("DateTimeProperty", value);
                    this.dateTimeProperty = value;
                    this.RaiseDataMemberChanged("DateTimeProperty");
                }
            }
        }

        /// <summary>
        /// Gets or sets an int.
        /// </summary>
        [DataMember()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "This property is only used in unit tests.")]
        [Display(AutoGenerateField = true, ShortName = "Int Property", Order = 3)]
        [Key()]
        public int IntProperty
        {
            get
            {
                return this.intProperty;
            }

            set
            {
                if ((this.intProperty != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("IntProperty", value);
                    this.intProperty = value;
                    this.RaiseDataMemberChanged("IntProperty");
                }
            }
        }

        /// <summary>
        /// Gets or sets an int.
        /// </summary>
        [DataMember()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "This property is only used in unit tests.")]
        [Display(ShortName = "Int Property Without AutoGenerateField", Order = -1)]
        public int IntPropertyWithoutAutoGenerateField
        {
            get
            {
                return this.intPropertyWithoutAutoGenerateField;
            }

            set
            {
                if ((this.intPropertyWithoutAutoGenerateField != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("IntPropertyWithoutAutoGenerateField", value);
                    this.intPropertyWithoutAutoGenerateField = value;
                    this.RaiseDataMemberChanged("IntPropertyWithoutAutoGenerateField");
                }
            }
        }

        /// <summary>
        /// Gets or sets an int.
        /// </summary>
        [DataMember()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "This property is only used in unit tests.")]
        [Display(AutoGenerateField = false, ShortName = "Non-Generated Int Property", Order = -2)]
        public int NonGeneratedIntProperty
        {
            get
            {
                return this.nonGeneratedIntProperty;
            }

            set
            {
                if ((this.nonGeneratedIntProperty != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("NonGeneratedIntProperty", value);
                    this.nonGeneratedIntProperty = value;
                    this.RaiseDataMemberChanged("NonGeneratedIntProperty");
                }
            }
        }

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        [DataMember()]
        [Display(AutoGenerateField = true, ShortName = "String Property", Order = 0)]
        public string StringProperty
        {
            get
            {
                return this.stringProperty;
            }

            set
            {
                if ((this.stringProperty != value))
                {
                    this.EnsureUnchangedDataClass();
                    this.ValidateProperty("StringProperty", value);
                    this.stringProperty = value;
                    this.RaiseDataMemberChanged("StringProperty");
                }
            }
        }

        /// <summary>
        /// Initializes the unchanged data class used in IRevertableChangeTracking.
        /// </summary>
        private void EnsureUnchangedDataClass()
        {
            if (this.unchangedDataClass == null)
            {
                this.unchangedDataClass = new DataClassWithOrder();
                this.unchangedDataClass.boolProperty = this.boolProperty;
                this.unchangedDataClass.dateTimeProperty = this.dateTimeProperty;
                this.unchangedDataClass.intProperty = this.intProperty;
                this.unchangedDataClass.intPropertyWithoutAutoGenerateField = this.intPropertyWithoutAutoGenerateField;
                this.unchangedDataClass.nonGeneratedIntProperty = this.nonGeneratedIntProperty;
                this.unchangedDataClass.stringProperty = this.stringProperty;
            }
        }

        /// <summary>
        /// Raises the case where a data member has changed.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        private void RaiseDataMemberChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);

            this.IsChanged =
                this.unchangedDataClass.boolProperty != this.boolProperty ||
                this.unchangedDataClass.dateTimeProperty != this.dateTimeProperty ||
                this.unchangedDataClass.intProperty != this.intProperty ||
                this.unchangedDataClass.intPropertyWithoutAutoGenerateField != this.intPropertyWithoutAutoGenerateField ||
                this.unchangedDataClass.nonGeneratedIntProperty != this.nonGeneratedIntProperty ||
                this.unchangedDataClass.stringProperty != this.stringProperty;
        }

        /// <summary>
        /// Raises the case where a property has changed.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Validates a property.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value to validate against.</param>
        private void ValidateProperty(string propertyName, object value)
        {
            ValidationContext context = new ValidationContext(this, null, null);
            context.MemberName = propertyName;
            Validator.ValidateProperty(value, context);
        }

        #region IEditableObject Members

        /// <summary>
        /// Begins an edit.
        /// </summary>
        public void BeginEdit()
        {
            if (this.oldDataClass == null)
            {
                this.oldDataClass = new DataClassWithOrder();
                this.oldDataClass.boolProperty = this.boolProperty;
                this.oldDataClass.dateTimeProperty = this.dateTimeProperty;
                this.oldDataClass.intProperty = this.intProperty;
                this.oldDataClass.intPropertyWithoutAutoGenerateField = this.intPropertyWithoutAutoGenerateField;
                this.oldDataClass.nonGeneratedIntProperty = this.nonGeneratedIntProperty;
                this.oldDataClass.stringProperty = this.stringProperty;
            }
        }

        /// <summary>
        /// Cancels an edit.
        /// </summary>
        public void CancelEdit()
        {
            if (this.oldDataClass != null)
            {
                this.boolProperty = this.oldDataClass.boolProperty;
                this.dateTimeProperty = this.oldDataClass.dateTimeProperty;
                this.intProperty = this.oldDataClass.intProperty;
                this.intPropertyWithoutAutoGenerateField = this.oldDataClass.intPropertyWithoutAutoGenerateField;
                this.nonGeneratedIntProperty = this.oldDataClass.nonGeneratedIntProperty;
                this.stringProperty = this.oldDataClass.stringProperty;
                this.oldDataClass = null;
            }
        }

        /// <summary>
        /// Ends an edit.
        /// </summary>
        public void EndEdit()
        {
            this.oldDataClass = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fires when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IRevertibleChangeTracking Members

        /// <summary>
        /// Rejects the changes to this data class.
        /// </summary>
        public void RejectChanges()
        {
            this.boolProperty = this.unchangedDataClass.boolProperty;
            this.dateTimeProperty = this.unchangedDataClass.dateTimeProperty;
            this.intProperty = this.unchangedDataClass.intProperty;
            this.intPropertyWithoutAutoGenerateField = this.unchangedDataClass.intPropertyWithoutAutoGenerateField;
            this.nonGeneratedIntProperty = this.unchangedDataClass.nonGeneratedIntProperty;
            this.stringProperty = this.unchangedDataClass.stringProperty;
            this.IsChanged = false;
        }

        #endregion

        #region IChangeTracking Members

        /// <summary>
        /// Accepts the changes to this data class.
        /// </summary>
        public void AcceptChanges()
        {
            this.unchangedDataClass.boolProperty = this.boolProperty;
            this.unchangedDataClass.dateTimeProperty = this.dateTimeProperty;
            this.unchangedDataClass.intProperty = this.intProperty;
            this.unchangedDataClass.intPropertyWithoutAutoGenerateField = this.intPropertyWithoutAutoGenerateField;
            this.unchangedDataClass.nonGeneratedIntProperty = this.nonGeneratedIntProperty;
            this.unchangedDataClass.stringProperty = this.stringProperty;
            this.IsChanged = false;
        }

        /// <summary>
        /// Gets whether or not this data class is changed.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool IsChanged
        {
            get
            {
                return this.isChanged;
            }

            private set
            {
                if (value != this.isChanged)
                {
                    this.isChanged = value;
                    this.RaisePropertyChanged("IsChanged");
                }
            }
        }

        #endregion
    }
}
