﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides data for the
    /// <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" />
    /// event.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public class DatePickerDateValidationErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Inherited code: Requires comment.
        /// </summary>
        private bool _throwException;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.DatePickerDateValidationErrorEventArgs" />
        /// class.
        /// </summary>
        /// <param name="exception">
        /// The initial exception from the
        /// <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" />
        /// event.
        /// </param>
        /// <param name="text">
        /// The text that caused the
        /// <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" />
        /// event.
        /// </param>
        public DatePickerDateValidationErrorEventArgs(Exception exception, string text)
        {
            this.Text = text;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the initial exception associated with the
        /// <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" />
        /// event.
        /// </summary>
        /// <value>
        /// The exception associated with the validation failure.
        /// </value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the text that caused the
        /// <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" />
        /// event.
        /// </summary>
        /// <value>
        /// The text that caused the validation failure.
        /// </value>
        public string Text { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// <see cref="P:System.Windows.Controls.DatePickerDateValidationErrorEventArgs.Exception" />
        /// should be thrown.
        /// </summary>
        /// <value>
        /// True if the exception should be thrown; otherwise, false.
        /// </value>
        /// <exception cref="T:System.ArgumentException">
        /// If set to true and
        /// <see cref="P:System.Windows.Controls.DatePickerDateValidationErrorEventArgs.Exception" />
        /// is null.
        /// </exception>
        public bool ThrowException
        {
            get { return this._throwException; }
            set
            {
                if (value && this.Exception == null)
                {
                    throw new ArgumentException(System.Windows.Controls.Properties.Resources.DatePickerDateValidationErrorEventArgs_ThrowException_NoException);
                }
                this._throwException = value;
            }
        }
    }
}