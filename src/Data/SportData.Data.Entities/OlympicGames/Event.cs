namespace SportData.Data.Entities.OlympicGames;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;
using SportData.Data.Entities.Enumerations;

[Table("Events", Schema = "dbo")]
public class Event : BaseDeletableEntity<int>, IUpdatable<Event>
{
    public Event()
    {
        this.EventVenues = new HashSet<EventVenue>();
        this.Participants = new HashSet<Participant>();
        this.Teams = new HashSet<Team>();
        this.Results = new HashSet<Result>();
    }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(200)]
    public string OriginalName { get; set; }

    [Required]
    [MaxLength(200)]
    public string NormalizedName { get; set; }

    [Required]
    public GenderType Gender { get; set; }

    public int DisciplineId { get; set; }
    public virtual Discipline Discipline { get; set; }

    public int GameId { get; set; }
    public virtual Game Game { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "Date")]
    public DateTime? EndDate { get; set; }

    [Required]
    public bool IsTeamEvent { get; set; } = false;

    [MaxLength(200)]
    public string AdditionalInfo { get; set; }

    public int Athletes { get; set; }

    public int NOCs { get; set; }

    public string Format { get; set; }

    public string Description { get; set; }

    public virtual ICollection<EventVenue> EventVenues { get; set; }

    public virtual ICollection<Participant> Participants { get; set; }

    public virtual ICollection<Team> Teams { get; set; }

    public virtual ICollection<Result> Results { get; set; }

    public bool IsUpdated(Event other)
    {
        var isUpdated = false;

        if (this.Name != other.Name)
        {
            this.Name = other.Name;
            isUpdated = true;
        }

        if (this.NormalizedName != other.NormalizedName)
        {
            this.NormalizedName = other.NormalizedName;
            isUpdated = true;
        }

        if (this.OriginalName != other.OriginalName)
        {
            this.OriginalName = other.OriginalName;
            isUpdated = true;
        }

        if (this.Gender != other.Gender)
        {
            this.Gender = other.Gender;
            isUpdated = true;
        }

        if (this.StartDate != other.StartDate)
        {
            this.StartDate = other.StartDate;
            isUpdated = true;
        }

        if (this.EndDate != other.EndDate)
        {
            this.EndDate = other.EndDate;
            isUpdated = true;
        }

        if (this.IsTeamEvent != other.IsTeamEvent)
        {
            this.IsTeamEvent = other.IsTeamEvent;
            isUpdated = true;
        }

        if (this.AdditionalInfo != other.AdditionalInfo)
        {
            this.AdditionalInfo = other.AdditionalInfo;
            isUpdated = true;
        }

        if (this.Athletes != other.Athletes)
        {
            this.Athletes = other.Athletes;
            isUpdated = true;
        }

        if (this.NOCs != other.NOCs)
        {
            this.NOCs = other.NOCs;
            isUpdated = true;
        }

        if (this.Format != other.Format)
        {
            this.Format = other.Format;
            isUpdated = true;
        }

        if (this.Description != other.Description)
        {
            this.Description = other.Description;
            isUpdated = true;
        }

        return isUpdated;
    }
}