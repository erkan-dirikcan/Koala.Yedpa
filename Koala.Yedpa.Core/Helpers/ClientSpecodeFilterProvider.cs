using Koala.Yedpa.Core.Models.Filters;

namespace Koala.Yedpa.Core.Helpers;

public static class ClientSpecodeFilterProvider
{
    public const string YedpaWebClientId = "Yedpa-Web";
    public const string YedpaOtoparkClientId = "Yedpa-Otopark";

    public static ClientSpecodeFilter GetFilter(string? clientId)
    {
        return clientId switch
        {
            YedpaWebClientId => new ClientSpecodeFilter
            {
                Mode = SpecodeFilterMode.Exclude,
                Values = new List<string> { "KIRMIZI" }
            },
            YedpaOtoparkClientId => new ClientSpecodeFilter
            {
                Mode = SpecodeFilterMode.IncludeOnly,
                Values = new List<string> { "MAVİ" }
            },
            _ => new ClientSpecodeFilter
            {
                Mode = SpecodeFilterMode.IncludeOnly,
                Values = new List<string>()
            }
        };
    }
}
