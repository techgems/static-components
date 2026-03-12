using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace TechGems.StaticComponents;

/// <summary>
/// JavascriptConvert allows you to convert C# objects into JavaScript Object Literals instead of JSON, which is useful when adding server-side state to scripts. The use of user generated strings is not recommended, as it can be a way to open the application to security vulnerabilities such as XSS and CSRF.
/// </summary>
public static class JavascriptConvert
{
    /// <summary>
    /// Serialize any CLR object into a JavaScript Object Literal.
    /// </summary>
    /// <param name="value">The object to serialize into a JavaScript Object Literal.</param>
    /// <param name="useSingleQuotesOnStringValues">Set this to true when you want string values to be serialized with single quotes instead. Useful when using this function as part of an HTML attribute.</param>
    /// <returns></returns>
    public static IHtmlContent SerializeObject(object value, bool useSingleQuotesOnStringValues = false)
    {
        using (var stringWriter = new StringWriter())
        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            // We don't want quotes around object names
            if(useSingleQuotesOnStringValues)
            {
                jsonWriter.QuoteChar = '\'';
            }
            
            jsonWriter.QuoteName = false;
            serializer.Serialize(jsonWriter, value);

            return new HtmlString(stringWriter.ToString());
        }
    }
}
