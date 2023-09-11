namespace SportData.Services;

using System.Text.RegularExpressions;

using SportData.Data.Entities.Enumerations;
using SportData.Data.Models.Converters;
using SportData.Data.Models.Enumerations;
using SportData.Data.Models.OlympicGames;
using SportData.Data.Models.OlympicGames.ArtisticGymnastics;
using SportData.Data.Models.OlympicGames.ArtisticSwimming;
using SportData.Data.Models.OlympicGames.Athletics;
using SportData.Services.Interfaces;

public class NormalizeService : INormalizeService
{
    public GAREventType MapArtisticGymnasticsEvent(string text)
    {
        text = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Trim();
        var type = GAREventType.None;
        switch (text)
        {
            case "Balance Beam": type = GAREventType.BalanceBeam; break;
            case "Club Swinging": type = GAREventType.ClubSwinging; break;
            case "Combined": type = GAREventType.Combined; break;
            case "Floor Exercise": type = GAREventType.FloorExercise; break;
            case "Horizontal Bar": type = GAREventType.HorizontalBar; break;
            case "Individual Standings":
            case "Individual All-Around": type = GAREventType.Individual; break;
            case "Parallel Bars": type = GAREventType.ParallelBars; break;
            case "Pommelled Horse":
            case "Pommel Horse": type = GAREventType.PommelHorse; break;
            case "Rings": type = GAREventType.Rings; break;
            case "Rope Climbing": type = GAREventType.RopeClimbing; break;
            case "Side Horse": type = GAREventType.SideHorse; break;
            case "Side Vault": type = GAREventType.SideVault; break;
            case "Team All-Around": type = GAREventType.Team; break;
            case "Team All-Around Free System": type = GAREventType.TeamFreeSystem; break;
            case "Team All-Around Swedish System": type = GAREventType.TeamSwedishSystem; break;
            case "Team Horizontal Bar": type = GAREventType.TeamHorizontalBar; break;
            case "Team Parallel Bars": type = GAREventType.TeamParallelBars; break;
            case "Team Portable Apparatus": type = GAREventType.TeamPortableApparatus; break;
            case "Triathlon": type = GAREventType.Triathlon; break;
            case "Tumbling": type = GAREventType.Tumbling; break;
            case "Uneven Bars": type = GAREventType.UnevenBars; break;
            case "Vault 1":
            case "Vault 2":
            case "Horse Vault":
            case "Vault": type = GAREventType.Vault; break;
        }

        return type;
    }

