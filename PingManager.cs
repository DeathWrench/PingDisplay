using System;
using System.Collections;
using Steamworks;
using UnityEngine;

namespace PingDisplay
{
	public class PingManager : MonoBehaviour
	{
		public int Ping { get; private set; }
		private void Start()
		{
			if (!ConfigSettings.PingEnabled.Value)
			{
				return;
			}
			this._coroutine = base.StartCoroutine(this.UpdatePingData());
		}
		private void OnDestroy()
		{
			base.StopCoroutine(this._coroutine);
		}
		private IEnumerator UpdatePingData()
		{
			while (StartOfRound.Instance == null)
			{
				yield return new WaitForSeconds(3f);
			}
			for (;;)
			{
				if (SteamNetworkingUtils.LocalPingLocation != null && SteamNetworkingUtils.LocalPingLocation != null)
				{
					this.Ping = SteamNetworkingUtils.EstimatePingTo(SteamNetworkingUtils.LocalPingLocation.Value);
					yield return new WaitForSeconds(0.5f);
				}
				else
				{
					Plugin.Log("Could not update ping data. Retrying in 10 seconds.");
					yield return new WaitForSeconds(10f);
				}
			}
			yield break;
		}
		private Coroutine _coroutine;
	}
}
