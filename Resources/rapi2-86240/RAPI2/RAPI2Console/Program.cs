using System;
using System.Devices;
using System.Text.RegularExpressions;

namespace RAPI2Console
{
	internal class Program
	{
		// Methods
		private static void Main(string[] args)
		{
			RemoteDeviceManager manager = new RemoteDeviceManager();
			RemoteDevice firstConnectedDevice = manager.Devices.FirstConnectedDevice;
			if (firstConnectedDevice != null)
			{
				Console.WriteLine(firstConnectedDevice.Name + ":" + firstConnectedDevice.Platform);
				Console.WriteLine(firstConnectedDevice.OSVersion);
				string input = string.Empty;
				do
				{
					Console.Write("> ");
					input = Console.ReadLine();
					MatchCollection matchs = Regex.Matches(input, "(\\w+)\\s*(?:([\\w\\\\\\.\\*\\?]+|\\\"[^\"]+\\\")\\s*)*", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (matchs.Count > 0)
					{
						string str3 = matchs[1].Groups[1].Value.ToLower();
						if (str3 != null)
						{
							if (!(str3 == "start"))
							{
								if (str3 == "exit")
								{
								}
							}
							else if (matchs.Count != 1)
							{
								string commandLine = null;
								if (matchs[2].Groups.Count > 2)
								{
									commandLine = matchs[2].Groups[2].Value;
								}
								firstConnectedDevice.CreateProcess(matchs[2].Groups[1].Value, commandLine, ProcessCreationFlags.None);
							}
						}
					}
				}
				while (input.Trim().ToLower() != "exit");
			}
		}
	}
}
