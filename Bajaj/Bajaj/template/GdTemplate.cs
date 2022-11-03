using Bajaj.View.GdSection;
using System;
using Xamarin.Forms;

namespace Bajaj.template
{
    public class GdTemplate : DataTemplateSelector
    {
        public DataTemplate SimpleDate { get; set; }
        public DataTemplate GroupData { get; set; }
        public DataTemplate RadioData { get; set; }
        public DataTemplate LastData { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            try
            {
                DataTemplate template = new DataTemplate();
                var group = item as TreeListModel;

                if (group != null)
                {
                    if (group.group_name == "SimpleData")
                    {
                        template = SimpleDate;
                    }
                    else if (group.group_name == "RadioData")
                    {
                        template = RadioData;
                    }
                    else if (group.group_name == "GroupData")
                    {
                        template = GroupData;
                    }
                    else if (group.group_name == "LastData")
                    {
                        template = LastData;
                    }
                }
                return template;
            }
            catch (Exception ex)
            {
                return null;
            }
            //return ((Person)item).DateOfBirth.Year >= 1980 ? ValidTemplate : InvalidTemplate;
        }
    }
}
