using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors
{
    public class AutoCompleteZeroMinimumPrefixLengthDropdownBehaviour : Behavior<AutoCompleteBox>
    {
        static AutoCompleteZeroMinimumPrefixLengthDropdownBehaviour()
        {
        }

        protected override void OnAttached()
        {
            if (AssociatedObject is not null)
            {
                AssociatedObject.KeyUp += OnKeyUp;
                AssociatedObject.DropDownOpening += DropDownOpening;
                AssociatedObject.GotFocus += OnGotFocus;

                Task.Delay(500).ContinueWith(_ => Avalonia.Threading.Dispatcher.UIThread.Invoke(CreateDropdownButton));
            }

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is not null)
            {
                AssociatedObject.KeyUp -= OnKeyUp;
                AssociatedObject.DropDownOpening -= DropDownOpening;
                AssociatedObject.GotFocus -= OnGotFocus;
            }

            base.OnDetaching();
        }

        //have to use KeyUp as AutoCompleteBox eats some of the KeyDown events
        private void OnKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if ((e.Key == Avalonia.Input.Key.Down || e.Key == Avalonia.Input.Key.F4))
            {
                if (string.IsNullOrEmpty(AssociatedObject?.Text))
                {
                    ShowDropdown();
                }
            }
        }

        private void DropDownOpening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var prop = AssociatedObject.GetType().GetProperty("TextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var tb = (TextBox?)prop?.GetValue(AssociatedObject);
            if (tb.IsReadOnly)
            {
                e.Cancel = true;
                return;
            }
        }

        private void ShowDropdown()
        {
            if (AssociatedObject is not null && !AssociatedObject.IsDropDownOpen)
            {
                typeof(AutoCompleteBox).GetMethod("PopulateDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { AssociatedObject, EventArgs.Empty });
                typeof(AutoCompleteBox).GetMethod("OpeningDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { false });

                if (!AssociatedObject.IsDropDownOpen)
                {
                    //We *must* set the field and not the property as we need to avoid the changed event being raised (which prevents the dropdown opening).
                    var ipc = typeof(AutoCompleteBox).GetField("_ignorePropertyChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if ((bool)ipc?.GetValue(AssociatedObject) == false)
                        ipc?.SetValue(AssociatedObject, true);

                    AssociatedObject.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, true);
                }
            }
        }

        private void CreateDropdownButton()
        {
            if (AssociatedObject != null)
            {
                var prop = AssociatedObject.GetType().GetProperty("TextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var tb = (TextBox?)prop?.GetValue(AssociatedObject);
                
                var btn = new Button()
                {
                    Content = "🢗",
                    Margin = new(3),
                    ClickMode = ClickMode.Press
                };
                btn.Click += (s, e) => ShowDropdown();

                tb.InnerRightContent = btn;
            }
        }

        private void OnGotFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                //CreateDropdownButton();
            }
        }
    }
}
