using Bajaj.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IConnectionRP
    {
        Task<ObservableCollection<BluetoothDevicesModel>> get_device_list();

        Task<string> ConnectDevice(string IP);

        Task<ReadDtcResponseModel> ReadDtc(string indexKey);

        Task<string> ClearDtc(string dtc_index);

        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<PidCode> pidList);

        Task<string> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file);

        Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList);
        
    }
}
