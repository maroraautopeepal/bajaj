using Bajaj.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IConnectionUSB
    {
        //Task Start(string name, int sleepTime, bool readAsCharArray);
        void Cancel();
        //ObservableCollection<BluetoothDevicesModel> PairedDevices();

        //Task<string> Connect();
        Task<string> GetDongleMacID(bool is_disconnct);
        Task<string> SetDongleProperties();
        void DisconnectUSB();
        Task<ReadDtcResponseModel> ReadDtc(string indexKey);
        Task<string> ClearDtc(string indexKey);

        void SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp);

        Task<string> unlockEcu(ResultUnlock unlockData);
        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<PidCode> pidList);
        Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList);

        //Task<object> ReadPid(ObservableCollection<ReadParameterPID> pidList);
        //Task<object> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList);
        Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList);
        Task<string> StartECUFlashing(string flashJson, string interpreter, Ecu2 ecu2, SeedkeyalgoFnIndex sklFIN, List<EcuMapFile> ecu_map_file);
        //Task<string> ClearDtc(string dtc_index);
        //Task<float> GetRuntimeFlashPercent();
        Task TXheader(string tx_header);
        Task RXheader(string rx_header);

        Task<string[]> SendTerminalCommands(string[] commands);
        Task<string> SetData(string commands);

        Task<ObservableCollection<WriteParameter_Status>> WriteAtuatorTest(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList, bool IsPlay);
        Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string
            start_command, string request_command, string stop_command, bool test_condition, int bit_position,
            List<string> active_command, string stopped_command, string fail_command, bool is_stop, int time_base);

        Task<TestRoutineResponseModel> ContinueIorTest(string seed_key, string write_para_index, string start_command,
            string request_command, string stop_command, bool test_condition, int bit_position, List<string> active_command,
           string stopped_command, string fail_command, bool is_stop, int time_base, bool is_timebase);

        void StartTesterPresent();
        void StopTesterPresent();

        Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string start_command);

        Task<TestRoutineResponseModel> RequestIorTest(string request_command);

        Task<TestRoutineResponseModel> StopIorTest(string stop_command);

        Task<ObservableCollection<Model.IvnReadDtcResponseModel>> IVN_ReadDtc(List<string> FrameIDC);

        Task<ObservableCollection<ReadPidPresponseModel>> IVN_ReadPid(ObservableCollection<IVN_SelectedPID> IVNpidList);

        Task<float> FlashingData();
    }
}
