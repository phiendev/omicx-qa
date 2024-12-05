using System.Text.RegularExpressions;
using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class RegexFilter : Filter<string>
{
    private const string _viString = "ỳọáầảấờễàạằệếýộậốũứĩõúữịỗìềểẩớặòùồợãụủíỹắẫựỉỏừỷởóéửỵẳẹèẽổẵẻỡơôưăêâđ";
    private static readonly string _alphabetString = $"[a-zA-Z{_viString}{_viString.ToUpper()}]";

    public RegexFilter(string field, string value) : base(field, value)
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new RegexpQuery
    {
        Field = FieldIncludePrefix(prefix),
        Value = SplitToRegex
    };

    private string SplitToRegex => string.Join("", SingleValue?.Select(c =>
    {
        var strC = c.ToString();
        return Regex.IsMatch(strC, _alphabetString) ? $"[{strC.ToLower()}{strC.ToUpper()}]" : strC;
    }) ?? Array.Empty<string>());
}