    public SWAEventType MapArtisticSwimmingEvent(string text)
    {
        text = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Trim();
        var type = SWAEventType.Duet;
        switch (text)
        {
            case "Solo": type = SWAEventType.Solo; break;
            case "Team": type = SWAEventType.Team; break;
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

    public AthleticsEventModel MapAthleticsEvent(string text)
    {
        var model = new AthleticsEventModel
        {
            Gender = this.MapGenderType(text)
        };

        text = text.Replace("Men", string.Empty).Replace("Women", string.Empty).Trim();
        switch (text)
        {
            case "10000m": model.EventType = ATHEventType.M10000; break;
            case "100m": model.EventType = ATHEventType.M100; break;
            case "100m Hurdles": model.EventType = ATHEventType.M100Hurdles; break;
            case "10km Race Walk": model.EventType = ATHEventType.RaceWalk10Km; break;
            case "10miles Race Walk": model.EventType = ATHEventType.RaceWalk10Miles; break;
            case "110m Hurdles": model.EventType = ATHEventType.M110Hurdles; break;
            case "1500m": model.EventType = ATHEventType.M1500; break;
            case "1600m Medley Relay": model.EventType = ATHEventType.Relay1600; break;
            case "200m": model.EventType = ATHEventType.M200; break;
            case "200m Hurdles": model.EventType = ATHEventType.M200Hurdels; break;
            case "20km Race Walk": model.EventType = ATHEventType.RaceWalk20km; break;
            case "2500m Steeplechase": model.EventType = ATHEventType.M2500Steeplechase; break;
            case "2590m Steeplechase": model.EventType = ATHEventType.M2590Steeplechase; break;
            case "3000m": model.EventType = ATHEventType.M3000; break;
            case "3000m Race Walk": model.EventType = ATHEventType.RaceWalk3000M; break;
            case "3000m Steeplechase": model.EventType = ATHEventType.M3000Steeplechase; break;
            case "3200m Steeplechase": model.EventType = ATHEventType.M3200Steeplechase; break;
            case "3500m Race Walk": model.EventType = ATHEventType.RaceWalk3500M; break;
            case "4000m Steeplechase": model.EventType = ATHEventType.M4000Steeplechase; break;
            case "400m": model.EventType = ATHEventType.M400; break;
            case "400m Hurdles": model.EventType = ATHEventType.M400Hurdles; break;
            case "4x100m Relay": model.EventType = ATHEventType.Relay4x100; break;
            case "4x400m Relay": model.EventType = ATHEventType.Relay4x400; break;
            case "Mixed 4x400m Relay": model.EventType = ATHEventType.Relay4x400; model.Gender = Gender.Mixed; break;
            case "5000m": model.EventType = ATHEventType.M5000; break;
            case "50km Race Walk": model.EventType = ATHEventType.RaceWalk50km; break;
            case "56-pound Weight Throw": model.EventType = ATHEventType.Pound56WeightThrow; break;
            case "5miles": model.EventType = ATHEventType.Miles5; break;
            case "60m": model.EventType = ATHEventType.M60; break;
            case "800m": model.EventType = ATHEventType.M800; break;
            case "80m Hurdles": model.EventType = ATHEventType.M80Hurdels; break;
            case "All-Around Championship": model.EventType = ATHEventType.AllRound; break;
            case "Decathlon": model.EventType = ATHEventType.Decathlon; break;
            case "Discus Throw": model.EventType = ATHEventType.DiscusThrow; break;
            case "Discus Throw Both Hands": model.EventType = ATHEventType.DiscusThrowBothHands; break;
            case "Discus Throw Greek Style": model.EventType = ATHEventType.DiscusThrowGreekStyle; break;
            case "Hammer Throw": model.EventType = ATHEventType.HammerThrow; break;
            case "Heptathlon": model.EventType = ATHEventType.Heptathlon; break;
            case "High Jump": model.EventType = ATHEventType.HighJump; break;
            case "Individual Cross-Country": model.EventType = ATHEventType.IndividualCrossCountry; break;
            case "Javelin Throw": model.EventType = ATHEventType.JavelinThrow; break;
            case "Javelin Throw Both Hands": model.EventType = ATHEventType.JavelinThrowBothHands; break;
            case "Javelin Throw Freestyle": model.EventType = ATHEventType.JavelinThrowFreestyle; break;
            case "Long Jump": model.EventType = ATHEventType.LongJump; break;
            case "Marathon": model.EventType = ATHEventType.Marathon; break;
            case "Pentathlon": model.EventType = ATHEventType.Pentathlon; break;
            case "Pole Vault": model.EventType = ATHEventType.PoleVault; break;
            case "Shot Put": model.EventType = ATHEventType.ShotPut; break;
            case "Shot Put Both Hands": model.EventType = ATHEventType.ShotPutBothHands; break;
            case "Standing High Jump": model.EventType = ATHEventType.StandingHighJump; break;
            case "Standing Long Jump": model.EventType = ATHEventType.StandingLongJump; break;
            case "Standing Triple Jump": model.EventType = ATHEventType.StandingTripleJump; break;
            case "Team 3000m": model.EventType = ATHEventType.M3000Team; break;
            case "Team 3miles": model.EventType = ATHEventType.Miles3Team; break;
            case "Team 4miles": model.EventType = ATHEventType.Miles4Team; break;
            case "Team 5000m": model.EventType = ATHEventType.M5000Team; break;
            case "Team Cross-Country": model.EventType = ATHEventType.TeamCrossCountry; break;
            case "Triple Jump": model.EventType = ATHEventType.TripleJump; break;
        }

        switch (model.EventType)
        {
            case ATHEventType.None:
            case ATHEventType.M100:
            case ATHEventType.M200:
            case ATHEventType.M400:
            case ATHEventType.M800:
            case ATHEventType.M1500:
            case ATHEventType.M5000:
            case ATHEventType.M10000:
            case ATHEventType.M100Hurdles:
            case ATHEventType.M110Hurdles:
            case ATHEventType.M400Hurdles:
            case ATHEventType.M3000Steeplechase:
            case ATHEventType.Relay4x100:
            case ATHEventType.Relay4x400:
            case ATHEventType.M60:
            case ATHEventType.M3000:
            case ATHEventType.M80Hurdels:
            case ATHEventType.M200Hurdels:
            case ATHEventType.M2500Steeplechase:
            case ATHEventType.M2590Steeplechase:
            case ATHEventType.M3200Steeplechase:
            case ATHEventType.M4000Steeplechase:
            case ATHEventType.Relay1600:
            case ATHEventType.M3000Team:
            case ATHEventType.M5000Team:
            case ATHEventType.Miles3Team:
            case ATHEventType.Miles4Team:
            case ATHEventType.Miles5:
            case ATHEventType.RaceWalk3500M:
            case ATHEventType.RaceWalk10Miles:
            case ATHEventType.RaceWalk10Km:
            case ATHEventType.RaceWalk3000M:
                model.GroupEventType = ATHGroupEventType.TrackEvents;
                break;
            case ATHEventType.Marathon:
            case ATHEventType.RaceWalk20km:
            case ATHEventType.RaceWalk50km:
                model.GroupEventType = ATHGroupEventType.RoadEvents;
                break;
            case ATHEventType.HighJump:
            case ATHEventType.PoleVault:
            case ATHEventType.LongJump:
            case ATHEventType.TripleJump:
            case ATHEventType.ShotPut:
            case ATHEventType.DiscusThrow:
            case ATHEventType.HammerThrow:
            case ATHEventType.JavelinThrow:
            case ATHEventType.StandingHighJump:
            case ATHEventType.StandingLongJump:
            case ATHEventType.StandingTripleJump:
            case ATHEventType.Pound56WeightThrow:
            case ATHEventType.DiscusThrowBothHands:
            case ATHEventType.DiscusThrowGreekStyle:
            case ATHEventType.JavelinThrowBothHands:
            case ATHEventType.JavelinThrowFreestyle:
            case ATHEventType.ShotPutBothHands:
                model.GroupEventType = ATHGroupEventType.FieldEvents;
                break;
            case ATHEventType.Heptathlon:
            case ATHEventType.Decathlon:
            case ATHEventType.AllRound:
            case ATHEventType.Pentathlon:
                model.GroupEventType = ATHGroupEventType.CombinedEvents;
                break;
            case ATHEventType.IndividualCrossCountry:
            case ATHEventType.TeamCrossCountry:
                model.GroupEventType = ATHGroupEventType.CrossCountryEvents;
                break;
        }

        return model;
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

    public Gender MapGenderType(string text)
    {
        if (text.ToLower().StartsWith("men"))
        {
            return Gender.Men;
        }
        else if (text.ToLower().StartsWith("women"))
        {
            return Gender.Women;
        }
        else if (text.ToLower().StartsWith("mixed"))
        {
            return Gender.Mixed;
        }

        return Gender.None;
    }

    public HeatType MapHeats(string text)
    {
        var heat = HeatType.None;
        switch (text.ToLower().Trim())
        {
            case "heat one":
            case "heat #1":
                heat = HeatType.One; break;
            case "heat two":
            case "heat #2":
            case "re-run of heat two":
                heat = HeatType.Two; break;
            case "heat three":
            case "heat #3":
                heat = HeatType.Three; break;
            case "heat four":
            case "heat #4":
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
                roundType = RoundType.FinalRound;
                break;
            case "classification":
            case "classification round 5-8":
            case "classification round 7-12":
            case "classification round 9-12":
            case "classification round 9-16":
            case "classification round 13-15":
            case "classification round 13-16":
            case "classification round 17-20":
            case "classification round 17-23":
            case "classification round 21-23":
                roundType = RoundType.Classification;
                break;
            case "semi-finals":
            case "semi-finals3":
                roundType = RoundType.Semifinals;
                break;
            case "quarter-finals":
            case "quarter-finals 64032":
                roundType = RoundType.Quaterfinals;
                break;
            case "group a":
            case "group b":
            case "group c":
            case "group d":
                roundType = RoundType.Group;
                break;
            case "round one":
            case "round one1":
                roundType = RoundType.RoundOne;
                break;
            case "round one repêchage":
                roundType = RoundType.RoundOneRepechage;
                break;
            case "round two":
                roundType = RoundType.RoundTwo;
                break;
            case "round two repêchage":
                roundType = RoundType.RoundTwoRepechage;
                break;
            case "round three":
                roundType = RoundType.RoundThree;
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
                roundType = RoundType.Repechage;
                break;
            case "preliminary round":
                roundType = RoundType.PreliminaryRound;
                break;
        }

        return roundType;
    }

    //public RoundType MapFinalRoundMatch(string text)
    //{
    //    if (text.Contains("1/2") || text.Contains("1-2"))
    //    {
    //        return RoundType.GoldMedal;
    //    }
    //    else if (text.Contains("3/4") || text.Contains("3-4"))
    //    {
    //        return RoundType.BronzeMedal;
    //    }
    //    else if (text.Contains("5/6") || text.Contains("5-6"))
    //    {
    //        return RoundType.PlaceMatchFive;
    //    }
    //    else if (text.Contains("7/8") || text.Contains("7-8"))
    //    {
    //        return RoundType.PlaceMatchSeven;
    //    }
    //    else
    //    {
    //        return RoundType.FinalRound;
    //    }
    //}

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
            .Replace("Coxed Pairs", "Coxed Pair")
            .Replace("Coxless Fours", "Coxless Four")
            .Replace("Coxless Pairs", "Coxless Pair")
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
}