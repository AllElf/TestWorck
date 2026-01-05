using Game.Player;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public sealed class SimpleHintUI : MonoBehaviour, IHintUI
    {
        [SerializeField] private TMP_Text text;

        public void SetHint(string value)
        {
            if (text == null) return;
            text.text = value ?? string.Empty;
            text.enabled = !string.IsNullOrEmpty(value);
        }
    }
}
