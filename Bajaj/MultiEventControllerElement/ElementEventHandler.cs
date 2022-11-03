using Xamarin.Forms;

namespace MultiEventController.Models
{
    public class ElementEventHandler
    {
        public string ElementName { get; set; }
        public string ElementValue { get; set; }
        public double ScrollX { get; set; }
        public double ScrollY { get; set; }
        public ElementType ElementType { get; set; }
        public object ListItem { get; set; }
        public string ItemIndex { get; set; }
        public string ToUserId { get; set; }
        public bool IsExpert { get; set; }
        public ItemTappedEventArgs eValues { get; set; }
        public object sender { get; set; }
        public ScrolledEventArgs e { get; set; }
    }
    public enum ElementType
    {
        TextBox,
        Button,
        ScroolView,
        Frame
    }
}
