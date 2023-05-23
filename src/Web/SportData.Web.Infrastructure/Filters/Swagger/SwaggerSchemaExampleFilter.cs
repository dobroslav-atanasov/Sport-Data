namespace SportData.Web.Infrastructure.Filters.Swagger;

using System.Reflection;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using SportData.Web.Infrastructure.Attributes;

using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerSchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo != null)
        {
            var schemaAttribute = context.MemberInfo.GetCustomAttributes<SwaggerSchemaExampleAttribute>().FirstOrDefault();
            if (schemaAttribute != null && schemaAttribute.Example != null)
            {
                schema.Example = new OpenApiString(schemaAttribute.Example);
            }
        }
    }
}