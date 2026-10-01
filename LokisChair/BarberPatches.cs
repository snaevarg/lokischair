using System;
using HarmonyLib;
using UnityEngine;

namespace LokisChair
{
    // The barber GUI is a second PlayerCustomizaton instance (the same class as character
    // creation). Vanilla remembers hair, beard and hair colour when the barber opens so Cancel
    // can revert them; these patches do the same for the body model and skin colour.
    [HarmonyPatch(typeof(PlayerCustomizaton))]
    internal static class BarberPatches
    {
        private static readonly AccessTools.FieldRef<bool> BarberWasHidden =
            AccessTools.StaticFieldRefAccess<bool>(AccessTools.Field(typeof(PlayerCustomizaton), "m_barberWasHidden"));

        private static bool s_haveSnapshot;
        private static int s_lastModel;
        private static Vector3 s_lastSkinColor;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerCustomizaton.ShowBarberGui))]
        private static void ShowBarberGuiPrefix()
        {
            // Same condition vanilla uses to snapshot m_lastHair: a fresh open, not a re-show
            // after the inventory or map temporarily hid the panel.
            Player player = Player.m_localPlayer;
            if (PlayerCustomizaton.m_barberInstance && player && !BarberWasHidden())
            {
                s_haveSnapshot = true;
                s_lastModel = player.GetPlayerModel();
                s_lastSkinColor = BodySection.SkinColor(player);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerCustomizaton.ShowBarberGui))]
        private static void ShowBarberGuiPostfix()
        {
            if (!PlayerCustomizaton.m_barberInstance)
            {
                return;
            }
            // A layout change in a game update must not stop the barber from opening.
            try
            {
                BodySection.Ensure(PlayerCustomizaton.m_barberInstance);
                BodySection.Refresh();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Could not add the Body and Skin Tone rows to the barber: " + e);
            }
        }

        // Prefix so vanilla's beard restore runs after us; switching to female clears the beard.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerCustomizaton.OnCancel))]
        private static void OnCancelPrefix()
        {
            Player player = Player.m_localPlayer;
            if (s_haveSnapshot && player)
            {
                player.SetPlayerModel(s_lastModel);
                player.SetSkinColor(s_lastSkinColor);
            }
            s_haveSnapshot = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerCustomizaton.OnApply))]
        private static void OnApplyPostfix()
        {
            s_haveSnapshot = false;
        }
    }
}
