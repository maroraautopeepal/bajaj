using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface ISaveLocalData
    {
        string GetData(string file_name);
        Task SaveData(string file_name, string Data);
    }
}
