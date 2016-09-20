//------------------------------------------------------------------------------
// <copyright file="QuestionFormControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.Composition;
using AskExtension.Core;

namespace AskExtension
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for QuestionFormControl.
    /// </summary>
    public partial class QuestionFormControl : UserControl, INotifyPropertyChanged
    {
        [Import]
        private SubmitionService _submitionService;

        public string codeToShow { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionFormControl"/> class.
        /// </summary>
        public QuestionFormControl()
        {
            ExtensionMefContainer.Service.SatisfyImportsOnce(this);
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void AskQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO call a method to send a question to so.
        }

        public void UpdateContent(string code)
        {
            this.codeToShow = code;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("codeToShow"));
            }
        }
    }
}