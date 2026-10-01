using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LokisChair
{
    // Adds "Body" and "Skin Tone" rows to the top of the barber panel, in character-creation
    // order, by cloning the vanilla Beard row and Hair Tone slider so they inherit the game's
    // fonts, sprites and sounds.
    //
    // The barber panel has no per-row containers: every label, arrow, value and slider is a direct
    // child of "root", centre-anchored, and HairPanel/BeardPanel are empty rects marking each row's
    // extent. Done/Cancel are anchored to root's bottom edge and the background stretches with it.
    internal static class BodySection
    {
        internal static readonly AccessTools.FieldRef<Player, Vector3> SkinColor =
            AccessTools.FieldRefAccess<Player, Vector3>("m_skinColor");

        private static readonly string[] ModelNames = { "Male", "Female" };

        private static PlayerCustomizaton s_gui;
        private static TMP_Text s_modelValue;
        private static Slider s_skinSlider;
        private static bool s_dumped;

        public static void Ensure(PlayerCustomizaton gui)
        {
            // A new world load brings a new BarberGui instance, which needs its own rows.
            if (s_gui == gui)
            {
                return;
            }
            s_gui = gui;
            s_modelValue = null;
            s_skinSlider = null;
            if (!s_dumped)
            {
                s_dumped = true;
                Plugin.Log.LogDebug("Barber GUI hierarchy:\n" + Dump(gui.transform));
            }

            RectTransform beardPanel = gui.m_beardPanel;
            RectTransform root = (RectTransform)beardPanel.parent;
            float halfRow = beardPanel.rect.height / 2f;
            float beardY = Y(beardPanel);
            Transform hairPanel = root.Find("HairPanel");
            float hairY = hairPanel ? Y(hairPanel) : beardY + Y(gui.m_selectedHair.transform) - Y(gui.m_selectedBeard.transform);
            float rowStep = hairY - beardY;
            float hairTop = Band(root, hairY, halfRow).Max(Y);

            RectTransform toneSlider = (RectTransform)gui.m_hairTone.transform;
            RectTransform levelSlider = (RectTransform)gui.m_hairLevel.transform;
            float maxLabelGap = (Y(toneSlider) - Y(levelSlider)) / 2f;
            RectTransform toneLabel = LabelAbove(root, toneSlider, maxLabelGap);
            RectTransform levelLabel = LabelAbove(root, levelSlider, maxLabelGap);

            // Copies of the Beard row and the Hair Tone label + slider, moved up one row step so
            // the Beard copy lands in the Hair row's slot and the slider keeps its spacing below it.
            Vector3 up = root.TransformVector(0f, rowStep, 0f);
            Dictionary<RectTransform, Vector3> templates = Band(root, beardY, halfRow)
                .Where(c => c != beardPanel)
                .Concat(new[] { toneLabel, toneSlider })
                .ToDictionary(c => c, c => c.position + up);

            // Hair then follows the skin slider with the gap that separates Hair Tone from Blondness.
            float newHairTop = Y(levelLabel) + rowStep;
            Grow(root, hairTop - newHairTop, hairY + halfRow);

            foreach (KeyValuePair<RectTransform, Vector3> entry in templates)
            {
                RectTransform clone = Object.Instantiate(entry.Key, root);
                clone.name = "LokisChair " + entry.Key.name;
                clone.position = entry.Value;
                Configure(gui, entry.Key, clone, toneLabel);
            }
        }

        public static void Refresh()
        {
            Player player = Player.m_localPlayer;
            if (!player)
            {
                return;
            }
            if (s_modelValue)
            {
                s_modelValue.text = ModelNames[Mathf.Clamp(player.GetPlayerModel(), 0, ModelNames.Length - 1)];
            }
            if (s_skinSlider)
            {
                s_skinSlider.SetValueWithoutNotify(SkinTone(SkinColor(player)));
            }
        }

        private static void Configure(PlayerCustomizaton gui, RectTransform template, RectTransform clone, RectTransform toneLabel)
        {
            // Fresh events also drop the listeners serialized in the prefab, which
            // RemoveAllListeners() would leave in place.
            if (clone.TryGetComponent(out Button button))
            {
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(ToggleModel);
            }
            else if (clone.TryGetComponent(out Slider slider))
            {
                s_skinSlider = slider;
                slider.onValueChanged = new Slider.SliderEvent();
                slider.onValueChanged.AddListener(SetSkinTone);
            }
            else if (template == gui.m_selectedBeard.transform)
            {
                s_modelValue = clone.GetComponent<TMP_Text>();
            }
            else if (clone.TryGetComponent(out TMP_Text label))
            {
                label.text = template == toneLabel ? "Skin Tone" : "Body";
            }
        }

        private static void ToggleModel()
        {
            Player player = Player.m_localPlayer;
            if (!player || !s_gui)
            {
                return;
            }
            // PlayerCustomizaton.SetPlayerModel also clears the beard when switching to female.
            s_gui.SetPlayerModel(player.GetPlayerModel() == 0 ? 1 : 0);
            Refresh();
        }

        // Same mapping as the character creator's skin slider.
        private static void SetSkinTone(float value)
        {
            Player player = Player.m_localPlayer;
            if (player && s_gui)
            {
                player.SetSkinColor(Utils.ColorToVec3(Color.Lerp(s_gui.m_skinColor0, s_gui.m_skinColor1, value)));
            }
        }

        // Inverse of SetSkinTone: the slider position whose colour is closest to `skin`.
        private static float SkinTone(Vector3 skin)
        {
            Vector3 from = Utils.ColorToVec3(s_gui.m_skinColor0);
            Vector3 range = Utils.ColorToVec3(s_gui.m_skinColor1) - from;
            return range.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(skin - from, range) / range.sqrMagnitude) : 0f;
        }

        // Extends root downwards by `step`, keeping the top edge in place. Children at or below
        // `belowY` move down by `step`; the rest keep their on-screen position. Stretched children
        // (the background) simply grow with root.
        private static void Grow(RectTransform root, float step, float belowY)
        {
            Vector3 down = root.TransformVector(0f, -step, 0f);
            Dictionary<RectTransform, Vector3> targets = root.Cast<RectTransform>()
                .Where(IsFixedHeight)
                .ToDictionary(c => c, c => Y(c) <= belowY ? c.position + down : c.position);

            root.sizeDelta += new Vector2(0f, step);
            root.anchoredPosition -= new Vector2(0f, step * (1f - root.pivot.y));

            foreach (KeyValuePair<RectTransform, Vector3> target in targets)
            {
                target.Key.position = target.Value;
            }
        }

        // Active, fixed-height children of root within `halfHeight` of `y`: one visual row.
        private static IEnumerable<RectTransform> Band(RectTransform root, float y, float halfHeight)
        {
            return root.Cast<RectTransform>()
                .Where(c => c.gameObject.activeSelf && IsFixedHeight(c) && Mathf.Abs(Y(c) - y) <= halfHeight);
        }

        // The nearest text sitting above `slider`, at most `maxGap` away: the slider's caption.
        private static RectTransform LabelAbove(RectTransform root, RectTransform slider, float maxGap)
        {
            return root.Cast<RectTransform>()
                .Where(c => c.gameObject.activeSelf && c.GetComponent<TMP_Text>() && Y(c) > Y(slider) && Y(c) - Y(slider) <= maxGap)
                .OrderBy(Y)
                .First();
        }

        private static float Y(Transform t)
        {
            return t.localPosition.y;
        }

        private static bool IsFixedHeight(RectTransform rt)
        {
            return rt.anchorMin.y == rt.anchorMax.y;
        }

        private static string Dump(Transform root)
        {
            StringBuilder sb = new StringBuilder();
            Dump(root, 0, sb);
            return sb.ToString();
        }

        private static void Dump(Transform t, int depth, StringBuilder sb)
        {
            sb.Append(' ', depth * 2).Append(t.name);
            if (!t.gameObject.activeSelf)
            {
                sb.Append(" (inactive)");
            }
            if (t is RectTransform rt)
            {
                sb.Append($" anchors={rt.anchorMin}-{rt.anchorMax} pivot={rt.pivot} pos={rt.anchoredPosition} size={rt.sizeDelta}");
            }
            sb.Append(" [")
                .Append(string.Join(", ", t.GetComponents<Component>().Where(c => c && !(c is Transform)).Select(c => c.GetType().Name)))
                .Append(']');
            if (t.TryGetComponent(out TMP_Text text))
            {
                sb.Append(" \"").Append(text.text).Append('"');
            }
            sb.AppendLine();
            foreach (Transform child in t)
            {
                Dump(child, depth + 1, sb);
            }
        }
    }
}
