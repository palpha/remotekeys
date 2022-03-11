namespace RemoteKeys;

public interface ITypist
{
	void Feed( IEnumerable<Instruction> instructions );
}