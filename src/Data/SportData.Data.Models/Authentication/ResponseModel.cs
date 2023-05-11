namespace SportData.Data.Models.Authentication;

using SportData.Data.Models.Entities.Enumerations;

public class ResponseModel
{
    /// <summary>
    /// Status 0 - Success
    /// Status 1 - Error
    /// </summary>
    public ResponseStatus Status { get; set; }

    public string Message { get; set; }
}