using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bajaj.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IConnectivity connectivity = CrossConnectivity.Current;

        public BaseViewModel()
        {
            this.connectivity.ConnectivityChanged += OnConnectivityChanged;

            connection();

            //CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //    }
            //    else
            //    {
            //    }

            //};
        }

        public async void connection()
        {
            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
            if (!isReachable)
            {
                //DisplayAlertPage();
                App.NetConnected = false;
            }
            else
            {
                App.NetConnected = true;
            }
        }

        protected bool SetProperty<T>(
          ref T backingStore, T value,
          [CallerMemberName] string propertyName = "",
          Action onChanged = null)
        {


            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;

            if (onChanged != null)
                onChanged();

            OnPropertyChanged(propertyName);
            return true;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            //IsConnected = e.IsConnected;

            if (e.IsConnected)
            {
                //var stringResponse = "A problem has occurred with your network connection.";
                //this.PageDialog.Toast(stringResponse, TimeSpan.FromSeconds(2));
                App.NetConnected = true;
            }
            else
            {
                App.NetConnected = false;
            }
        }
    }
}