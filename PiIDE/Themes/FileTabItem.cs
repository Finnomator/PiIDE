using System.Windows;
using System.Windows.Controls;

namespace PiIDE {
    internal class FileTabItem : TabItem {

        public static readonly RoutedEvent ConditionalClickEvent = EventManager.RegisterRoutedEvent(
            name: "ConditionalClick",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(FileTabItem));

        public FileTabItem() {
            Style = (Style) Application.Current.FindResource("FileTabItemStyle");

        }

        public event RoutedEventHandler ConditionalClick {
            add { AddHandler(ConditionalClickEvent, value); }
            remove { RemoveHandler(ConditionalClickEvent, value); }
        }

        void RaiseCustomRoutedEvent() {
            // Create a RoutedEventArgs instance.
            RoutedEventArgs routedEventArgs = new(routedEvent: ConditionalClickEvent);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }

        /*
        // For demo purposes, we use the Click event as a trigger.
        protected override void OnClick() {
            // Some condition combined with the Click event will trigger the ConditionalClick event.
            if (DateTime.Now > new DateTime())
                RaiseCustomRoutedEvent();

            // Call the base class OnClick() method so Click event subscribers are notified.
            base.OnClick();
        }*/
    }
}
