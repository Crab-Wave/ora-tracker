using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Databases
{
    public interface IClusterDatabase
    {
        void Close();

        byte[] Get(byte[] key);
        byte[] GetAll();

        void Put(byte[] key, byte[] cluster);

        void Delete(byte[] key);
    }
}
