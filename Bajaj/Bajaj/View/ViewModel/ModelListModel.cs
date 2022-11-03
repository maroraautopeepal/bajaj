using System.Collections.Generic;

namespace Bajaj.ViewModel
{
    public class ModelListModel
    {
        public Dictionary<string, ModelName> models { get; set; }
        public List<NaClass> NA_NA { get; set; }
    }


    public class ModelName
    {
        public class RootObject
        {
            public List<ModelName> Key { get; set; }

            public RootObject()
            {
                Key = new List<ModelName>();
            }
        }

    }

    public class ModelId
    {
        public List<NaClass> NA_NA { get; set; }
    }

    public class NaClass
    {
        public int model_id { get; set; }
    }

    public class ModelNameClass
    {
        public string ModelName { get; set; }
        public int id { get; set; }
    }

}
