//using System.Net.NetworkInformation;
//using System.Reflection.PortableExecutable;

//var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

//foreach (var networkInterface in networkInterfaces)
//{
//	if (networkInterface.OperationalStatus == OperationalStatus.Up)
//	{
//		Console.WriteLine($"Interface: {networkInterface.Name}");
//		foreach (var addr in networkInterface.GetIPProperties().UnicastAddresses)
//		{
//			Console.WriteLine($"  IP Address: {addr.Address}");
//		}
//	}
//}

//using System.DirectoryServices;
//var root = new DirectoryEntry("WinNT:");

//// Enumerate through all computers in the network
//foreach (DirectoryEntry computers in root.Children)
//{
//	foreach (DirectoryEntry computer in computers.Children)
//	{
//		if (computer.Name != "Schema" && computer.SchemaClassName == "Computer")
//		{
//			Console.WriteLine(computer.Name);
//		}
//	}
//}

//using Rssdp;

//using (var deviceLocator = new SsdpDeviceLocator())
//{
//	var foundDevices = await deviceLocator.SearchAsync(); // Can pass search arguments here (device type, uuid). No arguments means all devices.

//	foreach (var foundDevice in foundDevices)
//	{
//		// Device data returned only contains basic device details and location ]
//		// of full device description.
//		Console.WriteLine("Found " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());

//		// Can retrieve the full device description easily though.
//		var fullDevice = await foundDevice.GetDeviceInfo();
//		Console.WriteLine(fullDevice.FriendlyName);
//		Console.WriteLine();
//	}
//}


using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		int listenPort = 9876;
		var listeningTask = ListenForBroadcasts(listenPort);
		await BroadcastPresence(listenPort);
		await listeningTask;
	}

	static async Task BroadcastPresence(int port)
	{
		using (var client = new UdpClient())
		{
			client.EnableBroadcast = true;
			var endpoint = new IPEndPoint(IPAddress.Broadcast, port);
			while (true)
			{
				var bytes = Encoding.ASCII.GetBytes("Hello, are you there?");
				await client.SendAsync(bytes, bytes.Length, endpoint);
				await Task.Delay(5000); // Broadcast every 5 seconds
			}
		}
	}

	static async Task ListenForBroadcasts(int port)
	{
		var devices = new HashSet<string>();
		using (var listener = new UdpClient(port))
		{
			while (true)
			{
				var result = await listener.ReceiveAsync();
				var message = Encoding.ASCII.GetString(result.Buffer);
				var senderIp = result.RemoteEndPoint.Address.ToString();
				if (!devices.Contains(senderIp))
				{
					devices.Add(senderIp);
					Console.WriteLine($"Device found: {senderIp}");
				}
			}
		}
	}
}
