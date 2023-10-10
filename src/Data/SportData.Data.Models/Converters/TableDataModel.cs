namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class TableDataModel
{
    public HtmlNodeCollection Rows { get; set; }

    public List<string> Headers { get; set; }

    public Dictionary<string, int> Indexes { get; set; }
}