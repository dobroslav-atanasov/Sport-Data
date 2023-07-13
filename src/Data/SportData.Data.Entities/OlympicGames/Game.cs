namespace SportData.Data.Entities.OlympicGames;

using SportData.Data.Common.Interfaces;
using SportData.Data.Common.Models;

public class Game : BaseEntity<int>, IDeletableEntity, IEquatable<Game>
{

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public bool Equals(Game other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Game);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}