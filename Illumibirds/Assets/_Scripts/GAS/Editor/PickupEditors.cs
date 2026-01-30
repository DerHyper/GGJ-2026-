#if UNITY_EDITOR
using GAS.Pickups;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(EffectPickup))]
    public class EffectPickupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GASEditorStyles.DrawHeader("💊 Effect Pickup");

            EditorGUILayout.HelpBox(
                "ONE-TIME pickup that APPLIES EFFECTS when touched.\n\n" +
                "USE FOR:\n" +
                "• Health pickups (heal effect)\n" +
                "• Buff pickups (speed boost, damage boost)\n" +
                "• Stat upgrades (max health increase)\n" +
                "• Collectibles that give temporary powers\n\n" +
                "HOW IT WORKS:\n" +
                "Player touches → Effects applied → Pickup disappears (or respawns)",
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Comparison box
            GUI.color = new Color(1f, 1f, 0.8f);
            EditorGUILayout.HelpBox(
                "❓ WHEN TO USE WHICH?\n\n" +
                "• EffectPickup → Apply stats/buffs (Health +25, Speed x1.5)\n" +
                "• AbilityPickup → Grant new abilities (Fireball, Dash)\n" +
                "• DamageZone → Continuous area damage (Fire, Poison Gas)",
                MessageType.None);
            GUI.color = Color.white;

            EditorGUILayout.Space(10);

            DrawDefaultInspector();
        }
    }

    [CustomEditor(typeof(AbilityPickup))]
    public class AbilityPickupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GASEditorStyles.DrawHeader("⚔️ Ability Pickup");

            EditorGUILayout.HelpBox(
                "ONE-TIME pickup that GRANTS AN ABILITY when touched.\n\n" +
                "USE FOR:\n" +
                "• Weapon pickups (grants attack ability)\n" +
                "• Power-ups (grants special move)\n" +
                "• Unlockable skills (dash, double jump)\n" +
                "• Spell scrolls (grants magic ability)\n\n" +
                "HOW IT WORKS:\n" +
                "Player touches → Ability added to their list → Pickup disappears",
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Comparison box
            GUI.color = new Color(1f, 1f, 0.8f);
            EditorGUILayout.HelpBox(
                "❓ WHEN TO USE WHICH?\n\n" +
                "• EffectPickup → Apply stats/buffs (Health +25, Speed x1.5)\n" +
                "• AbilityPickup → Grant new abilities (Fireball, Dash)\n" +
                "• DamageZone → Continuous area damage (Fire, Poison Gas)",
                MessageType.None);
            GUI.color = Color.white;

            EditorGUILayout.Space(10);

            DrawDefaultInspector();
        }
    }

    [CustomEditor(typeof(DamageZone))]
    public class DamageZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GASEditorStyles.DrawHeader("🔥 Damage Zone");

            EditorGUILayout.HelpBox(
                "CONTINUOUS AREA that REPEATEDLY applies effects while inside.\n\n" +
                "USE FOR:\n" +
                "• Fire/lava (damage over time)\n" +
                "• Poison gas clouds\n" +
                "• Healing zones (apply heal every second)\n" +
                "• Buff areas (speed boost while inside)\n" +
                "• Debuff zones (slow while inside)\n\n" +
                "HOW IT WORKS:\n" +
                "Entity enters → Effect applied every X seconds → Stops when they leave",
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Comparison box
            GUI.color = new Color(1f, 1f, 0.8f);
            EditorGUILayout.HelpBox(
                "❓ WHEN TO USE WHICH?\n\n" +
                "• EffectPickup → Apply stats/buffs (Health +25, Speed x1.5)\n" +
                "• AbilityPickup → Grant new abilities (Fireball, Dash)\n" +
                "• DamageZone → Continuous area damage (Fire, Poison Gas)",
                MessageType.None);
            GUI.color = Color.white;

            EditorGUILayout.Space(10);

            DrawDefaultInspector();
        }
    }
}
#endif
