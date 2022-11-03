using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;

namespace Bajaj.Model
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class GdImageGD : BaseViewModel
    {
        public int id { get; set; }
        //public string gd_image { get; set; }
        public string image_name { get; set; }

        private string _gd_image;
        public string gd_image
        {
            get => _gd_image;
            set
            {
                //string[] Link = ApiServices.BaseUrl.Split('/');
                _gd_image = value;
                _gd_image = "http://159.89.167.72" + _gd_image; //  $"image_name{_gd_image}";
                OnPropertyChanged("gd_image");
            }
        }
    }

    public class ImageGD
    {
        public bool is_active { get; set; }
        public string image { get; set; }
    }

    public class GroupGD
    {
        public string upper_limit { get; set; }
        public string lower_limit { get; set; }
        public string unit { get; set; }
        public string entry_description { get; set; }
        public string group_name { get; set; }
    }

    public class TypeFormGD
    {
        public string description { get; set; }
        public string topic { get; set; }
        public List<string> entry_group_names { get; set; }
        public List<GroupGD> groups { get; set; }
        public List<string> entry_groups { get; set; }
    }

    public class DatumGD
    {
        public string text_val { get; set; }
        public int node { get; set; }
        public string type { get; set; }
    }

    public class DecisionsGD
    {
        public string type { get; set; }
        public List<DatumGD> data { get; set; }
    }

    public class DataGD
    {
        public string exit_script { get; set; }
        public List<ImageGD> images { get; set; }
        public string topic { get; set; }
        public bool is_active { get; set; }
        public string entry_script { get; set; }
        public TypeFormGD type_form { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public DecisionsGD decisions { get; set; }
        public string id { get; set; }
        public List<object> globals { get; set; }
    }

    public class TreeDataGD
    {
        public int parent { get; set; }
        public DataGD data { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string description { get; set; }
    }

    public class TreeSetGD
    {
        public int id { get; set; }
        public string tree_id { get; set; }
        public string vehicle_model { get; set; }
        public string model { get; set; }
        public string tree_description { get; set; }
        public List<TreeDataGD> tree_data { get; set; }
        public string is_active { get; set; }
    }

    public class DtcGD
    {
        public int id { get; set; }
        public string code { get; set; }
    }

    public class ResultGD
    {
        public int id { get; set; }
        public string gd_id { get; set; }
        public int name { get; set; }
        public string model { get; set; }
        public int model_year { get; set; }
        public string gd_description { get; set; }
        public string occurring_conditions { get; set; }
        public string effects_on_vehicle { get; set; }
        public string causes { get; set; }
        public string ecu_name { get; set; }
        public string remedial_actions { get; set; }
        public string is_active { get; set; }
        public List<GdImageGD> gd_images { get; set; }
        public List<TreeSetGD> tree_set { get; set; }
        public List<DtcGD> dtc { get; set; }
    }

    public class GdModelGD
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ResultGD> results { get; set; }
    }

    /////////-------------------old class

    public class GdModel
    {
        public int id { get; set; }
        public Data data { get; set; }
        public string name { get; set; }
        public int parent { get; set; }
        public string description { get; set; }
    }

    public class Group
    {
        public string upper_limit { get; set; }
        public string lower_limit { get; set; }
        public string unit { get; set; }
        public string entry_description { get; set; }
        public string group_name { get; set; }
    }

    public class TypeForm
    {
        public string topic { get; set; }
        public string description { get; set; }
        public List<string> entry_group_names { get; set; }
        public List<Group> groups { get; set; } = new List<Group>();

    }

    public class Decisions
    {
        public string type { get; set; }
        public List<DecisionData> data { get; set; } = new List<DecisionData>();
    }

    public class DecisionData
    {
        public long node { get; set; }
        public string text_val { get; set; }
        public string type { get; set; }
    }



    public class Data
    {
        public TypeForm type_form { get; set; } = new TypeForm();
        public string description { get; set; }
        public Decisions decisions { get; set; } = new Decisions();
        public bool is_active { get; set; }
        public string exit_script { get; set; }
        public string topic { get; set; }
        public List<object> globals { get; set; }
        public string entry_script { get; set; }
        public List<object> images { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Info
    {
        public string causes { get; set; }

        public DateTime created { get; set; }

        public int ecu_name_id { get; set; }

        public string effects_on_vehicle { get; set; }

        public string gd_description { get; set; }

        public string gd_id { get; set; }

        public int id { get; set; }

        public string is_active { get; set; }

        public DateTime modified { get; set; }

        public string model { get; set; }

        public int name_id { get; set; }

        public string occurring_conditions { get; set; }

        public string remedial_actions { get; set; }
    }

    public class GdImage
    {
        //[JsonProperty("image_name")]
        //public string gd_name { get; set; }

        //[JsonProperty("gd_image")]
        //public string gd_image { get; set; }

        //[JsonProperty("id")]
        //public long id { get; set; }

        //[JsonProperty("model")]
        //public string model { get; set; }


        public string gd_name { get; set; }

        public string gd_image { get; set; }


        public long id { get; set; }


        public string model { get; set; }
    }

    public class PCodeClass
    {
        public int gd_id { get; set; }
        public Info info { get; set; }
        public List<GdImage> gd_image { get; set; }
        public List<Dictionary<string, List<ser>>> tree { get; set; }
    }

    public class TreeName
    {
        public Dictionary<string, List<ser>> tree { get; set; }
    }

    public class ser
    {
        public long id { get; set; }
        public string name { get; set; }
        public long parent { get; set; }
        public string description { get; set; }
        public Data data { get; set; } = new Data();
    }

    public class BindableModel
    {

    }

    /// <summary>
    /// yes
    /// </summary>
    public class GdMainModel
    {
        public List<GdImage> gd_image { get; set; } = new List<GdImage>();
        //public Info info { get; set; } = new Info();
        public string causes { get; set; }

        public DateTime created { get; set; }

        public int ecu_name_id { get; set; }

        public string effects_on_vehicle { get; set; }

        public string gd_description { get; set; }

        public string gd_id { get; set; }

        public int id { get; set; }

        public string is_active { get; set; }

        public DateTime modified { get; set; }

        public string model { get; set; }

        public long name { get; set; }

        public string occurring_conditions { get; set; }

        public string remedial_actions { get; set; }
        public List<TreeSet> tree_set { get; set; } = new List<TreeSet>();
    }


    /// <summary>
    /// yes
    /// </summary>
    public class FirstListClass
    {
        public List<GdMainModel> main_list { get; set; } = new List<GdMainModel>();
    }


    public class TreeSet : BaseViewModel
    {
        public long id { get; set; }
        public string tree_id { get; set; }
        public string is_active { get; set; }
        public string model { get; set; }
        public string tree_description { get; set; }
        public string vehicle_model { get; set; }
        public List<TreeData> tree_data { get; set; } = new List<TreeData>();
    }

    public class TreeData
    {

        public long id { get; set; }
        public long parent { get; set; }
        public string description { get; set; }
        public string name { get; set; }

        public DataModel data { get; set; } = new DataModel();
    }

    public class DataModel
    {
        public TypeFormModel type_form { get; set; } = new TypeFormModel();
        public string description { get; set; }
        public DecisionsModel decisions { get; set; } = new DecisionsModel();
        public bool is_active { get; set; }
        public string exit_script { get; set; }
        public string topic { get; set; }
        public List<object> globals { get; set; }
        public string entry_script { get; set; }
        public List<object> images { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }

    public class DecisionsModel
    {
        public string type { get; set; }
        public List<DecissionListModel> data { get; set; } = new List<DecissionListModel>();
    }

    public class DecissionListModel
    {
        public long node { get; set; }
        public string text_val { get; set; }
        public string type { get; set; }
    }

    public class TypeFormModel
    {
        public string topic { get; set; }
        public string description { get; set; }
        public List<string> entry_group_names { get; set; }
        public List<GroupModel> groups { get; set; } = new List<GroupModel>();
    }

    public class GroupModel
    {
        public string upper_limit { get; set; }
        public string lower_limit { get; set; }
        public string unit { get; set; }
        public string entry_description { get; set; }
        public string group_name { get; set; }
    }
}
