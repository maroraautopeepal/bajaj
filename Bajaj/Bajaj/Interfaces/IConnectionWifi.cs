using Bajaj.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IConnectionWifi
    {
        Task<ObservableCollection<BluetoothDevicesModel>> get_device_list();
        Task<ObservableCollection<BluetoothDevicesModel>> enable_hotspots();
        void Connect_dongle();
        Task<string> SendFotaCommand(string command);
        Task<string> GetDongleMacID(string IP, bool is_disconnct);
        Task<string> SetDongleProperties();
        void SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp);
        Task<bool> CheckConnection();
        //Task<string> GetWifiMacIdCommand(string command);
        Task<string[]> GetSsidPassword();
        Task<string> GetFirmware1();
        Task<string> GetFirmware(string IP);
        Task<string> get_ipaddress();

        Task<string> Write_SSIDPassword(string RouterSSID, string RouterPassword);

        //Task<string> Connect(string IP);

        // DTC
        Task<ReadDtcResponseModel> ReadDtc(string indexKey);
        Task<string> ClearDtc(string indexKey);

        Task<string[]> SendTerminalCommands(string IP, string[] commands);

        Task<string> SetData(string commands);

        Task<string> unlockEcu(ResultUnlock unlockData);

        // PID
        //Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<ReadParameterPID> pidList);
        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<PidCode> pidList);

        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList);

        //Write PR
        Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList);

        //Flashing
        Task<string> StartECUFlashing(string flashJson, string interpreter, Ecu2 ecu2, SeedkeyalgoFnIndex sklFIN, List<EcuMapFile> ecu_map_file);

        Task<ObservableCollection<WriteParameter_Status>> WriteAtuatorTest(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList, bool IsPlay);

        Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string start_command);

        Task<TestRoutineResponseModel> RequestIorTest(string request_command);

        Task<TestRoutineResponseModel> StopIorTest(string stop_command);

        void StartTesterPresent();
        void StopTesterPresent();

        Task<ObservableCollection<Model.IvnReadDtcResponseModel>> IVN_ReadDtc(List<string> FrameIDC);

        Task<ObservableCollection<ReadPidPresponseModel>> IVN_ReadPid(ObservableCollection<IVN_SelectedPID> IVNpidList);

        Task<float> FlashingData();
    }
}
