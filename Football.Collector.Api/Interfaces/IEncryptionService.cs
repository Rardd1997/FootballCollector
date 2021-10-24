namespace Football.Collector.Interfaces
{
    public interface IEncryptionService
    {
        bool ValidatePassword(string clearPassword, string passwordHash);

        string GetPasswordHash(string password);
    }
}
