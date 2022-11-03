using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IBlueToothDevices
    {
        Task SearchBT();
        void GetDongles();
        Task EndScanning();
        bool PairDevice(string bluetoothName, string bluetoothAddress);
        bool ConnectDevice(string bluetoothName, string bluetoothAddress);
        string SendSecurityCommand(string SecurityCommand);
        string SendDongleVersionCommand(string SecurityCommand);
        string SendCommandToECU(string EcuCommand);
    }
}
