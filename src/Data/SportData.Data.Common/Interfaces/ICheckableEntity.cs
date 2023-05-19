namespace SportData.Data.Common.Interfaces;

public interface ICheckableEntity
{
    DateTime CreatedOn { get; set; }

    DateTime? ModifiedOn { get; set; }
}