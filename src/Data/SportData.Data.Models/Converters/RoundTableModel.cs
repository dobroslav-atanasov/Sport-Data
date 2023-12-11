namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class RoundTableModel
{
    public int Order { get; set; }

    public string OriginalHtml { get; set; }

    public string Html { get; set; }

    public int EventId { get; set; }

    public HtmlDocument HtmlDocument
    {
        get
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(this.Html);

            return htmlDocument;
        }
    }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public string Title { get; set; }

    public RoundModel Type { get; set; }

    public string Format { get; set; }

    public List<GroupModel> Groups { get; set; } = new();

    public HtmlNodeCollection Rows { get; set; }

    public List<string> Headers { get; set; } = new();
}