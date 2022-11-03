using System.Collections.Generic;

namespace Bajaj.Model
{

    public class WorkShopGroupModel
    {
        public string workshopsGroupName { get; set; }
        public List<WorkCity> CityList { get; set; }
    }



    public class WorkCity
    {
        public string city { get; set; }
        public List<WorkShopGroup> workshops { get; set; }
    }

    public class WorkShopGroup
    {
        public string pincode { get; set; }
        public string name1 { get; set; }
        public string region { get; set; }
        public string address { get; set; }
        public string created { get; set; }
        public string modified { get; set; }
        public long id { get; set; }
        public long city_id { get; set; }
        public long oem_id { get; set; }
        public long? parent_id { get; set; }
        public bool is_active { get; set; }
        public long? user_id { get; set; }
        public long group_name_id { get; set; }
        public long state_id { get; set; }
        public long country_id { get; set; }
        public string name { get; set; }

    }
    //public class RootObjectAgain
    //{
    //    public List<WorkCity> work_shop_group { get; set; }

    //    public RootObjectAgain()
    //    {
    //        work_shop_group = new List<WorkCity>();
    //    }
    //}

    //public class RootObject
    //{
    //    public List<WorkShopGroup> work_shop { get; set; }

    //    public RootObject()
    //    {
    //        work_shop = new List<WorkShopGroup>();
    //    }
    //}

    //public class ShowWorkShopGroupClass
    //{
    //    public string WorkShopGroup { get; set; }
    //}
}
