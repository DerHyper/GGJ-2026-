using System.Text;
using GAS.Core;
using GAS.Effects;
using UnityEngine;

namespace GAS.Debugger
{
    /// <summary>
    /// Runtime debug display for GAS state.
    /// Shows attributes, effects, and abilities on screen.
    /// </summary>
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class GASDebugDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool _showInGame = true;
        [SerializeField] private bool _showAttributes = true;
        [SerializeField] private bool _showEffects = true;
        [SerializeField] private bool _showAbilities = true;
        [SerializeField] private bool _showTags = true;

        [Header("Position")]
        [SerializeField] private Vector2 _screenOffset = new Vector2(10, 10);
        [SerializeField] private bool _followObject = false;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0, 2, 0);

        [Header("Style")]
        [SerializeField] private int _fontSize = 12;
        [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;

        private AbilitySystemComponent _asc;
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private StringBuilder _sb = new StringBuilder();

        private void Awake()
        {
            _asc = GetComponent<AbilitySystemComponent>();
        }

        private void OnGUI()
        {
            if (!_showInGame || _asc == null) return;

            InitStyles();

            Vector2 screenPos;
            if (_followObject)
            {
                var worldPos = transform.position + _worldOffset;
                screenPos = Camera.main.WorldToScreenPoint(worldPos);
                screenPos.y = Screen.height - screenPos.y; // Flip Y for GUI
            }
            else
            {
                screenPos = _screenOffset;
            }

            _sb.Clear();
            _sb.AppendLine($"<b>{gameObject.name}</b>");
            _sb.AppendLine("─────────────────");

            if (_showAttributes)
            {
                BuildAttributesText();
            }

            if (_showEffects)
            {
                BuildEffectsText();
            }

            if (_showAbilities)
            {
                BuildAbilitiesText();
            }

            if (_showTags)
            {
                BuildTagsText();
            }

            var content = new GUIContent(_sb.ToString());
            var size = _labelStyle.CalcSize(content);
            size.x += 20;
            size.y += 10;

            var rect = new Rect(screenPos.x, screenPos.y, size.x, size.y);

            GUI.Box(rect, "", _boxStyle);
            GUI.Label(new Rect(rect.x + 10, rect.y + 5, rect.width - 20, rect.height - 10), content, _labelStyle);
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, _backgroundColor);
            bgTex.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex }
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                normal = { textColor = _textColor },
                richText = true,
                wordWrap = false
            };

            _headerStyle = new GUIStyle(_labelStyle)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private void BuildAttributesText()
        {
            _sb.AppendLine("\n<color=#88CCFF>ATTRIBUTES</color>");

            foreach (var kvp in _asc.Attributes.Attributes)
            {
                var attr = kvp.Value;
                var def = kvp.Key;

                var name = def.name.Replace("Attr_", "");
                var value = attr.CurrentValue;
                var baseVal = attr.BaseValue;

                if (!Mathf.Approximately(value, baseVal))
                {
                    _sb.AppendLine($"  {name}: <color=yellow>{value:F0}</color> (base: {baseVal:F0})");
                }
                else
                {
                    _sb.AppendLine($"  {name}: {value:F0}");
                }
            }
        }

        private void BuildEffectsText()
        {
            if (_asc.ActiveEffects.Count == 0) return;

            _sb.AppendLine("\n<color=#FFCC88>EFFECTS</color>");

            foreach (var effect in _asc.ActiveEffects)
            {
                var name = effect.Definition.name.Replace("Effect_", "");
                var stack = effect.StackCount > 1 ? $" x{effect.StackCount}" : "";

                if (effect.Definition.DurationType == EffectDurationType.Duration)
                {
                    _sb.AppendLine($"  {name}{stack} ({effect.RemainingDuration:F1}s)");
                }
                else
                {
                    _sb.AppendLine($"  {name}{stack}");
                }
            }
        }

        private void BuildAbilitiesText()
        {
            if (_asc.GrantedAbilities.Count == 0) return;

            _sb.AppendLine("\n<color=#88FF88>ABILITIES</color>");

            foreach (var kvp in _asc.GrantedAbilities)
            {
                var ability = kvp.Value;
                var name = kvp.Key.name.Replace("Ability_", "");

                string status;
                if (ability.IsActive)
                {
                    status = "<color=green>ACTIVE</color>";
                }
                else if (ability.IsOnCooldown)
                {
                    status = $"<color=gray>CD {ability.CooldownRemaining:F1}s</color>";
                }
                else
                {
                    status = "<color=white>Ready</color>";
                }

                _sb.AppendLine($"  {name}: {status}");
            }
        }

        private void BuildTagsText()
        {
            if (_asc.OwnedTags.Count == 0) return;

            _sb.AppendLine("\n<color=#FF88CC>TAGS</color>");

            foreach (var tag in _asc.OwnedTags.Tags)
            {
                var name = tag.name.Replace("Tag_", "");
                _sb.AppendLine($"  • {name}");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_asc == null) _asc = GetComponent<AbilitySystemComponent>();
            if (_asc == null) return;

            // Draw range attribute if present
            foreach (var kvp in _asc.Attributes.Attributes)
            {
                if (kvp.Key.name.Contains("Range"))
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    Gizmos.DrawWireSphere(transform.position, kvp.Value.CurrentValue);
                }
            }
        }
#endif
    }
}
