namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class DocumentModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Format { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public string Html { get; set; }

    public HtmlDocument HtmlDocument { get; set; }

    public RoundModel Round { get; set; }

    public List<TableModel> Tables { get; set; } = new();
}