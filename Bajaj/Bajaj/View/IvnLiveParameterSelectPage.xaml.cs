using Acr.UserDialogs;
using MultiEventController.Models;
using Newtonsoft.Json;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IvnLiveParameterSelectPage : DisplayAlertPage
    {
        CollectionView collectionView;
        TapGestureRecognizer tab_gesture;
        IvnLiveParameterSelectViewModel viewModel;
        List<PidCode> SelectedParameterList;
        LiveParameterSelectModel model;
        string selected_ecu = string.Empty;
        double opacity = -1;
        Color color;
        bool IsPageRemove = false;

        public IvnLiveParameterSelectPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new IvnLiveParameterSelectViewModel();
                model = new LiveParameterSelectModel();
                //ems_List.ItemsSource = viewModel.EMSParameterList;
                SelectedParameterList = new List<PidCode>();
                tab_gesture = new TapGestureRecognizer();
                tab_gesture.Tapped += Tab_gesture_Tapped;
                //create_ems_tab(1);
                //create_iecu_tab(.5);

                var IVNPidList = new List<IVN_LiveParameterSelectModel>();


                var FrameId = new List<PIDFrameId>();
                var IVNTest = new List<IVN_LiveParameterFrame_name>();
                var IVNTestPDs = new List<IVN_frame_idsAndPID>();

                int class_id = 0;
                bool collectiionVisible = true;
                foreach (var ecu in StaticData.ecu_info)
                {
                    if (ecu.read_dtc_index == "IVN")
                    {
                        // Create Dynamically ECU's Tab
                        if (opacity == -1)
                        {
                            opacity = 1;
                        }
                        else
                        {
                            opacity = 0.5;
                        }

                        ColumnDefinition column = new ColumnDefinition
                        {
                            Width = GridLength.Star,
                        };
                        grd_tab.ColumnDefinitions.Add(column);
                        Grid grd = new Grid
                        {
                            BackgroundColor = color,
                            Opacity = opacity,
                            ClassId = Convert.ToString(class_id),
                        };
                        Label ecu_lbl = new Label
                        {
                            Text = ecu.ecu_name,
                            Style = (Style)this.Resources["txt_tab"]
                        };
                        grd.Children.Add(ecu_lbl);
                        grd_tab.Children.Add(grd, grd_tab.ColumnDefinitions.Count - 1, 0);
                        grd.GestureRecognizers.Add(tab_gesture);

                        //Create Dynamically ECU's PID Data List

                        model.ecu_name = ecu.ecu_name;
                        int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).pid_dataset_id;

                        //viewModel.LiveParameterList = model.roots = viewModel.get_pid(id);

                        if (viewModel.IVNAllPIDDetail.Count != 0)
                        {
                            
                            foreach (var item in viewModel.IVNAllPIDDetail.FirstOrDefault(x => x.id == id).frame_datasets)
                            {
                                var frame_id = item.frame_id;
                                var IVN_pid = item.frame_ids;
                                var check_ecu_name = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name);
                                if (check_ecu_name != null)
                                {
                                    foreach (var frame_ids_item in item.frame_ids)
                                    {
                                        FrameId.Add(new PIDFrameId
                                        {
                                            FramID = item.frame_id,
                                            bit_coded = frame_ids_item.bit_coded,
                                            @byte = frame_ids_item.@byte,
                                            frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                            message_type = frame_ids_item.message_type,
                                            no_of_bits = frame_ids_item.no_of_bits,
                                            offset = frame_ids_item.offset,
                                            pid_description = frame_ids_item.pid_description,
                                            resolution = frame_ids_item.resolution,
                                            Selected = frame_ids_item.Selected,
                                            start_bit = frame_ids_item.start_bit,
                                            start_byte = frame_ids_item.start_byte,
                                            unit = frame_ids_item.unit
                                        });
                                        check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
                                        FrameId.Clear();
                                    }
                                    //check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
                                }
                                else
                                {
                                    var Fram = new List<PIDFrameId>();
                                    foreach (var frame_ids_item in item.frame_ids)
                                    {
                                        FrameId.Add(new PIDFrameId
                                        {
                                            FramID = item.frame_id,
                                            bit_coded = frame_ids_item.bit_coded,
                                            @byte = frame_ids_item.@byte,
                                            frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                            message_type = frame_ids_item.message_type,
                                            no_of_bits = frame_ids_item.no_of_bits,
                                            offset = frame_ids_item.offset,
                                            pid_description = frame_ids_item.pid_description,
                                            resolution = frame_ids_item.resolution,
                                            Selected = frame_ids_item.Selected,
                                            start_bit = frame_ids_item.start_bit,
                                            start_byte = frame_ids_item.start_byte,
                                            unit = frame_ids_item.unit
                                        });
                                        Fram.Add(FrameId.FirstOrDefault());
                                        FrameId.Clear();
                                    }

                                    IVNPidList.Add(new IVN_LiveParameterSelectModel
                                    {
                                        frame_name = ecu.ecu_name,
                                        frame_id = item.frame_id,
                                        //frame_ids = item.frame_ids
                                        frame_ids = Fram
                                    });

                                    //Fram.Clear();
                                    //FrameId.Clear();
                                }
                            }


                            viewModel.IVN_PidList = IVNPidList;
                        }

                        collectionView = new CollectionView { ClassId = Convert.ToString(class_id), IsVisible = collectiionVisible };
                        collectionView.SetBinding(ItemsView.ItemsSourceProperty, $"IVN_PidList[{class_id}].frame_ids");

                        collectionView.ItemTemplate = new DataTemplate(() =>
                        {
                            Grid grid = new Grid { Padding = 10 };
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 33 });

                            Label short_name = new Label { Style = (Style)Application.Current.Resources["labelStyleHS"] };
                            short_name.SetBinding(Label.TextProperty, "pid_description");

                            CheckBox selection = new CheckBox { Style = (Style)Application.Current.Resources["CheckBoxStyle"] };
                            selection.SetBinding(CheckBox.IsCheckedProperty, "Selected");

                            grid.Children.Add(short_name, 0, 0);
                            grid.Children.Add(selection, 1, 0);

                            return grid;
                        });

                        collection_view.Children.Add(collectionView, 0, 0);

                        if (string.IsNullOrEmpty(selected_ecu))
                        {
                            selected_ecu = ecu.ecu_name;
                        }

                        class_id++;
                        collectiionVisible = false;
                    }
                    else
                    {
                        // Create Dynamically ECU's Tab
                        if (opacity == -1)
                        {
                            opacity = 1;
                        }
                        else
                        {
                            opacity = 0.5;
                        }

                        ColumnDefinition column = new ColumnDefinition
                        {
                            Width = GridLength.Star,
                        };
                        grd_tab.ColumnDefinitions.Add(column);
                        Grid grd = new Grid
                        {
                            BackgroundColor = color,
                            Opacity = opacity,
                            ClassId = Convert.ToString(class_id),
                        };
                        Label ecu_lbl = new Label
                        {
                            Text = ecu.ecu_name,
                            Style = (Style)this.Resources["txt_tab"]
                        };
                        grd.Children.Add(ecu_lbl);
                        grd_tab.Children.Add(grd, grd_tab.ColumnDefinitions.Count - 1, 0);
                        grd.GestureRecognizers.Add(tab_gesture);

                        //Create Dynamically ECU's PID Data List

                        model.ecu_name = ecu.ecu_name;
                        int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).pid_dataset_id;


                        //viewModel.LiveParameterList = model.roots = viewModel.get_pid(id);

                        if (viewModel.AllPid.Count == 0)
                        {
                            AllPid = viewModel.get_pidValues(id).Result;
                            //////
                            //viewModel.PidList.Add(new LiveParameterSelectModel { ecu_name = ecu.ecu_name, roots = AllPid.FirstOrDefault(x => x.id == id).codes });
                        }
                        else
                        {
                            //////
                            //viewModel.PidList.Add(new LiveParameterSelectModel { ecu_name = ecu.ecu_name, roots = viewModel.AllPid.FirstOrDefault(x => x.id == id).codes });
                        }


                        collectionView = new CollectionView { ClassId = Convert.ToString(class_id), IsVisible = collectiionVisible };
                        collectionView.SetBinding(ItemsView.ItemsSourceProperty, $"PidList[{class_id}].roots");

                        collectionView.ItemTemplate = new DataTemplate(() =>
                        {
                            Grid grid = new Grid { Padding = 10 };
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 33 });

                            Label short_name = new Label { Style = (Style)Application.Current.Resources["labelStyleHS"] };
                            short_name.SetBinding(Label.TextProperty, "short_name");

                            CheckBox selection = new CheckBox { Style = (Style)Application.Current.Resources["CheckBoxStyle"] };
                            selection.SetBinding(CheckBox.IsCheckedProperty, "Selected");

                            grid.Children.Add(short_name, 0, 0);
                            grid.Children.Add(selection, 1, 0);

                            return grid;
                        });

                        collection_view.Children.Add(collectionView, 0, 0);

                        if (string.IsNullOrEmpty(selected_ecu))
                        {

                            selected_ecu = ecu.ecu_name;
                        }

                        class_id++;
                        collectiionVisible = false;
                    }
                }

            }
            catch (Exception ex)
            {
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsPageRemove = true;
            var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
            if (data != null)
            {
                App.selectedOem = JsonConvert.DeserializeObject<AllOemModel>(data);
                color = ThemeManager.GetColorFromHexValue(App.selectedOem.color.ToString());
                //App.Current.Resources["theme_color"] = color;
            }
            //App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            //App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsPageRemove)
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "GoBack",
                        ElementValue = "GoBack",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
            }

            //App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
            //App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (StaticData.ecu_info.FirstOrDefault().read_dtc_index == "IVN")
                {
                    if (string.IsNullOrEmpty(txtSearch.Text))
                    {
                        imgClose.IsVisible = false;
                    }
                    else
                    {
                        imgClose.IsVisible = true;
                        collectionView.ItemsSource = viewModel.IVN_PidList.FirstOrDefault(x => x.frame_name == selected_ecu).frame_ids.Where(x => x.pid_description.ToLower().Contains(e.NewTextValue.ToLower()));// ;;                                  
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(txtSearch.Text))
                    {
                        imgClose.IsVisible = false;
                    }
                    else
                    {
                        imgClose.IsVisible = true;

                        // viewModel.PidList.Where(x => x.long_name.ToLower().Contains(e.NewTextValue.ToLower()) || x.long_name.ToLower().Contains(e.NewTextValue.ToLower()));// ;
                        collectionView.ItemsSource = viewModel.PidList.FirstOrDefault(x => x.ecu_name == selected_ecu).roots.Where(x => x.long_name.ToLower().Contains(e.NewTextValue.ToLower()));// ;;
                        
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void Selection_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool isSelected = true;
                if (btnSelect.IsVisible)
                {
                    btnSelect.IsVisible = false;
                    btnUnselect.IsVisible = true;
                    isSelected = true;
                }
                else
                {
                    btnSelect.IsVisible = true;
                    btnUnselect.IsVisible = false;
                    isSelected = false;
                }

                if (StaticData.ecu_info.FirstOrDefault().read_dtc_index == "IVN")
                {
                    var selected_ecu_list = viewModel.IVN_PidList.FirstOrDefault(x => x.frame_name == selected_ecu).frame_ids;
                    foreach (var pid in selected_ecu_list)
                    {
                        pid.Selected = isSelected;
                    }
                }
                else
                {
                    var selected_ecu_list = viewModel.PidList.FirstOrDefault(x => x.ecu_name == selected_ecu).roots;
                    foreach (var pid in selected_ecu_list)
                    {
                        pid.Selected = isSelected;
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private async void btnContinue_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (StaticData.ecu_info.FirstOrDefault().read_dtc_index == "IVN")
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        ObservableCollection<IVN_SelectedPID> IVNpidList = new ObservableCollection<IVN_SelectedPID>();

                        var selected_pid_list = viewModel.IVN_PidList.FirstOrDefault(x => x.frame_name == selected_ecu);

                        if (selected_pid_list != null)
                        {
                            if (selected_pid_list.frame_ids != null)
                            {
                                var PIDFrameId = new List<PIDFrameId>();
                                var selected_pids = selected_pid_list.frame_ids.Where(x => x.Selected == true);
                                foreach (var item in selected_pids)
                                {
                                    PIDFrameId.Add(new Model.PIDFrameId
                                    {
                                        FramID = item.FramID,
                                        bit_coded = item.bit_coded,
                                        @byte = item.@byte,
                                        frame_of_pid_message = item.frame_of_pid_message,
                                        message_type = item.message_type,
                                        no_of_bits = item.no_of_bits,
                                        offset = item.offset,
                                        pid_description = item.pid_description,
                                        resolution = item.resolution,
                                        Selected = item.Selected,
                                        start_bit = item.start_bit,
                                        start_byte = item.start_byte,
                                        unit = item.unit
                                    });
                                }
                                IVNpidList.Add(new IVN_SelectedPID { frame_id = selected_pid_list.frame_id, frame_ids = PIDFrameId });

                                //var json = JsonConvert.SerializeObject(registerDongleModel);
                            }
                        }

                        if (IVNpidList.Count < 1)
                        {
                            show_alert("Alert", "Please Selected a parameter.", false, true);
                            return;
                        }
                        await Navigation.PushAsync(new IvnLiveParameterSelectedPage(null, IVNpidList, selected_ecu));
                    }
                }
                else
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        ObservableCollection<ReadParameterPID> pidList = new ObservableCollection<ReadParameterPID>();

                        var selected_pid_list = viewModel.PidList.FirstOrDefault(x => x.ecu_name == selected_ecu);

                        if (selected_pid_list != null)
                        {
                            if (selected_pid_list.roots != null)
                            {
                                var selected_pids = selected_pid_list.roots.Where(x => x.Selected == true);
                                foreach (var item in selected_pids)
                                {
                                    var MessageModels = new List<SelectedParameterMessage>();
                                    foreach (var MessageItem in item.messages)
                                    {
                                        MessageModels.Add(new SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
                                    }
                                    pidList.Add(
                                new ReadParameterPID
                                {
                                    pid = item.code,
                                    totalLen = item.code.Length / 2,
                                    //totalbyte -
                                    startByte = item.byte_position,
                                    noOfBytes = item.length,


                                    IsBitcoded = item.bitcoded,
                                    //noofBits = (int?)item.start_bit_position - (int?)item.end_bit_position + 1,
                                    startBit = Convert.ToInt32(item.start_bit_position),
                                    noofBits = item.end_bit_position.GetValueOrDefault() - item.start_bit_position.GetValueOrDefault() + 1,
                                    resolution = item.resolution,
                                    offset = item.offset,

                                    datatype = item.message_type,
                                    //totalBytes = item.length,
                                    pidNumber = item.id,
                                    pidName = item.short_name,
                                    unit = item.unit,

                                    messages = MessageModels,

                                });

                                }
                            }
                        }

                        if (pidList.Count < 1)
                        {
                            show_alert("Alert", "Please Selected a parameter.", false, true);
                            return;
                        }
                        await Navigation.PushAsync(new IvnLiveParameterSelectedPage(pidList, null, selected_ecu));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write("-----------" + ex.Message + "-----------");
            }
        }

        public void show_alert(string title, string message, bool btnCancel, bool btnOk)
        {
            Working = true;
            TitleText = title;
            MessageText = message;
            OkVisible = btnOk;
            CancelVisible = btnCancel;
            CancelCommand = new Command(() =>
            {
                Working = false;
            });
        }


        private void CloseClick(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            imgClose.IsVisible = false;
        }

        private List<Results> allPid;
        public List<Results> AllPid
        {
            get => allPid;
            set
            {
                allPid = value;
            }
        }

        private void Tab_gesture_Tapped(object sender, EventArgs e)
        {
            try
            {
                var sen = sender as Grid;
                if (sen == null) { return; }
                var class_id = sen.ClassId;
                var lbl = ((Label)sen.Children.ElementAt(0));


                for (int i = 0; i < grd_tab.ColumnDefinitions.Count; i++)
                {
                    var grd = ((Grid)grd_tab.Children.ElementAt(i));
                    var collection = ((CollectionView)collection_view.Children.ElementAt(i));
                    if (grd.ClassId == class_id && collection.ClassId == class_id)
                    {
                        grd.Opacity = 1;
                        collection.IsVisible = true;
                        selected_ecu = lbl.Text;
                    }
                    else
                    {
                        grd.Opacity = 0.5;
                        collection.IsVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}