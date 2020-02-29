using System.Threading.Tasks;

namespace TwitterLite.Contracts.Services
{
    public interface IFileService
    {
        bool CheckIfFileExists(string fileName);
    }
}
