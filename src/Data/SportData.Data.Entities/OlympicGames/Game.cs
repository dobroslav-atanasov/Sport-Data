namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Countries;
using SportData.Data.Entities.Enumerations;

[Table("Games", Schema = "og")]
public class Game : BaseEntity<int>, IDeletableEntity, IEquatable<Game>
{
    public Game()
    {
        this.Events = new HashSet<Event>();
    }

    [Required]
    public int Year { get; set; }

    [MaxLength(10)]
    public string Number { get; set; }

    // TODO
    [Required]
    [MaxLength(100)]
    public string City { get; set; }

    // TODO
    [Required]
    public int CountryId { get; set; }
    public virtual Country Country { get; set; }

    [Required]
    public OlympicGameType Type { get; set; }

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

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Game other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Year == other.Year
           && this.Number == other.Number
           && this.City == other.City
           && this.CountryId == other.CountryId
           && this.Type == other.Type
           && this.OpenDate == other.OpenDate
           && this.CloseDate == other.CloseDate
           && this.StartCompetitionDate == other.StartCompetitionDate
           && this.EndCompetitionDate == other.EndCompetitionDate
           && this.ParticipantAthletes == other.ParticipantAthletes
           && this.ParticipantMenAthletes == other.ParticipantMenAthletes
           && this.ParticipantWomenAthletes == other.ParticipantWomenAthletes
           && this.ParticipantNOCs == other.ParticipantNOCs
           && this.MedalDisciplines == other.MedalDisciplines
           && this.MedalEvents == other.MedalEvents
           && this.MedalSports == other.MedalSports
           && this.OpenBy == other.OpenBy
           && this.Torchbearers == other.Torchbearers
           && this.AthleteOathBy == other.AthleteOathBy
           && this.JudgeOathBy == other.JudgeOathBy
           && this.CoachOathBy == other.CoachOathBy
           && this.OlympicFlagBearers == other.OlympicFlagBearers
           && this.Description == other.Description
           && this.BidProcess == other.BidProcess;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Game);
    }

    public override int GetHashCode()
    {
        return $"{this.Year}-{this.Number}-{this.City}-{this.CountryId}-{this.Type}".GetHashCode();
    }
}