namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

public class HeatModel
{
    public string Html { get; set; }

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

    public double? Wind { get; set; }
}