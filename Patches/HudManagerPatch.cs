using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PingDisplay.Patches
{
	[HarmonyPatch(typeof(HUDManager))]
	internal class HudManagerPatch
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		private static void PatchHudManagerStart(ref HUDManager __instance)
		{
			GameObject gameObject = new GameObject("PingManagerDisplay");
			gameObject.AddComponent<RectTransform>();
			TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
			RectTransform rectTransform = textMeshProUGUI.rectTransform;
			rectTransform.SetParent(__instance.debugText.transform.parent.parent.parent, false);
			rectTransform.parent = __instance.debugText.rectTransform.parent.parent.parent;
			rectTransform.anchorMin = new Vector2(1f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(1f, 1f);
			rectTransform.sizeDelta = new Vector2(100f, 100f);
			rectTransform.anchoredPosition = new Vector2(50f, -1f);
			textMeshProUGUI.font = __instance.controlTipLines[0].font;
			textMeshProUGUI.fontSize = 7f;
			textMeshProUGUI.text = string.Format("Ping: {0}ms", Plugin.PingManager.Ping);
			textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
			textMeshProUGUI.enabled = true;
			HudManagerPatch._displayText = textMeshProUGUI;
			Plugin.Log("PingManagerDisplay component added to Canvas.");
		}
		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		private static void PatchHudManagerUpdate(ref HUDManager __instance)
		{
			if (__instance.NetworkManager.IsHost)
			{
				HudManagerPatch._displayText.text = "Ping: Host";
				return;
			}
			HudManagerPatch._displayText.text = string.Format("Ping: {0}ms", Plugin.PingManager.Ping);
		}
		private static TextMeshProUGUI _displayText;
	}
}
