using Acr.UserDialogs;
using Plugin.Connectivity;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.GdSection;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IvnDtcListPage : DisplayAlertPage
    {
        JobCardListModel jobCard;
        TapGestureRecognizer tab_gesture;
        IvnDtcListViewModel viewModel;
        ApiServices services;

        string selected_ecu = string.Empty;
        double opacity = -1;

        public IvnDtcListPage(JobCardListModel jobCardSession)
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new IvnDtcListViewModel();

                jobCard = new JobCardListModel();
                jobCard = jobCardSession;

                services = new ApiServices();
                tab_gesture = new TapGestureRecognizer();
                tab_gesture.Tapped += Tab_gesture_Tapped; ;

                if (StaticData.ecu_info != null)
                {
                    if (StaticData.ecu_info.FirstOrDefault().protocol.name.Contains("IVN"))
                    {
                        NotIVN.IsVisible = false;
                        IVN.IsVisible = true;
                    }
                    else
                    {
                        NotIVN.IsVisible = true;
                        IVN.IsVisible = false;
                    }
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(200);
                        await CreateDynamicallyaDesign();
                    }
                });
            }
            catch (Exception ex)
            {
                show_alert("Alert", $"{ex.Message}\n\n\n\n{ex.StackTrace}", false, true);
            }

        }

        public bool dtcCount = true;
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            dtcCount = false;
        }

        public async Task CreateDynamicallyaDesign()
        {
            int class_id = 0;
            bool collectiionVisible = true;
            grd_tab.ColumnDefinitions.Clear();

            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(200);

                    foreach (var ecu in StaticData.ecu_info)
                    {
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(200);

                            if (ecu.protocol.name.Contains("IVN") == true)
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
                                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
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

                                //int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu).pid_dataset_id;
                                int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).dtc_dataset_id;

                                var IVN_Result = new List<IVN_Result>();
                                var Vales = viewModel.get_dtc_All_Recoed(ecu.dtc_dataset_id);
                                //var Vales = viewModel.get_dtc_All_Recoed();
                                var read_dtc_index = (dynamic)null;


                                foreach (var item in Vales.Result)
                                {
                                    if (item.DTC_R.Count != 0)
                                    {
                                        foreach (var item2 in item.DTC_R)
                                        {
                                            if (item2.id == id)
                                            {
                                                //var server_dtcc = viewModel.AllDtc.FirstOrDefault(x => x.id == id).dtc_code;
                                                var server_dtcc = Vales.Result.FirstOrDefault().DTC_R.FirstOrDefault(X => X.id == id).dtc_code1;
                                                await Task.Delay(500);
                                                viewModel.ServerDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = server_dtcc });
                                                read_dtc_index = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).read_dtc_index;
                                            }
                                        }
                                    }

                                    if (item.IDCT_R.Count != 0)
                                    {
                                        foreach (var item2 in item.IDCT_R)
                                        {
                                            if (item2.id == id)
                                            {
                                                //var IVNserver_dtc = item.IDCT_R.FirstOrDefault(x=>x.id == id).frame_datasets.FirstOrDefault().frame_ids;
                                                //await Task.Delay(500);
                                                //viewModel.ServerIVNDtcList.Add(new IVN_DtcModel { frame_name = ecu.ecu_name, dtc_list = IVNserver_dtc });

                                                //break;

                                                //var IVNserver_dtc = item.IDCT_R.FirstOrDefault(x => x.id == id).frame_datasets;
                                                //await Task.Delay(500);
                                                IVN_Result.Add(new IVN_Result
                                                {
                                                    id = item2.id,
                                                    code = item2.code,
                                                    description = item2.description,
                                                    frame_datasets = item2.frame_datasets
                                                });
                                            }
                                        }
                                    }
                                }

                                var ecu_dtc = await Task.Run(async () =>
                                {
                                    var result = await viewModel.readDtc(read_dtc_index, ecu.ecu_name, IVN_Result);
                                    return result;
                                });

                                //viewModel.LiveParameterList = model.roots = viewModel.get_pid(id);
                                //var server_dtc = viewModel.get_dtc(id);
                                //viewModel.ServerDtcList.Add(new DtcModel { ecu_name = ecu, dtc_list = server_dtc });
                                //var read_dtc_index = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu).read_dtc_index;
                                //var ecu_dtc = await viewModel.readDtc(read_dtc_index, ecu);
                                //viewModel.EcuDtcList.Add(new DtcModel { ecu_name = ecu, dtc_list = ecu_dtc });

                                //await Task.Delay(100);
                                if (viewModel.DTC_Error != "NO_ERROR")
                                {
                                    await this.DisplayAlert("ERROR", "Read Dtc Error - ( " + ecu.ecu_name + " ) " + viewModel.DTC_Error, "OK");

                                    viewModel.EcuDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = ecu_dtc });
                                }
                                else
                                {
                                    if (ecu_dtc.Count == 0)
                                    {
                                        await this.DisplayAlert("No DTC Found", "No DTC ( " + ecu.ecu_name + " ) ", "OK");

                                        viewModel.EcuDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = ecu_dtc });
                                    }
                                    else
                                    {
                                        if (viewModel.DTC_Error != "NO_ERROR" && viewModel.DTC_Error != "NoValue")
                                        {
                                            var result = await DisplayAlert("Error", "Read Dtc Error - ( " + ecu.ecu_name + " ) " + viewModel.DTC_Error, "Ok", "Cancel");
                                            if (result)
                                            {
                                                //await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
                                            }
                                            else
                                            {
                                                //await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
                                            }
                                        }
                                        viewModel.EcuDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = ecu_dtc });

                                        await this.DisplayAlert("DTC Found", "Number of [ " + Convert.ToString(ecu_dtc.Count) + " ] DTC Found in " + ecu.ecu_name + ".", "OK");
                                    }
                                }

                                CollectionView collectionView = new CollectionView
                                {
                                    ClassId = Convert.ToString(class_id),
                                    IsVisible = collectiionVisible,
                                    EmptyView = new StackLayout
                                    {
                                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                                        VerticalOptions = LayoutOptions.FillAndExpand,
                                        Children = { new Label { Text = "Dtc not found", TextColor = (Color)Application.Current.Resources["text_color"],
                            FontAttributes = FontAttributes.Bold, FontSize = 22, HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center } }
                                    }
                                };
                                collectionView = new CollectionView { ClassId = Convert.ToString(class_id), IsVisible = collectiionVisible };
                                collectionView.SetBinding(ItemsView.ItemsSourceProperty, $"EcuDtcList[{class_id}].dtc_list");

                                collectionView.ItemTemplate = new DataTemplate(() =>
                                {
                                    Grid main = new Grid { Margin = new Thickness(5, 0, 5, 0) };

                                    Frame frm = new Frame { HasShadow = true, CornerRadius = 3, Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(1) };
                                    main.Children.Add(frm, 0, 0);

                                    Grid mainGrid = new Grid { RowSpacing = 7 };
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = 1 });
                                    frm.Content = mainGrid;

                                    Grid firstRowGrid = new Grid { Padding = 8 };
                                    firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    mainGrid.Children.Add(firstRowGrid, 0, 0);

                                    Label p_code_label = new Label { Style = (Style)this.Resources["lbl"], FontAttributes = FontAttributes.Bold };
                                    p_code_label.SetBinding(Label.TextProperty, "code");
                                    firstRowGrid.Children.Add(p_code_label, 0, 0);

                                    Label status_label = new Label { Style = (Style)this.Resources["lbl"], HorizontalTextAlignment = TextAlignment.End };
                                    status_label.SetBinding(Label.TextProperty, "status");
                                    firstRowGrid.Children.Add(status_label, 1, 0);

                                    Grid thirdRowGrid = new Grid { };
                                    thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 120 });
                                    mainGrid.Children.Add(thirdRowGrid, 0, 1);

                                    BoxView boxView = new BoxView { BackgroundColor = (Color)Application.Current.Resources["theme_color"] };
                                    mainGrid.Children.Add(boxView, 0, 2);

                                    Label description_label = new Label { Style = (Style)this.Resources["lbl"], LineBreakMode = LineBreakMode.CharacterWrap };
                                    description_label.SetBinding(Label.TextProperty, "description");
                                    thirdRowGrid.Children.Add(description_label, 0, 0);

                                    Button troubleshoot_button = new Button { Text = "Troubleshoot", Style = (Style)this.Resources["troub_btn"] };
                                    thirdRowGrid.Children.Add(troubleshoot_button, 1, 0);
                                    troubleshoot_button.Clicked += GD_Clicked;

                                    #region Old Code
                                    //Grid main = new Grid { Margin = new Thickness(5, 0, 5, 0) };

                                    //Frame frm = new Frame { HasShadow = true, CornerRadius = 3, Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(1) };
                                    //main.Children.Add(frm, 0, 0);

                                    //Grid mainGrid = new Grid { RowSpacing = 7 };
                                    //mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    //mainGrid.RowDefinitions.Add(new RowDefinition { Height = 1 });
                                    //mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    //frm.Content = mainGrid;

                                    //Grid firstRowGrid = new Grid { Padding = 8 };
                                    //firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    //firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    //mainGrid.Children.Add(firstRowGrid, 0, 0);

                                    //Label p_code_label = new Label { Style = (Style)this.Resources["lbl"], FontAttributes = FontAttributes.Bold };
                                    //p_code_label.SetBinding(Label.TextProperty, "code");
                                    //firstRowGrid.Children.Add(p_code_label, 0, 0);

                                    //Label status_label = new Label { Style = (Style)this.Resources["lbl"], HorizontalTextAlignment = TextAlignment.End };
                                    //status_label.SetBinding(Label.TextProperty, "status");
                                    //firstRowGrid.Children.Add(status_label, 1, 0);

                                    //Grid thirdRowGrid = new Grid { };
                                    //thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    //thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 120 });
                                    //mainGrid.Children.Add(thirdRowGrid, 0, 1);

                                    //BoxView boxView = new BoxView { BackgroundColor = (Color)Application.Current.Resources["theme_color"] };
                                    //mainGrid.Children.Add(boxView, 0, 2);

                                    //Label description_label = new Label { Style = (Style)this.Resources["lbl"], LineBreakMode = LineBreakMode.CharacterWrap };
                                    //description_label.SetBinding(Label.TextProperty, "description");
                                    //thirdRowGrid.Children.Add(description_label, 0, 0);

                                    //Button troubleshoot_button = new Button { Text = "Troubleshoot", Style = (Style)this.Resources["troub_btn"] };
                                    //thirdRowGrid.Children.Add(troubleshoot_button, 1, 0);
                                    #endregion

                                    return mainGrid;
                                });

                                collection_view.Children.Add(collectionView, 0, 0);

                                if (string.IsNullOrEmpty(selected_ecu))
                                {
                                    selected_ecu = ecu.ecu_name;
                                }

                                class_id++;
                                collectiionVisible = false;
                            }

                            if (ecu.protocol.name.Contains("IVN") == false)
                            {
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
                                Label ecu_lbl = new Label
                                {
                                    Text = ecu.ecu_name,
                                    Style = (Style)this.Resources["txt_tab"]
                                };
                                Grid grd = new Grid
                                {
                                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                                    Opacity = opacity,
                                    ClassId = Convert.ToString(class_id),
                                };
                                grd.Children.Add(ecu_lbl);
                                grd_tab.Children.Add(grd, grd_tab.ColumnDefinitions.Count - 1, 0);
                                grd.GestureRecognizers.Add(tab_gesture);

                                //Create Dynamically ECU's PID Data List

                                int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).dtc_dataset_id;
                                //viewModel.LiveParameterList = model.roots = viewModel.get_pid(id);

                                viewModel.AllDtc = await services.get_dtc(App.JwtToken, id);

                                //viewModel.AllDtc = await viewModel.get_dtc_from_local();

                                var server_dtc = viewModel.AllDtc.FirstOrDefault(x => x.id == id).dtc_code1;

                                await Task.Delay(500);
                                viewModel.ServerDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = server_dtc });
                                var read_dtc_index = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).read_dtc_index;

                                var ecu_dtc = await Task.Run(async () =>
                                {
                                    var result = await viewModel.readDtc(read_dtc_index, ecu.ecu_name, null);
                                    return result;
                                });

                                //if (ecu_dtc.Count == 0)
                                //{
                                //    var ecu_dtc = await Task.Run(async () =>
                                //    {
                                //        var result = await viewModel.readDtc(read_dtc_index, ecu.ecu_name);
                                //        return result;
                                //    });
                                //}

                                await Task.Delay(100);
                                if (viewModel.DTC_Error != "NO_ERROR")
                                {
                                    await this.DisplayAlert("ERROR", "Read Dtc Error - " + viewModel.DTC_Error, "OK");
                                    viewModel.EcuDtcList.Add(new DtcModel { ecu_name = "", dtc_list = null });
                                }
                                else
                                {
                                    if (ecu_dtc.Count == 0)
                                    {
                                        await this.DisplayAlert("No DTC Found", "No DTC", "OK");
                                        viewModel.EcuDtcList.Add(new DtcModel { ecu_name = "", dtc_list = null });
                                    }
                                    else
                                    {
                                        if (viewModel.DTC_Error != "NO_ERROR" && viewModel.DTC_Error != "NoValue")
                                        {
                                            var result = await DisplayAlert("Error", "Read Dtc Error - " + viewModel.DTC_Error, "Ok", "Cancel");
                                            if (result)
                                            {
                                                //await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
                                            }
                                            else
                                            {
                                                //await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
                                            }
                                        }
                                        viewModel.EcuDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = ecu_dtc });

                                        //await this.DisplayAlert("DTC Found", "Number of DTC Found :- " + Convert.ToString(ecu_dtc.Count), "OK");
                                        await this.DisplayAlert("DTC Found", "Number of " + Convert.ToString(ecu_dtc.Count) + " DTC Found in " + ecu.ecu_name + ".", "OK");
                                    }
                                }

                                CollectionView collectionView = new CollectionView
                                {
                                    ClassId = Convert.ToString(class_id),
                                    IsVisible = collectiionVisible,
                                    EmptyView = new StackLayout
                                    {
                                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                                        VerticalOptions = LayoutOptions.FillAndExpand,
                                        Children = { new Label { Text = "No Dtc", TextColor = (Color)Application.Current.Resources["text_color"],
                            FontAttributes = FontAttributes.Bold, FontSize = 22, HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center } }
                                    }
                                };


                                collectionView.SetBinding(ItemsView.ItemsSourceProperty, $"EcuDtcList[{class_id}].dtc_list");

                                collectionView.ItemTemplate = new DataTemplate(() =>
                                {
                                    Grid main = new Grid { Margin = new Thickness(5, 0, 5, 0) };

                                    Frame frm = new Frame { HasShadow = true, CornerRadius = 3, Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(1) };
                                    main.Children.Add(frm, 0, 0);

                                    Grid mainGrid = new Grid { RowSpacing = 7 };
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = 1 });
                                    frm.Content = mainGrid;

                                    Grid firstRowGrid = new Grid { Padding = 8 };
                                    firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    firstRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    mainGrid.Children.Add(firstRowGrid, 0, 0);

                                    Label p_code_label = new Label { Style = (Style)this.Resources["lbl"], FontAttributes = FontAttributes.Bold };
                                    p_code_label.SetBinding(Label.TextProperty, "code");
                                    firstRowGrid.Children.Add(p_code_label, 0, 0);

                                    Label status_label = new Label { Style = (Style)this.Resources["lbl"], HorizontalTextAlignment = TextAlignment.End };
                                    status_label.SetBinding(Label.TextProperty, "status");
                                    firstRowGrid.Children.Add(status_label, 1, 0);

                                    Grid thirdRowGrid = new Grid { };
                                    thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                    thirdRowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 120 });
                                    mainGrid.Children.Add(thirdRowGrid, 0, 1);

                                    BoxView boxView = new BoxView { BackgroundColor = (Color)Application.Current.Resources["theme_color"] };
                                    mainGrid.Children.Add(boxView, 0, 2);

                                    Label description_label = new Label { Style = (Style)this.Resources["lbl"], LineBreakMode = LineBreakMode.CharacterWrap };
                                    description_label.SetBinding(Label.TextProperty, "description");
                                    thirdRowGrid.Children.Add(description_label, 0, 0);

                                    Button troubleshoot_button = new Button { Text = "Troubleshoot", Style = (Style)this.Resources["troub_btn"] };
                                    thirdRowGrid.Children.Add(troubleshoot_button, 1, 0);
                                    troubleshoot_button.Clicked += GD_Clicked;

                                    return mainGrid;
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
                }

            }
            catch (Exception ex)
            {

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
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

        private async void GD_Clicked(object sender, EventArgs e)
        {
            try
            {

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    string model = string.Empty;
                    var ee = e;
                    var SenderData = (Button)sender;
                    var ClickedRowValue = SenderData.Parent.BindingContext as DtcListModel;

                    //var DtcID = viewModel.AllDtc.FirstOrDefault().dtc_code.FirstOrDefault(x => x.code == ClickedRowValue.code);

                    if (ClickedRowValue != null)
                    {
                        var res = await services.Get_gd(App.JwtToken, ClickedRowValue.code, App.sub_model_id);

                        if (res.count != 0)
                        {
                            await this.Navigation.PushAsync(new InfoPage(res.results));
                        }
                        else
                        {
                            await this.DisplayAlert("", "GD Not Found for this DTC", "OK");
                        }
                    }
                    else
                    {
                        await this.DisplayAlert("", "GD Not Found for this DTC", "OK");
                    }




                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlert("", ex.ToString(), "OK");
            }


            #region Old Code
            //try
            //{

            //    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            //    {
            //        await Task.Delay(100);
            //        string model = string.Empty;
            //        var selectedItem = (DtcModel)((Button)sender).BindingContext;
            //        var res = services.Get_gd(App.JwtToken, 204, 13);

            //        #region Old Code
            //        //var Gd = GetGdauthor(App.JwtToken, selectedItem.code, model, res.Result);
            //        //await this.Navigation.PushAsync(new InfoPage(Gd, selectedItem.description, selectedItem.code));


            //        //////var ecuName = GetECU(selectedItem.NodeId.Id);
            //        ////if (selectedItem.code == "P0190-13")
            //        ////{
            //        ////    //model = $"{App.SelectedModel}_NA_NA_EMS";
            //        ////    model = "Pro6000 MDE5 And 8 BSVI_NA_NA_EMS";
            //        ////}
            //        ////else
            //        ////{
            //        ////    //model = $"{App.SelectedModel}_NA_NA_EMS";
            //        ////    model = "Pro2000 E366 DELPHI BSVI_NA_NA_EMS";
            //        ////}
            //        ////var json = DependencyService.Get<IGdLocalFile>().GetGdData().Result;
            //        //////Debug.WriteLine("CatchGDData   "+ json);
            //        ////var result = GetGdauthor(App.JwtToken, selectedItem.code, model, json);
            //        //////var result = apiServices.GetGdauthor(App.JwtToken, selectedItem.Identifier.FailureName, model, "GdJson.txt");
            //        ////if (result != null)
            //        ////{
            //        ////    await this.Navigation.PushAsync(new InfoPage(result.Result, selectedItem.description, selectedItem.code));
            //        ////}
            //        ///
            //        #endregion
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
            #endregion

        }

        public FirstListClass GetGdauthor(string token, string PCode, string model, dynamic json)
        {
            try
            {

                int index = 0;
                FirstListClass firstListClass = new FirstListClass();
                //TreeClass treeClass1 = new TreeClass();

                foreach (var pair_again in json["results"])
                {
                    GdMainModel mainModel = new GdMainModel();
                    GdImage gdImage = new GdImage();
                    TreeSet treeSet = new TreeSet();
                    List<TreeData> tree_data_list = new List<TreeData>();
                    //TreeData treeData = new TreeData();
                    foreach (var pair in pair_again)
                    {
                        var InfoKey = pair.Name;
                        var InfoValue = pair.Value;
                        if (pair.Name == "gd_id")
                        {
                            mainModel.gd_id = pair.Value.Value;
                        }
                        if (pair.Name == "causes")
                        {
                            mainModel.causes = pair.Value.Value;
                        }
                        else if (pair.Name == "effects_on_vehicle")
                        {
                            //var re = infoPair.Value;
                            mainModel.effects_on_vehicle = pair.Value.Value;
                        }
                        else if (pair.Name == "remedial_actions")
                        {
                            mainModel.remedial_actions = pair.Value.Value;
                        }
                        else if (pair.Name == "gd_description")
                        {
                            mainModel.gd_description = pair.Value.Value;
                        }
                        else if (pair.Name == "model")
                        {
                            mainModel.model = pair.Value.Value;
                        }
                        else if (pair.Name == "name")
                        {
                            mainModel.name = pair.Value.Value;
                        }
                        else if (pair.Name == "tree_set")
                        {
                            foreach (var treePair in pair.Value)
                            {
                                foreach (var pairr in treePair)
                                {
                                    if (pairr.Name == "id")
                                    {
                                        treeSet.id = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "is_active")
                                    {
                                        treeSet.is_active = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "model")
                                    {
                                        treeSet.model = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "tree_description")
                                    {
                                        treeSet.tree_description = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "tree_id")
                                    {
                                        treeSet.tree_id = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "vehicle_model")
                                    {
                                        treeSet.vehicle_model = pairr.Value.Value;
                                    }
                                    else if (pairr.Name == "tree_data")
                                    {
                                        foreach (var data in pairr)
                                        {
                                            foreach (var tree_data in data)
                                            {
                                                TreeData treeData = new TreeData();
                                                foreach (var ReData in tree_data)
                                                {
                                                    if (ReData.Name == "name")
                                                    {
                                                        treeData.name = ReData.Value.Value;
                                                    }
                                                    else if (ReData.Name == "parent")
                                                    {
                                                        treeData.parent = ReData.Value.Value;
                                                    }
                                                    else if (ReData.Name == "id")
                                                    {
                                                        treeData.id = ReData.Value.Value;
                                                    }
                                                    else if (ReData.Name == "description")
                                                    {
                                                        treeData.description = ReData.Value.Value;
                                                    }
                                                    else if (ReData.Name == "data")
                                                    {
                                                        foreach (var data_again in ReData)
                                                        {
                                                            foreach (var data1 in data_again)
                                                            {
                                                                if (data1.Name == "decisions")
                                                                {

                                                                    foreach (var deci in data1)
                                                                    {
                                                                        foreach (var deci1 in deci)
                                                                        {

                                                                            if (deci1.Name == "type")
                                                                            {
                                                                                treeData.data.decisions.type = deci1.Value.Value;
                                                                            }
                                                                            if (deci1.Name == "data")
                                                                            {

                                                                                foreach (var deci2 in deci1)
                                                                                {

                                                                                    foreach (var deci3 in deci2)
                                                                                    {
                                                                                        DecissionListModel decissionListModel = new DecissionListModel();
                                                                                        foreach (var deci4 in deci3)
                                                                                        {
                                                                                            if (deci4.Name == "node")
                                                                                            {
                                                                                                //decision.node = deci4.Value.Value;
                                                                                                decissionListModel.node = deci4.Value.Value;
                                                                                            }
                                                                                            if (deci4.Name == "text_val")
                                                                                            {
                                                                                                //decision.text_val = deci4.Value.Value;
                                                                                                decissionListModel.text_val = deci4.Value.Value;
                                                                                            }
                                                                                            if (deci4.Name == "type")
                                                                                            {
                                                                                                //decision.type = deci4.Value.Value;
                                                                                                decissionListModel.type = deci4.Value.Value;
                                                                                            }
                                                                                        }

                                                                                        treeData.data.decisions.data.Add(decissionListModel);
                                                                                    }

                                                                                }

                                                                            }
                                                                        }
                                                                        //reapeterClass.data.decisions.type = serr.data.decisions.type;
                                                                        //reapeterClass.data.decisions.data = serr.data.decisions.data;
                                                                    }
                                                                }
                                                                else if (data1.Name == "entry_script")
                                                                {
                                                                    treeData.data.entry_script = data1.Value.Value;
                                                                }
                                                                else if (data1.Name == "id")
                                                                {
                                                                    treeData.data.id = data1.Value.Value;
                                                                }
                                                                else if (data1.Name == "description")
                                                                {
                                                                    treeData.data.description = data1.Value.Value;
                                                                }
                                                                else if (data1.Name == "topic")
                                                                {
                                                                    treeData.data.topic = data1.Value.Value;
                                                                }
                                                                else if (data1.Name == "type_form")
                                                                {
                                                                    foreach (var TypeFormData in data1)
                                                                    {
                                                                        foreach (var ReTypeForm in TypeFormData)
                                                                        {
                                                                            if (ReTypeForm.Name == "topic")
                                                                            {
                                                                                treeData.data.type_form.topic = ReTypeForm.Value.Value;
                                                                            }
                                                                            else if (ReTypeForm.Name == "description")
                                                                            {
                                                                                treeData.data.type_form.description = ReTypeForm.Value.Value;
                                                                            }
                                                                            else if (ReTypeForm.Name == "groups")
                                                                            {
                                                                                foreach (var group in ReTypeForm)
                                                                                {
                                                                                    foreach (var group1 in group)
                                                                                    {
                                                                                        GroupModel groupModel = new GroupModel();
                                                                                        Group gr = new Group();
                                                                                        foreach (var group2 in group1)
                                                                                        {
                                                                                            if (group2.Name == "upper_limit")
                                                                                            {
                                                                                                gr.upper_limit = group2.Value.Value;
                                                                                            }
                                                                                            else if (group2.Name == "lower_limit")
                                                                                            {
                                                                                                gr.lower_limit = group2.Value.Value;
                                                                                            }
                                                                                            else if (group2.Name == "unit")
                                                                                            {
                                                                                                gr.unit = group2.Value.Value;
                                                                                            }
                                                                                            else if (group2.Name == "entry_description")
                                                                                            {
                                                                                                gr.entry_description = group2.Value.Value;
                                                                                            }
                                                                                            else if (group2.Name == "group_name")
                                                                                            {
                                                                                                gr.group_name = group2.Value.Value;
                                                                                            }
                                                                                        }
                                                                                        //serr.data.type_form.groups.Add(gr);
                                                                                        groupModel.entry_description = gr.entry_description;
                                                                                        groupModel.lower_limit = gr.lower_limit;
                                                                                        groupModel.unit = gr.unit;
                                                                                        groupModel.upper_limit = gr.upper_limit;
                                                                                        groupModel.group_name = gr.group_name;
                                                                                        treeData.data.type_form.groups.Add(groupModel);
                                                                                    }
                                                                                }
                                                                            }
                                                                            //else if (ReTypeForm.Name == "entry_group_names")
                                                                            //{
                                                                            //    foreach (var groupName in ReTypeForm)
                                                                            //    {
                                                                            //        foreach (var groupName1 in groupName)
                                                                            //        {
                                                                            //            var ite = groupName1[1].Value;
                                                                            //            foreach (var groupName2 in groupName1)
                                                                            //            {
                                                                            //                serr.data.type_form.entry_group_names = groupName2.Value.Value;
                                                                            //            }
                                                                            //        }
                                                                            //        //foreach (var groupName1 in groupName)
                                                                            //        //{
                                                                            //        //    foreach (var groupName2 in ReTypeForm)
                                                                            //        //    {
                                                                            //        //        serr.data.type_form.entry_group_names = groupName2.Value.Value;
                                                                            //        //    }
                                                                            //        //}
                                                                            //    }
                                                                            //}
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                        }
                                                    }

                                                }
                                                tree_data_list.Add(treeData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (pair.Name == "gd_images")
                        {
                            foreach (var imageList in pair)
                            {
                                foreach (var image in imageList)
                                {

                                    foreach (var image1 in image)
                                    {
                                        if (image1.Name == "image_name")
                                        {
                                            gdImage.gd_name = image1.Value.Value;
                                        }
                                        else if (image1.Name == "gd_image")
                                        {
                                            gdImage.gd_image = $"https://vecvdaliteplus.vecv.net/media/{image1.Value.Value}";
                                        }
                                        if (image1.Name == "id")
                                        {
                                            gdImage.id = image1.Value.Value;
                                        }
                                        else if (image1.Name == "model")
                                        {
                                            gdImage.model = image1.Value.Value;
                                        }
                                    }

                                }
                            }
                        }
                    }

                    firstListClass.main_list.Add(mainModel);
                    //index++;
                    firstListClass.main_list[index].gd_image.Add(gdImage);
                    treeSet.tree_data = tree_data_list;
                    firstListClass.main_list[index].tree_set.Add(treeSet);
                }
                return firstListClass;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("CatchGDData   " + json);
                return null;
            }
        }

        public async Task RefreshDTC()
        {
            viewModel.ServerDtcList.Clear();
            viewModel.EcuDtcList.Clear();
            viewModel.DtcList.Clear();

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
                BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                Opacity = opacity,
                ClassId = Convert.ToString(0),
            };

            grd.Children.Clear();

            grd_tab.ColumnDefinitions.Clear();

            CollectionView collectionView = new CollectionView { };

            collection_view.Children.Add(collectionView, 0, 0);

            collection_view.Children.Clear();

            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(200);
                    await CreateDynamicallyaDesign();
                }
            });
            //show_alert("Alert", $"DTC Refreshed", false, true);
        }

        private async void btnRefresh_Clicked(object sender, EventArgs e)
        {
            try
            {
                await RefreshDTC();
            }
            catch (Exception ex)
            {
                show_alert("Alert", $"DTC Not Refreshing", false, true);
            }
            //switch (Device.RuntimePlatform)
            //{
            //    case Device.iOS:
            //        //top = 20;
            //        break;
            //    case Device.Android:
            //        var Clear_dtc_android = await DependencyService.Get<Interfaces.IBth>().ClearDtc(StaticData.clear_dtc_index);
            //        break;
            //    case Device.UWP:
            //        var Clear_dtc_uwp = await DependencyService.Get<Interfaces.IConnectionUSB>().ClearDtc(StaticData.clear_dtc_index);
            //        break;
            //    default:
            //        //top = 0;
            //        break;
            //}
        }

        private async void btnClear_Clicked(object sender, EventArgs e)
        {
            var clear_dtc_index = StaticData.ecu_info.FirstOrDefault(X => X.ecu_name == selected_ecu).clear_dtc_index;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //top = 20;
                    break;
                case Device.Android:
                    if (App.ConnectedVia == "USB")
                    {
                        var Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ClearDtc(clear_dtc_index);
                        if (Clear_dtc_android != null)
                        {
                            if (Clear_dtc_android.Contains("NOERROR"))
                            {
                                var ClearDTC = new List<ClearDtcRecord>();
                                ClearDTC.Add(new ClearDtcRecord { session = jobCard.id, status = "NOERROR" });

                                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                                if (isReachable)
                                {
                                    services.ClearDtcRecord(ClearDTC, App.JwtToken, App.SessionId);
                                }
                                show_alert("Alert", $"DTC Cleared", false, true);
                            }
                            else
                            {
                                show_alert("Alert", $"Negative Acknowledgement \n\n {Clear_dtc_android}", false, true);
                            }
                        }
                        else
                        {
                            show_alert("Alert", $"Negative Acknowledgement \n\n Please check Dongle Connection status !!!", false, true);
                        }
                    }
                    else
                    {
                        var Clear_dtc_android = await DependencyService.Get<Interfaces.IBth>().ClearDtc(clear_dtc_index);
                        if (Clear_dtc_android != null)
                        {
                            if (Clear_dtc_android.Contains("NOERROR"))
                            {
                                var ClearDTC = new List<ClearDtcRecord>();
                                ClearDTC.Add(new ClearDtcRecord { session = jobCard.id, status = "NOERROR" });

                                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                                if (isReachable)
                                {
                                    services.ClearDtcRecord(ClearDTC, App.JwtToken, App.SessionId);
                                }

                                await DisplayAlert("Alert", "DTC Cleared", "OK");
                                //show_alert("Alert", $"DTC Cleared", false, true);
                            }
                            else
                            {
                                show_alert("Alert", $"Negative Acknowledgement \n\n {Clear_dtc_android}", false, true);
                            }
                        }
                        else
                        {
                            show_alert("Alert", $"Negative Acknowledgement \n\n Please check Dongle Connection status !!!", false, true);
                        }
                    }
                    break;
                case Device.UWP:
                    var Clear_dtc_uwp = await DependencyService.Get<Interfaces.IConnectionUSB>().ClearDtc(clear_dtc_index);
                    break;
                default:
                    //top = 0;
                    break;
            }
            //foreach (var ecu in StaticData.ecu_info)
            //{
            //    var read_dtc_index = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).read_dtc_index;
            //    var ecu_dtc = await Task.Run(async () =>
            //    {
            //        var result = await viewModel.readDtc(read_dtc_index, ecu.ecu_name);
            //        return result;
            //    });
            //}            
        }
    }
}