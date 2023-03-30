namespace SportData.WebAPI;

using System.ComponentModel.DataAnnotations;

using SportData.Web.Infrastructure.Attributes;

using Swashbuckle.AspNetCore.Annotations;

public class WeatherForecast
{
    [SwaggerSchema(Description = "Date and time for weather")]
    [SwaggerSchemaExample("2021-10-11")]
    public DateOnly Date { get; set; }

    [SwaggerSchema(Description = "asdasd", Required = new[] { "TemperatureC" })]
    [SwaggerSchemaExample("37")]
    //[Required]
    [MinLength(2)]
    [Range(10, 50)]
    public int TemperatureC { get; set; }

    [SwaggerSchema(Description = "asdddddddddddddddddddddddd")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [SwaggerSchema(Description = "asdasda")]
    public string Summary { get; set; }
}
