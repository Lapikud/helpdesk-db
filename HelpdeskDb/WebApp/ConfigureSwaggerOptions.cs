using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _descriptionProvider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider descriptionProvider)
    {
        _descriptionProvider = descriptionProvider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _descriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo()
                {
                    Title = $"API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString()
                    // Description, TermsOfServce, Contact, License, ...
                });
        }

        // use FullName for schemaId - avoids conflicts between classes using the same name (which are in different namespaces)
        options.CustomSchemaIds(t => t.FullName);


        // include xml comments (enable creation in csproj file)
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        var securityScheme = new OpenApiSecurityScheme()
        {
            Description =
                "JWT Authorization header using the Bearer scheme.\r\n<br>" +
                "Enter 'Bearer'[space] and then your token in the text box below.\r\n<br>" +
                "Example: <b>Bearer eyJhbGciOiJIUzUxMiIsIn...</b>\r\n<br>" +
                "You will get the bearer from the <code>account/login</code> or <code>account/register</code> endpoint.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
        };

        options.AddSecurityDefinition("Bearer", securityScheme);

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    }
}