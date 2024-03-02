using System;
using BepInEx;
using HarmonyLib;
using PingDisplay.Patches;
using UnityEngine;

namespace PingDisplay
{
	[BepInPlugin("DeathWrench.PingDisplay", "PingDisplay", "1.3.0")]
	public class Plugin : BaseUnityPlugin
	{
		public void Awake()
		{
			if (Plugin.Instance == null)
			{
				Plugin.Instance = this;
			}
			ConfigSettings.Init();
			if (ConfigSettings.PingEnabled.Value)
			{
				this._harmony.PatchAll(typeof(HudManagerPatch));
			}
			this.InitPingManager();
			base.Logger.LogInfo("DeathWrench.PingDisplay loaded!");
		}
		private void InitPingManager()
		{
			GameObject gameObject = new GameObject("PingManager");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			gameObject.AddComponent<PingManager>();
			Plugin.PingManager = gameObject.GetComponent<PingManager>();
		}
		public static void Log(string message)
		{
			Plugin.Instance.Logger.LogInfo(message);
		}
		private readonly Harmony _harmony = new Harmony("DeathWrench.PingDisplay");
		public static Plugin Instance;
		public static PingManager PingManager;
	}
}
