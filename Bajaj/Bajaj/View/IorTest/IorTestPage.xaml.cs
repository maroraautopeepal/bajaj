using Acr.UserDialogs;
using MultiEventController.Models;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.IorTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IorTestPage : ContentPage
    {
        ApiServices services;
        IorTestViewModel viewModel;
        JobCardListModel jobCard;
        public IorTestPage(JobCardListModel jobCardSession)
        {
            InitializeComponent();
            BindingContext = viewModel = new IorTestViewModel();
            jobCard = jobCardSession;
            services = new ApiServices();
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                GetIorTest();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;

            //App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.EcuList));

        }

        protected async override void OnDisappearing()
        {

            base.OnDisappearing();

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

            App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        }


        string[] PairedData = new string[2];
        private async void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                await Task.Delay(100);
                // bool InsternetActive = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        string data = (string)sender; //sender as string;
                        if (!string.IsNullOrEmpty(data))
                        {
                            if (data.Contains("EcuList*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.EcuList = JsonConvert.DeserializeObject<ObservableCollection<EcuTestRoutine>>(PairedData[1]); ;
                                DependencyService.Get<ILodingPageService>().HideLoadingPage();
                            }
                            else if (data.Contains("SelectedEcu*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(PairedData[1]);
                                DependencyService.Get<ILodingPageService>().HideLoadingPage();
                            }
                            else if (data.Contains("IorTestList*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.IorTestList = JsonConvert.DeserializeObject<ObservableCollection<IorResult>>(PairedData[1]); ;
                                DependencyService.Get<ILodingPageService>().HideLoadingPage();
                            }
                            else if (data.Contains("IorNotFound*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.RoutineListStatus = "IOR test not found...";
                                var errorpage = new Popup.DisplayAlertPage("Alert", PairedData[1], "OK");
                                await PopupNavigation.Instance.PushAsync(errorpage);
                            }
                            else if (data.Contains("IorNotFound1*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.RoutineListStatus = "IOR test not found...";
                            }
                            else if (data.Contains("SelectedNewEcu*#"))
                            {
                                PairedData = data.Split('#');

                                viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(PairedData[1]);

                                viewModel.IorTestList = new ObservableCollection<IorResult>();
                                foreach (var ecu in viewModel.EcuList.ToList())
                                {
                                    if (viewModel.SelectedEcu.id == ecu.id)
                                    {
                                        ecu.opacity = 1;
                                        viewModel.IorTestList = new ObservableCollection<IorResult>(ecu.results.ToList());
                                    }
                                    else
                                    {
                                        ecu.opacity = .5;
                                    }
                                }

                            }
                            //else if (data.Contains("SelectIorTest*#"))
                            //{
                            //    PairedData = data.Split('#');
                            //    viewModel.SelectedTest = JsonConvert.DeserializeObject<IorResult>(PairedData[1]);

                            //    SelectIorTestClicked();

                            //}

                        }
                        //InsternetActive = false;
                    }
                    catch (Exception ex)
                    {
                        DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        //InsternetActive = false;
                    }
                });
            }
            #endregion
        }


        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectParameterClicked"))
            {
                //viewModel.PidListVisible = true;
                //viewModel.NewValue = string.Empty;
                //viewModel.ShortNameVisible = viewModel.EnumrateDropDownVisible = viewModel.ManualEntryVisible = viewModel.BtnVisible = false;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("HidePIdViewClicked"))
            {
                await NoticeView.TranslateTo(0, 1000, 300);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("OkRoutineClicked"))
            {
                OkRoutineClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectEcuClicked"))
            {
                //App.controlEventManager.SendRequestData("SelectIorTest*#" + elementEventHandler.ElementName);
                viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(elementEventHandler.ElementName);

                SelectEcuClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectIorTestClicked"))
            {
                //App.controlEventManager.SendRequestData("SelectIorTest*#" + elementEventHandler.ElementName);
                viewModel.SelectedTest = JsonConvert.DeserializeObject<IorResult>(elementEventHandler.ElementName);

                SelectIorTestClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("CancelRoutineClicked"))
            {
                await NoticeView.TranslateTo(0, 1000, 300);
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }


        public async void GetIorTest()
        {
            try
            {
                var data = await services.GetIorTest(App.JwtToken, 0);
                 if (data != null)
                {
                    if (string.IsNullOrEmpty(data.error))
                    {
                        int count = 0;

                        if (data.results != null && data.results.Any())
                        {
                            //var EcuGroup = data.results.GroupBy(u => u.ecu.id).Select(ecu => ecu.ToList()).ToList();
                            //var EcuGroup = data.results.GroupBy(x => x.sub_model == jobCard.sub_model && x.model_year == jobCard.model_year);
                            var EcuGroup = data.results.GroupBy(u => u.sub_model == jobCard.sub_model).Select(sm => sm.ToList()).ToList();

                            foreach (var ecu_item in EcuGroup.ToList())
                            {
                                if(ecu_item.FirstOrDefault().sub_model == jobCard.sub_model)
                                {
                                    count++;
                                    if (count < 2)
                                    {
                                        viewModel.EcuList.Add(new Model.EcuTestRoutine
                                        {
                                            id = ecu_item.FirstOrDefault().ecu.id,
                                            ecu_name = ecu_item.FirstOrDefault().ecu.name,
                                            results = ecu_item,
                                            opacity = 1,
                                        });
                                        viewModel.IorTestList = new ObservableCollection<IorResult>(ecu_item.ToList());
                                        viewModel.SelectedEcu = new Model.EcuTestRoutine
                                        {
                                            id = ecu_item.FirstOrDefault().ecu.id,
                                            ecu_name = ecu_item.FirstOrDefault().ecu.name,
                                            results = ecu_item,
                                            opacity = 1,
                                        };
                                    }
                                    else
                                    {
                                        viewModel.EcuList.Add(new Model.EcuTestRoutine
                                        {
                                            id = ecu_item.FirstOrDefault().ecu.id,
                                            ecu_name = ecu_item.FirstOrDefault().ecu.name,
                                            results = ecu_item,
                                            opacity = .5,
                                        });
                                        viewModel.SelectedEcu = viewModel.EcuList.FirstOrDefault();
                                        viewModel.SelectedEcu.opacity = 1;
                                        viewModel.IorTestList = new ObservableCollection<IorResult>(viewModel.SelectedEcu.results.ToList());
                                    }
                                }

                                
                            }

                            App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.EcuList));
                            App.controlEventManager.SendRequestData("SelectedEcu*#" + JsonConvert.SerializeObject(viewModel.SelectedEcu));
                            App.controlEventManager.SendRequestData("IorTestList*#" + JsonConvert.SerializeObject(viewModel.IorTestList));
                        }
                    }
                    else
                    {
                        App.controlEventManager.SendRequestData($"IorNotFound*#{data.error}");
                        viewModel.RoutineListStatus = "IOR test not found...";
                        var errorpage = new Popup.DisplayAlertPage("Alert", data.error, "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);

                        //await DisplayAlert("Alert", data.error, "Ok");
                    }
                }
                else
                {
                    App.controlEventManager.SendRequestData($"IorNotFound1*#");
                    viewModel.RoutineListStatus = "IOR test not found...";
                    //r errorpage = new Popup.DisplayAlertPage("Alert", data.error, "OK");
                    //await PopupNavigation.Instance.PushAsync(errorpage);
                    //viewModel.RoutineListStatus = "IOR test not found...";
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void SelectEcuClicked(object sender, System.EventArgs e)
        {
            try
            {
                viewModel.SelectedEcu = (EcuTestRoutine)((Button)sender).BindingContext;

                if (viewModel.SelectedEcu == null)
                    return;

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = JsonConvert.SerializeObject(viewModel.SelectedEcu),
                        ElementValue = "SelectEcuClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                SelectEcuClicked();
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void SelectEcuClicked()
        {
            try
            {
                viewModel.IorTestList = new ObservableCollection<IorResult>();
                foreach (var ecu in viewModel.EcuList.ToList())
                {
                    if (viewModel.SelectedEcu.id == ecu.id)
                    {
                        ecu.opacity = 1;
                        viewModel.IorTestList = new ObservableCollection<IorResult>(ecu.results.ToList());
                    }
                    else
                    {
                        ecu.opacity = .5;
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void SelectIorTestClicked(object sender, System.EventArgs e)
        {
            try
            {
                viewModel.SelectedTest = (IorResult)((Button)sender).BindingContext;

                if (viewModel.SelectedTest == null)
                    return;

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = JsonConvert.SerializeObject(viewModel.SelectedTest),
                        ElementValue = "SelectIorTestClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                SelectIorTestClicked();
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void SelectIorTestClicked()
        {
            //if (!CurrentUserEvent.Instance.IsExpert)
            //{
            //App.controlEventManager.SendRequestData("SelectIorTest*#" + JsonConvert.SerializeObject(viewModel.SelectedTest));
            viewModel.RoutineNotice = $"{viewModel.SelectedTest.notice}.\n\nDo you want to continue this routine.";

            await NoticeView.TranslateTo(0, 0, 300);
            //}
        }


        private async void CancelRoutineClicked(object sender, System.EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                #region Check Internet Connection
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "CancelRoutineFrame",
                        ElementValue = "CancelRoutineClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                #endregion
            }
            await NoticeView.TranslateTo(0, 1000, 300);
        }

        private async void OkRoutineClicked(object sender, System.EventArgs e)
        {
            try
            {

                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    #region Check Internet Connection
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = "OkRoutineFrame",
                            ElementValue = "OkRoutineClicked",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                    }
                    #endregion
                }
                OkRoutineClicked();
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void OkRoutineClicked()
        {
            try
            {
                //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await NoticeView.TranslateTo(0, 1000, 300);
                    await Task.Delay(200);

                    string SeedIndex = StaticData.ecu_info.FirstOrDefault(x => x.ecu_ID == viewModel.SelectedEcu.id).seed_key_index.value;
                    string WriteFnIndex = StaticData.ecu_info.FirstOrDefault(x => x.ecu_ID == viewModel.SelectedEcu.id).write_pid_index;

                    if (viewModel.SelectedTest.pre_conditions == null || !viewModel.SelectedTest.pre_conditions.Any())
                    {
                        await Navigation.PushAsync(new IorTestPlayPage(viewModel.SelectedTest, SeedIndex, WriteFnIndex, viewModel.SelectedEcu.id));
                    }
                    else
                    {
                        await Navigation.PushAsync(new PreConditionPage(viewModel.SelectedTest, SeedIndex, WriteFnIndex,viewModel.SelectedEcu.id));
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }
    }
}