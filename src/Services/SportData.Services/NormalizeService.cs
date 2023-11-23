namespace SportData.Services;

using System.Text.RegularExpressions;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Entities.OlympicGames.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.Enumerations;
using SportData.Data.Models.OlympicGames.Athletics;
using SportData.Data.Models.OlympicGames.Gymnastics;
using SportData.Services.Interfaces;

public class NormalizeService : INormalizeService
{
    public string CleanEventName(string text)
    {
        var name = string.Empty;
        if (text.StartsWith("Open"))
        {
            name = text.Replace("Open", string.Empty).Trim();
        }
        else
        {
            name = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Replace("Mixed", string.Empty).Trim();
        }

        return name;
    }

    public GYMType MapGymnasticsType(string text)
    {
        text = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Trim();
        var type = GYMType.None;
        switch (text)
        {
            case "Balance Beam": type = GYMType.BalanceBeam; break;
            case "Club Swinging": type = GYMType.ClubSwinging; break;
            case "Combined": type = GYMType.Combined; break;
            case "Floor Exercise": type = GYMType.FloorExercise; break;
            case "Horizontal Bar": type = GYMType.HorizontalBar; break;
            case "Individual Standings":
            case "Individual All-Around": type = GYMType.Individual; break;
            case "Parallel Bars": type = GYMType.ParallelBars; break;
            case "Pommelled Horse":
            case "Pommel Horse": type = GYMType.PommelHorse; break;
            case "Rings": type = GYMType.Rings; break;
            case "Rope Climbing": type = GYMType.RopeClimbing; break;
            case "Side Horse": type = GYMType.SideHorse; break;
            case "Side Vault": type = GYMType.SideVault; break;
            case "Team All-Around": type = GYMType.Team; break;
            case "Team All-Around Free System": type = GYMType.TeamFreeSystem; break;
            case "Team All-Around Swedish System": type = GYMType.TeamSwedishSystem; break;
            case "Team Horizontal Bar": type = GYMType.TeamHorizontalBar; break;
            case "Team Parallel Bars": type = GYMType.TeamParallelBars; break;
            case "Team Portable Apparatus": type = GYMType.TeamPortableApparatus; break;
            case "Triathlon": type = GYMType.Triathlon; break;
            case "Tumbling": type = GYMType.Tumbling; break;
            case "Uneven Bars": type = GYMType.UnevenBars; break;
            case "Vault 1":
            case "Vault 2":
            case "Horse Vault":
            case "Vault": type = GYMType.Vault; break;
        }

        return type;
    }

