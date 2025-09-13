using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SlideDock.ViewModels;

namespace SlideDock.Views
{
    public partial class AppIconView : UserControl
    {

        public AppIconView()
        {
            InitializeComponent();
        }

        #region Helpers Visuais

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

        #endregion
    }
}
