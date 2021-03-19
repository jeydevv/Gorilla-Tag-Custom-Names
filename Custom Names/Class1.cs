using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using UnityEngine.XR;
using Photon.Pun;
using Photon;
using UnityEngine.UI;
using System.IO;

namespace Custom_Names
{
	[BepInPlugin("org.jeydevv.monkeytag.customnames", "Custom Names", "1.1.0")]
	public class CustomNameMain : BaseUnityPlugin
	{
		public void Awake()
		{
			var harmony = new Harmony("com.jeydevv.monkeytag.customnames");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(GorillaLocomotion.Player))]
	[HarmonyPatch("Update", MethodType.Normal)]
	class CustomNameLogic
	{
		public static bool loaded = false;
		public static string[] names;
		public static int frameDelay = 20;
		static int indexUsed = 0;
		static void Postfix()
		{
			try
			{
				if (Time.frameCount % frameDelay == 0 && loaded)
				{
					string text = names[indexUsed++ % names.Length];
					PhotonNetwork.LocalPlayer.NickName = text;
					GorillaComputer.instance.offlineVRRigNametagText.text = text;
					GorillaComputer.instance.savedName = text;
					PlayerPrefs.SetString("playerName", text);
					PlayerPrefs.Save();

					if (PhotonNetwork.InRoom)
					{
						GorillaTagger.Instance.myVRRig.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, new object[]
						{
							PlayerPrefs.GetFloat("redValue"),
							PlayerPrefs.GetFloat("greenValue"),
							PlayerPrefs.GetFloat("blueValue")
						});
					}
				}
			}
			catch (Exception e)
			{
				File.WriteAllText("customnames_postfixerror.log", e.ToString());
			}
		}

		[HarmonyPatch(typeof(GorillaLocomotion.Player))]
		[HarmonyPatch("Awake", MethodType.Normal)]
		class CustomNameConfig
		{
			static void Prefix()
			{
				try
				{
					string[] array = File.ReadAllLines("BepInEx\\plugins\\CustomNames\\name_config.txt");
					string frameDelay = array[0];

					int intFrameDelay = int.Parse(frameDelay);
					if (intFrameDelay < 20)
					{
						intFrameDelay = 20;
					}

					string[] tmpNames = new string[array.Length - 1];
					for (int i = 0; i < tmpNames.Length; i++)
					{
						tmpNames[i] = array[i + 1];
					}
					CustomNameLogic.names = tmpNames;

					CustomNameLogic.frameDelay = intFrameDelay;
					CustomNameLogic.loaded = true;
				}
				catch (Exception e)
				{
					File.WriteAllText("customnames_prefixerror.log", e.ToString());
				}
			}
		}
	}
}
