namespace RemoteKeys;

public interface IInputParser
{
	IEnumerable<Instruction> Parse( IEnumerable<string> arguments );
}