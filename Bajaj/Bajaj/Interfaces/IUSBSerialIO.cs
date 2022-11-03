using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IUSBSerialIO
    {
        Task<bool> Connect();


    }
}