    public AthleteType MapAthleteType(string text)
    {
        text = text.Replace(" •", ",");
        var type = AthleteType.None;
        switch (text)
        {
            case "Competed in Intercalated Games":
            case "Competed in Intercalated Games, Non-starter":
            case "Competed in Olympic Games":
            case "Competed in Olympic Games (non-medal events)":
            case "Competed in Olympic Games (non-medal events), Competed in Intercalated Games":
            case "Competed in Olympic Games (non-medal events), Non-starter":
            case "Competed in Olympic Games, Competed in Intercalated Games":
            case "Competed in Olympic Games, Competed in Intercalated Games, Non-starter":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events)":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events), Competed in Intercalated Games":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events), Non-starter":
            case "Competed in Olympic Games, Competed in Youth Olympic Games":
            case "Competed in Olympic Games, Competed in Youth Olympic Games (non-medal events)":
            case "Competed in Olympic Games, Competed in Youth Olympic Games, Non-starter":
            case "Competed in Olympic Games, Non-starter":
            case "Competed in Olympic Games, Other":
            case "Competed in Youth Olympic Games":
            case "Competed in Youth Olympic Games, Non-starter":
            case "Non-starter":
            case "Non-starter, Other":
            case "Other":
                type = AthleteType.Athlete;
                break;
            case "Coach":
                type = AthleteType.Coach;
                break;
            case "Referee":
                type = AthleteType.Referee;
                break;
            case "Coach, Other":
            case "Competed in Olympic Games (non-medal events), Coach":
            case "Competed in Olympic Games, Coach":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events), Coach":
            case "Competed in Olympic Games, Non-starter, Coach":
            case "Non-starter, Coach":
                type = AthleteType.Athlete | AthleteType.Coach;
                break;
            case "Coach, Referee":
                type = AthleteType.Coach | AthleteType.Referee;
                break;
            case "Competed in Olympic Games, Competed in Intercalated Games, Non-starter, Referee":
            case "Competed in Olympic Games, Competed in Intercalated Games, Referee":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events), Competed in Intercalated Games, Referee":
            case "Competed in Olympic Games, Competed in Olympic Games (non-medal events), Referee":
            case "Competed in Olympic Games, Non-starter, Referee":
            case "Competed in Olympic Games, Referee":
            case "Non-starter, Referee":
            case "Referee, Other":
                type = AthleteType.Athlete | AthleteType.Referee;
                break;
            case "Competed in Olympic Games, Coach, Referee":
            case "Competed in Olympic Games, Non-starter, Coach, Referee":
            case "Non-starter, Coach, Referee":
                type = AthleteType.Athlete | AthleteType.Coach | AthleteType.Referee;
                break;
            case "Competed in Olympic Games, IOC member":
            case "Competed in Olympic Games, Non-starter, IOC member":
                type = AthleteType.Athlete | AthleteType.IOCMember;
                break;
            case "Competed in Olympic Games, IOC member, Referee":
                type = AthleteType.Athlete | AthleteType.Referee | AthleteType.IOCMember;
                break;
            case "IOC member":
                type = AthleteType.IOCMember;
                break;
            case "IOC member, Coach":
                type = AthleteType.Coach | AthleteType.IOCMember;
                break;
            case "IOC member, Referee":
                type = AthleteType.Referee | AthleteType.IOCMember;
                break;
        }

        return type;
    }

    public string MapAthleticsCombinedEvents(string text)
    {
        var name = text;
        switch (name)
        {
            case "1,500 metres":
                name = "1500m"; break;
            case "100 metres":
                name = "100m"; break;
            case "100 metres Hurdles":
                name = "100m Hurdles"; break;
            case "110 metres Hurdles":
            case "110 metres Hurdles1":
                name = "110m Hurdles"; break;
            case "120 yards hurdles":
                name = "120yards Hurdles"; break;
            case "200 metres":
                name = "200m"; break;
            case "400 metres":
                name = "400m"; break;
            case "56 lb Weight Throw":
                name = "56-pound Weight Throw"; break;
            case "80 metres Hurdles":
                name = "80m Hurdles"; break;
            case "800 metres":
                name = "800m"; break;
            case "880 yards Walk":
                name = "880yards Walk"; break;
        }

        return name;
    }

    public ATHEventGroup MapAthleticsEventGroup(string text)
    {
        text = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Trim();
        var group = ATHEventGroup.None;
        switch (text)
        {
            case "10000m":
            case "100m":
            case "100 metres":
            case "100m Hurdles":
            case "100 metres Hurdles":
            case "10km Race Walk":
            case "10miles Race Walk":
            case "110m Hurdles":
            case "110 metres Hurdles":
            case "110 metres Hurdles1":
            case "1500m":
            case "1,500 metres":
            case "1600m Medley Relay":
            case "200m":
            case "200 metres":
            case "200m Hurdles":
            case "2500m Steeplechase":
            case "2590m Steeplechase":
            case "3000m":
            case "3000m Race Walk":
            case "3000m Steeplechase":
            case "3200m Steeplechase":
            case "3500m Race Walk":
            case "4000m Steeplechase":
            case "400m":
            case "400 metres":
            case "400m Hurdles":
            case "4x100m Relay":
            case "4x400m Relay":
            case "Mixed 4x400m Relay":
            case "5000m":
            case "5miles":
            case "60m":
            case "800m":
            case "800 metres":
            case "80m Hurdles":
            case "80 metres Hurdles":
            case "Team 3000m":
            case "Team 3miles":
            case "Team 4miles":
            case "Team 5000m":
            case "100 yards":
            case "1 mile":
            case "120 yards hurdles":
            case "880 yards Walk":
                group = ATHEventGroup.TrackEvents;
                break;
            case "Discus Throw":
            case "Discus Throw Both Hands":
            case "Discus Throw Greek Style":
            case "Hammer Throw":
            case "High Jump":
            case "Javelin Throw":
            case "Javelin Throw Both Hands":
            case "Javelin Throw Freestyle":
            case "Long Jump":
            case "Pole Vault":
            case "Shot Put":
            case "Shot Put Both Hands":
            case "Standing High Jump":
            case "Standing Long Jump":
            case "Standing Triple Jump":
            case "Triple Jump":
            case "56-pound Weight Throw":
            case "56 lb Weight Throw":
                group = ATHEventGroup.FieldEvents;
                break;
            case "Marathon":
            case "20km Race Walk":
            case "50km Race Walk":
                group = ATHEventGroup.RoadEvents;
                break;
            case "Heptathlon":
            case "Decathlon":
            case "All-Around Championship":
            case "Pentathlon":
                group = ATHEventGroup.CombinedEvents;
                break;
            case "Individual Cross-Country":
            case "Team Cross-Country":
                group = ATHEventGroup.CrossCountryEvents;
                break;
        }

        return group;
    }

    public string MapCityNameAndYearToNOCCode(string cityName, int year)
    {
        var text = $"{cityName} {year}";
        var code = string.Empty;
        switch (text)
        {
            case "Athens 1896": code = "GRE"; break;
            case "Paris 1900": code = "FRA"; break;
            case "St. Louis 1904": code = "USA"; break;
            case "London 1908": code = "GBR"; break;
            case "Stockholm 1912": code = "SWE"; break;
            case "Berlin 1916": code = "GER"; break;
            case "Antwerp 1920": code = "BEL"; break;
            case "Paris 1924": code = "FRA"; break;
            case "Amsterdam 1928": code = "NED"; break;
            case "Los Angeles 1932": code = "USA"; break;
            case "Berlin 1936": code = "GER"; break;
            case "Helsinki 1940": code = "FIN"; break;
            case "London 1944": code = "GBR"; break;
            case "London 1948": code = "GBR"; break;
            case "Helsinki 1952": code = "FIN"; break;
            case "Melbourne 1956": code = "AUS"; break;
            case "Rome 1960": code = "ITA"; break;
            case "Tokyo 1964": code = "JPN"; break;
            case "Mexico City 1968": code = "MEX"; break;
            case "Munich 1972": code = "FRG"; break;
            case "Montreal 1976": code = "CAN"; break;
            case "Moscow 1980": code = "URS"; break;
            case "Los Angeles 1984": code = "USA"; break;
            case "Seoul 1988": code = "KOR"; break;
            case "Barcelona 1992": code = "ESP"; break;
            case "Atlanta 1996": code = "USA"; break;
            case "Sydney 2000": code = "AUS"; break;
            case "Athens 2004": code = "GRE"; break;
            case "Beijing 2008": code = "CHN"; break;
            case "London 2012": code = "GBR"; break;
            case "Rio de Janeiro 2016": code = "BRA"; break;
            case "Tokyo 2020": code = "JPN"; break;
            case "Paris 2024": code = "FRA"; break;
            case "Los Angeles 2028": code = "USA"; break;
            case "Brisbane 2032": code = "AUS"; break;
            case "Chamonix 1924": code = "FRA"; break;
            case "St. Moritz 1928": code = "SUI"; break;
            case "Lake Placid 1932": code = "USA"; break;
            case "Garmisch-Partenkirchen 1936": code = "GER"; break;
            case "Garmisch-Partenkirchen 1940": code = "GER"; break;
            case "Cortina d'Ampezzo 1944": code = "ITA"; break;
            case "St. Moritz 1948": code = "SUI"; break;
            case "Oslo 1952": code = "NOR"; break;
            case "Cortina d'Ampezzo 1956": code = "ITA"; break;
            case "Squaw Valley 1960": code = "USA"; break;
            case "Innsbruck 1964": code = "AUT"; break;
            case "Grenoble 1968": code = "FRA"; break;
            case "Sapporo 1972": code = "JPN"; break;
            case "Innsbruck 1976": code = "AUT"; break;
            case "Lake Placid 1980": code = "USA"; break;
            case "Sarajevo 1984": code = "YUG"; break;
            case "Calgary 1988": code = "CAN"; break;
            case "Albertville 1992": code = "FRA"; break;
            case "Lillehammer 1994": code = "NOR"; break;
            case "Nagano 1998": code = "JPN"; break;
            case "Salt Lake City 2002": code = "USA"; break;
            case "Turin 2006": code = "ITA"; break;
            case "Vancouver 2010": code = "CAN"; break;
            case "Sochi 2014": code = "RUS"; break;
            case "PyeongChang 2018": code = "KOR"; break;
            case "Beijing 2022": code = "CHN"; break;
            case "Milano-Cortina d'Ampezzo 2026": code = "ITA"; break;
            case "Stockholm 1956": code = "SWE"; break;
        }

        return code;
    }

    public GenderType MapGenderType(string text)
    {
        if (text.ToLower().StartsWith("men"))
        {
            return GenderType.Men;
        }
        else if (text.ToLower().StartsWith("women"))
        {
            return GenderType.Women;
        }
        else if (text.ToLower().StartsWith("mixed") || text.ToLower().StartsWith("open"))
        {
            return GenderType.Mixed;
        }

        return GenderType.None;
    }

    public GroupType MapGroupType(string text)
    {
        var group = GroupType.None;
        if (string.IsNullOrEmpty(text))
        {
            return group;
        }

        switch (text.ToLower().Trim())
        {
            case "group a":
            case "group a1":
            case "group one":
            case "pool one":
            case "round one pool one":
                group = GroupType.A;
                break;
            case "group b":
            case "group b1":
            case "group two":
            case "pool two":
            case "round one pool two":
                group = GroupType.B;
                break;
            case "group c":
            case "pool three":
            case "round one pool three":
                group = GroupType.C;
                break;
            case "group d":
            case "pool four":
            case "round one pool four":
                group = GroupType.D;
                break;
            case "group e":
            case "pool five":
                group = GroupType.E;
                break;
            case "group f":
                group = GroupType.F;
                break;
            case "group g":
                group = GroupType.G;
                break;
            case "group h":
                group = GroupType.H;
                break;
            case "group i":
                group = GroupType.I;
                break;
            case "group j":
                group = GroupType.J;
                break;
            case "group k":
                group = GroupType.K;
                break;
            case "group l":
                group = GroupType.L;
                break;
            case "group m":
                group = GroupType.M;
                break;
            case "group n":
                group = GroupType.N;
                break;
            case "group o":
                group = GroupType.O;
                break;
            case "group p":
                group = GroupType.P;
                break;
        }

        return group;
    }

    public HeatType MapHeats(string text)
    {
        var heat = HeatType.None;
        if (string.IsNullOrEmpty(text))
        {
            return heat;
        }

        switch (text.ToLower().Trim())
        {
            case "heat one":
            case "heat #1":
            case "final a":
            case "match 1/2":
            case "final heat one":
            case "match 1-6":
            case "heat 1/2":
            case "heat 1-6":
                heat = HeatType.One; break;
            case "heat two":
            case "heat #2":
            case "re-run of heat two":
            case "final b":
            case "match 3/4":
            case "heat two re-run":
            case "final heat two":
            case "match 7-10":
            case "heat 3/4":
            case "heat 7-12":
                heat = HeatType.Two; break;
            case "heat three":
            case "heat #3":
            case "match 5-8":
            case "match 5-7":
            case "heat 5-8":
            case "heat 5/6":
                heat = HeatType.Three; break;
            case "heat four":
            case "heat #4":
            case "repêchage final":
            case "match 9-12":
            case "heat 9-12":
            case "heat 7/8":
                heat = HeatType.Four; break;
            case "heat five":
            case "heat #5":
                heat = HeatType.Five; break;
            case "heat six":
            case "heat #6":
            case "heat six re-run":
                heat = HeatType.Six; break;
            case "heat seven":
            case "heat #7":
                heat = HeatType.Seven; break;
            case "heat eight":
            case "heat #8":
                heat = HeatType.Eight; break;
            case "heat nine":
            case "heat #9":
                heat = HeatType.Nine; break;
            case "heat ten":
                heat = HeatType.Ten; break;
            case "heat eleven":
                heat = HeatType.Eleven; break;
            case "heat twelve":
                heat = HeatType.Twelve; break;
            case "heat thirteen":
                heat = HeatType.Thirteen; break;
            case "heat fourteen":
                heat = HeatType.Fourteen; break;
            case "heat fifteen":
                heat = HeatType.Fifteen; break;
            case "heat sixteen":
                heat = HeatType.Sixteen; break;
            case "heat seventeen":
                heat = HeatType.Seventeen; break;
            case "heat eighteen":
                heat = HeatType.Eighteen; break;
        }

        return heat;
    }

    public string MapOlympicGamesCountriesAndWorldCountries(string code)
    {
        return code switch
        {
            "AFG" => "AFG",
            "ALB" => "ALB",
            "ALG" => "DZA",
            "ASA" => "ASM",
            "AND" => "AND",
            "ANG" => "AGO",
            "ANT" => "ATG",
            "ARG" => "ARG",
            "ARM" => "ARM",
            "ARU" => "ABW",
            "AUS" => "AUS",
            "AUT" => "AUT",
            "AZE" => "AZE",
            "BAH" => "BHS",
            "BRN" => "BHR",
            "BAN" => "BGD",
            "BAR" => "BRB",
            "BLR" => "BLR",
            "BEL" => "BEL",
            "BIZ" => "BLZ",
            "BEN" => "BEN",
            "BER" => "BMU",
            "BHU" => "BTN",
            "BOL" => "BOL",
            "BIH" => "BIH",
            "BOT" => "BWA",
            "BRA" => "BRA",
            "IVB" => "VGB",
            "BRU" => "BRN",
            "BUL" => "BGR",
            "BUR" => "BFA",
            "BDI" => "BDI",
            "CAM" => "KHM",
            "CMR" => "CMR",
            "CAN" => "CAN",
            "CPV" => "CPV",
            "CAY" => "CYM",
            "CAF" => "CAF",
            "CHA" => "TCD",
            "CHI" => "CHL",
            "COL" => "COL",
            "COM" => "COM",
            "CGO" => "COG",
            "COK" => "COK",
            "CRC" => "CRI",
            "CIV" => "CIV",
            "CRO" => "HRV",
            "CUB" => "CUB",
            "CYP" => "CYP",
            "CZE" => "CZE",
            "PRK" => "PRK",
            "COD" => "COD",
            "DEN" => "DNK",
            "DJI" => "DJI",
            "DMA" => "DMA",
            "DOM" => "DOM",
            "ECU" => "ECU",
            "EGY" => "EGY",
            "ESA" => "SLV",
            "GEQ" => "GNQ",
            "ERI" => "ERI",
            "EST" => "EST",
            "SWZ" => "SWZ",
            "ETH" => "ETH",
            "FSM" => "FSM",
            "FIJ" => "FJI",
            "FIN" => "FIN",
            "FRA" => "FRA",
            "GAB" => "GAB",
            "GEO" => "GEO",
            "GER" => "DEU",
            "GHA" => "GHA",
            "GBR" => "GBR",
            "GRE" => "GRC",
            "GRN" => "GRD",
            "GUM" => "GUM",
            "GUA" => "GTM",
            "GUI" => "GIN",
            "GBS" => "GNB",
            "GUY" => "GUY",
            "HAI" => "HTI",
            "HON" => "HND",
            "HKG" => "HKG",
            "HUN" => "HUN",
            "ISL" => "ISL",
            "IND" => "IND",
            "INA" => "IDN",
            "IRQ" => "IRQ",
            "IRL" => "IRL",
            "IRI" => "IRN",
            "ISR" => "ISR",
            "ITA" => "ITA",
            "JAM" => "JAM",
            "JPN" => "JPN",
            "JOR" => "JOR",
            "KAZ" => "KAZ",
            "KEN" => "KEN",
            "KSA" => "SAU",
            "KIR" => "KIR",
            "KOS" => "UNK",
            "KUW" => "KWT",
            "KGZ" => "KGZ",
            "LAO" => "LAO",
            "LAT" => "LVA",
            "LBN" => "LBN",
            "LES" => "LSO",
            "LBR" => "LBR",
            "LBA" => "LBY",
            "LIE" => "LIE",
            "LTU" => "LTU",
            "LUX" => "LUX",
            "MAD" => "MDG",
            "MAW" => "MWI",
            "MAS" => "MYS",
            "MDV" => "MDV",
            "MLI" => "MLI",
            "MLT" => "MLT",
            "MHL" => "MHL",
            "MTN" => "MRT",
            "MRI" => "MUS",
            "MEX" => "MEX",
            "MON" => "MCO",
            "MGL" => "MNG",
            "MNE" => "MNE",
            "MAR" => "MAR",
            "MOZ" => "MOZ",
            "MYA" => "MMR",
            "NAM" => "NAM",
            "NRU" => "NRU",
            "NEP" => "NPL",
            "NED" => "NLD",
            "NZL" => "NZL",
            "NCA" => "NIC",
            "NIG" => "NER",
            "NGR" => "NGA",
            "MKD" => "MKD",
            "NOR" => "NOR",
            "OMA" => "OMN",
            "PAK" => "PAK",
            "PLW" => "PLW",
            "PLE" => "PSE",
            "PAN" => "PAN",
            "PNG" => "PNG",
            "PAR" => "PRY",
            "CHN" => "CHN",
            "PER" => "PER",
            "PHI" => "PHL",
            "POL" => "POL",
            "POR" => "PRT",
            "PUR" => "PRI",
            "QAT" => "QAT",
            "KOR" => "KOR",
            "MDA" => "MDA",
            "ROU" => "ROU",
            "RUS" => "RUS",
            "RWA" => "RWA",
            "SKN" => "KNA",
            "LCA" => "LCA",
            "VIN" => "VCT",
            "SAM" => "WSM",
            "SMR" => "SMR",
            "STP" => "STP",
            "SEN" => "SEN",
            "SRB" => "SRB",
            "YUG" => "SRB",
            "SEY" => "SYC",
            "SLE" => "SLE",
            "SGP" => "SGP",
            "SVK" => "SVK",
            "SLO" => "SVN",
            "SOL" => "SLB",
            "SOM" => "SOM",
            "RSA" => "ZAF",
            "SSD" => "SSD",
            "ESP" => "ESP",
            "SRI" => "LKA",
            "SUD" => "SDN",
            "SUR" => "SUR",
            "SWE" => "SWE",
            "SUI" => "CHE",
            "SYR" => "SYR",
            "TJK" => "TJK",
            "THA" => "THA",
            "GAM" => "GMB",
            "TLS" => "TLS",
            "TOG" => "TGO",
            "TGA" => "TON",
            "TTO" => "TTO",
            "TUN" => "TUN",
            "TUR" => "TUR",
            "TKM" => "TKM",
            "TUV" => "TUV",
            "UGA" => "UGA",
            "UKR" => "UKR",
            "UAE" => "ARE",
            "TAN" => "TZA",
            "USA" => "USA",
            "ISV" => "VIR",
            "URU" => "URY",
            "UZB" => "UZB",
            "VAN" => "VUT",
            "VEN" => "VEN",
            "VIE" => "VNM",
            "YEM" => "YEM",
            "ZAM" => "ZMB",
            "ZIM" => "ZWE",
            _ => null
        };
    }

    public RoundType MapRoundType(string text)
    {
        var roundType = RoundType.None;
        switch (text.ToLower().Trim())
        {
            case "round robin":
            case "round-robin":
                roundType = RoundType.RoundRobin;
                break;
            case "final round":
            case "final round2":
                roundType = RoundType.FinalRound;
                break;
            case "classification":
            case "classification round":
            case "classification round 5-8":
            case "classification round 7-12":
            case "classification round 9-12":
            case "classification round 9-16":
            case "classification round 13-15":
            case "classification round 13-16":
            case "classification round 17-20":
            case "classification round 17-23":
            case "classification round 21-23":
            case "classification round one":
            case "classification round two":
            case "classification round three":
            case "classification final 1":
            case "classification final 2":
                roundType = RoundType.Classification;
                break;
            case "semi-finals":
            case "semi-finals3":
                roundType = RoundType.Semifinals;
                break;
            case "quarter finals":
            case "quarter-finals":
            case "quarter-finals 64032":
                roundType = RoundType.Quaterfinals;
                break;
            case "group a":
            case "group b":
            case "group c":
            case "group d":
            case "group e":
            case "group f":
            case "group g":
            case "group h":
            case "group i":
            case "group j":
            case "group k":
            case "group l":
            case "group m":
            case "group n":
            case "group o":
            case "group p":
                roundType = RoundType.Group;
                break;
            case "round one":
            case "round one1":
            case "part #1":
            case "qualifying round one":
                roundType = RoundType.RoundOne;
                break;
            case "round one repêchage":
            case "round one repêchage final":
                roundType = RoundType.RoundOneRepechage;
                break;
            case "round two":
            case "part #2":
            case "qualifying round two":
                roundType = RoundType.RoundTwo;
                break;
            case "round two repêchage":
            case "round two repêchage final":
                roundType = RoundType.RoundTwoRepechage;
                break;
            case "round three":
                roundType = RoundType.RoundThree;
                break;
            case "round three repêchage":
                roundType = RoundType.RoundThreeRepechage;
                break;
            case "ranking round":
                roundType = RoundType.RankingRound;
                break;
            case "final":
            case "original final":
            case "final1":
                roundType = RoundType.Final;
                break;
            case "qualifying":
            case "qualification":
            case "qualifying round":
                roundType = RoundType.Qualification;
                break;
            case "figures":
                roundType = RoundType.Figures;
                break;
            case "repêchage":
            case "repêchage final":
            case "repêchage heats":
            case "quarter-finals repêchage":
            case "semi-finals repêchage":
            case "1/8-final repêchage":
            case "1/8-final repêchage final":
                roundType = RoundType.Repechage;
                break;
            case "preliminary round":
                roundType = RoundType.PreliminaryRound;
                break;
            case "downhill":
            case "downhill1":
            case "run #1":
            case "run #11":
                roundType = RoundType.RunOne;
                break;
            case "slalom":
            case "slalom1":
            case "run #2":
            case "run #21":
                roundType = RoundType.RunTwo;
                break;
            case "run #3":
                roundType = RoundType.RunThree;
                break;
            case "eighth-finals":
                roundType = RoundType.Eightfinals;
                break;
            case "run #4":
                roundType = RoundType.RunFour;
                break;
            case "lucky loser round":
                roundType = RoundType.LuckyLoser;
                break;
            case "seeding round":
                roundType = RoundType.SeedingRound;
                break;
            case "grand prix freestyle":
                roundType = RoundType.GrandPrixFreestyle;
                break;
            case "grand prix special":
                roundType = RoundType.GrandPrixSpecial;
                break;
            case "grand prix":
                roundType = RoundType.GrandPrix;
                break;
            case "jump-off":
            case "jump-off for 1-2":
            case "jump-off for 3-9":
                roundType = RoundType.JumpOff;
                break;
        }

        return roundType;
    }

    public string NormalizeEventName(string name, int gameYear, string disciplineName)
    {
        name = Regex.Replace(name, @"(\d+)\s+(\d+)", me =>
        {
            return $"{me.Groups[1].Value.Trim()}{me.Groups[2].Value.Trim()}";
        });

        name = Regex.Replace(name, @"(\d+),(\d+)", me =>
        {
            return $"{me.Groups[1].Value.Trim()}{me.Groups[2].Value.Trim()}";
        });

        name = name.Replace(" x ", "x")
            .Replace("82½", "82.5")
            .Replace("67½", "67.5")
            .Replace("333⅓", "333 1/3")
            .Replace(" × ", "x")
            .Replace("¼", "1/4")
            .Replace("⅓", "1/3")
            .Replace("½", "1/2")
            .Replace("²", string.Empty)
            .Replace("kilometer", "kilometers")
            .Replace("metres", "meters")
            .Replace("kilometres", "kilometers")
            .Replace("≤", "-")
            .Replace(">", "+");

        name = name.Replace(" / ", "/")
            .Replace(" meters", "m")
            .Replace(" kilometers", "km")
            .Replace(" miles", "miles")
            .Replace(" mile", "mile")
            .Replace(" km", "km")
            .Replace("Pommelled Horse", "Pommel Horse")
            .Replace("Teams", "Team")
            .Replace("Horse Vault", "Vault")
            .Replace("Alpine Combined", "Combined")
            .Replace("Super Combined", "Combined")
            .Replace("Birds", "Bird")
            .Replace("Pole Archery", "Fixed")
            .Replace("Apparatus Work and Field Sports", string.Empty)
            .Replace("Individual All-Around, Apparatus Work", "Triathlon")
            .Replace("Individual All-Around, 4 Events", "Combined")
            .Replace("European System", string.Empty)
            .Replace("Four/Five", "Four")
            .Replace("Canadian Singles", "C-1")
            .Replace("Canadian Doubles", "C-2")
            .Replace("Kayak Singles", "K-1")
            .Replace("Kayak Doubles", "K-2")
            .Replace("Kayak Fours", "K-4")
            .Replace("Kayak Relay", "K-1")
            .Replace("Two-Man Teams With Cesta", "Team")
            .Replace("Eights", "Eight")
            .Replace("Coxed Fours", "Coxed Four")
            .Replace("Coxed Teams", "Coxed Pair")
            .Replace("Coxless Fours", "Coxless Four")
            .Replace("Coxless Teams", "Coxless Pair")
            .Replace("Covered Courts", "Indoor")
            //.Replace("", "")
            //.Replace("", "")
            .Replace("Target Archery", "Moving Bird");

        if (gameYear == 1924 && disciplineName == "Artistic Gymnastics" && name == "Side Horse, Men")
        {
            name = "Pommel Horse, Men";
        }

        return name;
    }

    public string NormalizeHostCityName(string hostCity)
    {
        return hostCity switch
        {
            "Athina" => "Athens",
            "Antwerpen" => "Antwerp",
            "Ciudad de México" => "Mexico City",
            "Moskva" => "Moscow",
            "Sankt Moritz" => "St. Moritz",
            "Roma" => "Rome",
            "München" => "Munich",
            "Montréal" => "Montreal",
            "Torino" => "Turin",
            _ => hostCity
        };
    }

    public string ReplaceNonEnglishLetters(string name)
    {
        name = name.Replace("-", "-")
            .Replace("‐", "-")
            .Replace("–", "-")
            .Replace(",", string.Empty)
            .Replace(".", string.Empty)
            .Replace("'", string.Empty)
            .Replace("’", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("`", string.Empty)
            .Replace("а", "a")
            .Replace("А", "A")
            .Replace("і", "i")
            .Replace("о", "o")
            .Replace("á", "а")
            .Replace("Á", "А")
            .Replace("à", "а")
            .Replace("À", "А")
            .Replace("ă", "а")
            .Replace("ằ", "а")
            .Replace("â", "а")
            .Replace("Â", "А")
            .Replace("ấ", "а")
            .Replace("ầ", "а")
            .Replace("ẩ", "а")
            .Replace("å", "а")
            .Replace("Å", "А")
            .Replace("ä", "а")
            .Replace("Ä", "А")
            .Replace("ã", "а")
            .Replace("ą", "а")
            .Replace("ā", "а")
            .Replace("Ā", "А")
            .Replace("ả", "а")
            .Replace("ạ", "а")
            .Replace("ặ", "а")
            .Replace("ậ", "а")
            .Replace("æ", "ае")
            .Replace("Æ", "Ae")
            .Replace("ć", "c")
            .Replace("Ć", "C")
            .Replace("č", "c")
            .Replace("Č", "C")
            .Replace("ç", "c")
            .Replace("Ç", "C")
            .Replace("ď", "d")
            .Replace("Ď", "D")
            .Replace("đ", "d")
            .Replace("Đ", "D")
            .Replace("ð", "d")
            .Replace("Ð", "D")
            .Replace("é", "e")
            .Replace("É", "E")
            .Replace("è", "e")
            .Replace("È", "E")
            .Replace("ĕ", "e")
            .Replace("ê", "e")
            .Replace("Ê", "E")
            .Replace("ế", "e")
            .Replace("ề", "e")
            .Replace("ễ", "e")
            .Replace("ể", "e")
            .Replace("ě", "e")
            .Replace("ë", "e")
            .Replace("ė", "e")
            .Replace("ę", "e")
            .Replace("ē", "e")
            .Replace("Ē", "E")
            .Replace("ệ", "e")
            .Replace("ə", "e")
            .Replace("Ə", "E")
            .Replace("Ǵ", "G")
            .Replace("ğ", "g")
            .Replace("ģ", "g")
            .Replace("Ģ", "G")
            .Replace("í", "i")
            .Replace("Í", "I")
            .Replace("ì", "i")
            .Replace("î", "i")
            .Replace("ï", "i")
            .Replace("İ", "I")
            .Replace("ī", "i")
            .Replace("ị", "i")
            .Replace("ı", "i")
            .Replace("ķ", "k")
            .Replace("Ķ", "K")
            .Replace("ľ", "l")
            .Replace("Ľ", "L")
            .Replace("ļ", "l")
            .Replace("ł", "l")
            .Replace("Ł", "L")
            .Replace("ń", "n")
            .Replace("ň", "n")
            .Replace("ñ", "n")
            .Replace("ņ", "n")
            .Replace("Ņ", "N")
            .Replace("ó", "o")
            .Replace("Ó", "O")
            .Replace("ò", "o")
            .Replace("ô", "o")
            .Replace("ố", "o")
            .Replace("ồ", "o")
            .Replace("ỗ", "o")
            .Replace("ö", "o")
            .Replace("Ö", "O")
            .Replace("ő", "o")
            .Replace("Ő", "O")
            .Replace("õ", "o")
            .Replace("Õ", "O")
            .Replace("ø", "o")
            .Replace("Ø", "O")
            .Replace("ơ", "o")
            .Replace("ớ", "o")
            .Replace("ờ", "o")
            .Replace("ọ", "o")
            .Replace("œ", "oe")
            .Replace("ř", "r")
            .Replace("Ř", "R")
            .Replace("ś", "s")
            .Replace("Ś", "S")
            .Replace("š", "s")
            .Replace("Š", "S")
            .Replace("ş", "s")
            .Replace("Ş", "S")
            .Replace("ș", "s")
            .Replace("Ș", "S")
            .Replace("ß", "ss")
            .Replace("ť", "t")
            .Replace("Ť", "T")
            .Replace("ţ", "t")
            .Replace("Ţ", "T")
            .Replace("ț", "t")
            .Replace("Ț", "T")
            .Replace("ú", "u")
            .Replace("Ú", "U")
            .Replace("ù", "u")
            .Replace("û", "u")
            .Replace("ů", "u")
            .Replace("ü", "u")
            .Replace("Ü", "U")
            .Replace("ű", "u")
            .Replace("ũ", "u")
            .Replace("ū", "u")
            .Replace("Ū", "U")
            .Replace("ủ", "u")
            .Replace("ư", "u")
            .Replace("ứ", "u")
            .Replace("ữ", "u")
            .Replace("ụ", "u")
            .Replace("ý", "y")
            .Replace("Ý", "Y")
            .Replace("ỳ", "y")
            .Replace("ÿ", "y")
            .Replace("ỹ", "y")
            .Replace("ỷ", "y")
            .Replace("ź", "z")
            .Replace("Ź", "Z")
            .Replace("ž", "z")
            .Replace("Ž", "Z")
            .Replace("ż", "z")
            .Replace("Ż", "Z")
            .Replace("þ", "th")
            .Replace("Þ", "Th")
            .Replace("ϊ", "i");

        return name;
    }

    public RoundModel MapRound(string title)
    {
        switch (title)
        {
            case "1/8-Final Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Eightfinals, Group = GroupNumber.None, Description = null };
            case "1/8-Final Repêchage Final": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Eightfinals, Group = GroupNumber.None, Description = null };
            case "100 yards": return new RoundModel { Name = title, Status = RoundStatus.Yards100, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "2nd-Place Final Round": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = null };
            case "2nd-Place Round One": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "2nd-Place Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "2nd-Place Tournament": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "3rd-Place Final Round": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = null };
            case "3rd-Place Quarter-Finals": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Quarterfinals, Group = GroupNumber.None, Description = null };
            case "3rd-Place Round One": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "3rd-Place Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "3rd-Place Tournament": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Apparatus": return new RoundModel { Name = title, Status = RoundStatus.Apparatus, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "B Final": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = null };
            case "Balance Beam": return new RoundModel { Name = title, Status = RoundStatus.BalanceBeam, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Classification 5-8": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "5-8" };
            case "Classification 9-12": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "9-12" };
            case "Classification Final 1": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Quarterfinals, Group = GroupNumber.None, Description = null };
            case "Classification Final 2": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Quarterfinals, Group = GroupNumber.None, Description = null };
            case "Classification Round": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Classification Round 13-15": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "13-15" };
            case "Classification Round 13-16": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "13-16" };
            case "Classification Round 17-20": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "17-20" };
            case "Classification Round 17-23": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "17-23" };
            case "Classification Round 21-23": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "21-23" };
            case "Classification Round 2-3": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Classification Round 3rd Place": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Classification Round 5-11": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "5-11" };
            case "Classification Round 5-8": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "5-8" };
            case "Classification Round 5-82": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "5-82" };
            case "Classification Round 7-10": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "7-10" };
            case "Classification Round 7-12": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "7-12" };
            case "Classification Round 9-11": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "9-11" };
            case "Classification Round 9-12": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "9-12" };
            case "Classification Round 9-123": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "9-123" };
            case "Classification Round 9-16": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "9-16" };
            case "Classification Round Five": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundFive, Group = GroupNumber.None, Description = null };
            case "Classification Round for 5/6": return new RoundModel { Name = title, Status = RoundStatus.Classification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "for 5/6" };
            case "Classification Round Four": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundFour, Group = GroupNumber.None, Description = null };
            case "Classification Round One": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Classification Round Six": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundSix, Group = GroupNumber.None, Description = null };
            case "Classification Round Three": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.None, Description = null };
            case "Classification Round Two": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Compulsory Dance": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Compulsory Dance 1": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Compulsory Dance 2": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Compulsory Dances": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Compulsory Dances Summary": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Compulsory Figures": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryFigures, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Consolation Round": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Consolation Round - Final": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Final, Group = GroupNumber.None, Description = null };
            case "Consolation Round - Round One": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Consolation Round - Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "Consolation Round: Final": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Final, Group = GroupNumber.None, Description = null };
            case "Consolation Round: Quarter-Finals": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Quarterfinals, Group = GroupNumber.None, Description = null };
            case "Consolation Round: Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "Consolation Tournament": return new RoundModel { Name = title, Status = RoundStatus.ConsolationRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Drill Section": return new RoundModel { Name = title, Status = RoundStatus.DrillSection, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Eighth-Finals": return new RoundModel { Name = title, Status = RoundStatus.Eightfinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Elimination Round": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds, Round Five Repêchage": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Repechage, Group = GroupNumber.None, Description = "Round Five" };
            case "Elimination Rounds, Round Four": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.RoundFour, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds, Round Four Repêchage": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Repechage, Group = GroupNumber.None, Description = "Round Four" };
            case "Elimination Rounds, Round One": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds, Round One Repêchage": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Repechage, Group = GroupNumber.None, Description = "RoundOne" };
            case "Elimination Rounds, Round Three": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds, Round Three Repêchage": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Repechage, Group = GroupNumber.None, Description = "RoundThree" };
            case "Elimination Rounds, Round Two": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Elimination Rounds, Round Two Repêchage": return new RoundModel { Name = title, Status = RoundStatus.EliminationRound, SubStatus = RoundStatus.Repechage, Group = GroupNumber.None, Description = "Round Two" };
            case "Figures": return new RoundModel { Name = title, Status = RoundStatus.CompulsoryFigures, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Final": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Final Pool": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Final Pool, Barrage 1-2": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Gold Medal" };
            case "Final Pool, Barrage 2-3": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Final Pool, Barrage 3-4": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Final Round": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Final Round 1": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Final Round 2": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Final Round 3": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.None, Description = null };
            case "Final Round One": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Final Round Three": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.None, Description = null };
            case "Final Round Two": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Final Round2": return new RoundModel { Name = title, Status = RoundStatus.FinalRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Final, Swim-Off": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Swim Off" };
            case "Final1": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "First Final": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "First" };
            case "Fleet Races": return new RoundModel { Name = title, Status = RoundStatus.FleetRaces, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Floor Exercise": return new RoundModel { Name = title, Status = RoundStatus.FloorExercise, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Free Dance": return new RoundModel { Name = title, Status = RoundStatus.FreeSkating, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Free Skating": return new RoundModel { Name = title, Status = RoundStatus.FreeSkating, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Grand Prix": return new RoundModel { Name = title, Status = RoundStatus.GrandPrix, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Grand Prix Freestyle": return new RoundModel { Name = title, Status = RoundStatus.GrandPrix, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Freestyle" };
            case "Grand Prix Special": return new RoundModel { Name = title, Status = RoundStatus.GrandPrix, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Special" };
            case "Group A": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Final": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Final, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Five": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundFive, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Four": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundFour, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round One": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Seven": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundSeven, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Six": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundSix, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Three": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.A, Description = "Group" };
            case "Group A - Round Two": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.A, Description = "Group" };
            case "Group A1": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group" };
            case "Group B": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Final": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Final, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Five": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundFive, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Four": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundFour, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round One": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Seven": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundSeven, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Six": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundSix, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Three": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.B, Description = "Group" };
            case "Group B - Round Two": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.B, Description = "Group" };
            case "Group B2": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group" };
            case "Group C": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Group" };
            case "Group C3": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Group" };
            case "Group D": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.D, Description = "Group" };
            case "Group E": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.E, Description = "Group" };
            case "Group Exercises": return new RoundModel { Name = title, Status = RoundStatus.GroupExercise, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Group F": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.F, Description = "Group" };
            case "Group G": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.G, Description = "Group" };
            case "Group H": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.H, Description = "Group" };
            case "Group I": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.I, Description = "Group" };
            case "Group J": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.J, Description = "Group" };
            case "Group K": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.K, Description = "Group" };
            case "Group L": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.L, Description = "Group" };
            case "Group M": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.M, Description = "Group" };
            case "Group N": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.N, Description = "Group" };
            case "Group O": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.O, Description = "Group" };
            case "Group One": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group" };
            case "Group P": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.P, Description = "Group" };
            case "Group Two": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group" };
            case "Horizontal Bar": return new RoundModel { Name = title, Status = RoundStatus.HorizontalBar, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Horse Vault": return new RoundModel { Name = title, Status = RoundStatus.HorseVault, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Individual Standings": return new RoundModel { Name = title, Status = RoundStatus.IndividualStandings, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Jump-Off": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Jump-Off for 1-2": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Gold Medal" };
            case "Jump-Off for 3-9": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Long Jump": return new RoundModel { Name = title, Status = RoundStatus.LongJump, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Lucky Loser Round": return new RoundModel { Name = title, Status = RoundStatus.RoundLuckyLoser, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Medal Pool": return new RoundModel { Name = title, Status = RoundStatus.RoundTwo, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Original Final": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Original" };
            case "Original Round One": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Original" };
            case "Original Set Pattern Dance": return new RoundModel { Name = title, Status = RoundStatus.OriginalSetPatternDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Parallel Bars": return new RoundModel { Name = title, Status = RoundStatus.ParallelBars, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Play-Off for Bronze Medal": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Play-Off for Silver Medal": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Play-offs": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Pommelled Horse": return new RoundModel { Name = title, Status = RoundStatus.PommellHorse, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Pool A": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Pool" };
            case "Pool B": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Pool" };
            case "Pool C": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Pool" };
            case "Pool D": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.D, Description = "Pool" };
            case "Pool E": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.E, Description = "Pool" };
            case "Pool F": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.F, Description = "Pool" };
            case "Pool G": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.G, Description = "Pool" };
            case "Pool H": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.H, Description = "Pool" };
            case "Precision Section": return new RoundModel { Name = title, Status = RoundStatus.PrecisionSection, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Preliminary Round": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Qualification": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Qualification Round": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Qualifying": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Qualifying Round": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Qualifying Round 1": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Qualifying Round 2": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Qualifying Round One": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Qualifying Round Two": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Qualifying Round, Group A": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group A" };
            case "Qualifying Round, Group A Re-Jump": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Re Jump" };
            case "Qualifying Round, Group A1": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group A" };
            case "Qualifying Round, Group B": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group B" };
            case "Qualifying Round, Group B1": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group B" };
            case "Qualifying Round, Group C": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Group C" };
            case "Qualifying Round, Group C3": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Group C" };
            case "Qualifying Round, Group D": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.D, Description = "Group D" };
            case "Qualifying Round, Group D4": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.D, Description = "Group D" };
            case "Qualifying Round, Group E": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.E, Description = "Group E" };
            case "Qualifying Round, Group F": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.F, Description = "Group F" };
            case "Qualifying Round, Group One": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Group One" };
            case "Qualifying Round, Group Two": return new RoundModel { Name = title, Status = RoundStatus.Qualification, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Group Two" };
            case "Quarter Finals": return new RoundModel { Name = title, Status = RoundStatus.Quarterfinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Quarter-Finals": return new RoundModel { Name = title, Status = RoundStatus.Quarterfinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Quarter-Finals Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Quarterfinals, Group = GroupNumber.None, Description = null };
            case "Quarter-Finals, 64032": return new RoundModel { Name = title, Status = RoundStatus.Quarterfinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Ranking Round": return new RoundModel { Name = title, Status = RoundStatus.RankingRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Repêchage Final": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Final, Group = GroupNumber.None, Description = null };
            case "Repêchage Heats": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Repêchage Round One": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Repêchage Round Two": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Re-run Final": return new RoundModel { Name = title, Status = RoundStatus.Final, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Re Run" };
            case "Rhythm Dance": return new RoundModel { Name = title, Status = RoundStatus.RhythmDance, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Rings": return new RoundModel { Name = title, Status = RoundStatus.Rings, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Five": return new RoundModel { Name = title, Status = RoundStatus.RoundFive, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Four": return new RoundModel { Name = title, Status = RoundStatus.RoundFour, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Four5": return new RoundModel { Name = title, Status = RoundStatus.RoundFour, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round One": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round One Pool Five": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.E, Description = "Pool" };
            case "Round One Pool Four": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.D, Description = "Pool" };
            case "Round One Pool One": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.A, Description = "Pool" };
            case "Round One Pool Six": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.F, Description = "Pool" };
            case "Round One Pool Three": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.C, Description = "Pool" };
            case "Round One Pool Two": return new RoundModel { Name = title, Status = RoundStatus.PreliminaryRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.B, Description = "Pool" };
            case "Round One Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Round One Repêchage Final": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = "Final" };
            case "Round One Rerace": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Rerace" };
            case "Round One, Heat Ten": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Heat Ten" };
            case "Round One1": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round One9": return new RoundModel { Name = title, Status = RoundStatus.RoundOne, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Robin": return new RoundModel { Name = title, Status = RoundStatus.RoundRobin, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Seven": return new RoundModel { Name = title, Status = RoundStatus.RoundSeven, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Six": return new RoundModel { Name = title, Status = RoundStatus.RoundSix, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Three": return new RoundModel { Name = title, Status = RoundStatus.RoundThree, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Three Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundThree, Group = GroupNumber.None, Description = null };
            case "Round Two": return new RoundModel { Name = title, Status = RoundStatus.RoundTwo, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Round Two Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Round Two Repêchage Final": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = "Final" };
            case "Round-Robin": return new RoundModel { Name = title, Status = RoundStatus.RoundRobin, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Second Place Tournament - Final": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Second Place Tournament - Round One": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundOne, Group = GroupNumber.None, Description = null };
            case "Second Place Tournament - Round Two": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.RoundTwo, Group = GroupNumber.None, Description = null };
            case "Second Place Tournament - Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "Second-Place Tournament": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Second-to-Fifth Place Tournament": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Seeding Round": return new RoundModel { Name = title, Status = RoundStatus.RankingRound, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Semi-Final": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Semi-Final Round": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Semi-Finals": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Semi-Finals A/B": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "A/B" };
            case "Semi-Finals C/D": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "C/D" };
            case "Semi-Finals E/F": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "E/F" };
            case "Semi-Finals Repêchage": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.Semifinals, Group = GroupNumber.None, Description = null };
            case "Semi-Finals3": return new RoundModel { Name = title, Status = RoundStatus.Semifinals, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Shoot-Off": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Shoot-Off 1": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Shoot-Off 2": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Shoot-Off for 1st Place": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Gold Medal" };
            case "Shoot-Off for 2nd Place": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Silver Medal" };
            case "Shoot-Off for 3rd Place": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Short Dance": return new RoundModel { Name = title, Status = RoundStatus.ShortProgram, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Short Program": return new RoundModel { Name = title, Status = RoundStatus.ShortProgram, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Shot Put": return new RoundModel { Name = title, Status = RoundStatus.ShotPut, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Side Horse": return new RoundModel { Name = title, Status = RoundStatus.SideHorse, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Team Drill": return new RoundModel { Name = title, Status = RoundStatus.TeamDrill, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Third-Place Tournament": return new RoundModel { Name = title, Status = RoundStatus.Repechage, SubStatus = RoundStatus.FinalRound, Group = GroupNumber.None, Description = "Bronze Medal" };
            case "Tie-Breaker": return new RoundModel { Name = title, Status = RoundStatus.PlayOff, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            case "Uneven Bars": return new RoundModel { Name = title, Status = RoundStatus.UnevenBars, SubStatus = RoundStatus.Unknown, Group = GroupNumber.None, Description = null };
            default: return null;
        }
    }
}