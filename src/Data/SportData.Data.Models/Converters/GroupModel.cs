namespace SportData.Data.Models.Converters;

using HtmlAgilityPack;

using SportData.Data.Entities.Enumerations;

public class GroupModel
{
    public int Number { get; set; }

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

    public RoundStatus Status { get; set; }

    public bool IsGroup { get; set; } = true;

    //public double? Wind { get; set; }


    public HtmlNodeCollection Rows { get; set; }

    public List<string> Headers { get; set; } = new();
}