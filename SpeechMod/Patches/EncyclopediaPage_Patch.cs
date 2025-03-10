﻿using System;
using HarmonyLib;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Encyclopedia;
using SpeechMod.Unity;
using System.Linq;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using SpeechMod.voice;
using TMPro;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(EncyclopediaPagePCView), "UpdateView")]
public static class EncyclopediaPage_Patch
{
    private static readonly string m_ButtonName = "EncyclopediaSpeechButton";

    private const string BODY_GROUP_PATH = "ServiceWindowsPCView/Background/Windows/EncyclopediaPCView/EncyclopediaPageView/BodyGroup";

    public static void Postfix()
    {
        if (!Main.VoiceSettings.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(EncyclopediaPagePCView)}_UpdateView_Postfix");
#endif

        var bodyGroup = UIHelper.TryFindInStaticCanvas(BODY_GROUP_PATH);
        if (bodyGroup == null)
        {
#if DEBUG
            Debug.Log("Couldn't find BodyGroup...");
#endif
            return;
        }

        var content = bodyGroup.TryFind("ObjectivesGroup/StandardScrollView/Viewport/Content");
        if (content == null)
        {
#if DEBUG
            Debug.Log("Couldn't any TextMeshProUGUI...");
#endif
            return;
        }

        // Only get the texts that is not in the unit view.
        var allTexts = content.gameObject?.GetComponentsInChildren<TextMeshProUGUI>(true).Where(t => t.transform.name.Equals("Text")).ToArray();
        if (allTexts == null || allTexts.Length == 0)
        {
#if DEBUG
            Debug.Log("Couldn't find any TextMeshProUGUI...");
#endif
            return;
        }

        foreach (var textMeshPro in allTexts)
        {
            var parent = textMeshPro.transform;

            var button = parent.TryFind(m_ButtonName)?.gameObject;

            if (button != null)
            {
#if DEBUG
                Debug.Log("Button already added, relocating and activating...");
#endif
                button.transform.localRotation = Quaternion.Euler(0, 0, 90);
                button.RectAlignTopLeft();
                button.transform.localPosition = new Vector3(-36, -26, 0);
                continue;
            }

#if DEBUG
            Debug.Log("Adding playbutton...");
#endif
            button = ButtonFactory.CreatePlayButton(parent, () =>
            {
                var text = textMeshPro.text ?? string.Empty;
                Main.WaveOutEvent?.Stop();
                using var md5 = MD5.Create();
                var inputBytes = System.Text.Encoding.ASCII.GetBytes(text);
                var guid = new Guid(md5.ComputeHash(inputBytes));
                _ = VoicePlayer.PlayText(text, guid.ToString(), Gender.Female, Constants.Narrator);
            });
            button.name = m_ButtonName;
            button.transform.localRotation = Quaternion.Euler(0, 0, 90);
            button.RectAlignTopLeft();
            button.transform.localPosition = new Vector3(-36, -26, 0);
            button.gameObject.SetActive(true);
        }
    }
}