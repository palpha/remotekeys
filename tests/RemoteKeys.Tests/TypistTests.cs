namespace RemoteKeys.Tests;

public class TypistTests
{
	[Fact]
	public void When_fed_instruction()
	{
		var mockKeyboard = new Mock<IKeyboardSimulator>();
		var sut = new Typist( mockKeyboard.Object );

		sut.Feed( new[] { new KeyPress( VirtualKeyCode.VK_A ) } );

		mockKeyboard.Verify( x => x.KeyPress( VirtualKeyCode.VK_A ), Times.Once() );
	}

	record StrangeInstruction : Instruction;

	[Fact]
	public void When_fed_strange_instruction()
	{
		var sut = new Typist( Mock.Of<IKeyboardSimulator>() );

		sut.Invoking( x => x.Feed( new[] { new StrangeInstruction() } ) )
			.Should().Throw<UnfamiliarInstructionException>()
			.Which.Instruction.Should().BeOfType<StrangeInstruction>();
	}
}