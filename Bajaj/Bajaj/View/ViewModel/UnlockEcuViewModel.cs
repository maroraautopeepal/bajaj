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
    public class UnlockEcuViewModel : BaseViewModel
    {
        public UnlockEcuViewModel()
        {

        }

        public ICommand EcuTabCommand => new Command(async (obj) =>
        {
            try
            {
                var selected = (UnlockModel)obj;
                selected.opacity = 1;
                selected_ecu = selected;

                foreach (var ecu in ecus_list)
                {
                    if (selected != ecu)
                    {
                        ecu.opacity = .5;
                    }
                }

                
                if (App.ConnectedVia == "USB")
                {
                    DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(selected_ecu.protocol.autopeepal,
                                            selected_ecu.tx_header,
                                            selected_ecu.rx_header);
                }
                else if (App.ConnectedVia == "BT")
                {
                    DependencyService.Get<Interfaces.IBth>().SetDongleProperties(selected_ecu.protocol.autopeepal,
                                            selected_ecu.tx_header,
                                            selected_ecu.rx_header);
                }
                else
                {
                    DependencyService.Get<Interfaces.IConnectionWifi>().SetDongleProperties(selected_ecu.protocol.autopeepal,
                                            selected_ecu.tx_header,
                                            selected_ecu.rx_header);
                }

                
            }
            catch (Exception ex)
            {
            }
        });

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

        private UnlockModel _selected_ecu;
        public UnlockModel selected_ecu
        {
            get => _selected_ecu;
            set
            {
                _selected_ecu = value;
                OnPropertyChanged("selected_ecu");
            }
        }

        private ObservableCollection<UnlockModel> _ecus_list;
        public ObservableCollection<UnlockModel> ecus_list
        {
            get => _ecus_list;
            set
            {
                _ecus_list = value;
                OnPropertyChanged("ecus_list");
            }
        }
    }
}
