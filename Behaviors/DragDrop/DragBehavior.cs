using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;
using SlideDock.Models;

namespace SlideDock.Behaviors.DragDrop
{
    public class DragBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DragBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }

        private Point _startPoint;

        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(AssociatedObject);
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(AssociatedObject);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (Command != null && Command.CanExecute(null))
                    {
                        if (AssociatedObject is FrameworkElement fe)
                        {
                            var dragStartInfo = new DragStartInfo
                            {
                                ViewModel = fe.DataContext,
                                DragSource = fe
                            };
                            Command.Execute(dragStartInfo);
                        }
                        else
                        {
                            Command.Execute(new DragStartInfo { ViewModel = null, DragSource = null }); // Fallback
                        }
                    }
                    // We don't start the actual drag drop operation here, 
                    // just trigger the command. The ViewModel will handle the DoDragDrop.
                }
            }
        }
    }
}
