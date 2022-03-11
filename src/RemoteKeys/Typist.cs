using InputSimulatorStandard;

namespace RemoteKeys;

public class Typist
{
	private IKeyboardSimulator Keyboard { get; }

	public Typist( IKeyboardSimulator keyboard ) =>
		Keyboard = keyboard;

	public void Feed( IEnumerable<Instruction> instructions )
	{
		foreach ( var instruction in instructions )
		{
			_ = instruction switch
				{
					Delay x => Keyboard.Sleep( x.Milliseconds ),
					KeyPress x => Keyboard.KeyPress( x.Key ),
					ModifiedKeyPress x => Keyboard.ModifiedKeyStroke( x.Modifiers, x.Key ),
					TextEntry x => Keyboard.TextEntry( x.Text ),
					_ => throw new UnfamiliarInstructionException( instruction )
				};
		}
	}
}