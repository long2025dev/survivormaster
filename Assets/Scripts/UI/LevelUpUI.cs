using System;
using SurvivorMaster.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorMaster.UI
{
    public sealed class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Text[] optionTitles;
        [SerializeField] private Text[] optionDescriptions;

        private readonly UpgradeDefinition[] _current = new UpgradeDefinition[3];
        private Action<UpgradeDefinition> _onSelected;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }
            Hide();
        }

        public void Show(UpgradeDefinition[] options, Action<UpgradeDefinition> onSelected)
        {
            _onSelected = onSelected;
            if (root != null)
            {
                root.SetActive(true);
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                UpgradeDefinition def = options != null && i < options.Length ? options[i] : null;
                _current[i] = def;

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => Select(index));

                if (optionTitles != null && i < optionTitles.Length && optionTitles[i] != null)
                {
                    optionTitles[i].text = def != null ? def.DisplayName : "N/A";
                }

                if (optionDescriptions != null && i < optionDescriptions.Length && optionDescriptions[i] != null)
                {
                    optionDescriptions[i].text = def != null ? def.Description : string.Empty;
                }
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void Select(int index)
        {
            if (index < 0 || index >= _current.Length)
            {
                return;
            }

            _onSelected?.Invoke(_current[index]);
        }
    }
}
