using RemoteKeys;

var server = Server.Compose();
var cts = new CancellationTokenSource();
Console.CancelKeyPress += ( _, _ ) => cts.Cancel();
await server.Run( cts.Token );