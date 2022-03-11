namespace RemoteKeys;

public interface IPathParser
{
	IEnumerable<string> Parse( string path );
}