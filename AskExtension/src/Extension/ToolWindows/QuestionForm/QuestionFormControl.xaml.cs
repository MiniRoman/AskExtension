//------------------------------------------------------------------------------
// <copyright file="QuestionFormControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AskExtension
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for QuestionFormControl.
    /// </summary>
    public partial class QuestionFormControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionFormControl"/> class.
        /// </summary>
        public QuestionFormControl()
        {
            this.InitializeComponent();
        }

        private void AskQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO call a method to send a question to so.
        }
    }
}