using System;
using BepInEx.Configuration;

namespace PingDisplay
{
	public static class ConfigSettings
	{
		public static ConfigEntry<bool> PingEnabled { get; set; }
		public static void Init()
		{
			ConfigSettings.PingEnabled = Plugin.Instance.Config.Bind<bool>("Ping Management", "PingEnabled", true, "Enable or disable the ping display in the top right corner.");
		}
	}
}
