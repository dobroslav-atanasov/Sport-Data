namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class TableModel
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

    public RoundModel Round { get; set; }

    public string Format { get; set; }

    public bool IsGroup { get; set; } = false;

    public int Number { get; set; }

    public List<TableModel> Groups { get; set; } = new();

    public HtmlNodeCollection Rows { get; set; }

    public List<string> Headers { get; set; } = new();

    public Dictionary<string, int> Indexes { get; set; } = new();
}