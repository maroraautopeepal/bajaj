using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IGdLocalFile
    {
        Task SaveGdData(string Data);
        Task<string> GetLogsData(string path);

        Task<string> GetGdData();
    }
}
