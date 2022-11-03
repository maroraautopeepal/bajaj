using Microsoft.AspNetCore.SignalR.Client;
using MultiEventController.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MultiEventController
{
    public class ControlEventManager
    {
        public event EventHandler OnRecieved;
        public event EventHandler OnRecievedData;

        public HubConnection hubConnection;
        public ElementEventHandler ObjectReceiveValue { get; set; }
        public string ObjectReceiveValueData { get; set; }

        string urlMain = "https://sutraveinfotech.com/sendhub";
        string connection_result = string.Empty;
        //public async Task Init(string url = "https://sutraveinfotech.com/sendhub")
        public async Task<string> Init(string url = "https://www.sutraveinfotech.com/sendhub")
        {
            try
            {
                hubConnection = new HubConnectionBuilder().WithUrl(url).Build();

                hubConnection.On<ElementEventHandler>("ElementEventReceiveMessage", (value) =>
                {
                    ObjectReceiveValue = value;
                    OnRecieved?.Invoke(value, new EventArgs());
                });

                hubConnection.On<string>("ElementEventReceiveMessageData", (value) =>
                {
                    ObjectReceiveValueData = value;
                    OnRecievedData?.Invoke(value, new EventArgs());
                });

                if (hubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await hubConnection.StartAsync();
                        connection_result = "Connected";
                    }
                    catch (System.Exception ex)
                    {
                        connection_result = ex.Message;
                    }
                }
                return connection_result;
            }
            catch (Exception)
            {
                return connection_result = "Disconnect";
            }
        }
        public void SendRequestControlEvents(ElementEventHandler elementEventHandler)
        {
            try
            {
                if (elementEventHandler.IsExpert)
                {
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        //if (ReceiveValue == null)
                        hubConnection.SendAsync("ElementEventSendMessageFromServer", elementEventHandler);

                        //else if (ReceiveValue.ElementValue != elementEventHandler.ElementValue)
                        //    hubConnection.SendAsync("ElementEventSendMessageFromServer", elementEventHandler);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public void SendRequestData(string DtcDataModelJson)
        {
            try
            {
                //DependencyService.Get<IBth>().ReadPid(viewModel.PidPrecondition);
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    hubConnection.SendAsync("ElementEventSendMessageFromServerData", DtcDataModelJson);
                }
            }
            catch (Exception)
            {

            }
        }
        //int i = 0;
        public void RecieveCallControlEvents(Page page, ElementEventHandler sender, string ownerId, List<Object> list = null)
        {
            try
            {
                var elementEventHandler = sender as ElementEventHandler;
                if (elementEventHandler == null || string.IsNullOrEmpty(elementEventHandler.ElementName))
                    throw new Exception("Please Enter Element Name");

                if (elementEventHandler.ToUserId == ownerId)
                {
                    var control = page?.FindByName(elementEventHandler.ElementName);

                    var controleType = control?.GetType()?.Name;

                    if (control == null)
                    {
                        if (elementEventHandler.ElementName == "GoBack")
                        {
                            controleType = elementEventHandler.ElementName;
                        }
                    }

                    switch (controleType)
                    {
                        case "Image":
                            var img = control as Image;
                            TapGestureRecognizer tapGesture = (TapGestureRecognizer)img.GestureRecognizers[0];
                            if (tapGesture != null)
                            {
                                tapGesture.SendTapped(img);
                            }
                            break;
                        case "Button":
                            (control as IButtonController)?.SendClicked();
                            break;
                        case "Entry":
                            (control as Entry).Text = elementEventHandler.ElementValue;
                            break;
                        case "ScrollView":
                            (control as ScrollView)?.ScrollToAsync(elementEventHandler.ScrollX, elementEventHandler.ScrollY, true);
                            break;
                        case "ImageButton":
                            (control as IButtonController)?.SendClicked();
                            break;
                        case "StackLayout":
                            var Stack = control as StackLayout;
                            TapGestureRecognizer StacktapGesture = (TapGestureRecognizer)Stack.GestureRecognizers[0];
                            if (StacktapGesture != null)
                            {
                                StacktapGesture.SendTapped(Stack);
                            }
                            break;
                        case "Grid":
                            var GridC = control as Grid;
                            TapGestureRecognizer GridGesture = (TapGestureRecognizer)GridC.GestureRecognizers[0];
                            if (GridGesture != null)
                            {
                                GridGesture.SendTapped(GridC);
                            }
                            break;
                        case "GoBack":
                            page.Navigation.PopAsync(true);
                            elementEventHandler.ElementName = "AlreadyGoBack";
                            break;
                        case "ListView":
                            var listView = control as ListView;
                            if (list != null)
                            {
                                var item = listView.SelectedItem = list[int.Parse(sender.ElementValue)];
                            }

                            // var ListViewEvent = control as ListView;
                            //ItemTappedEventArgs ItemTapped = (ItemTappedEventArgs)elementEventHandler.eValues;
                            //var ListItemTap = control as ItemTappedEventArgs;
                            //ItemTappedEventArgs IT = (ItemTappedEventArgs)ListItemTap.Item;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        //private static void RecieveCallControlEvents(Page page, string elementName, string elementValue = "")
        //{
        //    var control = page.FindByName(elementName);
        //    if (control == null)
        //        return;

        //    var controleType = control?.GetType()?.Name;
        //    if (string.IsNullOrEmpty(controleType))
        //        return;

        //    switch (controleType)
        //    {
        //        case "Button":
        //            (control as IButtonController)?.SendClicked();
        //            break;

        //        case "Entry":
        //            (control as Entry).Text = elementValue;
        //            break;

        //        case "ScrollView":
        //            string[] totalAxis = elementValue?.Split('-');
        //            var x = System.Convert.ToDouble(totalAxis[0]);
        //            var y = System.Convert.ToDouble(totalAxis[1]);
        //            (control as ScrollView)?.ScrollToAsync(x, y, true);
        //            break;

        //        default:
        //            break;
        //    }
        //}

    }
}
