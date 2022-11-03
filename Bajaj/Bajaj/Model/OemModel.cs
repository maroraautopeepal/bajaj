using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class OemModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<AllOemModel> results { get; set; }
    }

    public class AllOemModel : BaseViewModel
    {
        public int id { get; set; }
        private string _name;
        public string name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }
        public int? admin { get; set; }
        public string oem_file { get; set; }
        public object color { get; set; }
        public object app_name { get; set; }
        public bool is_active { get; set; }
    }


}
