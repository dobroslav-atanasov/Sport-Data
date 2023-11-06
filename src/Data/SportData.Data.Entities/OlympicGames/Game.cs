namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using global::SportData.Data.Common.Interfaces;
using global::SportData.Data.Common.Models;
using global::SportData.Data.Entities.OlympicGames.Enumerations;

[Table("Games", Schema = "dbo")]
public class Game : BaseDeletableEntity<int>, IUpdatable<Game>
{
    public Game()
    {
        this.Events = new HashSet<Event>();
        this.Hosts = new HashSet<Host>();
    }

    [Required]
    public int Year { get; set; }

    [MaxLength(10)]
    public string Number { get; set; }

    [Required]
    public OlympicGameType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string OfficialName { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? OpenDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? CloseDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? StartCompetitionDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? EndCompetitionDate { get; set; }

    public int ParticipantAthletes { get; set; }

    public int ParticipantMenAthletes { get; set; }

    public int ParticipantWomenAthletes { get; set; }

    public int ParticipantNOCs { get; set; }

    public int MedalEvents { get; set; }

    public int MedalDisciplines { get; set; }

    public int MedalSports { get; set; }

    [MaxLength(500)]
    public string OpenBy { get; set; }

    [MaxLength(5000)]
    public string Torchbearers { get; set; }

    [MaxLength(500)]
    public string AthleteOathBy { get; set; }

    [MaxLength(500)]
    public string JudgeOathBy { get; set; }

    [MaxLength(500)]
    public string CoachOathBy { get; set; }

    [MaxLength(500)]
    public string OlympicFlagBearers { get; set; }

    [MaxLength(50000)]
    public string Description { get; set; }

    [MaxLength(10000)]
    public string BidProcess { get; set; }

    public virtual ICollection<Event> Events { get; set; }

    public virtual ICollection<Host> Hosts { get; set; }

    public bool IsUpdated(Game other)
    {
        var isUpdated = false;

        if (this.Number != other.Number)
        {
            this.Number = other.Number;
            isUpdated = true;
        }

        if (this.OfficialName != other.OfficialName)
        {
            this.OfficialName = other.OfficialName;
            isUpdated = true;
        }

        if (this.OpenDate != other.OpenDate)
        {
            this.OpenDate = other.OpenDate;
            isUpdated = true;
        }

        if (this.CloseDate != other.CloseDate)
        {
            this.CloseDate = other.CloseDate;
            isUpdated = true;
        }

        if (this.StartCompetitionDate != other.StartCompetitionDate)
        {
            this.StartCompetitionDate = other.StartCompetitionDate;
            isUpdated = true;
        }

        if (this.EndCompetitionDate != other.EndCompetitionDate)
        {
            this.EndCompetitionDate = other.EndCompetitionDate;
            isUpdated = true;
        }

        if (this.ParticipantAthletes != other.ParticipantAthletes)
        {
            this.ParticipantAthletes = other.ParticipantAthletes;
            isUpdated = true;
        }

        if (this.ParticipantMenAthletes != other.ParticipantMenAthletes)
        {
            this.ParticipantMenAthletes = other.ParticipantMenAthletes;
            isUpdated = true;
        }

        if (this.ParticipantWomenAthletes != other.ParticipantWomenAthletes)
        {
            this.ParticipantWomenAthletes = other.ParticipantWomenAthletes;
            isUpdated = true;
        }

        if (this.ParticipantNOCs != other.ParticipantNOCs)
        {
            this.ParticipantNOCs = other.ParticipantNOCs;
            isUpdated = true;
        }

        if (this.MedalEvents != other.MedalEvents)
        {
            this.MedalEvents = other.MedalEvents;
            isUpdated = true;
        }

        if (this.MedalDisciplines != other.MedalDisciplines)
        {
            this.MedalDisciplines = other.MedalDisciplines;
            isUpdated = true;
        }

        if (this.MedalSports != other.MedalSports)
        {
            this.MedalSports = other.MedalSports;
            isUpdated = true;
        }

        if (this.OpenBy != other.OpenBy)
        {
            this.OpenBy = other.OpenBy;
            isUpdated = true;
        }

        if (this.Torchbearers != other.Torchbearers)
        {
            this.Torchbearers = other.Torchbearers;
            isUpdated = true;
        }

        if (this.AthleteOathBy != other.AthleteOathBy)
        {
            this.AthleteOathBy = other.AthleteOathBy;
            isUpdated = true;
        }

        if (this.JudgeOathBy != other.JudgeOathBy)
        {
            this.JudgeOathBy = other.JudgeOathBy;
            isUpdated = true;
        }

        if (this.CoachOathBy != other.CoachOathBy)
        {
            this.CoachOathBy = other.CoachOathBy;
            isUpdated = true;
        }

        if (this.OlympicFlagBearers != other.OlympicFlagBearers)
        {
            this.OlympicFlagBearers = other.OlympicFlagBearers;
            isUpdated = true;
        }

        if (this.Description != other.Description)
        {
            this.Description = other.Description;
            isUpdated = true;
        }

        if (this.BidProcess != other.BidProcess)
        {
            this.BidProcess = other.BidProcess;
            isUpdated = true;
        }

        return isUpdated;
    }
}