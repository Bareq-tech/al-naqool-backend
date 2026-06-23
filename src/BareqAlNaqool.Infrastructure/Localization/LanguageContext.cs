using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BareqAlNaqool.Infrastructure.Localization;

public class LanguageContext(IHttpContextAccessor httpContextAccessor) : ILanguageContext
{
    public string Language
    {
        get
        {
            var query = httpContextAccessor.HttpContext?.Request.Query["lang"].FirstOrDefault();
            return string.Equals(query, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        }
    }
}
