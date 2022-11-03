using Bajaj.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IBth
    {
        Task<string> Start(string name, int sleepTime, bool readAsCharArray);
        void Cancel();
        ObservableCollection<BluetoothDevicesModel> PairedDevices();
        Task<bool> CheckBtConnection();
        Task<string> GetFirmware1();
        Task<string> GetFirmware();
        //Task<string> GetMacIdCommand(string command);

        Task<string> GetDongleMacID(bool is_disconnct);

        Task<string> GetDongleMacID(bool is_disconnct, string protocol_name, uint protocol_value, string tx_header, string rx_header);
        Task<string> SetDongleProperties(string protocol_name);
        Task SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp);
        Task<string> SetDongleProperties();
        //500C65EFA86574FE9A890018CE16
        Task<string> Connect();
        Task<string[]> GetSsidPassword();
        Task<ReadDtcResponseModel> ReadDtc(string indexKey);
        Task<string> ClearDtc(string indexKey);
        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<PidCode> pidList);

        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<ReadParameterPID> pidList);

        Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList);

        Task<string> unlockEcu(ResultUnlock unlockData);

        void StartTesterPresent();

        void StopTesterPresent();

        Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string start_command);

        Task<TestRoutineResponseModel> RequestIorTest(string request_command);

        Task<TestRoutineResponseModel> StopIorTest(string stop_command);

        Task<string> StartECUFlashing(string flashJson,string interpreter, Ecu2 ecu2, SeedkeyalgoFnIndex sklFIN, List<EcuMapFile> ecu_map_file);
        Task<string> GetDongleMacIDForTerminal(bool is_disconnct, string protocol);

        Task<string[]> SendTerminalCommands(string[] commands);

        Task<string> SetData(string commands);

        Task<ReadPidPresponseModel> SetTestRoutineCommand(string command);

        Task<ObservableCollection<Model.IvnReadDtcResponseModel>> IVN_ReadDtc(List<string> FrameIDC);

        Task<ObservableCollection<ReadPidPresponseModel>> IVN_ReadPid(ObservableCollection<IVN_SelectedPID> IVNpidList);

        Task<float> FlashingData();

        Task<ObservableCollection<WriteParameter_Status>> WriteAtuatorTest(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList, bool IsPlay);
    }
}
