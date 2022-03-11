using System.Text.RegularExpressions;

namespace RemoteKeys;

public class PathParser : IPathParser
{
	private Regex Valid { get; } = new(@"^[a-z0-9\+]+$");

	public IEnumerable<string> Parse(string path)
	{
		var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
		return parts.Any(x => Valid.IsMatch(x) == false)
			? Enumerable.Empty<string>()
			: parts;
	}
}