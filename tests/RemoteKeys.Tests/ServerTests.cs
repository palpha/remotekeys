using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RemoteKeys.Tests;

public sealed class ServerTests : IDisposable
{
	private static int GetFreePort()
	{
		using var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
		socket.Bind( new IPEndPoint( IPAddress.Loopback, 0 ) );
		return ((IPEndPoint) socket.LocalEndPoint!).Port;
	}

	private static int Port { get; } = GetFreePort();
	private string Prefix { get; } = $"http://*:{Port}/";
	private static ILogger<Server> Logger => NullLogger<Server>.Instance;

	private CancellationTokenSource Cts { get; } = new();
	private Mock<IPathParser> PathParser { get; } = new();
	private Mock<IInputParser> InputParser { get; } = new();
	private Mock<ITypist> Typist { get; } = new();

	private Server Sut { get; }

	public ServerTests() => Sut =
		new(
			PathParser.Object,
			InputParser.Object,
			Typist.Object,
			config: new( Prefix, false ),
			Logger );

	[Fact]
	public void When_configured_with_prefix()
	{
		Sut.Listener.Prefixes.Should().BeEquivalentTo( Prefix );
		Sut.Listener.IsListening.Should().BeFalse();
	}

	[Fact]
	public async Task When_run_is_called()
	{
		var task = Sut.Run( Cts.Token );
		Sut.Listener.IsListening.Should().BeTrue();
		Cts.Cancel();
		await task;
		Sut.Listener.IsListening.Should().BeFalse();
	}

	[Fact]
	public async Task When_called_with_valid_input()
	{
		var task = Sut.Run( Cts.Token );

		await TestValidInput();

		Cts.Cancel();
		await task;
	}

	[Fact]
	public async Task When_called_with_input_filtered_by_path_parser()
	{
		PathParser
			.Setup( x => x.Parse( It.IsAny<string>() ) )
			.Returns( ArraySegment<string>.Empty );
		var task = Sut.Run( Cts.Token );

		await TestValidInput();

		Cts.Cancel();
		await task;
	}

	[Fact]
	public async Task When_called_with_input_filtered_by_input_parser()
	{
		InputParser
			.Setup( x => x.Parse( It.IsAny<IEnumerable<string>>() ) )
			.Returns( ArraySegment<Instruction>.Empty );
		var task = Sut.Run( Cts.Token );

		await TestValidInput();

		Cts.Cancel();
		await task;
	}

	private async Task TestValidInput()
	{
		var client = new HttpClient();
		// ReSharper disable once MethodSupportsCancellation
		var response = await client.SendAsync( new( HttpMethod.Head, $"http://localhost:{Port}/ctrl+s" ) );
		response.EnsureSuccessStatusCode();
		PathParser.Verify( x => x.Parse( It.IsAny<string>() ), Times.Once() );
	}

	public void Dispose() => Sut.Dispose();
}