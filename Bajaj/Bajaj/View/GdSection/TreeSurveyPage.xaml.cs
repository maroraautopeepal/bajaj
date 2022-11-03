using Bajaj.Model;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.GdSection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class TreeSurveyPage : DisplayAlertPage
    {
        ResultGD resultGD;
        List<GdImageGD> gd_image = new List<GdImageGD>();
        //List<ReapeterClass> gd_list = new List<ReapeterClass>();
        int less_hieght = 0;
        int first_item = 0;
        //int translate = 10;
        long current_page_id = -1;
        long next_page_id = -1;
        long continue_page_id = -1;
        long gd_decisssion = -1;
        string current_page_type = string.Empty;
        string LoadCode = string.Empty;
        string LoadDescription = string.Empty;
        public TreeSurveyPage(ResultGD resultGD, string Description, string Code)
        {
            try
            {
                InitializeComponent();

                LoadDescription = Description;
                LoadCode = Code;

                BindingContext = this;
                this.resultGD = resultGD;
                Title = Code;
                //this.gd_list = gd_list;
                this.gd_image = resultGD.gd_images;
                if (Xamarin.Forms.Device.Idiom == TargetIdiom.Phone)
                {
                    less_hieght = 82;
                }
                else if (Xamarin.Forms.Device.Idiom == TargetIdiom.Tablet)
                {
                    less_hieght = 89;
                }

                StartGD(resultGD.tree_set[0].tree_data);
                var TreeJson = JsonConvert.SerializeObject(Treelist);

            }
            catch (Exception ex)
            {
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                App.TreeSurveyPage = true;

                App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
                App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (App.TreeSurveyPage && CurrentUserEvent.Instance.IsExpert)
            {
                App.TreeL = true;
                App.DCTPage = true;
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "GoBack",
                    ElementValue = "GoBack",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }

            App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        }
        private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                #region Check Internet Connection
                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    await Task.Delay(100);
                    bool InsternetActive = true;

                    string json = sender as string;
                    if (!string.IsNullOrEmpty(json))
                    {

                    }
                }
                #endregion
            });
        }
        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            if (ReceiveValue.Contains("Decission_Check_Tapped_") && !CurrentUserEvent.Instance.IsExpert)
            {
                string[] Value = { "Decission_Check_Tapped_" };
                string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
                var selectedItem = JsonConvert.DeserializeObject<DecissionModel>(Result[0]);
                if (selectedItem != null)
                {
                    foreach (var item in Treelist)
                    {
                        var decission_data_check = item.decission_list.FirstOrDefault(x => x.id == selectedItem.id);
                        if (decission_data_check != null)
                        {
                            foreach (var decission in item.decission_list)
                            {
                                if (decission.text_value == selectedItem.text_value)
                                {
                                    decission.isCheck = !decission.isCheck;
                                    next_page_id = decission.next_node;
                                    current_page_id = selectedItem.id;
                                    if (selectedItem.text_value == "NOT OK")
                                    {
                                        continue_page_id = decission.next_node - 1;
                                    }
                                }
                                else
                                {
                                    decission.isCheck = false;
                                }
                            }
                        }
                    }
                }
            }
            else if (ReceiveValue.Contains("Checked_OK_Or_Not_Clicked_BTN_") && !CurrentUserEvent.Instance.IsExpert)
            {
                var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
                if (next_page_id < 0)
                {
                    show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
                    //return;
                }
                else
                {
                    if (current_page_id != next_page_id)
                    {
                        GdList.ItemsSource = "";
                        if (next_page != null)
                        {
                            next_page.page_visible = true;
                            next_page.view_height = App.ScreenHeight - less_hieght;
                            current_page_type = next_page.group_name;
                        }

                        var list = Treelist.First(x => x.id == current_page_id);
                        if (list != null)
                        {
                            list.page_visible = false;
                            list.view_height = 0;
                        }
                        current_page_id = next_page_id;

                        //await Task.Delay(50000);
                        GdList.ItemsSource = Treelist;
                    }
                    else
                    {
                        show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
                    }
                    //GdList.ItemsSource = Treelist;
                    //GdList.HeightRequest = App.ScreenHeight-150;
                }
            }
            else if (ReceiveValue.Contains("Root_Cause_Check_Box_Checked_") && !CurrentUserEvent.Instance.IsExpert)
            {
                string[] Value = { "Root_Cause_Check_Box_Checked_" };
                string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
                var selectedItem = JsonConvert.DeserializeObject<LastQueCheckModel>(Result[0]);
                if (selectedItem != null)
                {
                    var group_list = Treelist.First(x => x.id == current_page_id);
                    foreach (var item in group_list.last_question_list.ToList())
                    {
                        if (item.id != selectedItem.id)
                        {
                            item.isCheck = false;
                        }
                        else
                        {
                            item.isCheck = !item.isCheck;
                            gd_decisssion = item.id;
                        }
                    }
                }
            }
            else if (ReceiveValue.Contains("Root_Cause_Next_BTN") && !CurrentUserEvent.Instance.IsExpert)
            {
                if (gd_decisssion == 1)
                {
                    var next_page = Treelist.FirstOrDefault(x => x.id == continue_page_id);
                    if (next_page == null)
                    {
                        return;
                    }
                    else
                    {

                        if (continue_page_id != current_page_id)
                        {
                            GdList.ItemsSource = "";
                            if (next_page != null)
                            {
                                next_page.page_visible = true;
                                next_page.view_height = App.ScreenHeight - less_hieght;
                                current_page_type = next_page.group_name;

                                current_page_id = next_page_id;
                            }

                            var list = Treelist.First(x => x.id == current_page_id);
                            if (list != null)
                            {
                                list.page_visible = false;
                                list.view_height = 0;
                            }
                            GdList.ItemsSource = Treelist;
                            next_page_id = current_page_id = next_page.id;
                        }
                        else
                        {
                            show_alert("Alert", "This is last page", false, true);
                        }


                        //await Task.Delay(50000);
                    }

                }
                else if (gd_decisssion == 2)
                {
                    //StartGD(resultGD.tree_set[0].tree_data);
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            //StartGD(resultGD.tree_set[0].tree_data);
                            //await this.Navigation.PushAsync(new TreeSurveyPage(resultGD, LoadDescription, LoadCode));
                            await Navigation.PopAsync();
                            break;
                        case Device.UWP:
                            //StartGD(resultGD.tree_set[0].tree_data);
                            //await this.Navigation.PushAsync(new TreeSurveyPage(resultGD, LoadDescription, LoadCode));
                            await Navigation.PopAsync();
                            break;
                        default:
                            break;
                    }
                }
                else if (gd_decisssion == 3)
                {
                    var Value = App.JCM;
                    for (var i = 1; i < 2; i++)
                    {
                        Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                    }
                    await Navigation.PopAsync();
                }
            }
            else if (ReceiveValue.Contains("btnGroupNext_Checked_") && !CurrentUserEvent.Instance.IsExpert)
            {
                string[] Value = { "btnGroupNext_Checked_" };
                string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
                var group_list = JsonConvert.DeserializeObject<TreeListModel>(Result[0]);
                if (group_list != null)
                {
                    if (current_page_type == "GroupData")
                    {
                        var IsNullData = group_list.group_list.FirstOrDefault(x => x.current_limit == null || x.current_limit == "");
                        if (IsNullData == null)
                        {
                            foreach (var group_list_item in group_list.group_list)
                            {
                                if (Convert.ToDouble(group_list_item.upper_limit) >= Convert.ToDouble(group_list_item.current_limit)
                                && Convert.ToDouble(group_list_item.lower_limit) <= Convert.ToDouble(group_list_item.current_limit))
                                {
                                    group_list_item.status_color = "#00b800";
                                    group_list_item.upper_lower_value_visible = true;
                                }
                                else
                                {
                                    group_list_item.status_color = "#FF0000";
                                    group_list_item.upper_lower_value_visible = true;
                                }

                            }

                            var item = group_list.group_list.FirstOrDefault(x => x.status_color == "#FF0000");
                            if (item == null)
                            {
                                next_page_id = group_list.ok_page_node_id;
                                //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
                                //next_page_id = group_list.not_ok_page_node_id;
                                current_page_type = group_list.group_name;
                            }
                            else
                            {

                                next_page_id = group_list.not_ok_page_node_id;
                                continue_page_id = group_list.ok_page_node_id;
                            }

                            //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);



                            await Task.Delay(4000);

                            foreach (var item1 in Treelist)
                            {
                                if (item1.id == next_page_id)
                                {
                                    item1.page_visible = true;
                                    item1.view_height = App.ScreenHeight - less_hieght;
                                }
                                else
                                {
                                    item1.page_visible = false;
                                    item1.view_height = 0;
                                }
                            }

                        }
                        else
                        {
                            show_alert("Alert", "Please fill all fields", false, true);
                        }
                    }
                    current_page_id = next_page_id;
                }
            }
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        public void StartGD(List<TreeDataGD> _gd_list)
        {
            try
            {
                first_item = 0;
                //translate = 10;
                current_page_id = -1;
                next_page_id = -1;
                gd_decisssion = -1;
                continue_page_id = -1;
                current_page_type = string.Empty;
                GdList.ItemsSource = null;
                Treelist = new List<TreeListModel>();

                foreach (var item in _gd_list)
                {
                    TreeListModel listModel = new TreeListModel();
                    listModel.id = item.id;
                    listModel.description = item.data.description;
                    listModel.topic = item.data.type_form.topic;

                    if (item.data.type_form.topic.Contains("Root Cause"))
                    {
                        listModel.description_background_color = "#FF0000";
                        listModel.description_text_color = "#FFFFFF";
                    }
                    else
                    {
                        listModel.description_background_color = "#FFFFFF";
                        listModel.description_text_color = "#4d4d4d";
                    }
                    //if (Xamarin.Forms.Device.Idiom == TargetIdiom.Phone)
                    //{
                    //    listModel.scroll_height = App.ScreenHeight - 90 - 50;
                    //}
                    //else if (Xamarin.Forms.Device.Idiom == TargetIdiom.Tablet)
                    //{
                    //    listModel.scroll_height = App.ScreenHeight - 90 - 60;
                    //}

                    if (item.data.decisions.data != null || item.data.decisions.data.Count <= 0)
                    {
                        foreach (var decission_data_item in item.data.decisions.data)
                        {
                            DecissionModel decissionModel = new DecissionModel();

                            decissionModel.isCheck = false;
                            if (decission_data_item.text_val == "OK" || decission_data_item.text_val == "ok" || decission_data_item.text_val == "true" || decission_data_item.text_val == "OK ")
                            {
                                decissionModel.text_value = "OK";
                                decissionModel.id = item.id;
                            }
                            else
                            {
                                decissionModel.text_value = "NOT OK";
                                decissionModel.id = item.id;
                            }
                            decissionModel.next_node = decission_data_item.node;
                            decissionModel.type = decission_data_item.type;
                            listModel.decission_list.Add(decissionModel);
                        }
                    }

                    if (string.IsNullOrEmpty(item.data.decisions.type))
                    {
                        if (item.data.decisions.data.Count > 0)
                        {
                            if (string.IsNullOrEmpty(item.data.decisions.data[0].type))
                            {
                                listModel.group_name = GetGroupName(item.data.decisions.type);
                                listModel.ok_page_node_id = item.data.decisions.data[0].node;
                                listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
                                if (listModel.group_name == "GroupData")
                                {
                                    foreach (var group_item in item.data.type_form.groups.ToList())
                                    {
                                        GroupListModel groupListModel = new GroupListModel();

                                        groupListModel.entry_description = group_item.entry_description;
                                        groupListModel.group_name = group_item.group_name;
                                        groupListModel.lower_limit = group_item.lower_limit;
                                        groupListModel.upper_limit = group_item.upper_limit;
                                        groupListModel.unit = group_item.unit;
                                        listModel.group_list.Add(groupListModel);
                                        //GroupList.Add(groupListModel);
                                    }

                                }
                                //listModel.ok_level_text = item.data.decisions.data[0].text_val;
                                //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
                            }
                            else
                            {
                                listModel.group_name = GetGroupName(item.data.decisions.data[0].type);
                                listModel.ok_page_node_id = item.data.decisions.data[0].node;
                                listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
                                //listModel.ok_level_text = item.data.decisions.data[0].text_val;
                                //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
                                if (listModel.group_name == "GroupData")
                                {
                                    foreach (var group_item in item.data.type_form.groups.ToList())
                                    {
                                        GroupListModel groupListModel = new GroupListModel();

                                        groupListModel.entry_description = group_item.entry_description;
                                        groupListModel.group_name = group_item.group_name;
                                        groupListModel.lower_limit = group_item.lower_limit;
                                        groupListModel.upper_limit = group_item.upper_limit;
                                        groupListModel.unit = group_item.unit;
                                        listModel.group_list.Add(groupListModel);
                                        //GroupList.Add(groupListModel);
                                    }

                                }
                            }
                        }
                        else
                        {
                            listModel.group_name = GetGroupName("LastData");
                            listModel.ok_page_node_id = -1;
                            listModel.not_ok_page_node_id = -1;
                            if (item.data.type_form.topic.Contains("All Ok") || _gd_list.Count == item.id)
                            {
                                listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
                                {
                                    //new LastQueCheckModel
                                    //{
                                    //    describe = "Continue to next step",
                                    //    id = 1,
                                    //    isCheck = false,
                                    //},
                                    new LastQueCheckModel
                                    {
                                        describe = "Restart GD",
                                        id = 2,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Quit GD",
                                        id = 3,
                                        isCheck = false,
                                    },
                                };
                            }
                            else
                            {
                                listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
                                {
                                    new LastQueCheckModel
                                    {
                                        describe = "Continue to next step",
                                        id = 1,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Restart GD",
                                        id = 2,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Quit GD",
                                        id = 3,
                                        isCheck = false,
                                    },
                                };
                            }
                        }
                    }
                    else
                    {
                        if (item.data.decisions.data.Count > 0)
                        {
                            listModel.ok_page_node_id = item.data.decisions.data[0].node;
                            listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
                            listModel.group_name = GetGroupName(item.data.decisions.type);
                            //listModel.ok_level_text = item.data.decisions.data[0].text_val;
                            //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
                        }
                        else
                        {
                            listModel.ok_page_node_id = -1;
                            listModel.not_ok_page_node_id = -1;
                            listModel.group_name = GetGroupName("LastData");
                            if (item.data.type_form.topic.Contains("All Ok") || _gd_list.Count == item.id)
                            {
                                listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
                                {
                                    //new LastQueCheckModel
                                    //{
                                    //    describe = "Continue to next step",
                                    //    id = 1,
                                    //    isCheck = false,
                                    //},
                                    new LastQueCheckModel
                                    {
                                        describe = "Restart GD",
                                        id = 2,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Quit GD",
                                        id = 3,
                                        isCheck = false,
                                    },
                                };
                            }
                            else
                            {
                                listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
                                {
                                    new LastQueCheckModel
                                    {
                                        describe = "Continue to next step",
                                        id = 1,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Restart GD",
                                        id = 2,
                                        isCheck = false,
                                    },
                                    new LastQueCheckModel
                                    {
                                        describe = "Quit GD",
                                        id = 3,
                                        isCheck = false,
                                    },
                                };
                            }
                            //listModel.ok_level_text = "";
                            //listModel.not_ok_level_text = "";
                        }

                    }

                    first_item++;
                    if (first_item == 1)
                    {
                        listModel.page_visible = true;
                        listModel.view_height = App.ScreenHeight - less_hieght;
                        current_page_type = listModel.group_name;
                    }
                    else
                    {
                        listModel.page_visible = false;
                        listModel.view_height = 0;
                    }
                    Treelist.Add(listModel);
                }
                GdList.ItemsSource = Treelist;
            }
            catch (Exception ex)
            {

            }
        }

        public void InitializeLastDataCheckList()
        { }

        private List<TreeListModel> treeList;
        public List<TreeListModel> Treelist
        {
            get => treeList;
            set
            {
                treeList = value;
                OnPropertyChanged();
            }
        }

        public string GetGroupName(string type)
        {
            string GroupName = string.Empty;
            switch (type)
            {
                case "radio":
                    GroupName = "RadioData";
                    break;

                case "static":
                    GroupName = "GroupData";
                    break;

                case "":
                    GroupName = "SimpleData";
                    break;

                case "LastData":
                    GroupName = "LastData";
                    break;

                default:
                    Console.WriteLine("Invalid selection. Please select 1, 2, or 3.");
                    break;
            }

            return GroupName;

        }

        private void BtnStart_Clicked(object sender, EventArgs e)
        {

        }

        private void CheckOk_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                CheckBox item = (CheckBox)sender;

                if (item == null) return;
                //GdList.ItemsSource = null;
                var class_id = item.ClassId;
                var selectedItem = (TreeListModel)((CheckBox)sender).BindingContext;

                if (class_id == "Ok")
                {
                    next_page_id = selectedItem.ok_page_node_id;
                    current_page_id = selectedItem.id;
                    //current_page_type = selectedItem.group_name;
                }
                else if (class_id == "NotOk")
                {
                    next_page_id = selectedItem.not_ok_page_node_id;
                    current_page_id = selectedItem.id;
                    continue_page_id = selectedItem.ok_page_node_id;
                    //current_page_type = selectedItem.group_name;
                }
                //GdList.ItemsSource = Treelist;
            }
            catch (Exception ex)
            {
            }
        }

        //OK Or Not Check Box Checked
        private void Decission_Check_Tapped(object sender, EventArgs e)
        {
            try
            {
                var selectedItem = (DecissionModel)((Grid)sender).BindingContext;
                foreach (var item in Treelist)
                {
                    var decission_data_check = item.decission_list.FirstOrDefault(x => x.id == selectedItem.id);
                    if (decission_data_check != null)
                    {
                        foreach (var decission in item.decission_list)
                        {
                            if (decission.text_value == selectedItem.text_value)
                            {
                                decission.isCheck = !decission.isCheck;
                                next_page_id = decission.next_node;
                                current_page_id = selectedItem.id;
                                if (selectedItem.text_value == "NOT OK")
                                {
                                    continue_page_id = decission.next_node - 1;
                                }
                            }
                            else
                            {
                                decission.isCheck = false;
                            }
                        }
                    }

                    #region Old Code
                    //if(decission_data_check!=null)
                    //{
                    //    decission_data_check.isCheck = !decission_data_check.isCheck;
                    //    next_page_id = decission_data_check.next_node;
                    //    current_page_id = item.id;
                    //}

                    //foreach(var decission_data_check in item.decission_list)
                    //{
                    //    if(decission_data_check.id == selectedItem.id)
                    //    {
                    //        decission_data_check.isCheck = !decission_data_check.isCheck;
                    //        next_page_id = decission_data_check.next_node;
                    //        current_page_id = item.id;
                    //    }
                    //    else
                    //    {

                    //    }
                    //}

                    //var decission_data_check = item.decission_list.First(x => x.id == selectedItem.id);
                    //if (decission_data_check != null)
                    //{
                    //    decission_data_check.isCheck = !decission_data_check.isCheck;
                    //    next_page_id = decission_data_check.next_node;
                    //    current_page_id = item.id;
                    //}

                    //var decission_data_uncheck = item.decission_list.First(x => x.text_value == selectedItem.text_value);
                    //if (decission_data_uncheck != null)
                    //{
                    //    decission_data_uncheck.isCheck = !decission_data_uncheck.isCheck;
                    //    next_page_id = continue_page_id = decission_data_uncheck.next_node;
                    //    current_page_id = item.id;
                    //    //continue_page_id = selectedItem.ok_page_node_id;
                    //}
                    #endregion
                }

                if (CurrentUserEvent.Instance.IsExpert)
                {
                    var JsonData = JsonConvert.SerializeObject(selectedItem);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "Decission_Checked",
                        ElementValue = "Decission_Check_Tapped_" + JsonData,
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
            }
            catch (Exception ex)
            { }
        }

        //Check Box OK OR Not Clicked Next BTN
        private async void btnNext_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "Checked_OK_Or_NotOK",
                        ElementValue = "Checked_OK_Or_Not_Clicked_BTN_",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
                if (next_page_id < 0)
                {
                    show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
                    //return;
                }
                else
                {
                    if (current_page_id != next_page_id)
                    {
                        GdList.ItemsSource = "";
                        if (next_page != null)
                        {
                            next_page.page_visible = true;
                            next_page.view_height = App.ScreenHeight - less_hieght;
                            current_page_type = next_page.group_name;
                        }

                        var list = Treelist.First(x => x.id == current_page_id);
                        if (list != null)
                        {
                            list.page_visible = false;
                            list.view_height = 0;
                        }
                        current_page_id = next_page_id;

                        //await Task.Delay(50000);
                        GdList.ItemsSource = Treelist;
                    }
                    else
                    {
                        show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
                    }
                    //GdList.ItemsSource = Treelist;
                    //GdList.HeightRequest = App.ScreenHeight-150;
                }
            }
            catch (Exception ex)
            {
                show_alert("Alert", $"{ex.Message} \n\n\n {ex.StackTrace}", false, true);
            }
        }

        private void btnGroupNext_Clicked(object sender, EventArgs e)
        {
            //bool is_empty_value = false;
            Task.Run(async () =>
            {
                // Run code here

                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {

                        bool IsWrongValue = false;
                        //GdList.ItemsSource = "";
                        if (current_page_type == "GroupData")
                        {
                            var group_list = Treelist.First(x => x.id == current_page_id);
                            var IsNullData = group_list.group_list.FirstOrDefault(x => x.current_limit == null || x.current_limit == "");
                            if (IsNullData == null)
                            {
                                foreach (var group_list_item in group_list.group_list)
                                {
                                    if (Convert.ToDouble(group_list_item.upper_limit) >= Convert.ToDouble(group_list_item.current_limit)
                                        && Convert.ToDouble(group_list_item.lower_limit) <= Convert.ToDouble(group_list_item.current_limit))
                                    {
                                        group_list_item.status_color = "#00b800";
                                        group_list_item.upper_lower_value_visible = true;
                                    }
                                    else
                                    {
                                        group_list_item.status_color = "#FF0000";
                                        group_list_item.upper_lower_value_visible = true;
                                    }

                                }

                                var item = group_list.group_list.FirstOrDefault(x => x.status_color == "#FF0000");
                                if (item == null)
                                {
                                    next_page_id = group_list.ok_page_node_id;
                                    //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
                                    //next_page_id = group_list.not_ok_page_node_id;
                                    current_page_type = group_list.group_name;
                                }
                                else
                                {

                                    next_page_id = group_list.not_ok_page_node_id;
                                    continue_page_id = group_list.ok_page_node_id;
                                }

                                //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);



                                await Task.Delay(4000);

                                foreach (var item1 in Treelist)
                                {
                                    if (item1.id == next_page_id)
                                    {
                                        item1.page_visible = true;
                                        item1.view_height = App.ScreenHeight - less_hieght;
                                    }
                                    else
                                    {
                                        item1.page_visible = false;
                                        item1.view_height = 0;
                                    }
                                }

                            }
                            else
                            {
                                show_alert("Alert", "Please fill all fields", false, true);
                            }

                            if (CurrentUserEvent.Instance.IsExpert)
                            {
                                var JsonData = JsonConvert.SerializeObject(group_list);
                                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                                {
                                    ElementName = "GroupNext",
                                    ElementValue = "btnGroupNext_Checked_" + JsonData,
                                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                                    IsExpert = CurrentUserEvent.Instance.IsExpert
                                });
                            }
                        }
                        current_page_id = next_page_id;
                    }
                    catch (Exception ex)
                    {
                    }
                });
                //await Task.Delay(10000);
                //GdList.ItemsSource = Treelist;
            }).ConfigureAwait(false);

        }

        // Root Cause Check Box is Checked BTN
        private async void btnLastPageNext_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "Checked_OK_Or_NotOK",
                        ElementValue = "Root_Cause_Next_BTN",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                if (gd_decisssion == 1)
                {

                    var next_page = Treelist.FirstOrDefault(x => x.id == continue_page_id);
                    if (next_page == null)
                    {
                        return;
                    }
                    else
                    {

                        if (continue_page_id != current_page_id)
                        {
                            GdList.ItemsSource = "";
                            if (next_page != null)
                            {
                                next_page.page_visible = true;
                                next_page.view_height = App.ScreenHeight - less_hieght;
                                current_page_type = next_page.group_name;

                                current_page_id = next_page_id;
                            }

                            var list = Treelist.First(x => x.id == current_page_id);
                            if (list != null)
                            {
                                list.page_visible = false;
                                list.view_height = 0;
                            }
                            GdList.ItemsSource = Treelist;
                            next_page_id = current_page_id = next_page.id;
                        }
                        else
                        {
                            show_alert("Alert", "This is last page", false, true);
                        }


                        //await Task.Delay(50000);
                    }


                }
                else if (gd_decisssion == 2)
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            //StartGD(resultGD.tree_set[0].tree_data);
                            //this.Navigation.PushAsync(new TreeSurveyPage(resultGD, LoadDescription, LoadCode));
                            await Navigation.PopAsync();
                            break;
                        case Device.UWP:
                            //StartGD(resultGD.tree_set[0].tree_data);
                            //this.Navigation.PushAsync(new TreeSurveyPage(resultGD, LoadDescription, LoadCode));
                            await Navigation.PopAsync();
                            break;
                        default:
                            break;
                    }
                    //StartGD(resultGD.tree_set[0].tree_data);
                }
                else if (gd_decisssion == 3)
                {
                    var Value = App.JCM;
                    //await Navigation.PushAsync(new DtcListPage(Value));
                    await Navigation.PushAsync(new DtcListPage());

                    //for (var i = 1; i < 3; i++)
                    //{
                    //    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                    //}
                    //await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

        //Root Cause Check Box Checked
        private void Check_Tapped(object sender, EventArgs e)
        {
            try
            {
                var selectedItem = (LastQueCheckModel)((Grid)sender).BindingContext;
                var group_list = Treelist.First(x => x.id == current_page_id);
                foreach (var item in group_list.last_question_list.ToList())
                {
                    if (item.id != selectedItem.id)
                    {
                        item.isCheck = false;
                    }
                    else
                    {
                        item.isCheck = !item.isCheck;
                        gd_decisssion = item.id;
                    }
                }

                if (CurrentUserEvent.Instance.IsExpert)
                {
                    var JsonData = JsonConvert.SerializeObject(selectedItem);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "Decission_Checked",
                        ElementValue = "Root_Cause_Check_Box_Checked_" + JsonData,
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
            }
            catch (Exception ex)
            { }
        }


        private async void MenuItem1_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "MenuItem1",
                        ElementValue = "MenuItem1_Clicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                this.Navigation.PushAsync(new GdImagePage(gd_image, Title));
            }
            catch (Exception ex)
            {
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
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

        private void Current_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var item = ((Entry)sender).BindingContext;
        }
    }

    #region Old
    //public partial class OldTreeSurveyPage : DisplayAlertPage
    //{
    //    ResultGD resultGD;
    //    List<GdImageGD> gd_image = new List<GdImageGD>();
    //    //List<ReapeterClass> gd_list = new List<ReapeterClass>();
    //    int less_hieght = 0;
    //    int first_item = 0;
    //    //int translate = 10;
    //    long current_page_id = -1;
    //    long next_page_id = -1;
    //    long continue_page_id = -1;
    //    long gd_decisssion = -1;
    //    string current_page_type = string.Empty;
    //    string LoadCode = string.Empty;
    //    string LoadDescription = string.Empty;
    //    public TreeSurveyPage(ResultGD resultGD, string Description, string Code)
    //    {
    //        try
    //        {
    //            InitializeComponent();

    //            LoadDescription = Description;
    //            LoadCode = Code;

    //            BindingContext = this;
    //            this.resultGD = resultGD;
    //            Title = Code;
    //            //this.gd_list = gd_list;
    //            this.gd_image = resultGD.gd_images;
    //            if (Xamarin.Forms.Device.Idiom == TargetIdiom.Phone)
    //            {
    //                less_hieght = 82;
    //            }
    //            else if (Xamarin.Forms.Device.Idiom == TargetIdiom.Tablet)
    //            {
    //                less_hieght = 89;
    //            }

    //            StartGD(resultGD.tree_set[0].tree_data);
    //            var TreeJson = JsonConvert.SerializeObject(Treelist);

    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }
    //    protected override void OnAppearing()
    //    {
    //        base.OnAppearing();
    //        try
    //        {
    //            App.TreeSurveyPage = true;

    //            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
    //            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }
    //    protected override void OnDisappearing()
    //    {
    //        base.OnDisappearing();

    //        if (App.TreeSurveyPage && CurrentUserEvent.Instance.IsExpert)
    //        {
    //            App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //            {
    //                ElementName = "GoBack",
    //                ElementValue = "GoBack",
    //                ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                IsExpert = CurrentUserEvent.Instance.IsExpert
    //            });
    //        }

    //        App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
    //        App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
    //    }
    //    private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
    //    {
    //        Device.BeginInvokeOnMainThread(async () =>
    //        {
    //            #region Check Internet Connection
    //            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
    //            {
    //                await Task.Delay(100);
    //                bool InsternetActive = true;

    //                string json = sender as string;
    //                if (!string.IsNullOrEmpty(json))
    //                {

    //                }
    //            }
    //            #endregion
    //        });
    //    }
    //    public string ReceiveValue = string.Empty;
    //    private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
    //    {
    //        var elementEventHandler = (sender as ElementEventHandler);
    //        this.ReceiveValue = elementEventHandler.ElementValue;
    //        if (ReceiveValue.Contains("Decission_Check_Tapped_") && !CurrentUserEvent.Instance.IsExpert)
    //        {
    //            string[] Value = { "Decission_Check_Tapped_" };
    //            string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
    //            var selectedItem = JsonConvert.DeserializeObject<DecissionModel>(Result[0]);
    //            if (selectedItem != null)
    //            {
    //                foreach (var item in Treelist)
    //                {
    //                    var decission_data_check = item.decission_list.FirstOrDefault(x => x.id == selectedItem.id);
    //                    if (decission_data_check != null)
    //                    {
    //                        foreach (var decission in item.decission_list)
    //                        {
    //                            if (decission.text_value == selectedItem.text_value)
    //                            {
    //                                decission.isCheck = !decission.isCheck;
    //                                next_page_id = decission.next_node;
    //                                current_page_id = selectedItem.id;
    //                                if (selectedItem.text_value == "NOT OK")
    //                                {
    //                                    continue_page_id = decission.next_node - 1;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                decission.isCheck = false;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else if (ReceiveValue.Contains("Checked_OK_Or_Not_Clicked_BTN_") && !CurrentUserEvent.Instance.IsExpert)
    //        {
    //            var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
    //            if (next_page_id < 0)
    //            {
    //                show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
    //                //return;
    //            }
    //            else
    //            {
    //                if (current_page_id != next_page_id)
    //                {
    //                    GdList.ItemsSource = "";
    //                    if (next_page != null)
    //                    {
    //                        next_page.page_visible = true;
    //                        next_page.view_height = App.ScreenHeight - less_hieght;
    //                        current_page_type = next_page.group_name;
    //                    }

    //                    var list = Treelist.First(x => x.id == current_page_id);
    //                    if (list != null)
    //                    {
    //                        list.page_visible = false;
    //                        list.view_height = 0;
    //                    }
    //                    current_page_id = next_page_id;

    //                    //await Task.Delay(50000);
    //                    GdList.ItemsSource = Treelist;
    //                }
    //                else
    //                {
    //                    show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
    //                }
    //                //GdList.ItemsSource = Treelist;
    //                //GdList.HeightRequest = App.ScreenHeight-150;
    //            }
    //        }
    //        else if (ReceiveValue.Contains("Root_Cause_Check_Box_Checked_") && !CurrentUserEvent.Instance.IsExpert)
    //        {
    //            string[] Value = { "Root_Cause_Check_Box_Checked_" };
    //            string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
    //            var selectedItem = JsonConvert.DeserializeObject<LastQueCheckModel>(Result[0]);
    //            if (selectedItem != null)
    //            {
    //                var group_list = Treelist.First(x => x.id == current_page_id);
    //                foreach (var item in group_list.last_question_list.ToList())
    //                {
    //                    if (item.id != selectedItem.id)
    //                    {
    //                        item.isCheck = false;
    //                    }
    //                    else
    //                    {
    //                        item.isCheck = !item.isCheck;
    //                        gd_decisssion = item.id;
    //                    }
    //                }
    //            }
    //        }
    //        else if (ReceiveValue.Contains("Root_Cause_Next_BTN") && !CurrentUserEvent.Instance.IsExpert)
    //        {
    //            if (gd_decisssion == 1)
    //            {
    //                var next_page = Treelist.FirstOrDefault(x => x.id == continue_page_id);
    //                if (next_page == null)
    //                {
    //                    return;
    //                }
    //                else
    //                {

    //                    if (continue_page_id != current_page_id)
    //                    {
    //                        GdList.ItemsSource = "";
    //                        if (next_page != null)
    //                        {
    //                            next_page.page_visible = true;
    //                            next_page.view_height = App.ScreenHeight - less_hieght;
    //                            current_page_type = next_page.group_name;

    //                            current_page_id = next_page_id;
    //                        }

    //                        var list = Treelist.First(x => x.id == current_page_id);
    //                        if (list != null)
    //                        {
    //                            list.page_visible = false;
    //                            list.view_height = 0;
    //                        }
    //                        GdList.ItemsSource = Treelist;
    //                        next_page_id = current_page_id = next_page.id;
    //                    }
    //                    else
    //                    {
    //                        show_alert("Alert", "This is last page", false, true);
    //                    }


    //                    //await Task.Delay(50000);
    //                }

    //            }
    //            else if (gd_decisssion == 2)
    //            {
    //                StartGD(resultGD.tree_set[0].tree_data);
    //            }
    //            else if (gd_decisssion == 3)
    //            {
    //                App.TreeSurveyPage = false;
    //                for (var i = 1; i < 3; i++)
    //                {
    //                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
    //                }
    //                await Navigation.PopAsync();                    
    //            }
    //        }
    //        else if (ReceiveValue.Contains("btnGroupNext_Checked_") && !CurrentUserEvent.Instance.IsExpert)
    //        {
    //            string[] Value = { "btnGroupNext_Checked_" };
    //            string[] Result = ReceiveValue.Split(Value, StringSplitOptions.RemoveEmptyEntries);
    //            var group_list = JsonConvert.DeserializeObject<TreeListModel>(Result[0]);
    //            if (group_list != null)
    //            {
    //                if (current_page_type == "GroupData")
    //                {
    //                    var IsNullData = group_list.group_list.FirstOrDefault(x => x.current_limit == null || x.current_limit == "");
    //                    if (IsNullData == null)
    //                    {
    //                        foreach (var group_list_item in group_list.group_list)
    //                        {
    //                            if (Convert.ToDouble(group_list_item.upper_limit) >= Convert.ToDouble(group_list_item.current_limit)
    //                            && Convert.ToDouble(group_list_item.lower_limit) <= Convert.ToDouble(group_list_item.current_limit))
    //                            {
    //                                group_list_item.status_color = "#00b800";
    //                                group_list_item.upper_lower_value_visible = true;
    //                            }
    //                            else
    //                            {
    //                                group_list_item.status_color = "#FF0000";
    //                                group_list_item.upper_lower_value_visible = true;
    //                            }

    //                        }

    //                        var item = group_list.group_list.FirstOrDefault(x => x.status_color == "#FF0000");
    //                        if (item == null)
    //                        {
    //                            next_page_id = group_list.ok_page_node_id;
    //                            //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
    //                            //next_page_id = group_list.not_ok_page_node_id;
    //                            current_page_type = group_list.group_name;
    //                        }
    //                        else
    //                        {

    //                            next_page_id = group_list.not_ok_page_node_id;
    //                            continue_page_id = group_list.ok_page_node_id;
    //                        }

    //                        //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);



    //                        await Task.Delay(4000);

    //                        foreach (var item1 in Treelist)
    //                        {
    //                            if (item1.id == next_page_id)
    //                            {
    //                                item1.page_visible = true;
    //                                item1.view_height = App.ScreenHeight - less_hieght;
    //                            }
    //                            else
    //                            {
    //                                item1.page_visible = false;
    //                                item1.view_height = 0;
    //                            }
    //                        }

    //                    }
    //                    else
    //                    {
    //                        show_alert("Alert", "Please fill all fields", false, true);
    //                    }
    //                }
    //                current_page_id = next_page_id;
    //            }
    //        }
    //        App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
    //    }

    //    public void StartGD(List<TreeDataGD> _gd_list)
    //    {
    //        try
    //        {
    //            first_item = 0;
    //            //translate = 10;
    //            current_page_id = -1;
    //            next_page_id = -1;
    //            gd_decisssion = -1;
    //            continue_page_id = -1;
    //            current_page_type = string.Empty;
    //            GdList.ItemsSource = null;
    //            Treelist = new List<TreeListModel>();

    //            foreach (var item in _gd_list)
    //            {
    //                TreeListModel listModel = new TreeListModel();
    //                listModel.id = item.id;
    //                listModel.description = item.data.description;
    //                listModel.topic = item.data.type_form.topic;

    //                if (item.data.type_form.topic.Contains("Root Cause"))
    //                {
    //                    listModel.description_background_color = "#FF0000";
    //                    listModel.description_text_color = "#FFFFFF";
    //                }
    //                else
    //                {
    //                    listModel.description_background_color = "#FFFFFF";
    //                    listModel.description_text_color = "#4d4d4d";
    //                }
    //                //if (Xamarin.Forms.Device.Idiom == TargetIdiom.Phone)
    //                //{
    //                //    listModel.scroll_height = App.ScreenHeight - 90 - 50;
    //                //}
    //                //else if (Xamarin.Forms.Device.Idiom == TargetIdiom.Tablet)
    //                //{
    //                //    listModel.scroll_height = App.ScreenHeight - 90 - 60;
    //                //}

    //                if (item.data.decisions.data != null || item.data.decisions.data.Count <= 0)
    //                {
    //                    foreach (var decission_data_item in item.data.decisions.data)
    //                    {
    //                        DecissionModel decissionModel = new DecissionModel();

    //                        decissionModel.isCheck = false;
    //                        if (decission_data_item.text_val == "OK" || decission_data_item.text_val == "ok" || decission_data_item.text_val == "true" || decission_data_item.text_val == "OK ")
    //                        {
    //                            decissionModel.text_value = "OK";
    //                            decissionModel.id = item.id;
    //                        }
    //                        else
    //                        {
    //                            decissionModel.text_value = "NOT OK";
    //                            decissionModel.id = item.id;
    //                        }
    //                        decissionModel.next_node = decission_data_item.node;
    //                        decissionModel.type = decission_data_item.type;
    //                        listModel.decission_list.Add(decissionModel);
    //                    }
    //                }

    //                if (string.IsNullOrEmpty(item.data.decisions.type))
    //                {
    //                    if (item.data.decisions.data.Count > 0)
    //                    {
    //                        if (string.IsNullOrEmpty(item.data.decisions.data[0].type))
    //                        {
    //                            listModel.group_name = GetGroupName(item.data.decisions.type);
    //                            listModel.ok_page_node_id = item.data.decisions.data[0].node;
    //                            listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
    //                            if (listModel.group_name == "GroupData")
    //                            {
    //                                foreach (var group_item in item.data.type_form.groups.ToList())
    //                                {
    //                                    GroupListModel groupListModel = new GroupListModel();

    //                                    groupListModel.entry_description = group_item.entry_description;
    //                                    groupListModel.group_name = group_item.group_name;
    //                                    groupListModel.lower_limit = group_item.lower_limit;
    //                                    groupListModel.upper_limit = group_item.upper_limit;
    //                                    groupListModel.unit = group_item.unit;
    //                                    listModel.group_list.Add(groupListModel);
    //                                    //GroupList.Add(groupListModel);
    //                                }

    //                            }
    //                            //listModel.ok_level_text = item.data.decisions.data[0].text_val;
    //                            //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
    //                        }
    //                        else
    //                        {
    //                            listModel.group_name = GetGroupName(item.data.decisions.data[0].type);
    //                            listModel.ok_page_node_id = item.data.decisions.data[0].node;
    //                            listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
    //                            //listModel.ok_level_text = item.data.decisions.data[0].text_val;
    //                            //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
    //                            if (listModel.group_name == "GroupData")
    //                            {
    //                                foreach (var group_item in item.data.type_form.groups.ToList())
    //                                {
    //                                    GroupListModel groupListModel = new GroupListModel();

    //                                    groupListModel.entry_description = group_item.entry_description;
    //                                    groupListModel.group_name = group_item.group_name;
    //                                    groupListModel.lower_limit = group_item.lower_limit;
    //                                    groupListModel.upper_limit = group_item.upper_limit;
    //                                    groupListModel.unit = group_item.unit;
    //                                    listModel.group_list.Add(groupListModel);
    //                                    //GroupList.Add(groupListModel);
    //                                }

    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        listModel.group_name = GetGroupName("LastData");
    //                        listModel.ok_page_node_id = -1;
    //                        listModel.not_ok_page_node_id = -1;
    //                        if (item.data.type_form.topic.Contains("All Ok") || _gd_list.Count == item.id)
    //                        {
    //                            listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
    //                            {
    //                                //new LastQueCheckModel
    //                                //{
    //                                //    describe = "Continue to next step",
    //                                //    id = 1,
    //                                //    isCheck = false,
    //                                //},
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Restart GD",
    //                                    id = 2,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Quit GD",
    //                                    id = 3,
    //                                    isCheck = false,
    //                                },
    //                            };
    //                        }
    //                        else
    //                        {
    //                            listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
    //                            {
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Continue to next step",
    //                                    id = 1,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Restart GD",
    //                                    id = 2,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Quit GD",
    //                                    id = 3,
    //                                    isCheck = false,
    //                                },
    //                            };
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (item.data.decisions.data.Count > 0)
    //                    {
    //                        listModel.ok_page_node_id = item.data.decisions.data[0].node;
    //                        listModel.not_ok_page_node_id = item.data.decisions.data[1].node;
    //                        listModel.group_name = GetGroupName(item.data.decisions.type);
    //                        //listModel.ok_level_text = item.data.decisions.data[0].text_val;
    //                        //listModel.not_ok_level_text = item.data.decisions.data[1].text_val;
    //                    }
    //                    else
    //                    {
    //                        listModel.ok_page_node_id = -1;
    //                        listModel.not_ok_page_node_id = -1;
    //                        listModel.group_name = GetGroupName("LastData");
    //                        if (item.data.type_form.topic.Contains("All Ok") || _gd_list.Count == item.id)
    //                        {
    //                            listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
    //                            {
    //                                //new LastQueCheckModel
    //                                //{
    //                                //    describe = "Continue to next step",
    //                                //    id = 1,
    //                                //    isCheck = false,
    //                                //},
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Restart GD",
    //                                    id = 2,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Quit GD",
    //                                    id = 3,
    //                                    isCheck = false,
    //                                },
    //                            };
    //                        }
    //                        else
    //                        {
    //                            listModel.last_question_list = new ObservableCollection<LastQueCheckModel>
    //                            {
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Continue to next step",
    //                                    id = 1,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Restart GD",
    //                                    id = 2,
    //                                    isCheck = false,
    //                                },
    //                                new LastQueCheckModel
    //                                {
    //                                    describe = "Quit GD",
    //                                    id = 3,
    //                                    isCheck = false,
    //                                },
    //                            };
    //                        }
    //                        //listModel.ok_level_text = "";
    //                        //listModel.not_ok_level_text = "";
    //                    }

    //                }

    //                first_item++;
    //                if (first_item == 1)
    //                {
    //                    listModel.page_visible = true;
    //                    listModel.view_height = App.ScreenHeight - less_hieght;
    //                    current_page_type = listModel.group_name;
    //                }
    //                else
    //                {
    //                    listModel.page_visible = false;
    //                    listModel.view_height = 0;
    //                }
    //                Treelist.Add(listModel);
    //            }
    //            GdList.ItemsSource = Treelist;
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }

    //    public void InitializeLastDataCheckList()
    //    { }

    //    private List<TreeListModel> treeList;
    //    public List<TreeListModel> Treelist
    //    {
    //        get => treeList;
    //        set
    //        {
    //            treeList = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    public string GetGroupName(string type)
    //    {
    //        string GroupName = string.Empty;
    //        switch (type)
    //        {
    //            case "radio":
    //                GroupName = "RadioData";
    //                break;

    //            case "static":
    //                GroupName = "GroupData";
    //                break;

    //            case "":
    //                GroupName = "SimpleData";
    //                break;

    //            case "LastData":
    //                GroupName = "LastData";
    //                break;

    //            default:
    //                Console.WriteLine("Invalid selection. Please select 1, 2, or 3.");
    //                break;
    //        }

    //        return GroupName;

    //    }

    //    private void BtnStart_Clicked(object sender, EventArgs e)
    //    {

    //    }

    //    private void CheckOk_CheckedChanged(object sender, CheckedChangedEventArgs e)
    //    {
    //        try
    //        {
    //            CheckBox item = (CheckBox)sender;

    //            if (item == null) return;
    //            //GdList.ItemsSource = null;
    //            var class_id = item.ClassId;
    //            var selectedItem = (TreeListModel)((CheckBox)sender).BindingContext;

    //            if (class_id == "Ok")
    //            {
    //                next_page_id = selectedItem.ok_page_node_id;
    //                current_page_id = selectedItem.id;
    //                //current_page_type = selectedItem.group_name;
    //            }
    //            else if (class_id == "NotOk")
    //            {
    //                next_page_id = selectedItem.not_ok_page_node_id;
    //                current_page_id = selectedItem.id;
    //                continue_page_id = selectedItem.ok_page_node_id;
    //                //current_page_type = selectedItem.group_name;
    //            }
    //            //GdList.ItemsSource = Treelist;
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    //OK Or Not Check Box Checked
    //    private void Decission_Check_Tapped(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            var selectedItem = (DecissionModel)((Grid)sender).BindingContext;
    //            foreach (var item in Treelist)
    //            {
    //                var decission_data_check = item.decission_list.FirstOrDefault(x => x.id == selectedItem.id);
    //                if (decission_data_check != null)
    //                {
    //                    foreach (var decission in item.decission_list)
    //                    {
    //                        if (decission.text_value == selectedItem.text_value)
    //                        {
    //                            decission.isCheck = !decission.isCheck;
    //                            next_page_id = decission.next_node;
    //                            current_page_id = selectedItem.id;
    //                            if (selectedItem.text_value == "NOT OK")
    //                            {
    //                                continue_page_id = decission.next_node - 1;
    //                            }
    //                        }
    //                        else
    //                        {
    //                            decission.isCheck = false;
    //                        }
    //                    }
    //                }

    //                #region Old Code
    //                //if(decission_data_check!=null)
    //                //{
    //                //    decission_data_check.isCheck = !decission_data_check.isCheck;
    //                //    next_page_id = decission_data_check.next_node;
    //                //    current_page_id = item.id;
    //                //}

    //                //foreach(var decission_data_check in item.decission_list)
    //                //{
    //                //    if(decission_data_check.id == selectedItem.id)
    //                //    {
    //                //        decission_data_check.isCheck = !decission_data_check.isCheck;
    //                //        next_page_id = decission_data_check.next_node;
    //                //        current_page_id = item.id;
    //                //    }
    //                //    else
    //                //    {

    //                //    }
    //                //}

    //                //var decission_data_check = item.decission_list.First(x => x.id == selectedItem.id);
    //                //if (decission_data_check != null)
    //                //{
    //                //    decission_data_check.isCheck = !decission_data_check.isCheck;
    //                //    next_page_id = decission_data_check.next_node;
    //                //    current_page_id = item.id;
    //                //}

    //                //var decission_data_uncheck = item.decission_list.First(x => x.text_value == selectedItem.text_value);
    //                //if (decission_data_uncheck != null)
    //                //{
    //                //    decission_data_uncheck.isCheck = !decission_data_uncheck.isCheck;
    //                //    next_page_id = continue_page_id = decission_data_uncheck.next_node;
    //                //    current_page_id = item.id;
    //                //    //continue_page_id = selectedItem.ok_page_node_id;
    //                //}
    //                #endregion
    //            }

    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                var JsonData = JsonConvert.SerializeObject(selectedItem);
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "Decission_Checked",
    //                    ElementValue = "Decission_Check_Tapped_" + JsonData,
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }
    //        }
    //        catch (Exception ex)
    //        { }
    //    }

    //    //Check Box OK OR Not Clicked Next BTN
    //    private async void btnNext_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "Checked_OK_Or_NotOK",
    //                    ElementValue = "Checked_OK_Or_Not_Clicked_BTN_",
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }

    //            var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
    //            if (next_page_id < 0)
    //            {
    //                show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
    //                //return;
    //            }
    //            else
    //            {
    //                if (current_page_id != next_page_id)
    //                {
    //                    GdList.ItemsSource = "";
    //                    if (next_page != null)
    //                    {
    //                        next_page.page_visible = true;
    //                        next_page.view_height = App.ScreenHeight - less_hieght;
    //                        current_page_type = next_page.group_name;
    //                    }

    //                    var list = Treelist.First(x => x.id == current_page_id);
    //                    if (list != null)
    //                    {
    //                        list.page_visible = false;
    //                        list.view_height = 0;
    //                    }
    //                    current_page_id = next_page_id;

    //                    //await Task.Delay(50000);
    //                    GdList.ItemsSource = Treelist;
    //                }
    //                else
    //                {
    //                    show_alert("Alert", "Please select OK or NOT OK checkbox", false, true);
    //                }
    //                //GdList.ItemsSource = Treelist;
    //                //GdList.HeightRequest = App.ScreenHeight-150;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            show_alert("Alert", $"{ex.Message} \n\n\n {ex.StackTrace}", false, true);
    //        }
    //    }

    //    private void btnGroupNext_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            //bool is_empty_value = false;
    //            Task.Run(async () =>
    //            {
    //                // Run code here

    //                Device.BeginInvokeOnMainThread(async () =>
    //                {
    //                    bool IsWrongValue = false;
    //                    //GdList.ItemsSource = "";
    //                    if (current_page_type == "GroupData")
    //                    {
    //                        var group_list = Treelist.First(x => x.id == current_page_id);
    //                        var IsNullData = group_list.group_list.FirstOrDefault(x => x.current_limit == null || x.current_limit == "");
    //                        if (IsNullData == null)
    //                        {
    //                            foreach (var group_list_item in group_list.group_list)
    //                            {
    //                                if (Convert.ToDouble(group_list_item.upper_limit) >= Convert.ToDouble(group_list_item.current_limit)
    //                                && Convert.ToDouble(group_list_item.lower_limit) <= Convert.ToDouble(group_list_item.current_limit))
    //                                {
    //                                    group_list_item.status_color = "#00b800";
    //                                    group_list_item.upper_lower_value_visible = true;
    //                                }
    //                                else
    //                                {
    //                                    group_list_item.status_color = "#FF0000";
    //                                    group_list_item.upper_lower_value_visible = true;
    //                                }

    //                            }

    //                            var item = group_list.group_list.FirstOrDefault(x => x.status_color == "#FF0000");
    //                            if (item == null)
    //                            {
    //                                next_page_id = group_list.ok_page_node_id;
    //                                //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);
    //                                //next_page_id = group_list.not_ok_page_node_id;
    //                                current_page_type = group_list.group_name;
    //                            }
    //                            else
    //                            {

    //                                next_page_id = group_list.not_ok_page_node_id;
    //                                continue_page_id = group_list.ok_page_node_id;
    //                            }

    //                            //var next_page = Treelist.FirstOrDefault(x => x.id == next_page_id);



    //                            await Task.Delay(4000);

    //                            foreach (var item1 in Treelist)
    //                            {
    //                                if (item1.id == next_page_id)
    //                                {
    //                                    item1.page_visible = true;
    //                                    item1.view_height = App.ScreenHeight - less_hieght;
    //                                }
    //                                else
    //                                {
    //                                    item1.page_visible = false;
    //                                    item1.view_height = 0;
    //                                }
    //                            }

    //                        }
    //                        else
    //                        {
    //                            show_alert("Alert", "Please fill all fields", false, true);
    //                        }

    //                        if (CurrentUserEvent.Instance.IsExpert)
    //                        {
    //                            var JsonData = JsonConvert.SerializeObject(group_list);
    //                            App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                            {
    //                                ElementName = "GroupNext",
    //                                ElementValue = "btnGroupNext_Checked_" + JsonData,
    //                                ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                                IsExpert = CurrentUserEvent.Instance.IsExpert
    //                            });
    //                        }
    //                    }
    //                    current_page_id = next_page_id;
    //                });
    //                //await Task.Delay(10000);
    //                //GdList.ItemsSource = Treelist;
    //            }).ConfigureAwait(false);
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    // Root Cause Check Box is Checked BTN
    //    private async void btnLastPageNext_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "Checked_OK_Or_NotOK",
    //                    ElementValue = "Root_Cause_Next_BTN",
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }

    //            if (gd_decisssion == 1)
    //            {

    //                var next_page = Treelist.FirstOrDefault(x => x.id == continue_page_id);
    //                if (next_page == null)
    //                {
    //                    return;
    //                }
    //                else
    //                {

    //                    if (continue_page_id != current_page_id)
    //                    {
    //                        GdList.ItemsSource = "";
    //                        if (next_page != null)
    //                        {
    //                            next_page.page_visible = true;
    //                            next_page.view_height = App.ScreenHeight - less_hieght;
    //                            current_page_type = next_page.group_name;

    //                            current_page_id = next_page_id;
    //                        }

    //                        var list = Treelist.First(x => x.id == current_page_id);
    //                        if (list != null)
    //                        {
    //                            list.page_visible = false;
    //                            list.view_height = 0;
    //                        }
    //                        GdList.ItemsSource = Treelist;
    //                        next_page_id = current_page_id = next_page.id;
    //                    }
    //                    else
    //                    {
    //                        show_alert("Alert", "This is last page", false, true);
    //                    }


    //                    //await Task.Delay(50000);
    //                }


    //            }
    //            else if (gd_decisssion == 2)
    //            {
    //                switch (Device.RuntimePlatform)
    //                {
    //                    case Device.Android:
    //                        StartGD(resultGD.tree_set[0].tree_data);
    //                        break;
    //                    case Device.UWP:
    //                        this.Navigation.PushAsync(new TreeSurveyPage(resultGD, LoadDescription, LoadCode));
    //                        break;
    //                    default:
    //                        break;
    //                }
    //                //StartGD(resultGD.tree_set[0].tree_data);
    //            }
    //            else if (gd_decisssion == 3)
    //            {
    //                for (var i = 1; i < 3; i++)
    //                {
    //                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
    //                }
    //                await Navigation.PopAsync();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    //Root Cause Check Box Checked
    //    private void Check_Tapped(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            var selectedItem = (LastQueCheckModel)((Grid)sender).BindingContext;
    //            var group_list = Treelist.First(x => x.id == current_page_id);
    //            foreach (var item in group_list.last_question_list.ToList())
    //            {
    //                if (item.id != selectedItem.id)
    //                {
    //                    item.isCheck = false;
    //                }
    //                else
    //                {
    //                    item.isCheck = !item.isCheck;
    //                    gd_decisssion = item.id;
    //                }
    //            }

    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                var JsonData = JsonConvert.SerializeObject(selectedItem);
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "Decission_Checked",
    //                    ElementValue = "Root_Cause_Check_Box_Checked_" + JsonData,
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }
    //        }
    //        catch (Exception ex)
    //        { }
    //    }


    //    private async void MenuItem1_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "MenuItem1",
    //                    ElementValue = "MenuItem1_Clicked",
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }

    //            this.Navigation.PushAsync(new GdImagePage(gd_image, Title));
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    protected override bool OnBackButtonPressed()
    //    {
    //        return false;
    //    }

    //    public void show_alert(string title, string message, bool btnCancel, bool btnOk)
    //    {
    //        Working = true;
    //        TitleText = title;
    //        MessageText = message;
    //        OkVisible = btnOk;
    //        CancelVisible = btnCancel;
    //        CancelCommand = new Command(() =>
    //        {
    //            Working = false;
    //        });
    //    }

    //    private void Current_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        //var item = ((Entry)sender).BindingContext;
    //    }
    //}
    #endregion
}