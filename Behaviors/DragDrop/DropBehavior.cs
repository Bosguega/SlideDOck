using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace SlideDock.Behaviors.DragDrop
{
    public class DropBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty DropCommandProperty = 
            DependencyProperty.Register(nameof(DropCommand), typeof(ICommand), typeof(DropBehavior), new PropertyMetadata(null));

        public ICommand DropCommand
        {
            get => (ICommand)GetValue(DropCommandProperty);
            set => SetValue(DropCommandProperty, value);
        }

        public static readonly DependencyProperty DragOverCommandProperty = 
            DependencyProperty.Register(nameof(DragOverCommand), typeof(ICommand), typeof(DropBehavior), new PropertyMetadata(null));

        public ICommand DragOverCommand
        {
            get => (ICommand)GetValue(DragOverCommandProperty);
            set => SetValue(DragOverCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
            AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            AssociatedObject.Drop -= AssociatedObject_Drop;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            if (DragOverCommand != null && DragOverCommand.CanExecute(e))
            {
                DragOverCommand.Execute(e);
            }
            e.Handled = true;
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (DragOverCommand != null && DragOverCommand.CanExecute(e))
            {
                DragOverCommand.Execute(e);
                // The effect will be set by the ViewModel
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (DropCommand != null && DropCommand.CanExecute(e))
            {
                DropCommand.Execute(e);
            }
            e.Handled = true;
        }
    }
}
