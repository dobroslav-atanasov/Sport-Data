namespace SportData.Web.Services;

public interface IShortStringService
{
    string GetShort(string text, int minLength);
}

public class ShortStringService : IShortStringService
{
    public string GetShort(string text, int minLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (text.Length <= minLength)
        {
            return text;
        }

        return text.Substring(0, minLength) + "...";
    }
}
