using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Windows.Media;
// using SlideDock.Services; // Not needed anymore

namespace SlideDock.Views
{
    public partial class ExpandedDockView : UserControl
    {
        // private readonly IFileInteractionService _fileInteractionService; // Not needed anymore

        public ExpandedDockView()
        {
            InitializeComponent();
            // _fileInteractionService = new FileInteractionService(); // Not needed anymore
        }
    }
}