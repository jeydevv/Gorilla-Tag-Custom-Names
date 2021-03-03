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
	[BepInPlugin("org.jeydevv.monkeytag.customnames", "Custom Names", "1.0.1")]
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
		public static float red = 0f;
		public static float green = 0f;
		public static float blue = 0f;
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
							red,
							green,
							blue
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
					string[] tmpLine = array[0].Split('-');

					string frameDelay = tmpLine[0];
					string rgb = tmpLine[1];

					int intFrameDelay = int.Parse(frameDelay);
					if (intFrameDelay < 20)
					{
						intFrameDelay = 20;
					}

					CustomNameLogic.red = float.Parse(rgb[0].ToString()) / 10f;
					CustomNameLogic.green = float.Parse(rgb[1].ToString()) / 10f;
					CustomNameLogic.blue = float.Parse(rgb[2].ToString()) / 10f;

					string[] tmpNames = new string[array.Length - 2];
					for (int i = 2; i < array.Length; i++)
					{
						tmpNames[i - 2] = array[i];
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
