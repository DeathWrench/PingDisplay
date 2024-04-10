using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;

namespace PingDisplay
{

    [BepInPlugin("DeathWrench.PingDisplay", "PingDisplay", "1.3.2")]
    public class Plugin : BaseUnityPlugin
    {
        public enum DisplayPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        // Define config entries
        public static ConfigEntry<bool> pingEnabledConfig;
        public static ConfigEntry<int> fontSizeConfig;
        public static ConfigEntry<DisplayPosition> displayPositionConfig;
        public static float Margin; // Example value, adjust as needed

        public static TextMeshProUGUI _displayText;
        public static Plugin Instance;
        public static PingManager PingManager;

        private readonly Harmony _harmony = new Harmony("DeathWrench.PingDisplay");

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // Initialize config settings
            SetupConfig(pingEnabledConfig = Instance.Config.Bind("General", "Ping Enabled", true, "Toggle to enable/disable ping display"), value => _displayText.enabled = value);
            SetupConfig(displayPositionConfig = Config.Bind("General", "Display Position", DisplayPosition.TopRight, "Where on the HUD to display your latency"), PositionDisplay);
            SetupConfig(fontSizeConfig = Config.Bind("General", "Font Size", 12, ""), value => _displayText.fontSize = value);

            if (pingEnabledConfig.Value)
            {
                _harmony.PatchAll(typeof(HudManagerPatch));
            }
            InitPingManager();
            base.Logger.LogInfo("Ping Display loaded!");
        }
        static void SetupConfig<T>(ConfigEntry<T> config, Action<T> changedHandler)
        {
            config.SettingChanged += (_, _) => changedHandler(config.Value);
        }
        private void InitPingManager()
        {
            GameObject gameObject = new GameObject("PingManager");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.AddComponent<PingManager>();
            PingManager = gameObject.GetComponent<PingManager>();
        }

        public static void Log(string message)
        {
            Instance.Logger.LogInfo(message);
        }

        static void PositionDisplay(DisplayPosition position)
        {
            if (_displayText == null) return;

            var rect = _displayText.rectTransform;
            var pivot = position switch
            {
                DisplayPosition.TopLeft => new Vector2(0f, 1f),
                DisplayPosition.TopRight => new Vector2(1f, 1f),
                DisplayPosition.BottomLeft => new Vector2(0f, 0f),
                DisplayPosition.BottomRight => new Vector2(1f, 0f),
                _ => throw new ArgumentOutOfRangeException()
            };

            rect.pivot = pivot;
            AddMargins(ref pivot.x);
            AddMargins(ref pivot.y);

            rect.anchorMin = rect.anchorMax = pivot;
            rect.anchoredPosition = Vector2.zero;

            _displayText.alignment = position switch
            {
                DisplayPosition.TopLeft => TextAlignmentOptions.TopLeft,
                DisplayPosition.TopRight => TextAlignmentOptions.TopRight,
                DisplayPosition.BottomLeft => TextAlignmentOptions.BottomLeft,
                DisplayPosition.BottomRight => TextAlignmentOptions.BottomRight,
                _ => throw new ArgumentOutOfRangeException()
            };

            return;
        }

        public static void AddMargins(ref float value)
        {
            if (value == 0) value += Margin;
            else value -= Margin;
        }

        [HarmonyPatch(typeof(HUDManager))]
        internal class HudManagerPatch
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            private static void PatchHudManagerStart(ref HUDManager __instance)
            {
                var textGameObject = new GameObject("PingDisplay", typeof(RectTransform));
                textGameObject.transform.SetParent(__instance.HUDContainer.transform, false);

                _displayText = textGameObject.AddComponent<TextMeshProUGUI>();
                _displayText.faceColor = new Color32(255, 255, 255, 40);
                _displayText.font = __instance.weightCounter.font;
                _displayText.fontSize = fontSizeConfig.Value;

                PositionDisplay(displayPositionConfig.Value);
                _displayText.enabled = pingEnabledConfig.Value;

                var canvasGroup = textGameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;

                var hudElement = new HUDElement
                {
                    canvasGroup = canvasGroup,
                    targetAlpha = 1f
                };

                var hudElements = (HUDElement[])AccessTools.Field(typeof(HUDManager), "HUDElements").GetValue(__instance);
                Array.Resize(ref hudElements, hudElements.Length + 1);
                hudElements[^1] = hudElement;
            }

            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            private static void PatchHudManagerUpdate(ref HUDManager __instance)
            {
                if (!pingEnabledConfig.Value || _displayText == null)
                    return;

                if (__instance.NetworkManager.IsHost)
                {
                    _displayText.text = "Ping: Host";
                }
                else
                {
                    _displayText.text = string.Format("Ping: {0}ms", PingManager.Ping);
                }
            }
        }
    }

    public class PingManager : MonoBehaviour
    {
        public int Ping { get; private set; }

        private void Start()
        {
            if (!Plugin.pingEnabledConfig.Value)
            {
                return;
            }
            StartCoroutine(UpdatePingData());
        }

        private void OnDestroy()
        {
            StopCoroutine("UpdatePingData");
        }

        private IEnumerator UpdatePingData()
        {
            while (StartOfRound.Instance == null)
            {
                yield return new WaitForSeconds(3f);
            }
            for (; ; )
            {
                if (SteamNetworkingUtils.LocalPingLocation != null && SteamNetworkingUtils.LocalPingLocation != null)
                {
                    Ping = SteamNetworkingUtils.EstimatePingTo(SteamNetworkingUtils.LocalPingLocation.Value);
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Plugin.Log("Could not update ping data. Retrying in 10 seconds.");
                    yield return new WaitForSeconds(10f);
                }
            }
        }
    }
}
