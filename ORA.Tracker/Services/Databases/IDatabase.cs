namespace ORA.Tracker.Services.Databases
{
    public interface IDatabase
    {
        void Close();

        byte[] Get(byte[] key);
        byte[] GetAll();

        void Put(byte[] key, byte[] data);

        void Delete(byte[] key);
    }
}
