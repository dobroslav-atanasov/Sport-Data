namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

using SportData.Data.Entities.Enumerations;

public class TableModel
{
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

    public string Title { get; set; }

    public RoundType Round { get; set; }

    public string RoundInfo { get; set; }

    public GroupType GroupType { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}