namespace SportData.Services;

using SportData.Common.Constants;
using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.Enumerations;
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

    public IList<string> FindCountryCodes(string text)
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

    public Dictionary<string, int> FindIndexes(List<string> headers)
    {
        var indexes = new Dictionary<string, int>();

        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i].ToLower();
            switch (header)
            {
                case "pos":
                    indexes[ConverterConstants.INDEX_POSITION] = i;
                    break;
                case "noc":
                    indexes[ConverterConstants.INDEX_NOC] = i;
                    break;
                case "nr":
                case "number":
                    indexes[ConverterConstants.INDEX_NR] = i;
                    break;
                case "seed":
                    indexes[ConverterConstants.INDEX_SEED] = i;
                    break;
                case "group":
                    indexes[ConverterConstants.INDEX_GROUP] = i;
                    break;
                case "lane":
                    indexes[ConverterConstants.INDEX_LANE] = i;
                    break;
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
                case "time":
                case "adjusted time":
                    indexes[ConverterConstants.INDEX_TIME] = i;
                    break;
                case "run 1":
                case "run #1":
                    indexes[ConverterConstants.INDEX_RUN1] = i;
                    break;
                case "run 2":
                case "run #2":
                    indexes[ConverterConstants.INDEX_RUN2] = i;
                    break;
                case "downhill":
                    indexes[ConverterConstants.INDEX_DOWNHILL] = i;
                    break;
                case "slalom":
                    indexes[ConverterConstants.INDEX_SLALOM] = i;
                    break;
                case "points":
                case "total points":
                case "tp":
                case "team points":
                case "final points (raw points)":
                case "tie-breaker":
                case "total judges' points":
                case "original total judges' points":
                    indexes[ConverterConstants.INDEX_POINTS] = i;
                    break;
                case "10s":
                    indexes[ConverterConstants.INDEX_10S] = i;
                    break;
                case "9s":
                    indexes[ConverterConstants.INDEX_9S] = i;
                    break;
                case "xs":
                    indexes[ConverterConstants.INDEX_XS] = i;
                    break;
                case "target":
                    indexes[ConverterConstants.INDEX_TARGET] = i;
                    break;
                case "th":
                case "targets hit":
                    indexes[ConverterConstants.INDEX_TH] = i;
                    break;
                case "golds":
                    indexes[ConverterConstants.INDEX_GOLDS] = i;
                    break;
                case "score":
                    indexes[ConverterConstants.INDEX_SCORE] = i;
                    break;
                case "shoot-off":
                    indexes[ConverterConstants.INDEX_SHOOT_OFF] = i;
                    break;
                case "1st half":
                case "part #1":
                    indexes[ConverterConstants.INDEX_PART_1] = i;
                    break;
                case "2nd half":
                case "part #2":
                    indexes[ConverterConstants.INDEX_PART_2] = i;
                    break;
                case "set points":
                    indexes[ConverterConstants.INDEX_SET_POINTS] = i;
                    break;
                case "ip":
                case "individual points":
                    indexes[ConverterConstants.INDEX_INDIVIDUAL_POINTS] = i;
                    break;
                case "set 1":
                case "set 1 points":
                    indexes[ConverterConstants.INDEX_SET_1] = i;
                    break;
                case "set 2":
                case "set 2 points":
                    indexes[ConverterConstants.INDEX_SET_2] = i;
                    break;
                case "set 3":
                case "set 3 points":
                    indexes[ConverterConstants.INDEX_SET_3] = i;
                    break;
                case "set 4":
                case "set 4 points":
                    indexes[ConverterConstants.INDEX_SET_4] = i;
                    break;
                case "set 5":
                case "set 5 points":
                    indexes[ConverterConstants.INDEX_SET_5] = i;
                    break;
                case "1":
                case "arrow 1":
                    indexes[ConverterConstants.INDEX_ARROW_1] = i;
                    break;
                case "2":
                case "arrow 2":
                    indexes[ConverterConstants.INDEX_ARROW_2] = i;
                    break;
                case "3":
                case "arrow 3":
                    indexes[ConverterConstants.INDEX_ARROW_3] = i;
                    break;
                case "4":
                case "arrow 4":
                    indexes[ConverterConstants.INDEX_ARROW_4] = i;
                    break;
                case "5":
                case "arrow 5":
                    indexes[ConverterConstants.INDEX_ARROW_5] = i;
                    break;
                case "6":
                case "arrow 6":
                    indexes[ConverterConstants.INDEX_ARROW_6] = i;
                    break;
                case "7":
                case "arrow 7":
                    indexes[ConverterConstants.INDEX_ARROW_7] = i;
                    break;
                case "8":
                case "arrow 8":
                    indexes[ConverterConstants.INDEX_ARROW_8] = i;
                    break;
                case "9":
                case "arrow 9":
                    indexes[ConverterConstants.INDEX_ARROW_9] = i;
                    break;
                case "10":
                case "arrow 10":
                    indexes[ConverterConstants.INDEX_ARROW_10] = i;
                    break;
                case "11":
                case "arrow 11":
                    indexes[ConverterConstants.INDEX_ARROW_11] = i;
                    break;
                case "12":
                case "arrow 12":
                    indexes[ConverterConstants.INDEX_ARROW_12] = i;
                    break;
                case "13":
                case "arrow 13":
                    indexes[ConverterConstants.INDEX_ARROW_13] = i;
                    break;
                case "14":
                case "arrow 14":
                    indexes[ConverterConstants.INDEX_ARROW_14] = i;
                    break;
                case "15":
                case "arrow 15":
                    indexes[ConverterConstants.INDEX_ARROW_15] = i;
                    break;
                case "16":
                case "arrow 16":
                    indexes[ConverterConstants.INDEX_ARROW_16] = i;
                    break;
                case "bb":
                case "balance beam":
                    indexes[ConverterConstants.INDEX_BALANCE_BEAM] = i;
                    break;
                case "fe":
                case "floor exercise":
                    indexes[ConverterConstants.INDEX_FLOOR_EXERCISE] = i;
                    break;
                case "hb":
                case "horizontal bar":
                    indexes[ConverterConstants.INDEX_HORIZONTAL_BAR] = i;
                    break;
                case "hv":
                case "horse vault":
                    indexes[ConverterConstants.INDEX_HORSE_VAULT] = i;
                    break;
                case "pb":
                case "parallel bars":
                    indexes[ConverterConstants.INDEX_PARALLEL_BARS] = i;
                    break;
                case "ph":
                case "pommelled horse":
                    indexes[ConverterConstants.INDEX_POMMELLED_HORSE] = i;
                    break;
                case "rings":
                    indexes[ConverterConstants.INDEX_RINGS] = i;
                    break;
                case "ub":
                case "uneven bars":
                    indexes[ConverterConstants.INDEX_UNEVEN_BARS] = i;
                    break;
                case "cep":
                case "cp":
                case "tcp":
                case "1ep":
                case "c1ep":
                    indexes[ConverterConstants.INDEX_COMPULSORY_EXERCISES_POINTS] = i;
                    break;
                case "oep":
                case "op":
                case "top":
                case "2ep":
                case "o1ep":
                    indexes[ConverterConstants.INDEX_OPTIONAL_EXERCISES_POINTS] = i;
                    break;
                case "vault 1":
                case "1jp":
                case "fj#1p":
                case "j#1p":
                case "jump 1":
                case "c2ep":
                    indexes[ConverterConstants.INDEX_VAULT_1] = i;
                    break;
                case "j-o1jp":
                    indexes[ConverterConstants.INDEX_VAULT_OFF_1] = i;
                    break;
                case "vault 2":
                case "2jp":
                case "fj#2p":
                case "j#2p":
                case "jump 2":
                case "o2ep":
                    indexes[ConverterConstants.INDEX_VAULT_2] = i;
                    break;
                case "j-o2jp":
                    indexes[ConverterConstants.INDEX_VAULT_OFF_2] = i;
                    break;
                case "fp":
                    indexes[ConverterConstants.INDEX_FINAL_POINTS] = i;
                    break;
                case "line penalty":
                    indexes[ConverterConstants.INDEX_LINE_PENALTY] = i;
                    break;
                case "other penalty":
                    indexes[ConverterConstants.INDEX_OTHER_PENALTY] = i;
                    break;
                case "penalty":
                    indexes[ConverterConstants.INDEX_PENALTY] = i;
                    break;
                case "time penalty":
                case "penalty time":
                    indexes[ConverterConstants.INDEX_TIME_PENALTY] = i;
                    break;
                case "qp(50%)":
                    indexes[ConverterConstants.INDEX_QUALIFICATION_HALF_POINTS] = i;
                    break;
                //case "1ep":
                //    indexes[ConverterConstants.INDEX_EXERCISE_POINTS_1] = i;
                //    break;
                //case "2ep":
                //    indexes[ConverterConstants.INDEX_EXERCISE_POINTS_2] = i;
                //    break;
                case "1tt":
                    indexes[ConverterConstants.INDEX_TRIAL_TIME_1] = i;
                    break;
                case "2tt":
                    indexes[ConverterConstants.INDEX_TRIAL_TIME_2] = i;
                    break;
                case "3tt":
                    indexes[ConverterConstants.INDEX_TRIAL_TIME_3] = i;
                    break;
                case "j-op":
                    indexes[ConverterConstants.INDEX_VAULT_OFF_POINTS] = i;
                    break;
                case "d score":
                    indexes[ConverterConstants.INDEX_D_SCORE] = i;
                    break;
                case "e score":
                    indexes[ConverterConstants.INDEX_E_SCORE] = i;
                    break;
                case "d()":
                case "distance":
                case "best mark distance":
                    indexes[ConverterConstants.INDEX_DISTANCE] = i;
                    break;
                case "2nd best":
                case "second best mark distance":
                    indexes[ConverterConstants.INDEX_SECOND_DISTANCE] = i;
                    break;
                case "qop":
                    indexes[ConverterConstants.INDEX_QUALIFYING_OPTIONAL_POINTS] = i;
                    break;
                case "height":
                case "best height cleared":
                case "bhc":
                    indexes[ConverterConstants.INDEX_HEIGHT] = i;
                    break;
                case "100p":
                    indexes[ConverterConstants.INDEX_POINTS_100] = i;
                    break;
                case "ap":
                case "apparatus":
                case "pap":
                case "hap":
                    indexes[ConverterConstants.INDEX_APPARATUS_POINTS] = i;
                    break;
                case "atp(50%)":
                    indexes[ConverterConstants.INDEX_ADJUSTED_TEAM_POINS] = i;
                    break;
                case "gep":
                case "group exercise points":
                    indexes[ConverterConstants.INDEX_GROUP_EXERCISES_POINTS] = i;
                    break;
                case "round one points":
                    indexes[ConverterConstants.INDEX_ROUND_ONE_POINTS] = i;
                    break;
                case "round two points":
                    indexes[ConverterConstants.INDEX_ROUND_TWO_POINTS] = i;
                    break;
                case "ljp":
                    indexes[ConverterConstants.INDEX_LONG_JUMP_POINTS] = i;
                    break;
                case "spp":
                    indexes[ConverterConstants.INDEX_SHOT_PUT_POINTS] = i;
                    break;
                case "tdp":
                    indexes[ConverterConstants.INDEX_TEAM_DRILL_POINTS] = i;
                    break;
                case "tpp":
                    indexes[ConverterConstants.INDEX_TEAM_PRECISION_POINTS] = i;
                    break;
                case "30 m":
                case "30 y":
                    indexes[ConverterConstants.INDEX_30_METERS_POINTS] = i;
                    break;
                case "50 m":
                case "50 y":
                    indexes[ConverterConstants.INDEX_50_METERS_POINTS] = i;
                    break;
                case "70 m":
                case "80 y":
                    indexes[ConverterConstants.INDEX_70_METERS_POINTS] = i;
                    break;
                case "90 m":
                case "100 y":
                    indexes[ConverterConstants.INDEX_90_METERS_POINTS] = i;
                    break;
                case "40 y":
                    indexes[ConverterConstants.INDEX_40_YARDS] = i;
                    break;
                case "60 y":
                    indexes[ConverterConstants.INDEX_60_YARDS] = i;
                    break;
                case "musical routine points":
                    indexes[ConverterConstants.INDEX_SWA_MUSICAL_ROUTINE_POINTS] = i;
                    break;
                case "figure points":
                    indexes[ConverterConstants.INDEX_SWA_FIGURE_POINTS] = i;
                    break;
                case "technical routine":
                case "technical routine points":
                    indexes[ConverterConstants.INDEX_SWA_TECHNICAL_POINTS] = i;
                    break;
                case "technical merit":
                case "technical merit points":
                    indexes[ConverterConstants.INDEX_SWA_TECHNICAL_MERIT] = i;
                    break;
                case "artistic impression":
                case "artistic impression points":
                    indexes[ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION] = i;
                    break;
                case "execution":
                case "execution points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION] = i;
                    break;
                case "overall impression":
                case "overall impression points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION] = i;
                    break;
                case "execution judge 1 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_1_POINTS] = i;
                    break;
                case "execution judge 2 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_2_POINTS] = i;
                    break;
                case "execution judge 3 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_3_POINTS] = i;
                    break;
                case "execution judge 4 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_4_POINTS] = i;
                    break;
                case "execution judge 5 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_5_POINTS] = i;
                    break;
                case "execution judge 6 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_6_POINTS] = i;
                    break;
                case "execution judge 7 points":
                    indexes[ConverterConstants.INDEX_SWA_EXECUTION_JUDGE_7_POINTS] = i;
                    break;
                case "overall impression judge 1 points":
                case "artistic impression judge 1 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_1_POINTS] = i;
                    break;
                case "overall impression judge 2 points":
                case "artistic impression judge 2 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_2_POINTS] = i;
                    break;
                case "overall impression judge 3 points":
                case "artistic impression judge 3 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_3_POINTS] = i;
                    break;
                case "overall impression judge 4 points":
                case "artistic impression judge 4 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_4_POINTS] = i;
                    break;
                case "overall impression judge 5 points":
                case "artistic impression judge 5 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_5_POINTS] = i;
                    break;
                case "overall impression judge 6 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_6_POINTS] = i;
                    break;
                case "overall impression judge 7 points":
                    indexes[ConverterConstants.INDEX_SWA_OVERALL_IMPRESSION_JUDGE_7_POINTS] = i;
                    break;
                case "required element penalty":
                case "penalties":
                    indexes[ConverterConstants.INDEX_SWA_PENALTIES] = i;
                    break;
                case "difficulty":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY] = i;
                    break;
                case "routine 1 points":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_1_POINTS] = i;
                    break;
                case "routine 2 points":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_2_POINTS] = i;
                    break;
                case "routine 3 points":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_3_POINTS] = i;
                    break;
                case "routine 4 points":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_4_POINTS] = i;
                    break;
                case "routine 5 points":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_5_POINTS] = i;
                    break;
                case "routine 1 degree of difficulty":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_1_DEGREE_OF_DIFFICULTY] = i;
                    break;
                case "routine 2 degree of difficulty":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_2_DEGREE_OF_DIFFICULTY] = i;
                    break;
                case "routine 3 degree of difficulty":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_3_DEGREE_OF_DIFFICULTY] = i;
                    break;
                case "routine 4 degree of difficulty":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_4_DEGREE_OF_DIFFICULTY] = i;
                    break;
                case "routine 5 degree of difficulty":
                    indexes[ConverterConstants.INDEX_SWA_ROUTINE_5_DEGREE_OF_DIFFICULTY] = i;
                    break;
                case "free routine":
                case "free routine points":
                    indexes[ConverterConstants.INDEX_SWA_FREE_ROUTINE] = i;
                    break;
                case "technical merit execution points":
                    indexes[ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_EXECUTION_POINTS] = i;
                    break;
                case "technical merit synchronization points":
                    indexes[ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_SYNCHRONIZATION_POINTS] = i;
                    break;
                case "technical merit difficulty points":
                    indexes[ConverterConstants.INDEX_SWA_TECHNICAL_MERIT_DIFFICULTY_POINTS] = i;
                    break;
                case "artistic impression choreography points":
                    indexes[ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_CHOREOGRAPHY_POINTS] = i;
                    break;
                case "Artistic impression music interpretation points":
                    indexes[ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_MUSIC_INTERPRETATION_POINTS] = i;
                    break;
                case "artistic impression manner of presentation points":
                    indexes[ConverterConstants.INDEX_SWA_ARTISTIC_IMPRESSION_MANNER_OF_PRESENTATION_POINTS] = i;
                    break;
                case "difficulty judge 1 points":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_1_POINTS] = i;
                    break;
                case "difficulty judge 2 points":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_2_POINTS] = i;
                    break;
                case "difficulty judge 3 points":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_3_POINTS] = i;
                    break;
                case "difficulty judge 4 points":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_4_POINTS] = i;
                    break;
                case "difficulty judge 5 points":
                    indexes[ConverterConstants.INDEX_SWA_DIFFICULTY_JUDGE_5_POINTS] = i;
                    break;
                case "ord":
                    indexes[ConverterConstants.INDEX_ORDER] = i;
                    break;
                case "reaction time":
                    indexes[ConverterConstants.INDEX_REACTION_TIME] = i;
                    break;
                case "tie-breaking time":
                    indexes[ConverterConstants.INDEX_TIE_BREAKING_TIME] = i;
                    break;
                case "time (a)":
                case "time (automatic)":
                case "t(a)":
                    indexes[ConverterConstants.INDEX_TIME_AUTOMATIC] = i;
                    break;
                case "time (h)":
                case "time (hand)":
                case "t(h)":
                    indexes[ConverterConstants.INDEX_TIME_HAND] = i;
                    break;
                case "exchange":
                    indexes[ConverterConstants.INDEX_EXCHANGE_TIME] = i;
                    break;
                case "split (pos)":
                    indexes[ConverterConstants.INDEX_SPLIT_TIME] = i;
                    break;
                case "split rank":
                    indexes[ConverterConstants.INDEX_SPLIT_RANK] = i;
                    break;
                case "result":
                    indexes[ConverterConstants.INDEX_RESULT] = i;
                    break;
                case "bent knee":
                case "bent knee warnings":
                    indexes[ConverterConstants.INDEX_BENT_KNEE] = i;
                    break;
                case "loss of contact":
                case "loss of contact warnings":
                    indexes[ConverterConstants.INDEX_LOST_OF_CONTACT] = i;
                    break;
                case "total warnings":
                case "warnings":
                    indexes[ConverterConstants.INDEX_WARNINGS] = i;
                    break;
                case "half (pos)":
                case "half-marathon":
                    indexes[ConverterConstants.INDEX_HALF_SPLIT] = i;
                    break;
                case "1 km split (1 km rank)":
                    indexes[ConverterConstants.INDEX_KM1_SPLIT] = i;
                    break;
                case "2 km (pos)":
                case "2 km split (2 km rank)":
                    indexes[ConverterConstants.INDEX_KM2_SPLIT] = i;
                    break;
                case "3 km split (3 km rank)":
                    indexes[ConverterConstants.INDEX_KM3_SPLIT] = i;
                    break;
                case "4 km (pos)":
                case "4 km split (4 km rank)":
                    indexes[ConverterConstants.INDEX_KM4_SPLIT] = i;
                    break;
                case "5 km":
                case "5 km (pos)":
                case "5 km split (5 km rank)":
                    indexes[ConverterConstants.INDEX_KM5_SPLIT] = i;
                    break;
                case "6 km (pos)":
                case "6 km split (6 km rank)":
                    indexes[ConverterConstants.INDEX_KM6_SPLIT] = i;
                    break;
                case "7 km split (7 km rank)":
                    indexes[ConverterConstants.INDEX_KM7_SPLIT] = i;
                    break;
                case "8 km (pos)":
                case "8 km split (8 km rank)":
                    indexes[ConverterConstants.INDEX_KM8_SPLIT] = i;
                    break;
                case "9 km split (9 km rank)":
                    indexes[ConverterConstants.INDEX_KM9_SPLIT] = i;
                    break;
                case "10 km":
                case "10 km (pos)":
                case "10 km split (10 km rank)":
                    indexes[ConverterConstants.INDEX_KM10_SPLIT] = i;
                    break;
                case "11 km split (11 km rank)":
                    indexes[ConverterConstants.INDEX_KM11_SPLIT] = i;
                    break;
                case "12 km (pos)":
                    indexes[ConverterConstants.INDEX_KM12_SPLIT] = i;
                    break;
                case "13 km split (13 km rank)":
                    indexes[ConverterConstants.INDEX_KM13_SPLIT] = i;
                    break;
                case "14 km (pos)":
                case "14 km split (14 km rank)":
                    indexes[ConverterConstants.INDEX_KM14_SPLIT] = i;
                    break;
                case "15 km":
                case "15 km (pos)":
                case "15 km split (15 km rank)":
                    indexes[ConverterConstants.INDEX_KM15_SPLIT] = i;
                    break;
                case "16 km (pos)":
                case "16 km split (16 km rank)":
                    indexes[ConverterConstants.INDEX_KM16_SPLIT] = i;
                    break;
                case "17 km split (17 km rank)":
                    indexes[ConverterConstants.INDEX_KM17_SPLIT] = i;
                    break;
                case "18 km (pos)":
                case "18 km split (18 km rank)":
                    indexes[ConverterConstants.INDEX_KM18_SPLIT] = i;
                    break;
                case "19 km split (19 km rank)":
                    indexes[ConverterConstants.INDEX_KM19_SPLIT] = i;
                    break;
                case "20 km":
                case "20 km (pos)":
                    indexes[ConverterConstants.INDEX_KM20_SPLIT] = i;
                    break;
                case "25 km":
                case "25 km (pos)":
                    indexes[ConverterConstants.INDEX_KM25_SPLIT] = i;
                    break;
                case "26 km (pos)":
                    indexes[ConverterConstants.INDEX_KM26_SPLIT] = i;
                    break;
                case "28 km (pos)":
                    indexes[ConverterConstants.INDEX_KM28_SPLIT] = i;
                    break;
                case "30 km":
                case "30 km (pos)":
                    indexes[ConverterConstants.INDEX_KM30_SPLIT] = i;
                    break;
                case "31 km (pos)":
                    indexes[ConverterConstants.INDEX_KM31_SPLIT] = i;
                    break;
                case "35 km":
                case "35 km (pos)":
                    indexes[ConverterConstants.INDEX_KM35_SPLIT] = i;
                    break;
                case "36 km (pos)":
                    indexes[ConverterConstants.INDEX_KM36_SPLIT] = i;
                    break;
                case "37 km (pos)":
                    indexes[ConverterConstants.INDEX_KM37_SPLIT] = i;
                    break;
                case "38 km (pos)":
                    indexes[ConverterConstants.INDEX_KM38_SPLIT] = i;
                    break;
                case "40 km":
                case "40 km (pos)":
                    indexes[ConverterConstants.INDEX_KM40_SPLIT] = i;
                    break;
                case "45 km":
                case "45 km (pos)":
                    indexes[ConverterConstants.INDEX_KM45_SPLIT] = i;
                    break;
                case "46 km (pos)":
                    indexes[ConverterConstants.INDEX_KM46_SPLIT] = i;
                    break;
                case "total attempts":
                case "total attempts thru best height cleared":
                    indexes[ConverterConstants.INDEX_TOTAL_ATTEMPTS] = i;
                    break;
                case "total misses":
                case "total misses thru best height cleared":
                case "total misses at best height cleared":
                    indexes[ConverterConstants.INDEX_TOTAL_MISSES] = i;
                    break;
                case "misses":
                case "misses at best height cleared":
                    indexes[ConverterConstants.INDEX_MISSES] = i;
                    break;
                case "group b":
                case "group a":
                case "r1":
                case "round #1":
                case "group a round #1":
                case "group a round 1":
                case "group a round one":
                case "group b round #1":
                case "group b round 1":
                case "group b round one":
                case "group c round one":
                case "group d round one":
                case "group e round one":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_1] = i;
                    break;
                case "r2":
                case "round #2":
                case "group a round #2":
                case "group a round 2":
                case "group a round three":
                case "group b round #2":
                case "group b round 2":
                case "group b round three":
                case "group c round three":
                case "group d round three":
                case "group e round three":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_2] = i;
                    break;
                case "r3":
                case "round #3":
                case "group a round #3":
                case "group a round 3":
                case "group a round two":
                case "group b round #3":
                case "group b round 3":
                case "group b round two":
                case "group c round two":
                case "group d round two":
                case "group e round two":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_3] = i;
                    break;
                case "r4":
                case "round #4":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_4] = i;
                    break;
                case "r5":
                case "round #5":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_5] = i;
                    break;
                case "r6":
                case "round #6":
                    indexes[ConverterConstants.INDEX_ATH_ROUND_6] = i;
                    break;
                case "st. order":
                    indexes[ConverterConstants.INDEX_ATH_ORDER] = i;
                    break;
                case "service attempts":
                    indexes[ConverterConstants.INDEX_SERVICE_ATTEMPTS] = i;
                    break;
                case "service faults":
                    indexes[ConverterConstants.INDEX_SERVICE_FAULTS] = i;
                    break;
                case "service aces":
                    indexes[ConverterConstants.INDEX_SERVICE_ACES] = i;
                    break;
                case "fastest serve":
                    indexes[ConverterConstants.INDEX_FASTEST_SERVE] = i;
                    break;
                case "attack attempts":
                    indexes[ConverterConstants.INDEX_ATTACK_ATTEMPTS] = i;
                    break;
                case "attack successes":
                    indexes[ConverterConstants.INDEX_ATTACK_SUCCESSES] = i;
                    break;
                case "block successes":
                    indexes[ConverterConstants.INDEX_BLOCK_SUCCESSES] = i;
                    break;
                case "opponent errors":
                    indexes[ConverterConstants.INDEX_OPPONENT_ERRORS] = i;
                    break;
                case "dig successes":
                    indexes[ConverterConstants.INDEX_DIG_SUCCESSES] = i;
                    break;
                case "race":
                    indexes[ConverterConstants.INDEX_RACE] = i;
                    break;
                case "start behind":
                    indexes[ConverterConstants.INDEX_START_BEHIND] = i;
                    break;
                case "skiing":
                    indexes[ConverterConstants.INDEX_SKIING] = i;
                    break;
                case "shooting 1":
                    indexes[ConverterConstants.INDEX_SHOOTING_1] = i;
                    break;
                case "shooting 2":
                    indexes[ConverterConstants.INDEX_SHOOTING_2] = i;
                    break;
                case "shooting 3":
                    indexes[ConverterConstants.INDEX_SHOOTING_3] = i;
                    break;
                case "shooting 4":
                    indexes[ConverterConstants.INDEX_SHOOTING_4] = i;
                    break;
                case "shooting 1 misses":
                    indexes[ConverterConstants.INDEX_SHOOTING_1_MISSES] = i;
                    break;
                case "shooting 2 misses":
                    indexes[ConverterConstants.INDEX_SHOOTING_2_MISSES] = i;
                    break;
                case "shooting 3 misses":
                    indexes[ConverterConstants.INDEX_SHOOTING_3_MISSES] = i;
                    break;
                case "shooting 4 misses":
                    indexes[ConverterConstants.INDEX_SHOOTING_4_MISSES] = i;
                    break;
                case "shooting 1 extra shots":
                    indexes[ConverterConstants.INDEX_SHOOTING_1_EXTRA_SHOTS] = i;
                    break;
                case "shooting 2 extra shots":
                    indexes[ConverterConstants.INDEX_SHOOTING_2_EXTRA_SHOTS] = i;
                    break;
                case "extra shots":
                    indexes[ConverterConstants.INDEX_EXTRA_SHOTS] = i;
                    break;
                case "run 3":
                case "run #3":
                    indexes[ConverterConstants.INDEX_RUN3] = i;
                    break;
                case "run 4":
                case "run #4":
                    indexes[ConverterConstants.INDEX_RUN4] = i;
                    break;
                case "intermediate 1":
                    indexes[ConverterConstants.INDEX_INTERMEDIATE_1] = i;
                    break;
                case "intermediate 2":
                    indexes[ConverterConstants.INDEX_INTERMEDIATE_2] = i;
                    break;
                case "intermediate 3":
                    indexes[ConverterConstants.INDEX_INTERMEDIATE_3] = i;
                    break;
                case "intermediate 4":
                    indexes[ConverterConstants.INDEX_INTERMEDIATE_4] = i;
                    break;
                case "split 1":
                    indexes[ConverterConstants.INDEX_SPLIT_1] = i;
                    break;
                case "split 2":
                    indexes[ConverterConstants.INDEX_SPLIT_2] = i;
                    break;
                case "split 3":
                    indexes[ConverterConstants.INDEX_SPLIT_3] = i;
                    break;
                case "split 4":
                    indexes[ConverterConstants.INDEX_SPLIT_4] = i;
                    break;
                case "split 5":
                    indexes[ConverterConstants.INDEX_SPLIT_5] = i;
                    break;
                case "judges favoring":
                    indexes[ConverterConstants.INDEX_JUDGES_FAVORING] = i;
                    break;
                case "trunks":
                    indexes[ConverterConstants.INDEX_TRUNKS] = i;
                    break;
                case "round 1 score":
                    indexes[ConverterConstants.INDEX_ROUND_1_POINTS] = i;
                    break;
                case "round 2 score":
                    indexes[ConverterConstants.INDEX_ROUND_2_POINTS] = i;
                    break;
                case "round 3 score":
                    indexes[ConverterConstants.INDEX_ROUND_3_POINTS] = i;
                    break;
                case "round 4 score":
                    indexes[ConverterConstants.INDEX_ROUND_4_POINTS] = i;
                    break;
                case "judge #1 score":
                case "original judge #1 score":
                    indexes[ConverterConstants.INDEX_JUDGE_1_POINTS] = i;
                    break;
                case "judge #2 score":
                case "original judge #2 score":
                    indexes[ConverterConstants.INDEX_JUDGE_2_POINTS] = i;
                    break;
                case "judge #3 score":
                case "original judge #3 score":
                    indexes[ConverterConstants.INDEX_JUDGE_3_POINTS] = i;
                    break;
                case "judge #4 score":
                case "original judge #4 score":
                    indexes[ConverterConstants.INDEX_JUDGE_4_POINTS] = i;
                    break;
                case "judge #5 score":
                case "original judge #5 score":
                    indexes[ConverterConstants.INDEX_JUDGE_5_POINTS] = i;
                    break;
            }
        }

        return indexes;
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

    public IList<AthleteModel> GetAthletes(string text)
    {
        var matches = this.regExpService.Matches(text, @"<a href=""\/athletes\/(\d+)"">(.*?)<\/a>\s*(?:<small>\s*\((.*?)\)<\/small>)?");
        var athleteModels = new List<AthleteModel>();

        if (matches != null && matches.Count != 0)
        {

        }

        return athleteModels;
    }

    public bool IsMatchNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return this.regExpService.IsMatch(text, @"(?:Match|Game)\s*([\d\/#]+)");
    }

    public int FindMatchNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var match = this.regExpService.Match(text, @"(?:Match|Game)\s*#(\d+)");
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

    private void SetWinAndLose(MatchResult result)
    {
        if (result.Points1 > result.Points2)
        {
            result.Result1 = ResultType.Win;
            result.Result2 = ResultType.Lose;
        }
        else if (result.Points1 < result.Points2)
        {
            result.Result1 = ResultType.Lose;
            result.Result2 = ResultType.Win;
        }
    }

    public MatchResult GetMatchResult(string text, MatchResultType type)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = text.Replace("[", string.Empty).Replace("]", string.Empty);

        if (type == MatchResultType.Games)
        {
            var result = new MatchResult();
            var match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
            if (match != null)
            {
                result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[5].Value) };
                result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value), int.Parse(match.Groups[6].Value) };

                var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
                points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;
                points = result.Games1[2].Value > result.Games2[2].Value ? result.Points1++ : result.Points2++;

                this.SetWinAndLose(result);
                return result;
            }
            match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)\s*,\s*(\d+)\s*-\s*(\d+)");
            if (match != null)
            {
                result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value) };
                result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value), int.Parse(match.Groups[4].Value) };

                var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;
                points = result.Games1[1].Value > result.Games2[1].Value ? result.Points1++ : result.Points2++;

                this.SetWinAndLose(result);
                return result;
            }
            match = this.regExpService.Match(text, @"(\d+)\s*-\s*(\d+)");
            if (match != null)
            {
                result.Games1 = new List<int?> { int.Parse(match.Groups[1].Value) };
                result.Games2 = new List<int?> { int.Parse(match.Groups[2].Value) };

                var points = result.Games1[0].Value > result.Games2[0].Value ? result.Points1++ : result.Points2++;

                result.Result1 = ResultType.Win;
                result.Result2 = ResultType.Lose;

                return result;
            }

            return result;
        }
        else
        {
            var result = new MatchResult();
            var match = this.regExpService.Match(text, @"(\d+)\s*(?:-|–|—)\s*(\d+)");
            if (match != null)
            {
                result.Points1 = int.Parse(match.Groups[1].Value.Trim());
                result.Points2 = int.Parse(match.Groups[2].Value.Trim());

                this.SetWinAndLose(result);
            }
            match = this.regExpService.Match(text, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*(\d+)\.(\d+)");
            if (match != null)
            {
                result.Time1 = this.dateService.ParseTime($"{match.Groups[1].Value}.{match.Groups[2].Value}");
                result.Time2 = this.dateService.ParseTime($"{match.Groups[3].Value}.{match.Groups[4].Value}");

                if (result.Time1 < result.Time2)
                {
                    result.Result1 = ResultType.Win;
                    result.Result2 = ResultType.Lose;
                }
                else if (result.Time1 > result.Time2)
                {
                    result.Result1 = ResultType.Lose;
                    result.Result2 = ResultType.Win;
                }
            }
            match = this.regExpService.Match(text, @"(\d+)\.(\d+)\s*(?:-|–|—)\s*DNF");
            if (match != null)
            {
                result.Time1 = this.dateService.ParseTime($"{match.Groups[1].Value}.{match.Groups[2].Value}");
                result.Time2 = null;

                result.Result1 = ResultType.Win;
                result.Result2 = ResultType.Lose;
            }

            return result;
        }

        return null;
    }



    public MatchType FindMatchType(RoundType round, string text)
    {
        var matchType = MatchType.None;
        if (round == RoundType.FinalRound)
        {
            if (text.Contains("1/2") || text.Contains("1-2"))
            {
                matchType = MatchType.GoldMedalMatch;
            }
            else if (text.Contains("3/4") || text.Contains("3-4"))
            {
                matchType = MatchType.BronzeMedalMatch;
            }
            else
            {
                matchType = MatchType.ClassificationMatch;
            }
        }

        return matchType;
    }

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
            record = RecordType.WorldRecord;
        }

        match = this.regExpService.Match(text, @"Olympic\s*Record");
        if (match != null)
        {
            record = RecordType.OlympicRecord;
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

    public bool IsAthleteNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return this.regExpService.IsMatch(text, @"<a href=""\/athletes\/(\d+)"">");
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
}