using SlideDock.Services;
using SlideDock.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SlideDock.Views
{
    public partial class MenuGroupView : UserControl
    {
        private IDragDropUIService? _dragDropUIService;

        public MenuGroupView()
        {
            InitializeComponent();
        }

        private void MenuGroupView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MenuGroupViewModel viewModel)
            {
                _dragDropUIService = new DragDropUIService(this);
                viewModel.SetDragDropUIService(_dragDropUIService);
            }
        }
    }
}
