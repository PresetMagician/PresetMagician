using System.Windows.Controls;
using Be.Windows.Forms;
using Catel.Windows;
using Drachenkatze.PresetMagician.Utils.IssueReport;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class ReportIssueView 
    {
        public ReportIssueView()
        {
            InitializeComponent();
        }
        
        public ReportIssueView(ReportIssueViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            AddCustomButton(new DataWindowButton("Close without submitting", "CloseDialog"));
            AddCustomButton(new DataWindowButton("Submit Issue", "SubmitIssue"));

            InitializeComponent();
        }
    }
}
