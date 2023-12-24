namespace SportData.Services;

using SportData.Common.Constants;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.OlympicGames;
using SportData.Services.Interfaces;

public class OlympediaService : IOlympediaService
{
    private readonly IRegExpService regExpService;
    private readonly IDateService dateService;

    public OlympediaService(IRegExpService regExpService, IDateService dateService)
    {
        this.regExpService = regExpService;
        this.dateService = dateService;
    }

    public AthleteModel FindAthlete(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var match = this.regExpService.Match(text, @"<a href=""\/athletes\/(\d+)"">(.*?)<\/a>");
            if (match != null)
            {
                return new AthleteModel
                {
                    Code = int.Parse(match.Groups[1].Value),
                    Name = match.Groups[2].Value.Trim()
                };
            }
        }

        return null;
    }

    public List<AthleteModel> FindAthletes(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<AthleteModel>();
        }

        var numbers = this.regExpService
            .Matches(text, @"<a href=""\/athletes\/(\d+)"">(.*?)<\/a>")
            .Select(x => new AthleteModel { Code = int.Parse(x.Groups[1].Value.Trim()), Name = x.Groups[2].Value.Trim() })?
            .ToList();

        return numbers;
    }

    public string FindNOCCode(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = this.regExpService.Match(text, @"<a href=""\/countries\/(.*?)"">");
        if (match != null)
        {
            var code = match.Groups[1].Value.Trim();
            code = code.Replace("CHI%20", "CHI");
            return code;
        }

        return null;
    }

    public IList<string> FindNOCCodes(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        var codes = this.regExpService
            .Matches(text, @"<a href=""\/countries\/(.*?)"">")
            .Select(x => x.Groups[1].Value)?
            .Where(x => x != "UNK")
            .ToList();

        return codes;
    }

    public MedalType FindMedal(string text)
    {
        var match = this.regExpService.Match(text, @"<span class=""(?:Gold|Silver|Bronze)"">(Gold|Silver|Bronze)<\/span>");
        if (match != null)
        {
            var medalType = match.Groups[1].Value.ToLower();
            switch (medalType)
            {
                case "gold": return MedalType.Gold;
                case "silver": return MedalType.Silver;
                case "bronze": return MedalType.Bronze;
            }
        }

        return MedalType.None;
    }

    public MedalType FindMedal(string text, RoundType round)
    {
        var medalType = MedalType.None;
        if (round == RoundType.FinalRound)
        {
            if (text.Contains("1/2") || text.Contains("1-2"))
            {
                medalType = MedalType.Gold;
            }
            else if (text.Contains("3/4") || text.Contains("3-4"))
            {
                medalType = MedalType.Bronze;
            }
        }

        return medalType;
    }

    public FinishStatus FindStatus(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return FinishStatus.None;
        }

        var acMatch = this.regExpService.Match(text, @"<abbrev title=""Also Competed"">AC</abbrev>");
        if (acMatch != null)
        {
            return FinishStatus.AlsoCompeted;
        }

        var dnsMatch = this.regExpService.Match(text, @"<abbrev title=""Did Not Start"">DNS</abbrev>");
        if (dnsMatch != null)
        {
            return FinishStatus.DidNotStart;
        }

        var dnfMatch = this.regExpService.Match(text, @"<abbrev title=""Did Not Finish"">DNF</abbrev>");
        if (dnfMatch != null)
        {
            return FinishStatus.DidNotFinish;
        }

        var dqMatch = this.regExpService.Match(text, @"<abbrev title=""Disqualified"">DQ</abbrev>");
        if (dqMatch != null)
        {
            return FinishStatus.Disqualified;
        }

        var tnkMatch = this.regExpService.Match(text, @"<abbrev title=""Time Not Known"">TNK</abbrev>");
        if (tnkMatch != null)
        {
            return FinishStatus.TimeNotKnow;
        }

        return FinishStatus.Finish;
    }

    public List<int> FindVenues(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<int>();
        }

        var venues = this.regExpService
            .Matches(text, @"\/venues\/(\d+)")
            .Select(x => int.Parse(x.Groups[1].Value))?
            .ToList();

        return venues;
    }

    public int FindMatchNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var match = this.regExpService.Match(text, @"(?:Match|Game|Race)\s*#(\d+)");
        if (match != null)
        {
            var matchNumber = match.Groups[1].Value;
            return int.Parse(matchNumber);
        }

        return 0;
    }

    public int FindResultNumber(string text)
    {
        var match = this.regExpService.Match(text, @"<a href=""\/results\/(\d+)"">");
        if (match != null)
        {
            return int.Parse(match.Groups[1].Value);
        }

        return 0;
    }

    public void SetWinAndLose(MatchModel match)
    {
        if (match.Team1.Points > match.Team2.Points)
        {
            match.Team1.MatchResult = MatchResultType.Win;
            match.Team2.MatchResult = MatchResultType.Lose;
        }
        else if (match.Team1.Points < match.Team2.Points)
        {
            match.Team1.MatchResult = MatchResultType.Lose;
            match.Team2.MatchResult = MatchResultType.Win;
        }
        else if (match.Team1.Points == match.Team2.Points)
        {
            match.Team1.MatchResult = MatchResultType.Draw;
            match.Team2.MatchResult = MatchResultType.Draw;
        }
    }

    public string FindLocation(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return null;
        }

        var match = this.regExpService.Match(html, @"<a href=""\/venues\/(\d+)"">(.*?)<\/a>");
        if (match != null)
        {
            return match.Groups[2].Value.Trim();
        }

        return null;
    }

    //public MatchResultOld GetMatchResult(string text, MatchResultType type)
    //{
    //    if (string.IsNullOrEmpty(text))
    //    {
    //        return null;
    //    }

    //    text = text.Replace("[", string.Empty).Replace("]", string.Empty);

    //    if (type == MatchResultType.Games)
    //    {
    //        var result = new MatchResultOld();
    //        var match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
    //        if (match != null)
    //        {
    //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[5].Value) };
    //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value), int.Parse(match.Groups[6].Value) };

    //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
    //            points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;
    //            points = result.Games1[2].Value > result.Games2[2].Value ? result.Points1++ : result.Points2++;

    //            this.SetWinAndLose(result);
    //            return result;
    //        }
    //        match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
    //        if (match != null)
    //        {
    //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value) };
    //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value) };

    //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
    //            points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;

    //            this.SetWinAndLose(result);
    //            return result;
    //        }
    //        match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)");
    //        if (match != null)
    //        {
    //            result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value) };
    //            result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value) };

    //            var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;

    //            result.Result1 = ResultType.Win;
    //            result.Result2 = ResultType.Lose;

    //            return result;
    //        }

    //        return result;
    //    }
    //    else
    //    {
    //        var result = new MatchResultOld();
    //        var match = this.regExpService.Match(text, @"(\d+)\s*(?:-|–|—)\s*(\d+)");
    //        if (match != null)
    //        {
    //            result.Points1 = int.Parse(match.Groups[1].Value.Trim());
    //            result.Points2 = int.Parse(match.Groups[2].Value.Trim());

    //            this.SetWinAndLose(result);
    //        }
    //        match = this.regExpService.Match(text, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*(\d+)\.(\d+)");
    //        if (match != null)
    //        {
    //            result.Time1 = this.dateService.ParseTime($"{match.Groups[1].Value}.{match.Groups[2].Value}");
    //            result.Time2 = this.dateService.ParseTime($"{match.Groups[3].Value}.{match.Groups[4].Value}");

    //            if (result.Time1 < result.Time2)
    //            {
    //                result.Result1 = ResultType.Win;
    //                result.Result2 = ResultType.Lose;
    //            }
    //            else if (result.Time1 > result.Time2)
    //            {
    //                result.Result1 = ResultType.Lose;
    //                result.Result2 = ResultType.Win;
    //            }
    //        }
    //        match = this.regExpService.Match(text, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*DNF");
    //        if (match != null)
    //        {
    //            result.Time1 = this.dateService.ParseTime($"{match.Groups[1].Value}.{match.Groups[2].Value}");
    //            result.Time2 = null;

    //            result.Result1 = ResultType.Win;
    //            result.Result2 = ResultType.Lose;
    //        }

    //        return result;
    //    }

    //    return null;
    //}

    public string FindMatchInfo(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = this.regExpService.Match(text, @"(?:Match|Game)\s*(\d+)(?:\/|-)(\d+)");
        if (match != null)
        {
            return $"{match.Groups[1].Value}-{match.Groups[2].Value}";
        }

        return null;
    }

    public RecordType FindRecord(string text)
    {
        var record = RecordType.None;
        if (string.IsNullOrEmpty(text))
        {
            return record;
        }

        var match = this.regExpService.Match(text, @"World\s*Record");
        if (match != null)
        {
            record = RecordType.World;
        }

        match = this.regExpService.Match(text, @"Olympic\s*Record");
        if (match != null)
        {
            record = RecordType.Olympic;
        }

        return record;
    }

    public QualificationType FindQualification(string text)
    {
        var type = QualificationType.None;
        if (string.IsNullOrEmpty(text))
        {
            return type;
        }

        var match = this.regExpService.Match(text, @"Qualified");
        if (match != null)
        {
            type = QualificationType.Qualified;
        }

        return type;
    }

    public IList<int> FindResults(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<int>();
        }

        var results = this.regExpService
            .Matches(text, @"<option value=""(.*?)"">")
            .Where(x => !string.IsNullOrEmpty(x.Groups[1].Value))
            .Select(x => int.Parse(x.Groups[1].Value))?
            .ToList();

        return results;
    }

    public DecisionType FindDecision(string text)
    {
        var decision = DecisionType.None;

        if (string.IsNullOrEmpty(text))
        {
            return decision;
        }

        var match = this.regExpService.Match(text, @">bye<");
        if (match != null)
        {
            decision = DecisionType.Buy;
        }

        match = this.regExpService.Match(text, @">walkover<");
        if (match != null)
        {
            decision = DecisionType.Walkover;
        }

        return decision;
    }

    public int FindSeedNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var match = this.regExpService.Match(text, @"\((\d+)\)");
        if (match != null)
        {
            return int.Parse(match.Groups[1].Value);
        }

        return 0;
    }

    public Horse FindHorse(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var match = this.regExpService.Match(text, @"<a href=""/horses/(\d+)"">(.*?)</a>");
            if (match != null)
            {
                return new Horse
                {
                    Code = int.Parse(match.Groups[1].Value),
                    Name = match.Groups[2].Value.Trim()
                };
            }
        }

        return null;
    }

    public Dictionary<string, int> GetIndexes(List<string> headers)
    {
        var indexes = new Dictionary<string, int>();

        for (int i = 0; i < headers.Count; i++)
        {
            switch (headers[i])
            {
                case "10s":
                case "Center 10s":
                    indexes[ConverterConstants.S10] = i;
                    break;
                case "10s/9s":
                    indexes[ConverterConstants.S10S9] = i;
                    break;
                case "8s":
                    indexes[ConverterConstants.S8] = i;
                    break;
                case "9s":
                    indexes[ConverterConstants.S9] = i;
                    break;
                case "Actual Time":
                    indexes[ConverterConstants.ActualTime] = i;
                    break;
                case "Adjusted Points":
                case "Adj. Points":
                case "AIP":
                    indexes[ConverterConstants.AdjustedPoints] = i;
                    break;
                case "Adjusted Time":
                case "AT":
                case "Adjusted":
                    indexes[ConverterConstants.AdjustedTime] = i;
                    break;
                case "AI":
                    indexes[ConverterConstants.AI] = i;
                    break;
                case "Air &amp; Form Points":
                    indexes[ConverterConstants.AirFormPoints] = i;
                    break;
                case "Air Points":
                case "Air Score":
                    indexes[ConverterConstants.AirPoints] = i;
                    break;
                case "AP":
                case "Apparatus":
                case "HAP":
                case "PAP":
                    indexes[ConverterConstants.ApparatusPoints] = i;
                    break;
                case "Arrow 1":
                    indexes[ConverterConstants.Arrow1] = i;
                    break;
                case "Arrow 10":
                    indexes[ConverterConstants.Arrow10] = i;
                    break;
                case "Arrow 2":
                    indexes[ConverterConstants.Arrow2] = i;
                    break;
                case "Arrow 3":
                    indexes[ConverterConstants.Arrow3] = i;
                    break;
                case "Arrow 4":
                    indexes[ConverterConstants.Arrow4] = i;
                    break;
                case "Arrow 5":
                    indexes[ConverterConstants.Arrow5] = i;
                    break;
                case "Arrow 6":
                    indexes[ConverterConstants.Arrow6] = i;
                    break;
                case "Arrow 7":
                    indexes[ConverterConstants.Arrow7] = i;
                    break;
                case "Arrow 8":
                    indexes[ConverterConstants.Arrow8] = i;
                    break;
                case "Arrow 9":
                    indexes[ConverterConstants.Arrow9] = i;
                    break;
                case "Artistic Impression":
                case "Artistic Impression Points":
                    indexes[ConverterConstants.ArtisticImpression] = i;
                    break;
                case "Artistic Impression Choreography Points":
                    indexes[ConverterConstants.ArtisticImpressionChoreographyPoints] = i;
                    break;
                case "Artistic Impression Judge 1 Points":
                    indexes[ConverterConstants.ArtisticImpressionJudge1Points] = i;
                    break;
                case "Artistic Impression Judge 2 Points":
                    indexes[ConverterConstants.ArtisticImpressionJudge2Points] = i;
                    break;
                case "Artistic Impression Judge 3 Points":
                    indexes[ConverterConstants.ArtisticImpressionJudge3Points] = i;
                    break;
                case "Artistic Impression Judge 4 Points":
                    indexes[ConverterConstants.ArtisticImpressionJudge4Points] = i;
                    break;
                case "Artistic Impression Judge 5 Points":
                    indexes[ConverterConstants.ArtisticImpressionJudge5Points] = i;
                    break;
                case "Artistic Impression Manner of Presentation Points":
                    indexes[ConverterConstants.ArtisticImpressionMannerofPresentationPoints] = i;
                    break;
                case "Artistic Impression Music Interpretation Points":
                    indexes[ConverterConstants.ArtisticImpressionMusicInterpretationPoints] = i;
                    break;
                case "Artistic Judge 1 Score":
                    indexes[ConverterConstants.ArtisticJudge1] = i;
                    break;
                case "Artistic Judge 2 Score":
                    indexes[ConverterConstants.ArtisticJudge2] = i;
                    break;
                case "Artistic Judge 3 Score":
                    indexes[ConverterConstants.ArtisticJudge3] = i;
                    break;
                case "Artistic Judge 4 Score":
                    indexes[ConverterConstants.ArtisticJudge4] = i;
                    break;
                case "Artistic Penalty Score":
                    indexes[ConverterConstants.ArtisticPenalty] = i;
                    break;
                case "Artistic Points":
                    indexes[ConverterConstants.ArtisticPoints] = i;
                    break;
                case "Artistic Reference Score":
                case "Artistic Score":
                    indexes[ConverterConstants.ArtisticScore] = i;
                    break;
                case "Assists":
                case "AST":
                case "ASS":
                    indexes[ConverterConstants.Assits] = i;
                    break;
                case "1st Inning/At Bats":
                case "Inning 1/At Bats":
                    indexes[ConverterConstants.AtBats] = i;
                    break;
                case "Athletic Factor":
                    indexes[ConverterConstants.AthleticFactor] = i;
                    break;
                case "Attack Attempts":
                    indexes[ConverterConstants.AttackAttempts] = i;
                    break;
                case "Attack Successes":
                    indexes[ConverterConstants.AttackSuccesses] = i;
                    break;
                case "Attempts":
                case "Attempts to Zone":
                    indexes[ConverterConstants.Attempts] = i;
                    break;
                case "Attempts / Shots":
                    indexes[ConverterConstants.AttemptsShots] = i;
                    break;
                case "Attempts / Shots on Goal":
                    indexes[ConverterConstants.AttemptsShotsOnGoal] = i;
                    break;
                case "AVG":
                    indexes[ConverterConstants.Average] = i;
                    break;
                case "Bad Points":
                    indexes[ConverterConstants.BadPoints] = i;
                    break;
                case "Balance Beam":
                case "BB":
                    indexes[ConverterConstants.BalanceBeam] = i;
                    break;
                case "Ball":
                case "Ball/Ribbon":
                case "Balls":
                    indexes[ConverterConstants.Ball] = i;
                    break;
                case "BBL":
                    indexes[ConverterConstants.BarrageBoutsLoss] = i;
                    break;
                case "BBW":
                    indexes[ConverterConstants.BarrageBoutsWon] = i;
                    break;
                case "Bases-on-Balls":
                    indexes[ConverterConstants.BaseOnBalls] = i;
                    break;
                case "Batting Average":
                    indexes[ConverterConstants.BattingAverage] = i;
                    break;
                case "Bent Knee":
                case "Bent Knee Warnings":
                    indexes[ConverterConstants.BentKnee] = i;
                    break;
                case "Best Mark Distance":
                    indexes[ConverterConstants.BestDistance] = i;
                    break;
                case "Best Height Cleared":
                case "BHC":
                case "BHC(I)":
                    indexes[ConverterConstants.BestHeight] = i;
                    break;
                case "Best Time":
                    indexes[ConverterConstants.BestTime] = i;
                    break;
                case "Best Wave":
                    indexes[ConverterConstants.BestWave] = i;
                    break;
                case "B Final":
                    indexes[ConverterConstants.Bfinal] = i;
                    break;
                case "BLK":
                case "Blocks":
                case "Block Points":
                case "Block Points / Side-Outs":
                    indexes[ConverterConstants.Blocks] = i;
                    break;
                case "Block Successes":
                    indexes[ConverterConstants.BlockSuccesses] = i;
                    break;
                case "Bodyweight":
                    indexes[ConverterConstants.Bodyweight] = i;
                    break;
                case "Bonus Points":
                    indexes[ConverterConstants.BonusPoints] = i;
                    break;
                case "Boulder 1":
                    indexes[ConverterConstants.Boulder1] = i;
                    break;
                case "Boulder 2":
                    indexes[ConverterConstants.Boulder2] = i;
                    break;
                case "Boulder 3":
                    indexes[ConverterConstants.Boulder3] = i;
                    break;
                case "Boulder 4":
                    indexes[ConverterConstants.Boulder4] = i;
                    break;
                case "Bouldering":
                    indexes[ConverterConstants.Bouldering] = i;
                    break;
                case "Breakthrough Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.BreakthroughtShots] = i;
                    break;
                case "Card":
                    indexes[ConverterConstants.Card] = i;
                    break;
                case "Center Hits":
                case "Centrals":
                    indexes[ConverterConstants.Centrals] = i;
                    break;
                case "Centre Goals/Attempts":
                    indexes[ConverterConstants.CentreGoal] = i;
                    break;
                case "Classical":
                    indexes[ConverterConstants.Classical] = i;
                    break;
                case "Class. Pts":
                case "Classification Points":
                    indexes[ConverterConstants.ClassificationPoints] = i;
                    break;
                case "Classification Round":
                    indexes[ConverterConstants.ClassificationRound] = i;
                    break;
                case "C&J":
                case "Clean & Jerk":
                    indexes[ConverterConstants.CleanJerk] = i;
                    break;
                case "Clubs":
                case "Clubs Score":
                    indexes[ConverterConstants.Clubs] = i;
                    break;
                case "Code":
                    indexes[ConverterConstants.Code] = i;
                    break;
                case "Color":
                case "Colour":
                    indexes[ConverterConstants.Color] = i;
                    break;
                case "C1EP":
                case "CEP":
                case "CP":
                case "TCP":
                case "1EP":
                case "Compulsory Exercise Points":
                case "Comp. Points":
                    indexes[ConverterConstants.CompulsaryPoints] = i;
                    break;
                case "Compulsory":
                case "Compulsory Dance":
                    indexes[ConverterConstants.Compulsory] = i;
                    break;
                case "CD#1FP":
                    indexes[ConverterConstants.CompulsoryDance1FactoredPlacements] = i;
                    break;
                case "CD#2FP":
                    indexes[ConverterConstants.CompulsoryDance2FactoredPlacements] = i;
                    break;
                case "Conversions":
                    indexes[ConverterConstants.Conversions] = i;
                    break;
                case "Corner Kicks":
                    indexes[ConverterConstants.CornerKicks] = i;
                    break;
                case "Count Back":
                case "Countback":
                    indexes[ConverterConstants.Countback] = i;
                    break;
                case "Counter Attack Goals/Attempts":
                case "Counter-Attack Goals/Attempts":
                    indexes[ConverterConstants.CounterAttackGoals] = i;
                    break;
                case "Cross-Country":
                    indexes[ConverterConstants.CrossCountry] = i;
                    break;
                case "Cross-Country (20 km)":
                    indexes[ConverterConstants.CrossCountry20km] = i;
                    break;
                case "Cross-Country (50 km)":
                    indexes[ConverterConstants.CrossCountry50km] = i;
                    break;
                case "Cross-Country (5 km)":
                    indexes[ConverterConstants.CrossCountry5km] = i;
                    break;
                case "Cross-Country Penalty Points":
                    indexes[ConverterConstants.CrossCountryPenaltyPoints] = i;
                    break;
                case "Cross Country Skiing, 10 km":
                    indexes[ConverterConstants.CrossCountrySkiing10km] = i;
                    break;
                case "Cross Country Skiing, 15 km":
                    indexes[ConverterConstants.CrossCountrySkiing15km] = i;
                    break;
                case "Cross Country Skiing, 18 km":
                    indexes[ConverterConstants.CrossCountrySkiing18km] = i;
                    break;
                case "Cross Country Skiing, 3 × 10 km Relay":
                    indexes[ConverterConstants.CrossCountrySkiing3x10kmRelay] = i;
                    break;
                case "Cross Country Skiing, 4 × 5 km Relay":
                    indexes[ConverterConstants.CrossCountrySkiing4x10kmRelay] = i;
                    break;
                case "Cross Country Skiing, 7.5 km":
                    indexes[ConverterConstants.CrossCountrySkiing75km] = i;
                    break;
                case "Cumulative Time":
                    indexes[ConverterConstants.CumulativeTime] = i;
                    break;
                case "40 km Cycling":
                case "7.4 km Cycling":
                    indexes[ConverterConstants.Cycling] = i;
                    break;
                case "Dance 1":
                    indexes[ConverterConstants.Dance1] = i;
                    break;
                case "Dance 2":
                    indexes[ConverterConstants.Dance2] = i;
                    break;
                case "Dance 3":
                    indexes[ConverterConstants.Dance3] = i;
                    break;
                case "Date/Time":
                    indexes[ConverterConstants.DateTime] = i;
                    break;
                case "Deductions":
                    indexes[ConverterConstants.Deductions] = i;
                    break;
                case "DRB":
                    indexes[ConverterConstants.DefensiveRebounds] = i;
                    break;
                case "Deuk-Jeom":
                    indexes[ConverterConstants.DeukJeom] = i;
                    break;
                case "Difficulty Points":
                case "DD":
                case "DoD":
                case "Degree of Difficulty":
                case "Difficulty Score":
                case "Difficulty":
                    indexes[ConverterConstants.Difficulty] = i;
                    break;
                case "Difficulty Judge 1 Points":
                case "Difficulty Judge 1 Score":
                    indexes[ConverterConstants.DifficultyJudge1] = i;
                    break;
                case "Difficulty Judge 2 Points":
                case "Difficulty Judge 1-1 Score":
                    indexes[ConverterConstants.DifficultyJudge2] = i;
                    break;
                case "Difficulty Judge 3 Points":
                case "Difficulty Judge 1-2 Score":
                    indexes[ConverterConstants.DifficultyJudge3] = i;
                    break;
                case "Difficulty Judge 4 Points":
                case "Difficulty Judge 2 Score":
                    indexes[ConverterConstants.DifficultyJudge4] = i;
                    break;
                case "Difficulty Judge 5 Points":
                case "Difficulty Judge 2-1 Score":
                    indexes[ConverterConstants.DifficultyJudge5] = i;
                    break;
                case "Difficulty Judge 2-2 Score":
                    indexes[ConverterConstants.DifficultyJudge6] = i;
                    break;
                case "Difficulty Penalty Score":
                    indexes[ConverterConstants.DifficultyPenalty] = i;
                    break;
                case "Digs":
                    indexes[ConverterConstants.Digs] = i;
                    break;
                case "Dig Successes":
                    indexes[ConverterConstants.DigSuccesses] = i;
                    break;
                case "Discus Throw":
                    indexes[ConverterConstants.DiscusThrow] = i;
                    break;
                case "Disqualifications":
                case "DQ":
                    indexes[ConverterConstants.Disqualifcations] = i;
                    break;
                case "Disqualifications with Report":
                    indexes[ConverterConstants.DisqualificationsReport] = i;
                    break;
                case "D()":
                case "Distance":
                case "D(I)":
                case "Distance (Imp.)":
                case "Distance (Imperial)":
                case "Distance Points":
                    indexes[ConverterConstants.Distance] = i;
                    break;
                case "Dive":
                case "DC#":
                    indexes[ConverterConstants.Dive] = i;
                    break;
                case "Dive #1":
                    indexes[ConverterConstants.Dive1] = i;
                    break;
                case "Dive #2":
                    indexes[ConverterConstants.Dive2] = i;
                    break;
                case "Dive #3":
                    indexes[ConverterConstants.Dive3] = i;
                    break;
                case "Dive #4":
                    indexes[ConverterConstants.Dive4] = i;
                    break;
                case "Dive #5":
                    indexes[ConverterConstants.Dive5] = i;
                    break;
                case "Dive #6":
                    indexes[ConverterConstants.Dive6] = i;
                    break;
                case "Double Exclusion":
                    indexes[ConverterConstants.DoubleExclusion] = i;
                    break;
                case "Inning 4/Doubles":
                case "4th Inning/Doubles":
                    indexes[ConverterConstants.Doubles] = i;
                    break;
                case "Downhill":
                    indexes[ConverterConstants.Downhill] = i;
                    break;
                case "Bouts Tied":
                case "BWT":
                case "MT":
                case "Ties":
                case "T":
                    indexes[ConverterConstants.Draw] = i;
                    break;
                case "Draws":
                    indexes[ConverterConstants.Draws] = i;
                    break;
                case "Dressage":
                    indexes[ConverterConstants.Dressage] = i;
                    break;
                case "Dressage Penalty Points":
                    indexes[ConverterConstants.DressagePenaltyPoints] = i;
                    break;
                case "TDP":
                    indexes[ConverterConstants.DrillPoints] = i;
                    break;
                case "Driving Goals/Attempts":
                    indexes[ConverterConstants.DrivingGoals] = i;
                    break;
                case "Drop Goals":
                    indexes[ConverterConstants.DropGoals] = i;
                    break;
                case "D Score":
                    indexes[ConverterConstants.Dscore] = i;
                    break;
                case "Earned Run Average":
                    indexes[ConverterConstants.EarnedRunAverage] = i;
                    break;
                case "Earned Runs Allowed":
                case "Team Runs/Earned Runs Allowed":
                    indexes[ConverterConstants.EarnedRunsAllowed] = i;
                    break;
                case "Elimination Race":
                case "Elimination Race Points":
                    indexes[ConverterConstants.EliminationRace] = i;
                    break;
                case "Elimination Round":
                case "Elimination Round Points":
                    indexes[ConverterConstants.EliminationRound] = i;
                    break;
                case "Empty Goal Shots / Attempts":
                    indexes[ConverterConstants.EmptyGoalShots] = i;
                    break;
                case "Errors":
                    indexes[ConverterConstants.Erros] = i;
                    break;
                case "E Score":
                    indexes[ConverterConstants.Escore] = i;
                    break;
                case "Event Points":
                    indexes[ConverterConstants.EventPoints] = i;
                    break;
                case "Exchange":
                case "Exchange (Pos)":
                    indexes[ConverterConstants.Exchange] = i;
                    break;
                case "Exclusion":
                    indexes[ConverterConstants.Exclusion] = i;
                    break;
                case "Exclusion (20 seconds)":
                    indexes[ConverterConstants.Exclusion20Seconds] = i;
                    break;
                case "Exclusion (20 seconds) (Centre/Field)":
                    indexes[ConverterConstants.Exclusion20SecondsCentre] = i;
                    break;
                case "Exclusions (35 seconds)":
                    indexes[ConverterConstants.Exclusion35Seconds] = i;
                    break;
                case "Exclusions":
                    indexes[ConverterConstants.Exclusions] = i;
                    break;
                case "Exclusion with Sub":
                    indexes[ConverterConstants.ExclusionSub] = i;
                    break;
                case "Exclusion (w/wo Sub)":
                    indexes[ConverterConstants.ExclusionWSub] = i;
                    break;
                case "Execution":
                case "Execution Score":
                case "Execution Reference Score":
                    indexes[ConverterConstants.Execution] = i;
                    break;
                case "Execution Judge 1 Points":
                case "J1":
                case "J1S":
                case "E1":
                case "EJ1S":
                case "Execution Judge 1 Score":
                    indexes[ConverterConstants.ExecutionJudge1] = i;
                    break;
                case "Execution Judge 2 Points":
                case "J2":
                case "J2S":
                case "E2":
                case "EJ2S":
                case "Execution Judge 2 Score":
                    indexes[ConverterConstants.ExecutionJudge2] = i;
                    break;
                case "Execution Judge 3 Points":
                case "J3":
                case "J3S":
                case "E3":
                case "EJ3S":
                case "Execution Judge 3 Score":
                    indexes[ConverterConstants.ExecutionJudge3] = i;
                    break;
                case "Execution Judge 4 Points":
                case "J4":
                case "J4S":
                case "E4":
                case "Execution Judge 4 Score":
                    indexes[ConverterConstants.ExecutionJudge4] = i;
                    break;
                case "Execution Judge 5 Points":
                case "J5":
                case "J5S":
                case "E5":
                    indexes[ConverterConstants.ExecutionJudge5] = i;
                    break;
                case "Execution Judge 6 Points":
                case "J6":
                case "J6S":
                case "E6":
                    indexes[ConverterConstants.ExecutionJudge6] = i;
                    break;
                case "Execution Judge 7 Points":
                case "J7":
                case "J7S":
                    indexes[ConverterConstants.ExecutionJudge7] = i;
                    break;
                case "Execution Judge Average":
                    indexes[ConverterConstants.ExecutionJudgeAverage] = i;
                    break;
                case "Execution Judge Points":
                    indexes[ConverterConstants.ExecutionJudgePoints] = i;
                    break;
                case "Execution Penalty":
                    indexes[ConverterConstants.ExecutionPenalty] = i;
                    break;
                case "Execution Points":
                    indexes[ConverterConstants.ExecutionPoints] = i;
                    break;
                case "Execution Reference Points":
                    indexes[ConverterConstants.ExecutionReferencePoints] = i;
                    break;
                case "Execution Reference 1 Points":
                    indexes[ConverterConstants.ExecutionReferencePoints1] = i;
                    break;
                case "Execution Reference 2 Points":
                    indexes[ConverterConstants.ExecutionReferencePoints2] = i;
                    break;
                case "Extraman Goals":
                case "Extraman Goals/Attempts":
                    indexes[ConverterConstants.ExtramanGoals] = i;
                    break;
                case "Extra Shots":
                    indexes[ConverterConstants.ExtraShots] = i;
                    break;
                case "After Extra Time":
                    indexes[ConverterConstants.ExtraTime] = i;
                    break;
                case "Extra-time 1 Score":
                    indexes[ConverterConstants.ExtraTime1] = i;
                    break;
                case "Extra-time 2 Score":
                    indexes[ConverterConstants.ExtraTime2] = i;
                    break;
                case "Extra-Time 3 Score":
                    indexes[ConverterConstants.ExtraTime3] = i;
                    break;
                case "Extra-Time 2 Score / Penalty Shoot-Out":
                case "Extra-time 3/4/5/6 Score":
                    indexes[ConverterConstants.ExtraTimeScore] = i;
                    break;
                case "Fastbreak Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.FastbreakShots] = i;
                    break;
                case "Fastest Serve":
                    indexes[ConverterConstants.FastestServe] = i;
                    break;
                case "Fast Run":
                case "Fast Run Points":
                    indexes[ConverterConstants.FastRun] = i;
                    break;
                case "Faults":
                    indexes[ConverterConstants.Faults] = i;
                    break;
                case "Fencing":
                    indexes[ConverterConstants.Fencing] = i;
                    break;
                case "Field Exclusion":
                    indexes[ConverterConstants.FieldExclusion] = i;
                    break;
                case "Field Goals":
                    indexes[ConverterConstants.FieldGoal] = i;
                    break;
                case "Field Goal Attempts":
                    indexes[ConverterConstants.FieldGoalAttempts] = i;
                    break;
                case "FG/FGA":
                    indexes[ConverterConstants.FieldGoalFieldGoalAttempts] = i;
                    break;
                case "FG":
                    indexes[ConverterConstants.FieldGoals] = i;
                    break;
                case "Fielding Average":
                    indexes[ConverterConstants.FieldingAverage] = i;
                    break;
                case "Figure 1":
                    indexes[ConverterConstants.Figure1] = i;
                    break;
                case "Figure 2":
                    indexes[ConverterConstants.Figure2] = i;
                    break;
                case "Figure 3":
                    indexes[ConverterConstants.Figure3] = i;
                    break;
                case "Figure Points":
                case "Figures":
                    indexes[ConverterConstants.Figures] = i;
                    break;
                case "Final":
                case "FP":
                case "FRP":
                case "Jumping Final":
                case "Final Round":
                case "Final Points":
                case "Final Round Points":
                    indexes[ConverterConstants.Final] = i;
                    break;
                case "Final Round 1":
                    indexes[ConverterConstants.FinalRound1] = i;
                    break;
                case "Final Round 2":
                    indexes[ConverterConstants.FinalRound2] = i;
                    break;
                case "Final Round 3":
                    indexes[ConverterConstants.FinalRound3] = i;
                    break;
                case "First Final":
                    indexes[ConverterConstants.FirstFinal] = i;
                    break;
                case "1TT":
                    indexes[ConverterConstants.FirstTimeTrial] = i;
                    break;
                case "Floor Exercise":
                case "FE":
                    indexes[ConverterConstants.FloorExercise] = i;
                    break;
                case "250 m Flying Start":
                    indexes[ConverterConstants.FlyingStart250] = i;
                    break;
                case "Form Score":
                    indexes[ConverterConstants.FormScore] = i;
                    break;
                case "Fouls Committed":
                    indexes[ConverterConstants.FoulsCommitted] = i;
                    break;
                case "Free":
                    indexes[ConverterConstants.Free] = i;
                    break;
                case "Free Dance":
                    indexes[ConverterConstants.FreeDance] = i;
                    break;
                case "FDFP":
                    indexes[ConverterConstants.FreeDanceFactoredPlacements] = i;
                    break;
                case "Free Kicks":
                    indexes[ConverterConstants.FreeKicks] = i;
                    break;
                case "Free Routine":
                case "Free Routine Points":
                    indexes[ConverterConstants.FreeRoutine] = i;
                    break;
                case "Free Skating":
                case "Women's Free Skating":
                case "Pairs Free Skating":
                case "Men's Free Skating":
                    indexes[ConverterConstants.FreeSkating] = i;
                    break;
                case "Freestyle":
                    indexes[ConverterConstants.Freestyle] = i;
                    break;
                case "FT":
                    indexes[ConverterConstants.FreeThrows] = i;
                    break;
                case "Game 1":
                    indexes[ConverterConstants.Game1] = i;
                    break;
                case "Game 2":
                    indexes[ConverterConstants.Game2] = i;
                    break;
                case "Game 3":
                    indexes[ConverterConstants.Game3] = i;
                    break;
                case "Game 4":
                    indexes[ConverterConstants.Game4] = i;
                    break;
                case "Game 5":
                    indexes[ConverterConstants.Game5] = i;
                    break;
                case "Game 6":
                    indexes[ConverterConstants.Game6] = i;
                    break;
                case "Game 7":
                    indexes[ConverterConstants.Game7] = i;
                    break;
                case "GP":
                    indexes[ConverterConstants.GamePlayed] = i;
                    break;
                case "Games Won":
                case "Games":
                    indexes[ConverterConstants.Games] = i;
                    break;
                case "Gate":
                    indexes[ConverterConstants.Gate] = i;
                    break;
                case "Gate Points":
                    indexes[ConverterConstants.GatePoints] = i;
                    break;
                case "Goal from Mark":
                    indexes[ConverterConstants.GoalFromMark] = i;
                    break;
                case "GK In":
                    indexes[ConverterConstants.GoalkeeperIn] = i;
                    break;
                case "GK Out":
                    indexes[ConverterConstants.GoalkeeperOut] = i;
                    break;
                case "Goals":
                case "GLS":
                case "Goals/Saves":
                    indexes[ConverterConstants.Goals] = i;
                    break;
                case "5-metre Goals/Attempts":
                    indexes[ConverterConstants.Goals5M] = i;
                    break;
                case "6-metre Goals/Attempts":
                    indexes[ConverterConstants.Goals6M] = i;
                    break;
                case "7-metre Goals/Attempts":
                    indexes[ConverterConstants.Goals7M] = i;
                    break;
                case "GAA":
                    indexes[ConverterConstants.GoalsAgainst] = i;
                    break;
                case "Goals / Saves":
                    indexes[ConverterConstants.GoalsSaves] = i;
                    break;
                case "Goals/Shots":
                    indexes[ConverterConstants.GoalsShots] = i;
                    break;
                case "Golds":
                    indexes[ConverterConstants.Golds] = i;
                    break;
                case "Grand Prix":
                    indexes[ConverterConstants.GrandPrix] = i;
                    break;
                case "Grand Prix Freestyle":
                    indexes[ConverterConstants.GrandPrixFreestyle] = i;
                    break;
                case "Grand Prix Freestyle Artistic Points":
                    indexes[ConverterConstants.GrandPrixFreestyleArtisticPoints] = i;
                    break;
                case "Grand Prix Freestyle Points":
                    indexes[ConverterConstants.GrandPrixFreestylePoints] = i;
                    break;
                case "Grand Prix Freestyle Technical Points":
                    indexes[ConverterConstants.GrandPrixFreestyleTechnicalPoints] = i;
                    break;
                case "Grand Prix Special":
                    indexes[ConverterConstants.GrandPrixSpecial] = i;
                    break;
                case "Grand Prix Special Points":
                    indexes[ConverterConstants.GrandPrixSpecialPoints] = i;
                    break;
                case "Green":
                case "Green Cards":
                    indexes[ConverterConstants.GreenCards] = i;
                    break;
                case "Group":
                    indexes[ConverterConstants.Group] = i;
                    break;
                case "Group Exercise Points":
                case "GEP":
                    indexes[ConverterConstants.GroupExercise] = i;
                    break;
                case "1st-Half Points":
                    indexes[ConverterConstants.Half1] = i;
                    break;
                case "2nd-Half Points":
                    indexes[ConverterConstants.Half2] = i;
                    break;
                case "Half (Pos)":
                case "Half-Marathon":
                    indexes[ConverterConstants.HalfMarathon] = i;
                    break;
                case "50% Points":
                    indexes[ConverterConstants.HalfPoints] = i;
                    break;
                case "QP(50%)":
                    indexes[ConverterConstants.HalfQualificationPoints] = i;
                    break;
                case "ATP(50%)":
                    indexes[ConverterConstants.HalfTeamPoints] = i;
                    break;
                case "Hammer Throw":
                    indexes[ConverterConstants.HammerThrow] = i;
                    break;
                case "Handicap":
                    indexes[ConverterConstants.Handicap] = i;
                    break;
                case "Hand Used":
                    indexes[ConverterConstants.HandUsed] = i;
                    break;
                case "Height":
                    indexes[ConverterConstants.Height] = i;
                    break;
                case "High Jump":
                    indexes[ConverterConstants.HighJump] = i;
                    break;
                case "3rd Inning/Hits":
                case "Inning 3/Hits":
                case "Hits":
                    indexes[ConverterConstants.Hits] = i;
                    break;
                case "Hits Allowed":
                case "Team Errors/Hits Allowed":
                    indexes[ConverterConstants.HitsAllowed] = i;
                    break;
                case "Hold Reached":
                    indexes[ConverterConstants.HoldReached] = i;
                    break;
                case "Holes Up/To Play":
                    indexes[ConverterConstants.Holes] = i;
                    break;
                case "Home Runs":
                case "Inning 6/Home Runs":
                case "6th Inning/Home Runs":
                    indexes[ConverterConstants.HomeRuns] = i;
                    break;
                case "Home Runs Allowed":
                    indexes[ConverterConstants.HomeRunsAllowed] = i;
                    break;
                case "Hoop":
                case "Hoop Score":
                case "Hoop/Ball":
                case "Hoop/Ribbon":
                case "Hoops/Clubs":
                case "Hoops/Clubs Score":
                    indexes[ConverterConstants.Hoop] = i;
                    break;
                case "Horizontal Bar":
                case "HB":
                    indexes[ConverterConstants.HorizontalBar] = i;
                    break;
                case "Horizontal Displacement Points":
                    indexes[ConverterConstants.HorizontalDisplacementPoints] = i;
                    break;
                case "Horse":
                    indexes[ConverterConstants.Horse] = i;
                    break;
                case "Horse Vault":
                case "HV":
                    indexes[ConverterConstants.HorseVault] = i;
                    break;
                case "Ice Dance Free Dance":
                    indexes[ConverterConstants.IceDanceFreeDance] = i;
                    break;
                case "Ice Dance Rhythm Dance":
                    indexes[ConverterConstants.IceDanceRhythmDance] = i;
                    break;
                case "Ice Dance Short Dance":
                    indexes[ConverterConstants.IceDanceShortDance] = i;
                    break;
                case "Impr.":
                    indexes[ConverterConstants.Improvisation] = i;
                    break;
                case "IO":
                case "AIO":
                    indexes[ConverterConstants.IndividualOrdinalsPoints] = i;
                    break;
                case "Individual Penalty Points":
                    indexes[ConverterConstants.IndividualPenaltyPoints] = i;
                    break;
                case "Individual Points":
                case "IP":
                case "SIP":
                    indexes[ConverterConstants.IndividualPoints] = i;
                    break;
                case "3,000 m Individual Pursuit":
                    indexes[ConverterConstants.IndividualPursuit3000] = i;
                    break;
                case "4,000 m Individual Pursuit":
                    indexes[ConverterConstants.IndividualPursuit4000] = i;
                    break;
                case "Inner 10s":
                    indexes[ConverterConstants.Inner10s] = i;
                    break;
                case "Result First Innings":
                    indexes[ConverterConstants.Inning1] = i;
                    break;
                case "Result Second Innings":
                    indexes[ConverterConstants.Inning2] = i;
                    break;
                case "Innings Pitched":
                case "15th Inning/Innings Pitched":
                case "Home-Visiting Team/Innings Pitched":
                    indexes[ConverterConstants.InningsPitched] = i;
                    break;
                case "In/Out":
                    indexes[ConverterConstants.InOut] = i;
                    break;
                case "Intermediate":
                    indexes[ConverterConstants.Intermediate] = i;
                    break;
                case "Intermediate 1":
                case "Intermediate Time 1":
                    indexes[ConverterConstants.Intermediate1] = i;
                    break;
                case "Intermediate 10":
                    indexes[ConverterConstants.Intermediate10] = i;
                    break;
                case "Intermediate 2":
                case "Intermediate Time 2":
                    indexes[ConverterConstants.Intermediate2] = i;
                    break;
                case "Intermediate 3":
                case "Intermediate Time 3":
                    indexes[ConverterConstants.Intermediate3] = i;
                    break;
                case "Intermediate 4":
                case "Intermediate Time 4":
                    indexes[ConverterConstants.Intermediate4] = i;
                    break;
                case "Intermediate 5":
                case "Intermediate Time 5":
                    indexes[ConverterConstants.Intermediate5] = i;
                    break;
                case "Intermediate 6":
                    indexes[ConverterConstants.Intermediate6] = i;
                    break;
                case "Intermediate 7":
                    indexes[ConverterConstants.Intermediate7] = i;
                    break;
                case "Intermediate 8":
                    indexes[ConverterConstants.Intermediate8] = i;
                    break;
                case "Intermediate 9":
                    indexes[ConverterConstants.Intermediate9] = i;
                    break;
                case "Ippon":
                    indexes[ConverterConstants.Ippon] = i;
                    break;
                case "Scored?":
                    indexes[ConverterConstants.IsScored] = i;
                    break;
                case "Javelin Throw":
                    indexes[ConverterConstants.JavelinThrow] = i;
                    break;
                case "Judge #1 Score":
                case "Original Judge #1 Score":
                case "Judge #1":
                case "Athletic Points Judge 1":
                case "Judge 1 Points":
                    indexes[ConverterConstants.Judge1] = i;
                    break;
                case "Judge #10":
                    indexes[ConverterConstants.Judge10] = i;
                    break;
                case "Judge #11":
                    indexes[ConverterConstants.Judge11] = i;
                    break;
                case "Judge 1(Air &amp; Form) Score":
                case "Judge 1 Air &amp; Form Score":
                    indexes[ConverterConstants.Judge1AirFormScore] = i;
                    break;
                case "Judge 1 Air Score":
                    indexes[ConverterConstants.Judge1AirScore] = i;
                    break;
                case "Judge 1 (Turns) Deductions":
                    indexes[ConverterConstants.Judge1Deductions] = i;
                    break;
                case "Judge 1 Form Score":
                    indexes[ConverterConstants.Judge1FormScore] = i;
                    break;
                case "Judge 1 Landing Score":
                    indexes[ConverterConstants.Judge1LandingScore] = i;
                    break;
                case "Judge 1 (Turns) Score":
                case "Judge 1 Score":
                case "Judge #1 Score (Standard Airs)":
                    indexes[ConverterConstants.Judge1Score] = i;
                    break;
                case "Technical Points Judge 1":
                    indexes[ConverterConstants.Judge1TechnicalPoints] = i;
                    break;
                case "Judge #2 Score":
                case "Original Judge #2 Score":
                case "Judge #2":
                case "Athletic Points Judge 2":
                case "Judge 2 Points":
                    indexes[ConverterConstants.Judge2] = i;
                    break;
                case "Judge 2(Air &amp; Form) Score":
                case "Judge 2 Air &amp;  Form Score":
                    indexes[ConverterConstants.Judge2AirFormScore] = i;
                    break;
                case "Judge 2 Air Score":
                    indexes[ConverterConstants.Judge2AirScore] = i;
                    break;
                case "Judge 2 (Turns) Deductions":
                    indexes[ConverterConstants.Judge2Deductions] = i;
                    break;
                case "Judge 2 Form Score":
                    indexes[ConverterConstants.Judge2FormScore] = i;
                    break;
                case "Judge 2 Landing Score":
                    indexes[ConverterConstants.Judge2LandingScore] = i;
                    break;
                case "Judge 2 (Turns) Score":
                case "Judge 2 Score":
                case "Judge #2 Score (Rotations)":
                    indexes[ConverterConstants.Judge2Score] = i;
                    break;
                case "Technical Points Judge 2":
                    indexes[ConverterConstants.Judge2TechnicalPoints] = i;
                    break;
                case "Judge #3 Score":
                case "Original Judge #3 Score":
                case "Judge #3":
                case "Athletic Points Judge 3":
                case "Judge 3 Points":
                    indexes[ConverterConstants.Judge3] = i;
                    break;
                case "Judge 3(Air &amp; Form) Score":
                case "Judge 3 Air &amp; Form Score":
                    indexes[ConverterConstants.Judge3AirFormScore] = i;
                    break;
                case "Judge 3 Air Score":
                case "Judge 3 Score":
                    indexes[ConverterConstants.Judge3AirScore] = i;
                    break;
                case "Judge 3 (Turns) Deductions":
                    indexes[ConverterConstants.Judge3Deductions] = i;
                    break;
                case "Judge 3 Form Score":
                    indexes[ConverterConstants.Judge3FormScore] = i;
                    break;
                case "Judge 3 Landing Score":
                    indexes[ConverterConstants.Judge3LandingScore] = i;
                    break;
                case "Judge 3 (Turns) Score":
                case "Judge #3 Score (Amplitude)":
                    indexes[ConverterConstants.Judge3Score] = i;
                    break;
                case "Technical Points Judge 3":
                    indexes[ConverterConstants.Judge3TechnicalPoints] = i;
                    break;
                case "Judge #4 Score":
                case "Original Judge #4 Score":
                case "Judge #4":
                case "Athletic Points Judge 4":
                case "Judge 4 Points":
                    indexes[ConverterConstants.Judge4] = i;
                    break;
                case "Judge 4(Air &amp; Form) Score":
                case "Judge 4 Air &amp; Form Score":
                    indexes[ConverterConstants.Judge4AirFormScore] = i;
                    break;
                case "Judge 4 Air Score":
                    indexes[ConverterConstants.Judge4AirScore] = i;
                    break;
                case "Judge 4 (Turns) Deductions":
                    indexes[ConverterConstants.Judge4Deductions] = i;
                    break;
                case "Judge 4 Form Score":
                    indexes[ConverterConstants.Judge4FormScore] = i;
                    break;
                case "Judge 4 Landing Score":
                    indexes[ConverterConstants.Judge4LandingScore] = i;
                    break;
                case "Judge 4 (Turns) Score":
                case "Judge 4 Score":
                case "Judge #4 Score (Overall #1)":
                case "Judge #4 Score (Landings)":
                    indexes[ConverterConstants.Judge4Score] = i;
                    break;
                case "Technical Points Judge 4":
                    indexes[ConverterConstants.Judge4TechnicalPoints] = i;
                    break;
                case "Judge #5 Score":
                case "Original Judge #5 Score":
                case "Judge #5":
                case "Athletic Points Judge 5":
                case "Judge 5 Points":
                    indexes[ConverterConstants.Judge5] = i;
                    break;
                case "Judge 7 (Air #1) Score":
                    indexes[ConverterConstants.Judge7Air1Score] = i;
                    break;
                case "Judge 7 (Air #2) Score":
                    indexes[ConverterConstants.Judge7Air2Score] = i;
                    break;
                case "Judge 5(Air &amp; Form) Score":
                case "Judge 5 Air &amp; Form Score":
                    indexes[ConverterConstants.Judge5AirFormScore] = i;
                    break;
                case "Judge 5 Air Score":
                    indexes[ConverterConstants.Judge5AirScore] = i;
                    break;
                case "Judge 7 (Air) Score":
                    indexes[ConverterConstants.Judge7AirScore] = i;
                    break;
                case "Judge 5 (Turns) Deductions":
                    indexes[ConverterConstants.Judge5Deductions] = i;
                    break;
                case "Judge 5 Form Score":
                    indexes[ConverterConstants.Judge5FormScore] = i;
                    break;
                case "Judge 5 Landing Score":
                    indexes[ConverterConstants.Judge5LandingScore] = i;
                    break;
                case "Judge 7 (Landing) Score":
                    indexes[ConverterConstants.Judge7LandingScore] = i;
                    break;
                case "Judge 5 (Turns) Score":
                case "Judge 5 Score":
                case "Judge #5 Score (Technical Merit)":
                case "Judge #5 Score (Overall #2)":
                    indexes[ConverterConstants.Judge5Score] = i;
                    break;
                case "Technical Points Judge 5":
                    indexes[ConverterConstants.Judge5TechnicalPoints] = i;
                    break;
                case "Judge #6":
                case "Athletic Points Judge 6":
                    indexes[ConverterConstants.Judge6] = i;
                    break;
                case "Judge 6 (Air #1) Score":
                    indexes[ConverterConstants.Judge6Air1Score] = i;
                    break;
                case "Judge 6 (Air #2) Score":
                    indexes[ConverterConstants.Judge6Air2Score] = i;
                    break;
                case "Judge 6 (Air) Score":
                    indexes[ConverterConstants.Judge6AirScore] = i;
                    break;
                case "Judge 6 (Landing) Score":
                    indexes[ConverterConstants.Judge6LandingScore] = i;
                    break;
                case "Judge 6 Score":
                case "Judge #6 Score":
                    indexes[ConverterConstants.Judge6Score] = i;
                    break;
                case "Technical Points Judge 6":
                    indexes[ConverterConstants.Judge6TechnicalPoints] = i;
                    break;
                case "Judge #7":
                case "Athletic Points Judge 7":
                    indexes[ConverterConstants.Judge7] = i;
                    break;
                case "Technical Points Judge 7":
                    indexes[ConverterConstants.Judge7TechnicalPoints] = i;
                    break;
                case "Judge #8":
                    indexes[ConverterConstants.Judge8] = i;
                    break;
                case "Judge #9":
                    indexes[ConverterConstants.Judge9] = i;
                    break;
                case "Judge B":
                    indexes[ConverterConstants.JudgeB] = i;
                    break;
                case "Judge C":
                case "Judge C Points":
                    indexes[ConverterConstants.JudgeC] = i;
                    break;
                case "Judge E":
                case "Judge E Points":
                    indexes[ConverterConstants.JudgeE] = i;
                    break;
                case "Judge F":
                    indexes[ConverterConstants.JudgeF] = i;
                    break;
                case "Judge H":
                    indexes[ConverterConstants.JudgeH] = i;
                    break;
                case "Judge K":
                    indexes[ConverterConstants.JudgeK] = i;
                    break;
                case "Judge M":
                case "Judge M Points":
                    indexes[ConverterConstants.JudgeM] = i;
                    break;
                case "Total Judges' Points":
                case "Original Total Judges' Points":
                    indexes[ConverterConstants.Judges] = i;
                    break;
                case "Judges Favoring":
                case "Original Judges Favoring":
                    indexes[ConverterConstants.JudgesFavoring] = i;
                    break;
                case "Jump #1":
                case "Jump 1 Points":
                    indexes[ConverterConstants.Jump1] = i;
                    break;
                case "Jump #2":
                case "Jump 2 Points":
                    indexes[ConverterConstants.Jump2] = i;
                    break;
                case "Jump #1 Code":
                    indexes[ConverterConstants.Jump1Code] = i;
                    break;
                case "Jump #2 Code":
                    indexes[ConverterConstants.Jump2Code] = i;
                    break;
                case "Jump #1 Degree of Difficulty":
                    indexes[ConverterConstants.Jump1Difficulty] = i;
                    break;
                case "Jump #2 Degree of Difficulty":
                    indexes[ConverterConstants.Jump2Difficulty] = i;
                    break;
                case "Jump #3":
                    indexes[ConverterConstants.Jump3] = i;
                    break;
                case "Jump Code":
                case "Jump ID":
                    indexes[ConverterConstants.JumpCode] = i;
                    break;
                case "Jumping":
                    indexes[ConverterConstants.Jumping] = i;
                    break;
                case "Jump-Off":
                    indexes[ConverterConstants.JumpOff] = i;
                    break;
                case "Jump-Off Faults":
                    indexes[ConverterConstants.JumpOffFaults] = i;
                    break;
                case "Jump-Off Points/Time":
                    indexes[ConverterConstants.JumpOffPointsTime] = i;
                    break;
                case "Jump-Off Time":
                    indexes[ConverterConstants.JumpOffTime] = i;
                    break;
                case "Jump One Penalty Points":
                    indexes[ConverterConstants.JumpOnePenaltyPoints] = i;
                    break;
                case "Jump Penalties":
                    indexes[ConverterConstants.JumpPenalties] = i;
                    break;
                case "Jump Two Penalty Points":
                    indexes[ConverterConstants.JumpTwoPenaltyPoints] = i;
                    break;
                case "Kata":
                    indexes[ConverterConstants.Kata] = i;
                    break;
                case "Kata #1":
                    indexes[ConverterConstants.Kata1] = i;
                    break;
                case "Kata #2":
                    indexes[ConverterConstants.Kata2] = i;
                    break;
                case "Kilograms":
                case "K":
                    indexes[ConverterConstants.Kilograms] = i;
                    break;
                case "1 km (Pos)":
                case "1 km split (1 km rank)":
                    indexes[ConverterConstants.Km1] = i;
                    break;
                case "10 km":
                case "10 km (Pos)":
                case "10 km split (10 km rank)":
                    indexes[ConverterConstants.Km10] = i;
                    break;
                case "11 km split (11 km rank)":
                    indexes[ConverterConstants.Km11] = i;
                    break;
                case "12 km (Pos)":
                case "12 km split (12 km rank)":
                    indexes[ConverterConstants.Km12] = i;
                    break;
                case "13 km split (13 km rank)":
                    indexes[ConverterConstants.Km13] = i;
                    break;
                case "14 km (Pos)":
                case "14 km split (14 km rank)":
                    indexes[ConverterConstants.Km14] = i;
                    break;
                case "15 km":
                case "15 km (Pos)":
                case "15 km split (15 km rank)":
                    indexes[ConverterConstants.Km15] = i;
                    break;
                case "16 km (Pos)":
                case "16 km split (16 km rank)":
                    indexes[ConverterConstants.Km16] = i;
                    break;
                case "17 km split (17 km rank)":
                    indexes[ConverterConstants.Km17] = i;
                    break;
                case "18 km (Pos)":
                case "18 km split (18 km rank)":
                    indexes[ConverterConstants.Km18] = i;
                    break;
                case "19 km split (19 km rank)":
                    indexes[ConverterConstants.Km19] = i;
                    break;
                case "2 km (Pos)":
                case "2 km split (2 km rank)":
                    indexes[ConverterConstants.Km2] = i;
                    break;
                case "20 km":
                case "20 km (Pos)":
                    indexes[ConverterConstants.Km20] = i;
                    break;
                case "25 km":
                case "25 km (Pos)":
                    indexes[ConverterConstants.Km25] = i;
                    break;
                case "26 km (Pos)":
                    indexes[ConverterConstants.Km26] = i;
                    break;
                case "28 km (Pos)":
                    indexes[ConverterConstants.Km28] = i;
                    break;
                case "3 km (Pos)":
                case "3 km split (3 km rank)":
                    indexes[ConverterConstants.Km3] = i;
                    break;
                case "30 km":
                case "30 km (Pos)":
                    indexes[ConverterConstants.Km30] = i;
                    break;
                case "31 km (Pos)":
                    indexes[ConverterConstants.Km31] = i;
                    break;
                case "35 km":
                case "35 km (Pos)":
                    indexes[ConverterConstants.Km35] = i;
                    break;
                case "36 km (Pos)":
                    indexes[ConverterConstants.Km36] = i;
                    break;
                case "37 km (Pos)":
                    indexes[ConverterConstants.Km37] = i;
                    break;
                case "38 km (Pos)":
                    indexes[ConverterConstants.Km38] = i;
                    break;
                case "4 km (Pos)":
                case "4 km split (4 km rank)":
                    indexes[ConverterConstants.Km4] = i;
                    break;
                case "40 km":
                case "40 km (Pos)":
                    indexes[ConverterConstants.Km40] = i;
                    break;
                case "45 km":
                case "45 km (Pos)":
                    indexes[ConverterConstants.Km45] = i;
                    break;
                case "46 km (Pos)":
                    indexes[ConverterConstants.Km46] = i;
                    break;
                case "5 km":
                case "5 km (Pos)":
                case "5 km split (5 km rank)":
                    indexes[ConverterConstants.Km5] = i;
                    break;
                case "6 km (Pos)":
                case "6 km split (6 km rank)":
                    indexes[ConverterConstants.Km6] = i;
                    break;
                case "7 km (Pos)":
                case "7 km split (7 km rank)":
                    indexes[ConverterConstants.Km7] = i;
                    break;
                case "8 km (Pos)":
                case "8 km split (8 km rank)":
                    indexes[ConverterConstants.Km8] = i;
                    break;
                case "9 km (Pos)":
                case "9 km split (9 km rank)":
                    indexes[ConverterConstants.Km9] = i;
                    break;
                case "Kneeling":
                case "Kneeling Points":
                case "Kneeling/Sitting Points":
                    indexes[ConverterConstants.Kneeling] = i;
                    break;
                case "Landing Points":
                case "Landing Score":
                    indexes[ConverterConstants.LandingPoints] = i;
                    break;
                case "Lane":
                    indexes[ConverterConstants.Lane] = i;
                    break;
                case "Lane A Time":
                    indexes[ConverterConstants.Lane1] = i;
                    break;
                case "Lane B Time":
                    indexes[ConverterConstants.Lane2] = i;
                    break;
                case "Lap 1":
                case "Lap 1 Time":
                    indexes[ConverterConstants.Lap1] = i;
                    break;
                case "Lap 2":
                case "Lap 2 Time":
                    indexes[ConverterConstants.Lap2] = i;
                    break;
                case "Lap 3":
                case "Lap 3 Time":
                    indexes[ConverterConstants.Lap3] = i;
                    break;
                case "Lap 4":
                case "Lap 4 Time":
                    indexes[ConverterConstants.Lap4] = i;
                    break;
                case "Lap 5":
                case "Lap 5 Time":
                    indexes[ConverterConstants.Lap5] = i;
                    break;
                case "Lap 6":
                case "Lap 6 Time":
                    indexes[ConverterConstants.Lap6] = i;
                    break;
                case "Lap 7":
                case "Lap 7 Time":
                    indexes[ConverterConstants.Lap7] = i;
                    break;
                case "Lap 8 Time":
                    indexes[ConverterConstants.Lap8] = i;
                    break;
                case "Lap 9 Time":
                    indexes[ConverterConstants.Lap9] = i;
                    break;
                case "Lap Points":
                    indexes[ConverterConstants.LapPoints] = i;
                    break;
                case "Laps":
                    indexes[ConverterConstants.Laps] = i;
                    break;
                case "Last Race":
                    indexes[ConverterConstants.LastRace] = i;
                    break;
                case "Last Run Time":
                    indexes[ConverterConstants.LastRunTime] = i;
                    break;
                case "Lead":
                    indexes[ConverterConstants.Lead] = i;
                    break;
                case "Leg 1":
                    indexes[ConverterConstants.Leg1] = i;
                    break;
                case "Leg 2":
                    indexes[ConverterConstants.Leg2] = i;
                    break;
                case "Leg 3":
                    indexes[ConverterConstants.Leg3] = i;
                    break;
                case "Leg Rank":
                    indexes[ConverterConstants.LegRank] = i;
                    break;
                case "LG":
                    indexes[ConverterConstants.LG] = i;
                    break;
                case "Lift 1":
                    indexes[ConverterConstants.Lift1] = i;
                    break;
                case "Lift 2":
                    indexes[ConverterConstants.Lift2] = i;
                    break;
                case "Lift 3":
                    indexes[ConverterConstants.Lift3] = i;
                    break;
                case "Line Penalty":
                    indexes[ConverterConstants.LinePenalty] = i;
                    break;
                case "Location":
                    indexes[ConverterConstants.Location] = i;
                    break;
                case "LJP":
                case "Long Jump":
                    indexes[ConverterConstants.LongJump] = i;
                    break;
                case "Losses":
                case "L":
                case "Bouts Lost":
                case "ML":
                    indexes[ConverterConstants.Losses] = i;
                    break;
                case "Losses/Runs":
                    indexes[ConverterConstants.LossesRuns] = i;
                    break;
                case "Loss of Contact":
                case "Loss of Contact Warnings":
                    indexes[ConverterConstants.LostOfContact] = i;
                    break;
                case "100 metres":
                case "100 m":
                case "100 metre split":
                case "100 m Time":
                    indexes[ConverterConstants.M100] = i;
                    break;
                case "1,000 m":
                case "1000 m":
                    indexes[ConverterConstants.M1000] = i;
                    break;
                case "1,000-1,500 m":
                    indexes[ConverterConstants.M1000_1500] = i;
                    break;
                case "1,000-2,000 m":
                    indexes[ConverterConstants.M1000_2000] = i;
                    break;
                case "10,000 m":
                    indexes[ConverterConstants.M10000] = i;
                    break;
                case "100 metres Hurdles":
                    indexes[ConverterConstants.M100Hurdles] = i;
                    break;
                case "1100 m":
                case "1,100 m":
                    indexes[ConverterConstants.M1100] = i;
                    break;
                case "110 metres Hurdles":
                    indexes[ConverterConstants.M110Hurdles] = i;
                    break;
                case "1200 m":
                case "1,200 m":
                    indexes[ConverterConstants.M1200] = i;
                    break;
                case "1,300 m":
                    indexes[ConverterConstants.M1300] = i;
                    break;
                case "1,303 m":
                    indexes[ConverterConstants.M1303] = i;
                    break;
                case "1400 m":
                    indexes[ConverterConstants.M1400] = i;
                    break;
                case "150 m":
                case "150 metre split":
                case "150 m Time":
                    indexes[ConverterConstants.M150] = i;
                    break;
                case "1,500 metres":
                case "1,500 m":
                    indexes[ConverterConstants.M1500] = i;
                    break;
                case "1,500-2,000 m":
                    indexes[ConverterConstants.M1500_2000] = i;
                    break;
                case "1600 m":
                    indexes[ConverterConstants.M1600] = i;
                    break;
                case "1800 m":
                    indexes[ConverterConstants.M1800] = i;
                    break;
                case "200 metres":
                case "200 m":
                case "200 m Time":
                    indexes[ConverterConstants.M200] = i;
                    break;
                case "2000 m":
                case "2,000 m":
                    indexes[ConverterConstants.M2000] = i;
                    break;
                case "2200 m":
                    indexes[ConverterConstants.M2200] = i;
                    break;
                case "2400 m":
                    indexes[ConverterConstants.M2400] = i;
                    break;
                case "250 m":
                case "250 m Time":
                    indexes[ConverterConstants.M250] = i;
                    break;
                case "250-500 m":
                case "250-500 m Time":
                    indexes[ConverterConstants.M250_500] = i;
                    break;
                case "2600 m":
                    indexes[ConverterConstants.M2600] = i;
                    break;
                case "2800 m":
                    indexes[ConverterConstants.M2800] = i;
                    break;
                case "30 m":
                    indexes[ConverterConstants.M30] = i;
                    break;
                case "300 m":
                case "300 m Time":
                    indexes[ConverterConstants.M300] = i;
                    break;
                case "3000 m":
                case "3,000 m":
                    indexes[ConverterConstants.M3000] = i;
                    break;
                case "3200 m":
                    indexes[ConverterConstants.M3200] = i;
                    break;
                case "3400 m":
                    indexes[ConverterConstants.M3400] = i;
                    break;
                case "350 m":
                case "350 m Time":
                    indexes[ConverterConstants.M350] = i;
                    break;
                case "3600 m":
                    indexes[ConverterConstants.M3600] = i;
                    break;
                case "3800 m":
                    indexes[ConverterConstants.M3800] = i;
                    break;
                case "400 metres":
                case "400 m":
                    indexes[ConverterConstants.M400] = i;
                    break;
                case "4000 m":
                    indexes[ConverterConstants.M4000] = i;
                    break;
                case "4200 m":
                    indexes[ConverterConstants.M4200] = i;
                    break;
                case "4400 m":
                    indexes[ConverterConstants.M4400] = i;
                    break;
                case "4600 m":
                    indexes[ConverterConstants.M4600] = i;
                    break;
                case "4800 m":
                    indexes[ConverterConstants.M4800] = i;
                    break;
                case "50 m":
                case "50 metre split":
                case "50 m Time":
                    indexes[ConverterConstants.M50] = i;
                    break;
                case "500 m":
                    indexes[ConverterConstants.M500] = i;
                    break;
                case "500-1,000 m":
                    indexes[ConverterConstants.M500_1000] = i;
                    break;
                case "500-750 m":
                    indexes[ConverterConstants.M500_750] = i;
                    break;
                case "5,000 m":
                    indexes[ConverterConstants.M5000] = i;
                    break;
                case "5200 m":
                    indexes[ConverterConstants.M5200] = i;
                    break;
                case "5600 m":
                    indexes[ConverterConstants.M5600] = i;
                    break;
                case "6000 m":
                    indexes[ConverterConstants.M5800] = i;
                    break;
                case "60 m":
                    indexes[ConverterConstants.M60] = i;
                    break;
                case "600 m":
                    indexes[ConverterConstants.M600] = i;
                    break;
                case "6400 m":
                    indexes[ConverterConstants.M6000] = i;
                    break;
                case "6800 m":
                    indexes[ConverterConstants.M6800] = i;
                    break;
                case "70 m":
                    indexes[ConverterConstants.M70] = i;
                    break;
                case "700 m":
                    indexes[ConverterConstants.M700] = i;
                    break;
                case "7200 m":
                    indexes[ConverterConstants.M7200] = i;
                    break;
                case "750 m":
                    indexes[ConverterConstants.M750] = i;
                    break;
                case "750-1,000 m":
                    indexes[ConverterConstants.M750_1000] = i;
                    break;
                case "7600 m":
                    indexes[ConverterConstants.M7600] = i;
                    break;
                case "800 metres":
                case "800 m":
                    indexes[ConverterConstants.M800] = i;
                    break;
                case "8000 m":
                    indexes[ConverterConstants.M8000] = i;
                    break;
                case "80 metres Hurdles":
                    indexes[ConverterConstants.M80Hurdles] = i;
                    break;
                case "8400 m":
                    indexes[ConverterConstants.M8400] = i;
                    break;
                case "8800 m":
                    indexes[ConverterConstants.M8800] = i;
                    break;
                case "90 m":
                    indexes[ConverterConstants.M90] = i;
                    break;
                case "900 m":
                    indexes[ConverterConstants.M900] = i;
                    break;
                case "9200 m":
                    indexes[ConverterConstants.M9200] = i;
                    break;
                case "9600 m":
                    indexes[ConverterConstants.M9600] = i;
                    break;
                case "Maj. Ordinals":
                case "MO":
                    indexes[ConverterConstants.MajorityOrdinals] = i;
                    break;
                case "Maj. Placements":
                case "MP":
                    indexes[ConverterConstants.MajorityPlacements] = i;
                    break;
                case "Margin":
                case "Lap Margin":
                    indexes[ConverterConstants.Margin] = i;
                    break;
                case "Mark 1":
                    indexes[ConverterConstants.Mark1] = i;
                    break;
                case "Mark 2":
                    indexes[ConverterConstants.Mark2] = i;
                    break;
                case "Mark 3":
                    indexes[ConverterConstants.Mark3] = i;
                    break;
                case "Mark 4":
                    indexes[ConverterConstants.Mark4] = i;
                    break;
                case "Mark 5":
                    indexes[ConverterConstants.Mark5] = i;
                    break;
                case "Mark 6":
                    indexes[ConverterConstants.Mark6] = i;
                    break;
                case "Mark 7":
                    indexes[ConverterConstants.Mark7] = i;
                    break;
                case "Mark 8":
                    indexes[ConverterConstants.Mark8] = i;
                    break;
                case "Mark 9":
                    indexes[ConverterConstants.Mark9] = i;
                    break;
                case "Match":
                    indexes[ConverterConstants.Match] = i;
                    break;
                case "Matches":
                    indexes[ConverterConstants.Matches] = i;
                    break;
                case "Matches Played":
                    indexes[ConverterConstants.MatchesPlayed] = i;
                    break;
                case "Match Result":
                    indexes[ConverterConstants.MatchResult] = i;
                    break;
                case "Medal Race":
                case "MRP":
                    indexes[ConverterConstants.MedalRace] = i;
                    break;
                case "Merit":
                    indexes[ConverterConstants.Merit] = i;
                    break;
                case "1 mile":
                    indexes[ConverterConstants.Mile1] = i;
                    break;
                case "Mins":
                    indexes[ConverterConstants.Minutes] = i;
                    break;
                case "Total Misses":
                case "Total Misses thru Best Height Cleared":
                case "Misses":
                    indexes[ConverterConstants.Misses] = i;
                    break;
                case "Total Misses at Best Height Cleared":
                case "Misses at Best Height Cleared":
                    indexes[ConverterConstants.MissesAtBest] = i;
                    break;
                case "Musical Routine Points":
                    indexes[ConverterConstants.MusicalRoutinePoints] = i;
                    break;
                case "Team":
                case "Competitors":
                case "Competitor":
                case "Competitor(s)":
                case "Gymnast":
                case "Pair":
                case "Competitor (Seed)":
                case "Pair (Seed)":
                case "Cyclist":
                case "Diver":
                case "Divers":
                case "Competitors (Seed)":
                case "Athlete":
                case "Walker":
                case "Player":
                case "Athlete(s)":
                case "Diver(s)":
                case "Skater":
                case "Skater(s)":
                case "Judoka":
                case "Pentathlete":
                case "Boat":
                case "Swimmer":
                case "Lifter":
                case "Wrestler":
                    indexes[ConverterConstants.Name] = i;
                    break;
                case "Natural Action Goals":
                case "Natural Action Goals/Attempts":
                    indexes[ConverterConstants.NaturalActionGoals] = i;
                    break;
                case "Net Points":
                    indexes[ConverterConstants.NetPoints] = i;
                    break;
                case "NOC":
                case "Nat":
                    indexes[ConverterConstants.NOC] = i;
                    break;
                case "Notes":
                    indexes[ConverterConstants.Notes] = i;
                    break;
                case "Number":
                case "Nr":
                case "Bib":
                case "Start Nr.":
                case "Helmet Nr":
                    indexes[ConverterConstants.Number] = i;
                    break;
                case "Obstacle Penalties":
                    indexes[ConverterConstants.ObstaclePenalties] = i;
                    break;
                case "Offensive Faults":
                    indexes[ConverterConstants.OffensiveFaults] = i;
                    break;
                case "ORB":
                    indexes[ConverterConstants.OffensiveRebounds] = i;
                    break;
                case "Offsides":
                    indexes[ConverterConstants.Offsides] = i;
                    break;
                case "1P":
                    indexes[ConverterConstants.OnePoint] = i;
                    break;
                case "Opponent Errors":
                    indexes[ConverterConstants.OpponentErrors] = i;
                    break;
                case "OEP":
                case "OP":
                case "TOP":
                case "2EP":
                case "O1EP":
                case "Optional Exercise Points":
                    indexes[ConverterConstants.OptionalPoints] = i;
                    break;
                case "Ord":
                    indexes[ConverterConstants.Order] = i;
                    break;
                case "Ordinals":
                case "J'O":
                    indexes[ConverterConstants.Ordinals] = i;
                    break;
                case "Original":
                    indexes[ConverterConstants.Original] = i;
                    break;
                case "OSDFP":
                    indexes[ConverterConstants.OriginalSetDanceFactoredPlacements] = i;
                    break;
                case "Original Set Pattern Dance":
                    indexes[ConverterConstants.OriginalSetPatternDance] = i;
                    break;
                case "Other Penalties":
                    indexes[ConverterConstants.OtherPenalties] = i;
                    break;
                case "Other Penalty":
                    indexes[ConverterConstants.OtherPenalty] = i;
                    break;
                case "Overall Impression":
                    indexes[ConverterConstants.OverallImpression] = i;
                    break;
                case "Overall Impression Judge 1 Points":
                    indexes[ConverterConstants.OverallImpressionJudge1] = i;
                    break;
                case "Overall Impression Judge 2 Points":
                    indexes[ConverterConstants.OverallImpressionJudge2] = i;
                    break;
                case "Overall Impression Judge 3 Points":
                    indexes[ConverterConstants.OverallImpressionJudge3] = i;
                    break;
                case "Overall Impression Judge 4 Points":
                    indexes[ConverterConstants.OverallImpressionJudge4] = i;
                    break;
                case "Overall Impression Judge 5 Points":
                    indexes[ConverterConstants.OverallImpressionJudge5] = i;
                    break;
                case "Overall Impression Judge 6 Points":
                    indexes[ConverterConstants.OverallImpressionJudge6] = i;
                    break;
                case "Overall Impression Judge 7 Points":
                    indexes[ConverterConstants.OverallImpressionJudge7] = i;
                    break;
                case "Overall Impression Points":
                    indexes[ConverterConstants.OverallImpressionPoints] = i;
                    break;
                case "Overall Points":
                    indexes[ConverterConstants.OverallPoints] = i;
                    break;
                case "Overall Score (40%)":
                    indexes[ConverterConstants.OverallScore40] = i;
                    break;
                case "OTL":
                    indexes[ConverterConstants.OvertimeLose] = i;
                    break;
                case "OTW":
                    indexes[ConverterConstants.OvertimeWin] = i;
                    break;
                case "Parallel Bars":
                case "PB":
                    indexes[ConverterConstants.ParallelBars] = i;
                    break;
                case "Part #1":
                    indexes[ConverterConstants.Part1] = i;
                    break;
                case "Part #2":
                    indexes[ConverterConstants.Part2] = i;
                    break;
                case "Part #3":
                    indexes[ConverterConstants.Part3] = i;
                    break;
                case "Penalties":
                    indexes[ConverterConstants.Penalties] = i;
                    break;
                case "Penalty":
                    indexes[ConverterConstants.Penalty] = i;
                    break;
                case "Penalty Corner Attempts":
                    indexes[ConverterConstants.PenaltyCornerAttmepts] = i;
                    break;
                case "Penalty Corner Goals":
                    indexes[ConverterConstants.PenaltyCornerGoals] = i;
                    break;
                case "PC/PCA":
                    indexes[ConverterConstants.PenaltyCornerPenaltyCornerAttempts] = i;
                    break;
                case "Penalty Fouls":
                    indexes[ConverterConstants.PenaltyFouls] = i;
                    break;
                case "Penalty Goals":
                    indexes[ConverterConstants.PenaltyGoals] = i;
                    break;
                case "PIM":
                    indexes[ConverterConstants.PenaltyInfractionMinutes] = i;
                    break;
                case "Penalty Kicks":
                    indexes[ConverterConstants.PenaltyKicks] = i;
                    break;
                case "Penalty Points":
                case "PP":
                case "Penalty Points Earned":
                    indexes[ConverterConstants.PenaltyPoints] = i;
                    break;
                case "Penalty Shoot-Out":
                    indexes[ConverterConstants.PenaltyShootOut] = i;
                    break;
                case "Penalty Shot Goals":
                case "Penalty Shot Goals/Attempts":
                case "Penalty Shots":
                case "Penalty Shots/Attempts":
                    indexes[ConverterConstants.PenaltyShotGoals] = i;
                    break;
                case "Penalty Stroke Attempts":
                    indexes[ConverterConstants.PenaltyStrokeAttmepts] = i;
                    break;
                case "Penalty Stroke Goals":
                    indexes[ConverterConstants.PenaltyStrokeGoals] = i;
                    break;
                case "PS/PSA":
                    indexes[ConverterConstants.PenaltyStrokePenaltyStrokeAttempts] = i;
                    break;
                case "Penalty Time":
                case "Penalty Seconds":
                    indexes[ConverterConstants.PenaltyTime] = i;
                    break;
                case "%":
                    indexes[ConverterConstants.Percent] = i;
                    break;
                case "Period 1":
                case "Period 1 Score":
                    indexes[ConverterConstants.Period1] = i;
                    break;
                case "Period 2":
                case "Period 2 Score":
                    indexes[ConverterConstants.Period2] = i;
                    break;
                case "Period 3":
                case "Period 3 Score":
                    indexes[ConverterConstants.Period3] = i;
                    break;
                case "Period 4 Score":
                    indexes[ConverterConstants.Period4] = i;
                    break;
                case "Period 4":
                    indexes[ConverterConstants.Perion4] = i;
                    break;
                case "Personal Fouls":
                case "PF":
                    indexes[ConverterConstants.PersonalFouls] = i;
                    break;
                case "Pit Stop":
                    indexes[ConverterConstants.PitStop] = i;
                    break;
                case "P":
                case "Placement":
                    indexes[ConverterConstants.Placement] = i;
                    break;
                case "+/-":
                    indexes[ConverterConstants.PlusMinus] = i;
                    break;
                case "Points Race Points":
                    indexes[ConverterConstants.PointRacePoints] = i;
                    break;
                case "Points":
                case "Pts":
                case "DP":
                case "J'P":
                case "Athletic Points":
                case "PS":
                    indexes[ConverterConstants.Points] = i;
                    break;
                case "Points/Hits":
                    indexes[ConverterConstants.PointsHits] = i;
                    break;
                case "PPG":
                case "Scoring Average":
                    indexes[ConverterConstants.PointsPerGame] = i;
                    break;
                case "20 km Points Race":
                    indexes[ConverterConstants.PointsRace20km] = i;
                    break;
                case "30 km Points Race":
                    indexes[ConverterConstants.PointsRace30km] = i;
                    break;
                case "Points Ratio":
                    indexes[ConverterConstants.PointsRatio] = i;
                    break;
                case "Pole Vault":
                    indexes[ConverterConstants.PoleVault] = i;
                    break;
                case "Pommelled Horse":
                case "PH":
                    indexes[ConverterConstants.PommellHorse] = i;
                    break;
                case "Pos":
                case "Positions":
                    indexes[ConverterConstants.Position] = i;
                    break;
                case "Pounds (lbs)":
                    indexes[ConverterConstants.Pounds] = i;
                    break;
                case "Precision":
                case "Precision Points":
                    indexes[ConverterConstants.Precision] = i;
                    break;
                case "TPP":
                    indexes[ConverterConstants.PrecisionPoints] = i;
                    break;
                case "Preliminary Round":
                case "Preliminary Points":
                case "Preliminary Round Points":
                    indexes[ConverterConstants.PreliminaryRound] = i;
                    break;
                case "Presentation":
                    indexes[ConverterConstants.Presentation] = i;
                    break;
                case "Progr. Components":
                    indexes[ConverterConstants.ProgramComponents] = i;
                    break;
                case "Prone":
                case "Prone Points":
                    indexes[ConverterConstants.Prone] = i;
                    break;
                case "Pulls Won":
                    indexes[ConverterConstants.PullsWon] = i;
                    break;
                case "Putouts":
                    indexes[ConverterConstants.Putots] = i;
                    break;
                case "Qualification":
                case "QOP":
                case "Qualifying":
                case "QF":
                case "QRP":
                case "Jumping Qualifying":
                case "Qualifying Round":
                case "Qualifying Points":
                case "Qualification Points":
                    indexes[ConverterConstants.Qualification] = i;
                    break;
                case "Qualifying Round 1":
                    indexes[ConverterConstants.Qualification1] = i;
                    break;
                case "Qualifying Round 2":
                    indexes[ConverterConstants.Qualification2] = i;
                    break;
                case "Qualifying Round One":
                    indexes[ConverterConstants.QualifyingRoundOne] = i;
                    break;
                case "Qualifying Round Two":
                    indexes[ConverterConstants.QualifyingRoundTwo] = i;
                    break;
                case "After Quarter 1":
                    indexes[ConverterConstants.Quarter1] = i;
                    break;
                case "After Quarter 2":
                    indexes[ConverterConstants.Quarter2] = i;
                    break;
                case "After Quarter 3":
                    indexes[ConverterConstants.Quarter3] = i;
                    break;
                case "After Quarter 4":
                    indexes[ConverterConstants.Quarter4] = i;
                    break;
                case "Race":
                    indexes[ConverterConstants.Race] = i;
                    break;
                case "Race #1":
                case "Race 1":
                    indexes[ConverterConstants.Race1] = i;
                    break;
                case "Race 10":
                case "Race #10":
                    indexes[ConverterConstants.Race10] = i;
                    break;
                case "Race 11":
                case "Race #11":
                    indexes[ConverterConstants.Race11] = i;
                    break;
                case "Race 12":
                case "Race #12":
                    indexes[ConverterConstants.Race12] = i;
                    break;
                case "Race 13":
                    indexes[ConverterConstants.Race13] = i;
                    break;
                case "Race 14":
                    indexes[ConverterConstants.Race14] = i;
                    break;
                case "Race 15":
                    indexes[ConverterConstants.Race15] = i;
                    break;
                case "Race #2":
                case "Race 2":
                    indexes[ConverterConstants.Race2] = i;
                    break;
                case "Race #3":
                case "Race 3":
                    indexes[ConverterConstants.Race3] = i;
                    break;
                case "Race #4":
                case "Race 4":
                    indexes[ConverterConstants.Race4] = i;
                    break;
                case "Race #5":
                case "Race 5":
                    indexes[ConverterConstants.Race5] = i;
                    break;
                case "Race #6":
                case "Race 6":
                    indexes[ConverterConstants.Race6] = i;
                    break;
                case "Race #7":
                case "Race 7":
                    indexes[ConverterConstants.Race7] = i;
                    break;
                case "Race #8":
                case "Race 8":
                    indexes[ConverterConstants.Race8] = i;
                    break;
                case "Race #9":
                case "Race 9":
                    indexes[ConverterConstants.Race9] = i;
                    break;
                case "Rank":
                    indexes[ConverterConstants.Rank] = i;
                    break;
                case "Ranking Points":
                    indexes[ConverterConstants.RankingPoints] = i;
                    break;
                case "Rapid":
                case "Rapid Points":
                    indexes[ConverterConstants.Rapid] = i;
                    break;
                case "Final Points (Raw Points)":
                case "Raw Points":
                case "Raw Score":
                case "Raw Athletic Points":
                    indexes[ConverterConstants.RawPoints] = i;
                    break;
                case "Raw Technical Points":
                    indexes[ConverterConstants.RawTechnicalPoints] = i;
                    break;
                case "RBIs":
                case "Inning 7/RBIs":
                case "7th Inning/RBIs":
                    indexes[ConverterConstants.RBIs] = i;
                    break;
                case "Reaction Time":
                case "Reaction":
                case "RT":
                    indexes[ConverterConstants.ReactionTime] = i;
                    break;
                case "Reason":
                    indexes[ConverterConstants.Reason] = i;
                    break;
                case "Receptions":
                    indexes[ConverterConstants.Receptions] = i;
                    break;
                case "Red Cards":
                case "Red":
                    indexes[ConverterConstants.RedCard] = i;
                    break;
                case "RE":
                case "Reduced":
                    indexes[ConverterConstants.Reduced] = i;
                    break;
                case "Reduced Points":
                    indexes[ConverterConstants.ReducedPoints] = i;
                    break;
                case "Required Element Penalty":
                    indexes[ConverterConstants.RequiredElementPenalty] = i;
                    break;
                case "Result":
                    indexes[ConverterConstants.Result] = i;
                    break;
                case "After Extra Time Half 1":
                    indexes[ConverterConstants.ResultAfterExtraTimeHalf1] = i;
                    break;
                case "After Extra Time Half 2":
                    indexes[ConverterConstants.ResultAfterExtraTimeHalf2] = i;
                    break;
                case "After Extra Time Half 3":
                    indexes[ConverterConstants.ResultAfterExtraTimeHalf3] = i;
                    break;
                case "After Half 1":
                    indexes[ConverterConstants.ResultAfterHalf1] = i;
                    break;
                case "After Half 2":
                    indexes[ConverterConstants.ResultAfterHalf2] = i;
                    break;
                case "Rhythm Dance":
                    indexes[ConverterConstants.RhythmDance] = i;
                    break;
                case "Ribbon":
                case "Ribbon Score":
                case "Ribbons":
                    indexes[ConverterConstants.Ribbon] = i;
                    break;
                case "Riding":
                    indexes[ConverterConstants.Riding] = i;
                    break;
                case "Rings":
                    indexes[ConverterConstants.Rings] = i;
                    break;
                case "Rope":
                case "Rope Score":
                case "Ropes":
                case "Ropes Score":
                    indexes[ConverterConstants.Rope] = i;
                    break;
                case "Round":
                    indexes[ConverterConstants.Round] = i;
                    break;
                case "Round One Points":
                case "Group A Round #1":
                case "Group A Round 1":
                case "Group A Round One":
                case "Group B Round #1":
                case "Group B Round 1":
                case "Group B Round One":
                case "Group C Round One":
                case "Group D Round One":
                case "Group E Round One":
                case "R1":
                case "Round #1":
                case "Round 1 Score":
                case "Round One Score":
                case "ROP":
                case "Round 1 Points":
                case "Round 1 Deuk-Jeom":
                case "1":
                    indexes[ConverterConstants.Round1] = i;
                    break;
                case "Round Two Points":
                case "Group A Round #2":
                case "Group A Round 2":
                case "Group A Round Two":
                case "Group B Round #2":
                case "Group B Round 2":
                case "Group B Round Two":
                case "Group C Round Two":
                case "Group D Round Two":
                case "Group E Round Two":
                case "R2":
                case "Round #2":
                case "Round 2 Score":
                case "Round Two Score":
                case "Round 2 Points":
                case "Round 2 Deuk-Jeom":
                case "2":
                    indexes[ConverterConstants.Round2] = i;
                    break;
                case "Group A Round #3":
                case "Group A Round 3":
                case "Group A Round Three":
                case "Group B Round #3":
                case "Group B Round 3":
                case "Group B Round Three":
                case "Group C Round Three":
                case "Group D Round Three":
                case "Group E Round Three":
                case "R3":
                case "Round #3":
                case "Round 3 Score":
                case "Round 3 Points":
                case "Round 3 Deuk-Jeom":
                case "3":
                    indexes[ConverterConstants.Round3] = i;
                    break;
                case "R4":
                case "Round #4":
                case "Round 4 Score":
                case "Round 4 Points":
                case "Round 4 Deuk-Jeom":
                    indexes[ConverterConstants.Round4] = i;
                    break;
                case "R5":
                case "Round #5":
                case "Round 5 Points":
                    indexes[ConverterConstants.Round5] = i;
                    break;
                case "R6":
                case "Round #6":
                case "Round 6 Points":
                    indexes[ConverterConstants.Round6] = i;
                    break;
                case "Round #7":
                    indexes[ConverterConstants.Round7] = i;
                    break;
                case "Round One Penalties":
                    indexes[ConverterConstants.RoundOnePenalties] = i;
                    break;
                case "Round Three Penalties":
                    indexes[ConverterConstants.RoundThreePenalties] = i;
                    break;
                case "Round Two Penalties":
                    indexes[ConverterConstants.RoundTwoPenalties] = i;
                    break;
                case "Routine 1 Points":
                case "Routine #1":
                    indexes[ConverterConstants.Routine1] = i;
                    break;
                case "Routine 1 Degree Of Difficulty":
                    indexes[ConverterConstants.Routine1DegreeOfDifficulty] = i;
                    break;
                case "Routine 2 Points":
                case "Routine #2":
                    indexes[ConverterConstants.Routine2] = i;
                    break;
                case "Routine 2 Degree Of Difficulty":
                    indexes[ConverterConstants.Routine2DegreeOfDifficulty] = i;
                    break;
                case "Routine 3 Points":
                    indexes[ConverterConstants.Routine3] = i;
                    break;
                case "Routine 3 Degree Of Difficulty":
                    indexes[ConverterConstants.Routine3DegreeOfDifficulty] = i;
                    break;
                case "Routine 4 Points":
                    indexes[ConverterConstants.Routine4] = i;
                    break;
                case "Routine 4 Degree Of Difficulty":
                    indexes[ConverterConstants.Routine4DegreeOfDifficulty] = i;
                    break;
                case "Routine 5 Points":
                    indexes[ConverterConstants.Routine5] = i;
                    break;
                case "Routine 5 Degree Of Difficulty":
                    indexes[ConverterConstants.Routine5DegreeOfDifficulty] = i;
                    break;
                case "Run #1":
                case "Run 1 Points":
                case "Run 1":
                    indexes[ConverterConstants.Run1] = i;
                    break;
                case "Run #2":
                case "Run 2 Points":
                case "Run 2":
                    indexes[ConverterConstants.Run2] = i;
                    break;
                case "Run #3":
                case "Run 3 Points":
                case "Run 3":
                    indexes[ConverterConstants.Run3] = i;
                    break;
                case "Run #4":
                    indexes[ConverterConstants.Run4] = i;
                    break;
                case "Run #5":
                    indexes[ConverterConstants.Run5] = i;
                    break;
                case "Run #6":
                    indexes[ConverterConstants.Run6] = i;
                    break;
                case "10 km Running":
                case "2 km Running":
                    indexes[ConverterConstants.Running] = i;
                    break;
                case "Running score":
                    indexes[ConverterConstants.RunningScore] = i;
                    break;
                case "Running & Shooting":
                    indexes[ConverterConstants.RunningShooting] = i;
                    break;
                case "Running Time":
                    indexes[ConverterConstants.RunningTime] = i;
                    break;
                case "Runs":
                case "2nd Inning/Runs":
                case "Inning 2/Runs":
                    indexes[ConverterConstants.Runs] = i;
                    break;
                case "Runs Against/Triples":
                    indexes[ConverterConstants.RunsAgainstTriples] = i;
                    break;
                case "Runs Allowed":
                case "Team Hits/Runs Allowed":
                case "Team LOB/Home Runs Allowed":
                    indexes[ConverterConstants.RunsAllowed] = i;
                    break;
                case "Runs For/Doubles":
                    indexes[ConverterConstants.RunsForDoubles] = i;
                    break;
                case "Sacrifice Hits/Sacrifice Flies":
                case "12th Inning/Sacrifice Hits":
                case "13th Inning/Sacrifice Flies":
                case "Team Errors/Sacrifice Flies":
                case "Team Hits/Sacrifice Hits":
                    indexes[ConverterConstants.SacrificeHitsSacrificeFlies] = i;
                    break;
                case "SVS":
                case "Saves":
                    indexes[ConverterConstants.Saves] = i;
                    break;
                case "Score":
                    indexes[ConverterConstants.Score] = i;
                    break;
                case "Scorer":
                case "Scorer (assists)":
                    indexes[ConverterConstants.Scorer] = i;
                    break;
                case "10 km Scratch":
                    indexes[ConverterConstants.Scratch10km] = i;
                    break;
                case "15 km Scratch":
                    indexes[ConverterConstants.Scratch15km] = i;
                    break;
                case "Scratch Race Points":
                    indexes[ConverterConstants.ScratchRacePoints] = i;
                    break;
                case "Second Best Mark Distance":
                case "2nd Best":
                    indexes[ConverterConstants.SecondBestDistance] = i;
                    break;
                case "Second-Best Wave":
                    indexes[ConverterConstants.SecondBestWave] = i;
                    break;
                case "Round 5 (2 seconds) Points":
                    indexes[ConverterConstants.Seconds2] = i;
                    break;
                case "Round 4 (3 seconds) Points":
                    indexes[ConverterConstants.Seconds3] = i;
                    break;
                case "4 Seconds Points":
                case "Round 3 (4 seconds) Points":
                    indexes[ConverterConstants.Seconds4] = i;
                    break;
                    indexes[ConverterConstants.Seconds5] = i;
                    break;
                case "6 Seconds Points":
                case "Round 2 (6 seconds) Points":
                    indexes[ConverterConstants.Seconds6] = i;
                    break;
                case "8 Seconds Points":
                case "Round 1 (8 seconds) Points":
                    indexes[ConverterConstants.Seconds8] = i;
                    break;
                case "2TT":
                    indexes[ConverterConstants.SecondTimeTrial] = i;
                    break;
                case "Section 1 Points":
                    indexes[ConverterConstants.Section1] = i;
                    break;
                case "Section 2 Points":
                    indexes[ConverterConstants.Section2] = i;
                    break;
                case "Section 3 Points":
                    indexes[ConverterConstants.Section3] = i;
                    break;
                case "Section 4 Points":
                    indexes[ConverterConstants.Section4] = i;
                    break;
                case "Section 5 Points":
                    indexes[ConverterConstants.Section5] = i;
                    break;
                case "Section 6 Points":
                    indexes[ConverterConstants.Section6] = i;
                    break;
                case "Sections Points":
                    indexes[ConverterConstants.SectionPoints] = i;
                    break;
                case "Sections Score (60%)":
                    indexes[ConverterConstants.SectionsScore60] = i;
                    break;
                case "Seed":
                    indexes[ConverterConstants.Seed] = i;
                    break;
                case "Seeding Round":
                    indexes[ConverterConstants.SeedingRound] = i;
                    break;
                case "SF":
                case "S-FP":
                case "S-FRP":
                case "Semi-Final":
                case "Semi-Final Points":
                case "Semi-Final Round Points":
                    indexes[ConverterConstants.Semifinals] = i;
                    break;
                case "Series 1 Points":
                case "String #1":
                case "String 1 Points":
                case "Stage #1":
                case "Stage 1 Points":
                case "Stage One Points":
                    indexes[ConverterConstants.Series1] = i;
                    break;
                case "Series 10 Points":
                    indexes[ConverterConstants.Series10] = i;
                    break;
                case "Series 2 Points":
                case "String #2":
                case "String 2 Points":
                case "Stage #2":
                case "Stage 2 Points":
                case "Stage Two Points":
                    indexes[ConverterConstants.Series2] = i;
                    break;
                case "Series 3 Points":
                case "String #3":
                case "String 3 Points":
                case "Stage Three Points":
                    indexes[ConverterConstants.Series3] = i;
                    break;
                case "Series 4 Points":
                case "String #4":
                    indexes[ConverterConstants.Series4] = i;
                    break;
                case "Series 5 Points":
                case "String #5":
                    indexes[ConverterConstants.Series5] = i;
                    break;
                case "Series 6 Points":
                case "String #6":
                    indexes[ConverterConstants.Series6] = i;
                    break;
                case "Series 7 Points":
                    indexes[ConverterConstants.Series7] = i;
                    break;
                case "Series 8 Points":
                    indexes[ConverterConstants.Series8] = i;
                    break;
                case "Series 9 Points":
                    indexes[ConverterConstants.Series9] = i;
                    break;
                case "Serves":
                    indexes[ConverterConstants.Serves] = i;
                    break;
                case "Service Aces":
                    indexes[ConverterConstants.ServiceAces] = i;
                    break;
                case "Service Attempts":
                    indexes[ConverterConstants.ServiceAttempts] = i;
                    break;
                case "Service Faults":
                    indexes[ConverterConstants.ServiceFaults] = i;
                    break;
                case "Service Points":
                    indexes[ConverterConstants.ServicePoints] = i;
                    break;
                case "Set 1 Points":
                case "Set 1":
                case "Line-up Set 1":
                    indexes[ConverterConstants.Set1] = i;
                    break;
                case "Set 2 Points":
                case "Set 2":
                case "Line-up Set 2":
                    indexes[ConverterConstants.Set2] = i;
                    break;
                case "Set 3 Points":
                case "Set 3":
                case "Line-up Set 3":
                    indexes[ConverterConstants.Set3] = i;
                    break;
                case "Set 4 Points":
                case "Set 4":
                case "Line-up Set 4":
                    indexes[ConverterConstants.Set4] = i;
                    break;
                case "Set 5 Points":
                case "Set 5":
                case "Line-up Set 5":
                    indexes[ConverterConstants.Set5] = i;
                    break;
                case "Set 6":
                    indexes[ConverterConstants.Set6] = i;
                    break;
                case "Set Ratio":
                    indexes[ConverterConstants.SetRatio] = i;
                    break;
                case "Set Points":
                case "Sets":
                case "Sets Won":
                    indexes[ConverterConstants.Sets] = i;
                    break;
                case "Shift":
                    indexes[ConverterConstants.Shift] = i;
                    break;
                case "Shooting 1 Extra Shots":
                    indexes[ConverterConstants.Shooting1ExtraShots] = i;
                    break;
                case "Shooting 1 Misses":
                    indexes[ConverterConstants.Shooting1Misses] = i;
                    break;
                case "Shooting 1 Penalties":
                    indexes[ConverterConstants.Shooting1Penalties] = i;
                    break;
                case "Shooting 2 Extra Shots":
                    indexes[ConverterConstants.Shooting2ExtraShots] = i;
                    break;
                case "Shooting 2 Misses":
                    indexes[ConverterConstants.Shooting2Misses] = i;
                    break;
                case "Shooting 2 Penalties":
                    indexes[ConverterConstants.Shooting2Penalties] = i;
                    break;
                case "Shooting 3 Misses":
                    indexes[ConverterConstants.Shooting3Misses] = i;
                    break;
                case "Shooting 3 Penalties":
                    indexes[ConverterConstants.Shooting3Penalties] = i;
                    break;
                case "Shooting 4 Misses":
                    indexes[ConverterConstants.Shooting4Misses] = i;
                    break;
                case "Shooting 4 Penalties":
                    indexes[ConverterConstants.Shooting4Penalties] = i;
                    break;
                case "SE":
                    indexes[ConverterConstants.ShootingEfficiency] = i;
                    break;
                case "Shooting Time":
                case "Total Shooting Time":
                    indexes[ConverterConstants.ShootingTime] = i;
                    break;
                case "Shoot-off":
                case "Shoot-Off Points":
                case "Shoot-Off for 1st place":
                case "Shoot-Off for 3rd place":
                case "Shoot-Off 1/2":
                    indexes[ConverterConstants.ShootOff] = i;
                    break;
                case "Shoot-Off #1":
                case "Shoot-off 1":
                case "Shoot-off 1 Points":
                    indexes[ConverterConstants.ShootOff1] = i;
                    break;
                case "Shoot-Off #2":
                case "Shoot-off 2":
                case "Shoot-off 2 Points":
                    indexes[ConverterConstants.ShootOff2] = i;
                    break;
                case "Shoot-Off #3":
                case "Shoot-off 3 Points":
                    indexes[ConverterConstants.ShootOff3] = i;
                    break;
                case "Shoot-Off Arrow":
                    indexes[ConverterConstants.ShootOffArrow] = i;
                    break;
                case "Shoot-out Goals":
                    indexes[ConverterConstants.ShootOutGoals] = i;
                    break;
                case "Shooting 1":
                    indexes[ConverterConstants.Shooying1] = i;
                    break;
                case "Shooting 2":
                    indexes[ConverterConstants.Shooying2] = i;
                    break;
                case "Shooting 3":
                    indexes[ConverterConstants.Shooying3] = i;
                    break;
                case "Shooting 4":
                    indexes[ConverterConstants.Shooying4] = i;
                    break;
                case "Short Dance":
                    indexes[ConverterConstants.ShortDance] = i;
                    break;
                case "Short-handed Goals":
                    indexes[ConverterConstants.ShortHandedGoals] = i;
                    break;
                case "Short Program":
                case "Men's Short Program":
                case "Women's Short Program":
                case "Pairs Short Program":
                case "Short":
                    indexes[ConverterConstants.ShortProgram] = i;
                    break;
                case "Shot":
                case "Shot #":
                    indexes[ConverterConstants.Shot] = i;
                    break;
                case "Shot 1 Points":
                    indexes[ConverterConstants.Shot1] = i;
                    break;
                case "Shot 10 Points":
                    indexes[ConverterConstants.Shot10] = i;
                    break;
                case "Shot 11 Points":
                    indexes[ConverterConstants.Shot11] = i;
                    break;
                case "Shot 12 Points":
                    indexes[ConverterConstants.Shot12] = i;
                    break;
                case "Shot 13 Points":
                    indexes[ConverterConstants.Shot13] = i;
                    break;
                case "Shot 14 Points":
                    indexes[ConverterConstants.Shot14] = i;
                    break;
                case "Shot 15 Points":
                    indexes[ConverterConstants.Shot15] = i;
                    break;
                case "Shot 16 Points":
                    indexes[ConverterConstants.Shot16] = i;
                    break;
                case "Shot 17 Points":
                    indexes[ConverterConstants.Shot17] = i;
                    break;
                case "Shot 18 Points":
                    indexes[ConverterConstants.Shot18] = i;
                    break;
                case "Shot 19 Points":
                    indexes[ConverterConstants.Shot19] = i;
                    break;
                case "Shot 2 Points":
                    indexes[ConverterConstants.Shot2] = i;
                    break;
                case "Shot 20 Points":
                    indexes[ConverterConstants.Shot20] = i;
                    break;
                case "Shot 21 Points":
                    indexes[ConverterConstants.Shot21] = i;
                    break;
                case "Shot 22 Points":
                    indexes[ConverterConstants.Shot22] = i;
                    break;
                case "Shot 23 Points":
                    indexes[ConverterConstants.Shot23] = i;
                    break;
                case "Shot 24 Points":
                    indexes[ConverterConstants.Shot24] = i;
                    break;
                case "Shot 3 Points":
                    indexes[ConverterConstants.Shot3] = i;
                    break;
                case "Shot 4 Points":
                    indexes[ConverterConstants.Shot4] = i;
                    break;
                case "Shot 5 Points":
                    indexes[ConverterConstants.Shot5] = i;
                    break;
                case "Shot 6 Points":
                    indexes[ConverterConstants.Shot6] = i;
                    break;
                case "Shot 7 Points":
                    indexes[ConverterConstants.Shot7] = i;
                    break;
                case "Shot 8 Points":
                    indexes[ConverterConstants.Shot8] = i;
                    break;
                case "Shot 9 Points":
                    indexes[ConverterConstants.Shot9] = i;
                    break;
                case "SOG":
                    indexes[ConverterConstants.ShotOnGoal] = i;
                    break;
                case "SPP":
                case "Shot Put":
                    indexes[ConverterConstants.ShotPut] = i;
                    break;
                case "Shots":
                    indexes[ConverterConstants.Shots] = i;
                    break;
                case "Shots 1":
                case "Valid Shots at Station #1":
                    indexes[ConverterConstants.Shots1] = i;
                    break;
                case "Shots 2":
                case "Valid Shots at Station #2":
                    indexes[ConverterConstants.Shots2] = i;
                    break;
                case "Shots 3":
                case "Valid Shots at Station #3":
                    indexes[ConverterConstants.Shots3] = i;
                    break;
                case "Shots 4":
                case "Valid Shots at Station #4":
                    indexes[ConverterConstants.Shots4] = i;
                    break;
                case "6 metre Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.Shots6M] = i;
                    break;
                case "9 metre Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.Shots9M] = i;
                    break;
                case "7 metre Shots / Attempts":
                case "7 metre Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.Shots7M] = i;
                    break;
                case "Shots on Goal":
                    indexes[ConverterConstants.ShotsOnGoal] = i;
                    break;
                case "Side Outs":
                    indexes[ConverterConstants.SideOuts] = i;
                    break;
                case "Situation":
                    indexes[ConverterConstants.Situation] = i;
                    break;
                case "Skiing":
                    indexes[ConverterConstants.Skiing] = i;
                    break;
                case "Ski Jumping, Large Hill":
                    indexes[ConverterConstants.SkiJumpingLargeHill] = i;
                    break;
                case "Ski Jumping, Normal Hill":
                    indexes[ConverterConstants.SkiJumpingNormalHill] = i;
                    break;
                case "Slalom":
                    indexes[ConverterConstants.Slalom] = i;
                    break;
                case "Slow Run":
                case "Slow Run Points":
                    indexes[ConverterConstants.SlowRun] = i;
                    break;
                case "Snatch":
                    indexes[ConverterConstants.Snatch] = i;
                    break;
                case "Speed":
                    indexes[ConverterConstants.Speed] = i;
                    break;
                case "Spike Points":
                case "Spike Points / Side-Outs":
                    indexes[ConverterConstants.SpikePoints] = i;
                    break;
                case "Spikes":
                    indexes[ConverterConstants.Spikes] = i;
                    break;
                case "Split (Pos)":
                    indexes[ConverterConstants.Split] = i;
                    break;
                case "Split 1":
                case "Split time lap 1":
                    indexes[ConverterConstants.Split1] = i;
                    break;
                case "Split time lap 10":
                    indexes[ConverterConstants.Split10] = i;
                    break;
                case "Split time lap 11":
                    indexes[ConverterConstants.Split11] = i;
                    break;
                case "Split time lap 12":
                    indexes[ConverterConstants.Split12] = i;
                    break;
                case "Split time lap 13":
                    indexes[ConverterConstants.Split13] = i;
                    break;
                case "Split time lap 14":
                    indexes[ConverterConstants.Split14] = i;
                    break;
                case "Split time lap 15":
                    indexes[ConverterConstants.Split15] = i;
                    break;
                case "Split 2":
                case "Split time lap 2":
                    indexes[ConverterConstants.Split2] = i;
                    break;
                case "Split 3":
                case "Split time lap 3":
                    indexes[ConverterConstants.Split3] = i;
                    break;
                case "Split 4":
                case "Split time lap 4":
                    indexes[ConverterConstants.Split4] = i;
                    break;
                case "Split 5":
                case "Split time lap 5":
                    indexes[ConverterConstants.Split5] = i;
                    break;
                case "Split time lap 6":
                    indexes[ConverterConstants.Split6] = i;
                    break;
                case "Split time lap 7":
                    indexes[ConverterConstants.Split7] = i;
                    break;
                case "Split time lap 8":
                    indexes[ConverterConstants.Split8] = i;
                    break;
                case "Split time lap 9":
                    indexes[ConverterConstants.Split9] = i;
                    break;
                case "SP":
                    indexes[ConverterConstants.Sprint] = i;
                    break;
                case "Sprint 1 Points":
                case "Split Time 1":
                    indexes[ConverterConstants.Sprint1] = i;
                    break;
                case "Sprint 10 Points":
                    indexes[ConverterConstants.Sprint10] = i;
                    break;
                case "Sprint 11 Points":
                    indexes[ConverterConstants.Sprint11] = i;
                    break;
                case "Sprint 12 Points":
                    indexes[ConverterConstants.Sprint12] = i;
                    break;
                case "Sprint 13 Points":
                    indexes[ConverterConstants.Sprint13] = i;
                    break;
                case "Sprint 14 Points":
                    indexes[ConverterConstants.Sprint14] = i;
                    break;
                case "Sprint 15 Points":
                    indexes[ConverterConstants.Sprint15] = i;
                    break;
                case "Sprint 16 Points":
                    indexes[ConverterConstants.Sprint16] = i;
                    break;
                case "Sprint 17 Points":
                    indexes[ConverterConstants.Sprint17] = i;
                    break;
                case "Sprint 18 Points":
                    indexes[ConverterConstants.Sprint18] = i;
                    break;
                case "Sprint 19 Points":
                    indexes[ConverterConstants.Sprint19] = i;
                    break;
                case "Sprint 2 Points":
                case "Split Time 2":
                    indexes[ConverterConstants.Sprint2] = i;
                    break;
                case "Sprint 20 Points":
                    indexes[ConverterConstants.Sprint20] = i;
                    break;
                case "Sprint 3 Points":
                case "Split Time 3":
                    indexes[ConverterConstants.Sprint3] = i;
                    break;
                case "Sprint 4 Points":
                case "Split Time 4":
                    indexes[ConverterConstants.Sprint4] = i;
                    break;
                case "Sprint 5 Points":
                case "Split Time 5":
                    indexes[ConverterConstants.Sprint5] = i;
                    break;
                case "Sprint 6 Points":
                    indexes[ConverterConstants.Sprint6] = i;
                    break;
                case "Sprint 7 Points":
                    indexes[ConverterConstants.Sprint7] = i;
                    break;
                case "Sprint 8 Points":
                    indexes[ConverterConstants.Sprint8] = i;
                    break;
                case "Sprint 9 Points":
                    indexes[ConverterConstants.Sprint9] = i;
                    break;
                case "Sprints Won/Contested":
                    indexes[ConverterConstants.SprintsWon] = i;
                    break;
                case "200ST":
                    indexes[ConverterConstants.ST200] = i;
                    break;
                case "Standing":
                case "Standing Points":
                    indexes[ConverterConstants.Standing] = i;
                    break;
                case "Start Behind":
                    indexes[ConverterConstants.StartBehind] = i;
                    break;
                case "Start Loop":
                    indexes[ConverterConstants.StartLoop] = i;
                    break;
                case "Steals":
                case "STL":
                    indexes[ConverterConstants.Steals] = i;
                    break;
                case "Steeplechase":
                    indexes[ConverterConstants.Steeplechase] = i;
                    break;
                case "Stolen Bases":
                case "Inning 10/Stolen Bases":
                case "Stolen Bases/Caught Stealing":
                case "10th Inning/Stolen Bases":
                    indexes[ConverterConstants.StolenBasesCaughtStealing] = i;
                    break;
                case "St. Order":
                    indexes[ConverterConstants.StOrder] = i;
                    break;
                case "Strikeouts":
                case "9th Inning/Strikeouts":
                case "Inning 9/Strikeouts":
                case "Home-Visiting Team/Strikeouts":
                    indexes[ConverterConstants.Strieouts] = i;
                    break;
                case "Strokes":
                    indexes[ConverterConstants.Strokes] = i;
                    break;
                case "Strokes Hole 1":
                    indexes[ConverterConstants.Strokes1] = i;
                    break;
                case "Strokes Hole 2":
                    indexes[ConverterConstants.Strokes2] = i;
                    break;
                case "Strokes Hole 3":
                    indexes[ConverterConstants.Strokes3] = i;
                    break;
                case "Strokes Hole 4":
                    indexes[ConverterConstants.Strokes4] = i;
                    break;
                case "Style Points":
                    indexes[ConverterConstants.StylePoints] = i;
                    break;
                case "Suspensions":
                    indexes[ConverterConstants.Suspensions] = i;
                    break;
                case "2-minute Suspensions":
                    indexes[ConverterConstants.Suspensions2Minutes] = i;
                    break;
                case "1.5 km Swimming":
                case "300 m Swimming":
                case "Swimming":
                    indexes[ConverterConstants.Swimming] = i;
                    break;
                case "Swim-Off":
                    indexes[ConverterConstants.SwimOff] = i;
                    break;
                case "S1":
                case "SJ1S":
                    indexes[ConverterConstants.Synchronization1] = i;
                    break;
                case "S2":
                case "SJ2S":
                    indexes[ConverterConstants.Synchronization2] = i;
                    break;
                case "S3":
                case "SJ3S":
                    indexes[ConverterConstants.Synchronization3] = i;
                    break;
                case "S4":
                case "SJ4S":
                    indexes[ConverterConstants.Synchronization4] = i;
                    break;
                case "S5":
                case "SJ5S":
                    indexes[ConverterConstants.Synchronization5] = i;
                    break;
                case "Target":
                    indexes[ConverterConstants.Target] = i;
                    break;
                case "Round 1 Targets Hit":
                case "Round 1 Targets Hits":
                    indexes[ConverterConstants.TargetHits1] = i;
                    break;
                case "Round 2 Targets Hit":
                    indexes[ConverterConstants.TargetHits2] = i;
                    break;
                case "Targets":
                    indexes[ConverterConstants.Targets] = i;
                    break;
                case "Targets Hit":
                case "TH":
                    indexes[ConverterConstants.TargetsHit] = i;
                    break;
                case "Team Penalty Points":
                    indexes[ConverterConstants.TeamPenaltyPoints] = i;
                    break;
                case "Team Points":
                case "TP":
                    indexes[ConverterConstants.TeamPoints] = i;
                    break;
                case "Technical Factor":
                    indexes[ConverterConstants.TechnicalFactor] = i;
                    break;
                case "Technical Faults":
                    indexes[ConverterConstants.TechnicalFaults] = i;
                    break;
                case "Technical Merit":
                case "Technical Merit Points":
                    indexes[ConverterConstants.TechnicalMerit] = i;
                    break;
                case "Technical Merit Difficulty Points":
                    indexes[ConverterConstants.TechnicalMeritDifficultyPoints] = i;
                    break;
                case "Technical Merit Execution Points":
                    indexes[ConverterConstants.TechnicalMeritExecutionPoints] = i;
                    break;
                case "Technical Merit Synchronization Points":
                    indexes[ConverterConstants.TechnicalMeritSynchronizationPoints] = i;
                    break;
                case "Tech. Points":
                case "Technical Points":
                    indexes[ConverterConstants.TechnicalPoints] = i;
                    break;
                case "Technical Routine":
                case "Technical Routine Points":
                    indexes[ConverterConstants.TechnicalRoutine] = i;
                    break;
                case "Tech.":
                case "Tech. Elements":
                case "Technique":
                case "T/E":
                    indexes[ConverterConstants.Technique] = i;
                    break;
                case "Tempo Race Points":
                    indexes[ConverterConstants.TempoRacePoints] = i;
                    break;
                case "3TT":
                    indexes[ConverterConstants.ThirdTimeTrial] = i;
                    break;
                case "3P":
                    indexes[ConverterConstants.ThreePoints] = i;
                    break;
                case "3-point Field Goals Attempts":
                    indexes[ConverterConstants.ThreePointsAttempts] = i;
                    break;
                case "3-point Field Goals Made":
                    indexes[ConverterConstants.ThreePointsMade] = i;
                    break;
                case "Throw-Off":
                case "Throw-Off (Imperial)":
                    indexes[ConverterConstants.ThrowOff] = i;
                    break;
                case "56 lb Weight Throw":
                    indexes[ConverterConstants.ThrowWeightLb56] = i;
                    break;
                case "Tie-Break":
                case "Tie-breaker":
                case "Tie-breaking Score":
                case "TS-OP":
                case "Tiebreak Points":
                case "Tie-Break Score":
                case "S-OP":
                case "Match tie-break":
                    indexes[ConverterConstants.TieBreak] = i;
                    break;
                case "Tiebreak 1":
                case "Tie-break 1":
                    indexes[ConverterConstants.TieBreak1] = i;
                    break;
                case "Tiebreak 2":
                case "Tie-break 2":
                    indexes[ConverterConstants.TieBreak2] = i;
                    break;
                case "Tie-break 3":
                    indexes[ConverterConstants.TieBreak3] = i;
                    break;
                case "Tie-break 4":
                    indexes[ConverterConstants.TieBreak4] = i;
                    break;
                case "Tie-Breaking Time":
                    indexes[ConverterConstants.TieBreakingTime] = i;
                    break;
                case "Time":
                case "Time Adjustment":
                    indexes[ConverterConstants.Time] = i;
                    break;
                case "Time (A)":
                case "Time (Automatic)":
                case "T(A)":
                    indexes[ConverterConstants.TimeAutomatic] = i;
                    break;
                case "Tim./Exec.":
                    indexes[ConverterConstants.TimeExec] = i;
                    break;
                case "Time Fault/Other Faults":
                    indexes[ConverterConstants.TimeFaultOtherFaults] = i;
                    break;
                case "Time of Flight Points":
                    indexes[ConverterConstants.TimeFlightPoints] = i;
                    break;
                case "Time (H)":
                case "Time (Hand)":
                case "T(H)":
                    indexes[ConverterConstants.TimeHand] = i;
                    break;
                case "Time/Margin":
                case "Time Margin":
                case "T/M":
                    indexes[ConverterConstants.TimeMargin] = i;
                    break;
                case "Time Penalties":
                    indexes[ConverterConstants.TimePenalties] = i;
                    break;
                case "Time Penalty":
                    indexes[ConverterConstants.TimePenalty] = i;
                    break;
                //case "TP":
                //    indexes[ConverterConstants.TimePlayed] = i;
                //    break;
                case "Time Points":
                    indexes[ConverterConstants.TimePoints] = i;
                    break;
                case "Time at Station #1":
                case "Shoot Time 1":
                    indexes[ConverterConstants.TimeStation1] = i;
                    break;
                case "Time at Station #2":
                case "Shoot Time 2":
                    indexes[ConverterConstants.TimeStation2] = i;
                    break;
                case "Time at Station #3":
                case "Shoot Time 3":
                    indexes[ConverterConstants.TimeStation3] = i;
                    break;
                case "Time at Station #4":
                case "Shoot Time 4":
                    indexes[ConverterConstants.TimeStation4] = i;
                    break;
                case "TT":
                    indexes[ConverterConstants.TimeTrial] = i;
                    break;
                case "1,000 m Time Trial":
                    indexes[ConverterConstants.TimeTrial1000] = i;
                    break;
                case "500 m Time Trial":
                    indexes[ConverterConstants.TimeTrial500] = i;
                    break;
                case "To Par":
                    indexes[ConverterConstants.ToPar] = i;
                    break;
                case "Total":
                    indexes[ConverterConstants.Total] = i;
                    break;
                case "Total Attempts":
                case "Total Attempts thru Best Height Cleared":
                    indexes[ConverterConstants.TotalAttempts] = i;
                    break;
                case "TFP":
                    indexes[ConverterConstants.TotalFactoredPlacements] = i;
                    break;
                case "TO":
                case "TOM":
                case "TOM/TO":
                    indexes[ConverterConstants.TotalOridianls] = i;
                    break;
                case "Total Penalty Points":
                    indexes[ConverterConstants.TotalPenaltyPoints] = i;
                    break;
                case "Total Points":
                case "Total Score":
                    indexes[ConverterConstants.TotalPoints] = i;
                    break;
                case "TPF":
                case "In Favor":
                    indexes[ConverterConstants.TotalPointsInFavor] = i;
                    break;
                case "TPG":
                    indexes[ConverterConstants.TotalPointsPerGame] = i;
                    break;
                case "TRB":
                case "Total Rebounds":
                    indexes[ConverterConstants.TotalRebounds] = i;
                    break;
                case "Total Shots at Station #1":
                    indexes[ConverterConstants.TotalShots1] = i;
                    break;
                case "Total Shots at Station #2":
                    indexes[ConverterConstants.TotalShots2] = i;
                    break;
                case "Total Shots at Station #3":
                    indexes[ConverterConstants.TotalShots3] = i;
                    break;
                case "Total Shots at Station #4":
                    indexes[ConverterConstants.TotalShots4] = i;
                    break;
                case "Total Time":
                    indexes[ConverterConstants.TotalTime] = i;
                    break;
                case "TD":
                case "Touches Delivered":
                case "Team Touches Delivered":
                case "Individual Touches Delivered":
                    indexes[ConverterConstants.TouchesDelivered] = i;
                    break;
                case "TR":
                    indexes[ConverterConstants.TouchesReceived] = i;
                    break;
                case "Transition 1":
                case "Transition 1 (swim-to-cycle)":
                    indexes[ConverterConstants.Transition1] = i;
                    break;
                case "Transition 2":
                case "Transition 2 (cycle-to-run)":
                    indexes[ConverterConstants.Transition2] = i;
                    break;
                case "Trick 1":
                    indexes[ConverterConstants.Trick1] = i;
                    break;
                case "Trick 2":
                    indexes[ConverterConstants.Trick2] = i;
                    break;
                case "Trick 3":
                    indexes[ConverterConstants.Trick3] = i;
                    break;
                case "Trick 4":
                    indexes[ConverterConstants.Trick4] = i;
                    break;
                case "Trick 5":
                    indexes[ConverterConstants.Trick5] = i;
                    break;
                case "Trick 6":
                    indexes[ConverterConstants.Trick6] = i;
                    break;
                case "Trick ID":
                    indexes[ConverterConstants.TrickId] = i;
                    break;
                case "Tries":
                    indexes[ConverterConstants.Tries] = i;
                    break;
                case "Inning 5/Triples":
                case "5th Inning/Triples":
                    indexes[ConverterConstants.Triples] = i;
                    break;
                case "Trunks":
                    indexes[ConverterConstants.Trunks] = i;
                    break;
                case "Turnover Fouls":
                    indexes[ConverterConstants.TurnoverFouls] = i;
                    break;
                case "TOV":
                    indexes[ConverterConstants.Turnovers] = i;
                    break;
                case "Turns Points":
                    indexes[ConverterConstants.TurnsPoints] = i;
                    break;
                case "2P":
                    indexes[ConverterConstants.TwoPoints] = i;
                    break;
                case "2-point Field Goals Attempts":
                    indexes[ConverterConstants.TwoPointsAttempts] = i;
                    break;
                case "2-point Field Goals Made":
                    indexes[ConverterConstants.TwoPointsMade] = i;
                    break;
                case "Uneven Bars":
                    indexes[ConverterConstants.UnevenBars] = i;
                    break;
                case "VAL":
                    indexes[ConverterConstants.Value] = i;
                    break;
                case "Vault 1":
                case "Jump 1":
                case "1JP":
                case "FJ#1P":
                case "J#1P":
                    indexes[ConverterConstants.Vault1] = i;
                    break;
                case "Vault 2":
                case "Jump 2":
                case "2JP":
                case "FJ#2P":
                case "J#2P":
                case "C2EP":
                case "O2EP":
                    indexes[ConverterConstants.Vault2] = i;
                    break;
                case "J-O1JP":
                case "J-OP":
                    indexes[ConverterConstants.VaultOff1] = i;
                    break;
                case "J-O2JP":
                    indexes[ConverterConstants.VaultOff2] = i;
                    break;
                case "Victories in Tie Group":
                    indexes[ConverterConstants.VictoriesInTieGroup] = i;
                    break;
                case "Walks":
                case "Inning 8/Walks":
                case "8th Inning/Walks":
                    indexes[ConverterConstants.Walks] = i;
                    break;
                case "Warm-Up Penalties":
                case "W-UP":
                    indexes[ConverterConstants.WarmUpPenalties] = i;
                    break;
                case "Warnings":
                case "Total Warnings":
                    indexes[ConverterConstants.Warnings] = i;
                    break;
                case "Wave 1":
                    indexes[ConverterConstants.Wave1] = i;
                    break;
                case "Wave 10":
                    indexes[ConverterConstants.Wave10] = i;
                    break;
                case "Wave 11":
                    indexes[ConverterConstants.Wave11] = i;
                    break;
                case "Wave 12":
                    indexes[ConverterConstants.Wave12] = i;
                    break;
                case "Wave 13":
                    indexes[ConverterConstants.Wave13] = i;
                    break;
                case "Wave 14":
                    indexes[ConverterConstants.Wave14] = i;
                    break;
                case "Wave 15":
                    indexes[ConverterConstants.Wave15] = i;
                    break;
                case "Wave 16":
                    indexes[ConverterConstants.Wave16] = i;
                    break;
                case "Wave 2":
                    indexes[ConverterConstants.Wave2] = i;
                    break;
                case "Wave 3":
                    indexes[ConverterConstants.Wave3] = i;
                    break;
                case "Wave 4":
                    indexes[ConverterConstants.Wave4] = i;
                    break;
                case "Wave 5":
                    indexes[ConverterConstants.Wave5] = i;
                    break;
                case "Wave 6":
                    indexes[ConverterConstants.Wave6] = i;
                    break;
                case "Wave 7":
                    indexes[ConverterConstants.Wave7] = i;
                    break;
                case "Wave 8":
                    indexes[ConverterConstants.Wave8] = i;
                    break;
                case "Wave 9":
                    indexes[ConverterConstants.Wave9] = i;
                    break;
                case "Waza-ari":
                    indexes[ConverterConstants.Wazaari] = i;
                    break;
                case "Weight":
                    indexes[ConverterConstants.Weight] = i;
                    break;
                case "Wild Pitches":
                    indexes[ConverterConstants.WildPitches] = i;
                    break;
                case "Wind":
                case "Wind Speed":
                    indexes[ConverterConstants.Wind] = i;
                    break;
                case "Wind Compensation Points":
                    indexes[ConverterConstants.WindPoints] = i;
                    break;
                case "Wing Shots / Attempts - Saves / Shots":
                    indexes[ConverterConstants.WingShots] = i;
                    break;
                case "Wins":
                case "W":
                case "Races Won":
                case "Bouts Won":
                case "MW":
                case "Matches Won":
                    indexes[ConverterConstants.Wins] = i;
                    break;
                case "Wins/At Bats":
                    indexes[ConverterConstants.WinsAtBats] = i;
                    break;
                case "Won/Lost/Saves":
                case "14th Inning/Win/Lost/Save":
                case "Team LOB/Win/Lost/Save":
                    indexes[ConverterConstants.WonLostSaves] = i;
                    break;
                case "Xs":
                    indexes[ConverterConstants.Xs] = i;
                    break;
                case "100 y":
                case "100P":
                case "100 yard Points":
                case "100 yards":
                    indexes[ConverterConstants.Y100] = i;
                    break;
                case "1,000 y":
                    indexes[ConverterConstants.Y1000] = i;
                    break;
                case "110 y Time (Rank)":
                    indexes[ConverterConstants.Y110] = i;
                    break;
                case "1,100 y Time (Rank)":
                    indexes[ConverterConstants.Y1100] = i;
                    break;
                case "120 yards hurdles":
                    indexes[ConverterConstants.Y120hurdles] = i;
                    break;
                case "1,210 y Time (Rank)":
                    indexes[ConverterConstants.Y1210] = i;
                    break;
                case "1,320 y Time (Rank)":
                    indexes[ConverterConstants.Y1320] = i;
                    break;
                case "1,430 y Time (Rank)":
                    indexes[ConverterConstants.Y1430] = i;
                    break;
                case "1,540 y Time (Rank)":
                    indexes[ConverterConstants.Y1540] = i;
                    break;
                case "1,650 y Time (Rank)":
                    indexes[ConverterConstants.Y1650] = i;
                    break;
                case "200 y":
                    indexes[ConverterConstants.Y200] = i;
                    break;
                case "220 y Time (Rank)":
                    indexes[ConverterConstants.Y220] = i;
                    break;
                case "30 y":
                    indexes[ConverterConstants.Y30] = i;
                    break;
                case "330 y Time (Rank)":
                    indexes[ConverterConstants.Y330] = i;
                    break;
                case "40 y":
                    indexes[ConverterConstants.Y40] = i;
                    break;
                case "440 y Time (Rank)":
                    indexes[ConverterConstants.Y440] = i;
                    break;
                case "50 y":
                case "50 yard Points":
                    indexes[ConverterConstants.Y50] = i;
                    break;
                case "500 y":
                    indexes[ConverterConstants.Y500] = i;
                    break;
                case "550 y Time (Rank)":
                    indexes[ConverterConstants.Y550] = i;
                    break;
                case "60 y":
                    indexes[ConverterConstants.Y60] = i;
                    break;
                case "600 y":
                    indexes[ConverterConstants.Y600] = i;
                    break;
                case "660 y Time (Rank)":
                    indexes[ConverterConstants.Y660] = i;
                    break;
                case "770 y Time (Rank)":
                    indexes[ConverterConstants.Y770] = i;
                    break;
                case "80 y":
                    indexes[ConverterConstants.Y80] = i;
                    break;
                case "800 y":
                    indexes[ConverterConstants.Y800] = i;
                    break;
                case "880 y Time (Rank)":
                    indexes[ConverterConstants.Y880] = i;
                    break;
                case "880 yards Walk":
                    indexes[ConverterConstants.Y880Walk] = i;
                    break;
                case "900 y":
                case "990 y Time (Rank)":
                    indexes[ConverterConstants.Y900] = i;
                    break;
                case "Yellow Cards":
                case "Yellow":
                    indexes[ConverterConstants.YellowCard] = i;
                    break;
                case "Yellow Card Warnings":
                    indexes[ConverterConstants.YellowCardWarnings] = i;
                    break;
                case "Yuko":
                    indexes[ConverterConstants.Yuko] = i;
                    break;

            }
        }

        return indexes;
    }

    public Dictionary<string, int> FindIndexes(List<string> headers)
    {
        var indexes = new Dictionary<string, int>();

        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i].ToLower();
            switch (header)
            {
                case "archer":
                case "athlete":
                case "biathlete":
                case "boarder":
                case "boat":
                case "bobsleigh":
                case "boxer":
                case "climber":
                case "competitor":
                case "competitor (seed)":
                case "competitor(s)":
                case "competitors":
                case "cyclist":
                case "cyclists":
                case "diver":
                case "divers":
                case "fencer":
                case "fencers":
                case "fighter":
                case "gymnast":
                case "gymnasts":
                case "judoka":
                case "jumper":
                case "karateka":
                case "lifter":
                case "pair":
                case "pair (seed)":
                case "pentathlete":
                case "player":
                case "rider":
                case "shooter":
                case "skater":
                case "skier":
                case "slider":
                case "surfer":
                case "swimmer":
                case "team":
                case "triathlete":
                case "walker":
                case "wrestler":
                    indexes[ConverterConstants.INDEX_NAME] = i;
                    break;
            }
        }

        return indexes;
    }
}