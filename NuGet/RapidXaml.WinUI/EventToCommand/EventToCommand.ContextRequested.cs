//<auto-generated />
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using Microsoft.Xaml.Interactions.Core;

namespace RapidXaml;

public partial class EventToCommand : DependencyObject
{
    public static ICommand GetContextRequested(DependencyObject obj)
        => (ICommand)obj.GetValue(ContextRequestedProperty);

    public static void SetContextRequested(DependencyObject obj, ICommand value)
        => obj.SetValue(ContextRequestedProperty, value);

    public static readonly DependencyProperty ContextRequestedProperty =
        DependencyProperty.RegisterAttached("ContextRequested", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, OnContextRequestedChanged));

    private static void OnContextRequestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement uie)
        {
            // Clear any existing behavior(s)
            Interaction.SetBehaviors(uie, null);

            // Add the new one if there is one
            if (e.NewValue is ICommand newCmd)
            {
                var etb = new EventTriggerBehavior { EventName = "ContextRequested" };
                etb.Actions.Add(new InvokeCommandAction { Command = newCmd });

                Interaction.SetBehaviors(uie, new BehaviorCollection { etb });
            }
        }
    }
}