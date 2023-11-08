namespace SportData.Converters.OlympicGames;

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Crawlers;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;
using SportData.Services.Interfaces;

public class AthleteConverter : BaseOlympediaConverter
{
    private readonly IDateService dateService;
    private readonly IAthletesService athletesService;
    private readonly INationalitiesService nationalitiesService;

    public AthleteConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService,
        IRegExpService regExpService, INormalizeService normalizeService, IDataCacheService dataCacheService, IOlympediaService olympediaService,
        IDateService dateService, IAthletesService athletesService, INationalitiesService nationalitiesService)
        : base(logger, crawlersService, logsService, groupsService, zipService, regExpService, normalizeService, dataCacheService, olympediaService)
    {
        this.dateService = dateService;
        this.athletesService = athletesService;
        this.nationalitiesService = nationalitiesService;
    }

    protected override async Task ProcessGroupAsync(Group group)
    {
        try
        {
            var document = this.CreateHtmlDocument(group.Documents.Single());
            var number = int.Parse(new Uri(group.Documents.Single().Url).Segments.Last());
            var name = document.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();
            var typeMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>Roles<\/th>\s*<td>(.*?)<\/td><\/tr>");
            var genderMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>Sex<\/th>\s*<td>(Male|Female)<\/td><\/tr>");
            var bornMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>Born<\/th>\s*<td>(.*?)<\/td><\/tr>");
            var diedMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>Died<\/th>\s*<td>(.*?)<\/td><\/tr>");

            var athlete = new Athlete
            {
                Name = name,
                Number = number,
                EnglishName = this.NormalizeService.ReplaceNonEnglishLetters(name),
                Type = this.NormalizeService.MapAthleteType(typeMatch.Groups[1].Value),
                Gender = genderMatch.Groups[1].Value.UpperFirstChar().ToEnum<GenderType>(),
                FullName = this.RegExpService.MatchFirstGroup(document.ParsedText, @"<tr>\s*<th>Full name<\/th>\s*<td>(.*?)<\/td><\/tr>")?.Replace("•", " "),
                Association = this.ExtractAssociations(document.ParsedText),
                Description = this.RegExpService.CutHtml(this.RegExpService.MatchFirstGroup(document.ParsedText, @"<div class=(?:""|')description(?:""|')>(.*?)<\/div>")),
                Nationality = this.ExtractNationality(document.ParsedText),
                BirthDate = bornMatch != null ? this.dateService.ParseDate(bornMatch.Groups[1].Value).From : null,
                BirthPlace = bornMatch != null ? this.RegExpService.MatchFirstGroup(bornMatch.Groups[1].Value, @"<a href=(?:""|')\/place_names\/[\d]+(?:""|')>(.*?)<\/a>") : null,
                DiedDate = diedMatch != null ? this.dateService.ParseDate(diedMatch.Groups[1].Value).From : null,
                DiedPlace = diedMatch != null ? this.RegExpService.MatchFirstGroup(diedMatch.Groups[1].Value, @"<a href=(?:""|')\/place_names\/[\d]+(?:""|')>(.*?)<\/a>") : null,
            };

            var measurmentsMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>Measurements<\/th>\s*<td>(.*?)<\/td><\/tr>");
            if (measurmentsMatch != null)
            {
                var heightMatch = this.RegExpService.Match(measurmentsMatch.Groups[1].Value, @"([\d]+)\s*cm");
                if (heightMatch != null)
                {
                    athlete.Height = int.Parse(heightMatch.Groups[1].Value);
                }

                var weightMatch = this.RegExpService.Match(measurmentsMatch.Groups[1].Value, @"([\d]+)\s*kg");
                if (weightMatch != null)
                {
                    athlete.Weight = int.Parse(weightMatch.Groups[1].Value);
                }
            }

            athlete = await athletesService.AddOrUpdateAsync(athlete);

            var nocMatch = this.RegExpService.Match(document.ParsedText, @"<tr>\s*<th>NOC(?:\(s\))?<\/th>\s*<td>(.*?)<\/td>\s*<\/tr>");
            if (nocMatch != null)
            {
                var countryCodes = this.OlympediaService.FindCountryCodes(nocMatch.Groups[1].Value);
                foreach (var code in countryCodes)
                {
                    var nocCacheModel = this.DataCacheService.NOCCacheModels.FirstOrDefault(c => c.Code == code);
                    if (nocCacheModel != null)
                    {
                        await this.nationalitiesService.AddOrUpdateAsync(new Nationality { AthleteId = athlete.Id, NOCId = nocCacheModel.Id });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process group: {group.Identifier}");
        }
    }

    private string ExtractNationality(string text)
    {
        var match = this.RegExpService.Match(text, @"<tr>\s*<th>Nationality<\/th>\s*<td>(.*?)<\/td><\/tr>");
        if (match != null)
        {
            var countryMatch = this.RegExpService.Match(match.Groups[1].Value, @"<a href=""/countries/(.*?)"">(.*?)</a>");
            if (countryMatch != null)
            {
                return countryMatch.Groups[2].Value.Trim();
            }
        }

        return null;
    }

    private string ExtractAssociations(string text)
    {
        var associationMatch = this.RegExpService.Match(text, @"<tr>\s*<th>Affiliations<\/th>\s*<td>(.*?)<\/td><\/tr>");
        if (associationMatch != null)
        {
            var matches = this.RegExpService.Matches(associationMatch.Groups[1].Value, @"<a href=""/affiliations/(\d+)"">(.*?)</a>");
            if (matches != null && matches.Count != 0)
            {
                var result = string.Join("|", matches.Select(x => x.Groups[2].Value.Trim()).ToList());
                return result;
            }
            else
            {
                return associationMatch.Groups[1].Value.Trim();
            }
        }

        return null;
    }
}