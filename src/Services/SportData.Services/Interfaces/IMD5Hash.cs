namespace SportData.Services.Interfaces;

public interface IMD5Hash
{
    string Hash(byte[] data);
}