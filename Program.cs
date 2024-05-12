using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

var listenPort = 9876;
var listeningTask = ListenForBroadcasts(listenPort);
await BroadcastPresence(listenPort);
await listeningTask;

async Task BroadcastPresence(int port)
{
	using var client = new UdpClient();
	client.EnableBroadcast = true;
	var endpoint = new IPEndPoint(IPAddress.Broadcast, port);
	while (true)
	{
		var bytes = Encoding.ASCII.GetBytes("Hello, are you there?");
		await client.SendAsync(bytes, bytes.Length, endpoint);
		await Task.Delay(5000);
	}
}

async Task ListenForBroadcasts(int port)
{
	var devices = new HashSet<string>();
	using var listener = new UdpClient(port);

	var localAddresses = NetworkInterface.GetAllNetworkInterfaces()
		.Where(ni => ni.OperationalStatus == OperationalStatus.Up)
		.Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
		.SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
		.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork) // IP v4
		.Select(a => a.Address)
		.Where(a => a is not null)
		.Select(a => a.ToString())
		.ToHashSet();

	while (true)
	{
		var result = await listener.ReceiveAsync();
		var message = Encoding.ASCII.GetString(result.Buffer);
		var senderIp = result.RemoteEndPoint.Address.ToString();

		if (!devices.Contains(senderIp) && !localAddresses.Contains(senderIp) && message == "Hello, are you there?")
		{
			devices.Add(senderIp);
			Console.WriteLine($"Device found: {senderIp}");
		}
	}
}


