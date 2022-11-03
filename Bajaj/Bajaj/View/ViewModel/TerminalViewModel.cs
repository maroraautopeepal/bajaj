using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class TerminalViewModel : BaseViewModel
    {
        public TerminalViewModel(string[] Result)
        {
            DataList = new ObservableCollection<DataClass>(); 

            if (Result==null)
            {
                ConnectImage = "ic_link.png";
                TxHeader = "Not Set";
                Protocol = "Not Set";
                RxHeader = "Not Set";
                IsPadding = "Disabled";
                IsTesterPresent = "Disabled";
                ButtonEnable = false;
                WriteEnable = true;
                FV = string.Empty;
            }
            else
            {
                ConnectImage = "ic_unlink.png";
                ButtonEnable = true;
                WriteEnable = false;
                FV = Result[6];
                if (string.IsNullOrEmpty(Result[5]))
                {
                    Protocol = "Not Set";
                }
                else
                {
                    Protocol = Result[5];
                }

                if (string.IsNullOrEmpty(Result[1]))
                {
                    TxHeader = "Not Set";
                }
                else
                {
                    TxHeader = Result[1];
                }

                if (string.IsNullOrEmpty(Result[2]))
                {
                    RxHeader = "Not Set";
                }
                else
                {
                    RxHeader = Result[2];
                }

                if (string.IsNullOrEmpty(Result[3]))
                {
                    IsPadding = "Disabled";
                }
                else
                {
                    IsPadding = Result[3];
                }

                if (string.IsNullOrEmpty(Result[4]))
                {
                    IsTesterPresent = "Disabled";
                }
                else
                {
                    IsTesterPresent = Result[4];
                }
               
            }
        }

        private string connectImage;
        public string ConnectImage
        {
            get => connectImage;
            set
            {
                connectImage = value;
                OnPropertyChanged("ConnectImage");
            }
        }

        private string protocol;
        public string Protocol
        {
            get => protocol;
            set
            {
                protocol = value;
                OnPropertyChanged("Protocol");
            }
        }

        private string txHeader;
        public string TxHeader
        {
            get => txHeader;
            set
            {
                txHeader = value;
                OnPropertyChanged("TxHeader");
            }
        }

        private string rxHeader;
        public string RxHeader
        {
            get => rxHeader;
            set
            {
                rxHeader = value;
                OnPropertyChanged("RxHeader");
            }
        }

        private string isPadding;
        public string IsPadding
        {
            get => isPadding;
            set
            {
                isPadding = value;
                OnPropertyChanged("IsPadding");
            }
        }

        private string isTesterPresent;
        public string IsTesterPresent
        {
            get => isTesterPresent;
            set
            {
                isTesterPresent = value;
                OnPropertyChanged("IsTesterPresent");
            }
        }

        private bool buttonEnable;
        public bool ButtonEnable
        {
            get => buttonEnable;
            set
            {
                buttonEnable = value;
                OnPropertyChanged("ButtonEnable");
            }
        }

        private bool writeEnable;
        public bool WriteEnable
        {
            get => writeEnable;
            set
            {
                writeEnable = value;
                OnPropertyChanged("WriteEnable");
            }
        }

        private string senderCommand = "1902ff";
        public string SenderCommand
        {
            get => senderCommand;
            set
            {
                senderCommand = value;
                OnPropertyChanged("SenderCommand");
            }
        }

        private ObservableCollection<DataClass> dataList;
        public ObservableCollection<DataClass> DataList
        {
            get => dataList;
            set
            {
                dataList = value;
                OnPropertyChanged("DataList");
            }
        }

        private string fV;
        public string FV
        {
            get => fV;
            set
            {
                fV = value;
                OnPropertyChanged("FV");
            }
        }
    }

    public class DataClass
    {
        public string SendCommand { get; set; }
        public string ReceiveCommand { get; set; }
    }

}
