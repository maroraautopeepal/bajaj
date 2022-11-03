using Bajaj.Model;
using System;
using Xamarin.Forms;

namespace Bajaj.template
{
    public class PreconditionTemplate : DataTemplateSelector
    {
        public DataTemplate StaticData { get; set; }
        public DataTemplate ManualData{ get; set; }
        public DataTemplate DynamicData { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            try
            {
                DataTemplate template = new DataTemplate();
                var data = item as PreCondition;

                if (data != null)
                {
                    if (data.pre_condition_type == "static")
                    {
                        template = StaticData;
                    }
                    else if (data.pre_condition_type == "manual_confirm")
                    {
                        template = ManualData;
                    }
                    else if (data.pre_condition_type == "pid")
                    {
                        template = DynamicData;
                    }
                }
                return template;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
