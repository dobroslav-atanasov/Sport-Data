namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.SportDataDb.Interfaces;
using SportData.Services.Interfaces;

public class NOCConverter : BaseConverter
{
    private readonly ICountriesService countriesService;
    private readonly INOCsService nocsService;

    public NOCConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, ICountriesService countriesService, INOCsService nocsService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService)
    {
        this.countriesService = countriesService;
        this.nocsService = nocsService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var countryDocument = this.CreateHtmlDocument(group.Documents.Single(d => d.Order == 1));
            var header = countryDocument.DocumentNode.SelectSingleNode("//h1");
            var noc = new NOC();
            var match = this.RegExpService.Match(header.InnerText, @"(.*?)\((.*?)\)");
            if (match != null)
            {
                noc.Name = match.Groups[1].Value.Decode().Trim();
                noc.Code = match.Groups[2].Value.Decode().Trim().ToUpper();
                noc.RelatedNOCCode = this.FindRelatedCountry(noc.Code);
            }

            if (noc.Code != null && noc.Code != "UNK" && noc.Code != "CRT")
            {
                var countryDescription = countryDocument
                    .DocumentNode
                    .SelectSingleNode("//div[@class='description']")
                    .OuterHtml
                    .Decode();

                noc.CountryDescription = this.RegExpService.CutHtml(countryDescription);

                if (group.Documents.Count > 1)
                {
                    var committeeDocument = this.CreateHtmlDocument(group.Documents.Single(d => d.Order == 2));
                    var title = committeeDocument.DocumentNode.SelectSingleNode("//h1").InnerText.Decode();
                    noc.Title = title;

                    var abbreavition = this.RegExpService.MatchFirstGroup(committeeDocument.DocumentNode.OuterHtml, @"<tr>\s*<th>Abbreviation<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
                    var foundedYear = this.RegExpService.MatchFirstGroup(committeeDocument.DocumentNode.OuterHtml, @"<tr>\s*<th>Founded<\/th>\s*<td>([\d]*)<\/td>\s*<\/tr>");
                    var disbandedYear = this.RegExpService.MatchFirstGroup(committeeDocument.DocumentNode.OuterHtml, @"<tr>\s*<th>Disbanded<\/th>\s*<td>([\d]*)<\/td>\s*<\/tr>");
                    var recognizedYear = this.RegExpService.MatchFirstGroup(committeeDocument.DocumentNode.OuterHtml, @"<tr>\s*<th>Recognized by the IOC<\/th>\s*<td>([\d]*)<\/td>\s*<\/tr>");

                    noc.Abbreviation = !string.IsNullOrEmpty(abbreavition) ? abbreavition : null;
                    noc.FoundedYear = !string.IsNullOrEmpty(foundedYear) ? int.Parse(foundedYear) : null;
                    noc.DisbandedYear = !string.IsNullOrEmpty(disbandedYear) ? int.Parse(disbandedYear) : null;
                    noc.RecognationYear = !string.IsNullOrEmpty(recognizedYear) ? int.Parse(recognizedYear) : null;

                    var committeeDescription = committeeDocument
                        .DocumentNode
                        .SelectSingleNode("//div[@class='description']")?
                        .OuterHtml?
                        .Decode();

                    noc.NOCDescription = !string.IsNullOrEmpty(committeeDescription) ? this.RegExpService.CutHtml(committeeDescription) : null;
                }

                var countryCode = this.NormalizeService.MapOlympicGamesCountriesAndWorldCountries(noc.Code);
                if (countryCode != null)
                {
                    var country = await this.countriesService.GetAsync(countryCode);
                    noc.CountryId = country.Id;
                    noc.CountryFlag = country.Flag;
                }
                else
                {
                    noc.CountryId = null;
                }

                await this.nocsService.AddOrUpdateAsync(noc);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    public string FindRelatedCountry(string code)
    {
        string relatedCountryCode = null;
        switch (code)
        {
            case "ANZ":
                relatedCountryCode = "AUS";
                break;
            case "TCH":
            case "BOH":
                relatedCountryCode = "CZE";
                break;
            case "GDR":
            case "FRG":
            case "SAA":
                relatedCountryCode = "GER";
                break;
            case "MAL":
            case "NBO":
                relatedCountryCode = "MAS";
                break;
            case "AHO":
                relatedCountryCode = "NED";
                break;
            case "ROC":
            case "EUN":
            case "URS":
                relatedCountryCode = "RUS";
                break;
            case "YUG":
            case "SCG":
                relatedCountryCode = "SRB";
                break;
            case "YMD":
            case "YAR":
                relatedCountryCode = "YEM";
                break;
        }

        return relatedCountryCode;
    }
}