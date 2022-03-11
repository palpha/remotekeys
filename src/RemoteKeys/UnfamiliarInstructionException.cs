namespace RemoteKeys;

public class UnfamiliarInstructionException : Exception
{
	public UnfamiliarInstructionException( Instruction instruction )
		: base( $"Unfamiliar instruction type {instruction.GetType()}" )
	{
		Instruction = instruction;
	}

	public Instruction Instruction { get; }
}