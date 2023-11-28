namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class DocumentModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public DateTime? From { get; set; }

    public string Html { get; set; }

    public HtmlDocument HtmlDocument { get; set; }

    public List<RoundTableModel> Tables { get; set; } = new();
}