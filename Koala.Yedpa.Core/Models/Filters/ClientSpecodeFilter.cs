namespace Koala.Yedpa.Core.Models.Filters;

public enum SpecodeFilterMode
{
    None,
    Exclude,
    IncludeOnly
}

public class ClientSpecodeFilter
{
    public SpecodeFilterMode Mode { get; set; }
    public List<string> Values { get; set; } = new();

    public string ToSqlWhere(string tableAlias)
    {
        return Mode switch
        {
            SpecodeFilterMode.None => "",
            SpecodeFilterMode.Exclude when Values.Count > 0 =>
                $" AND ISNULL({tableAlias}.SPECODE,'') NOT IN ({string.Join(",", Values.Select(v => $"'{v}'"))})",
            SpecodeFilterMode.IncludeOnly when Values.Count > 0 =>
                $" AND {tableAlias}.SPECODE IN ({string.Join(",", Values.Select(v => $"'{v}'"))})",
            _ => " AND 1=0"
        };
    }

    public bool IsAllowed(string? specode)
    {
        return Mode switch
        {
            SpecodeFilterMode.None => true,
            SpecodeFilterMode.Exclude => !Values.Contains(specode ?? ""),
            SpecodeFilterMode.IncludeOnly => Values.Contains(specode ?? ""),
            _ => false
        };
    }
}
