using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    class FlashEcuViewModel : BaseViewModel
    {
        public FlashEcuViewModel()
        {
           
        }

        public ICommand EcuTabCommand => new Command(async (obj) =>
        {
            try
            {
                var selected = (FlashEcusModel)obj;
                selected.opacity = 1;
                selected_ecu = selected;

                foreach (var ecu in ecus_list)
                {
                    if (selected != ecu)
                    {
                        ecu.opacity = .5;
                    }
                }

                flash_file_list = new ObservableCollection<File>(selected_ecu.flash_file_list);

                //GetPidList();
                //StaticData.ecu_info
                if (App.ConnectedVia == "USB")
                {
                    DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(selected_ecu.ecu2.protocol.autopeepal,
                                            selected_ecu.ecu2.tx_header,
                                            selected_ecu.ecu2.rx_header);
                }
                else if (App.ConnectedVia == "BT")
                {
                    DependencyService.Get<Interfaces.IBth>().SetDongleProperties(selected_ecu.ecu2.protocol.autopeepal,
                                            selected_ecu.ecu2.tx_header,
                                            selected_ecu.ecu2.rx_header);
                }
                else
                {
                    DependencyService.Get<Interfaces.IConnectionWifi>().SetDongleProperties(selected_ecu.ecu2.protocol.autopeepal,
                                            selected_ecu.ecu2.tx_header,
                                            selected_ecu.ecu2.rx_header);
                }
            }
            catch (Exception ex)
            {
            }
        });


        private ObservableCollection<FlashEcusModel> _ecus_list;
        public ObservableCollection<FlashEcusModel> ecus_list
        {
            get => _ecus_list;
            set
            {
                _ecus_list = value;
                OnPropertyChanged("ecus_list");
            }
        }

        private FlashEcusModel _selected_ecu;
        public FlashEcusModel selected_ecu
        {
            get => _selected_ecu;
            set
            {
                _selected_ecu = value;
                OnPropertyChanged("selected_ecu");
            }
        }

        private AllModelsModel _all_models_list;
        public AllModelsModel all_models_list
        {
            get => _all_models_list;
            set
            {
                _all_models_list = value;
                OnPropertyChanged("all_models_list");
            }
        }

        private ObservableCollection<File> _flash_file_list;
        public ObservableCollection<File> flash_file_list
        {
            get => _flash_file_list;
            set
            {
                _flash_file_list = value;
                OnPropertyChanged("flash_file_list");
            }
        }

        private ObservableCollection<File> _static_flash_file_list;
        public ObservableCollection<File> static_flash_file_list
        {
            get => _static_flash_file_list;
            set
            {
                _static_flash_file_list = value;
                OnPropertyChanged("static_flash_file_list");
            }
        }

        private File _selected_flash_file;
        public File selected_flash_file
        {
            get => _selected_flash_file;
            set
            {
                _selected_flash_file = value;
                OnPropertyChanged("selected_flash_file");
            }
        }

        //private ObservableCollection<EcuMapFile> _ecu_map_file;
        //public ObservableCollection<EcuMapFile> ecu_map_file
        //{
        //    get => _ecu_map_file;
        //    set
        //    {
        //        _ecu_map_file = value;
        //        OnPropertyChanged("ecu_map_file");
        //    }
        //}

        private string _select_dataset = "Select Dataset to Flash";
        public string select_dataset
        {
            get => _select_dataset;
            set
            {
                _select_dataset = value;
                OnPropertyChanged("select_dataset");
            }
        }

        private string _search_key;
        public string search_key
        {
            get => _search_key;
            set
            {
                _search_key = value;
                OnPropertyChanged("search_key");
            }
        }

        private bool _flash_file_view_visible;
        public bool flash_file_view_visible
        {
            get => _flash_file_view_visible;
            set
            {
                _flash_file_view_visible = value;
                OnPropertyChanged("flash_file_view_visible");
            }
        }

        private string _flash_timer;
        public string flash_timer
        {
            get => _flash_timer;
            set
            {
                _flash_timer = value;
                OnPropertyChanged("flash_timer");
            }
        }

        private bool _timer_visible = false;
        public bool timer_visible
        {
            get => _timer_visible;
            set
            {
                _timer_visible = value;
                OnPropertyChanged("timer_visible");
            }
        }

        private bool _progress_visible = false;
        public bool progress_visible
        {
            get => _progress_visible;
            set
            {
                _progress_visible = value;
                OnPropertyChanged("progress_visible");
            }
        }

        private bool _extra_view_visible = false;
        public bool extra_view_visible
        {
            get => _extra_view_visible;
            set
            {
                _extra_view_visible = value;
                OnPropertyChanged("extra_view_visible");
            }
        }

        private string _flash_percent;
        public string flash_percent
        {
            get => _flash_percent;
            set
            {
                _flash_percent = value;
                OnPropertyChanged("flash_percent");
            }
        }

        private bool _flashProgress_visible = false;
        public bool flashProgress_visible
        {
            get => _flashProgress_visible;
            set
            {
                _flashProgress_visible = value;
                OnPropertyChanged("flashProgress_visible");
            }
        }
    }
}
