﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.UI;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace SpeechMod
{
    [HarmonyPatch(typeof(StaticCanvas), "Initialize")]
    static class DialogCurrentPart_Patch
    {
        static void Postfix()
        {
            if (!Main.Enabled)
                return;

            Debug.Log("Speech Mod Initializing...");

            var parent = Game.Instance.UI.Canvas.transform.Find("DialogPCView/Body/View/Scroll View");
            var originalButton = Game.Instance.UI.Canvas.transform.Find("DialogPCView/Body/View/Scroll View/ButtonEdge").gameObject;

            var buttonGameObject = GameObject.Instantiate(originalButton, parent);
            buttonGameObject.name = "SpeechButton";
            buttonGameObject.transform.localPosition = new Vector3(-493, 164, 0);
            buttonGameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);

            buttonGameObject.AddComponent<WindowsVoice>();
            
            var button = buttonGameObject.GetComponent<OwlcatButton>();
            button.OnLeftClick.RemoveAllListeners();
            button.OnLeftClick.AddListener(Speak);

            buttonGameObject.SetActive(true);

            Debug.Log("Speech Mod Initialized!");
        }

        private static void Speak()
        {
            string text = Game.Instance.DialogController.CurrentCue.DisplayText;
            text = text.Replace("—", ".");
            WindowsVoice.speak(text);
        }
    }
}
