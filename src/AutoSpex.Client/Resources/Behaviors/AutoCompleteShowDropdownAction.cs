using System;
using Avalonia;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class AutoCompleteShowDropdownAction : AvaloniaObject, IAction
{
    public object? Execute(object? sender, object? parameter)
    {
        /*if (AssociatedObject is not null && !AssociatedObject.IsDropDownOpen)
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
        }*/
        throw new NotImplementedException();
    }
}