using InputSimulatorStandard;
using Microsoft.Extensions.Logging;

namespace RemoteKeys;

public class Typist : ITypist
{
	private IKeyboardSimulator Keyboard { get; }
	private ILogger<Typist> Logger { get; }

	public Typist(
		IKeyboardSimulator keyboard,
		ILogger<Typist> logger )
	{
		Keyboard = keyboard;
		Logger = logger;
	}

	public void Feed( IEnumerable<Instruction> instructions )
	{
		foreach ( var instruction in instructions )
		{
			Logger.LogInformation( "Handling {instruction}", instruction );

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