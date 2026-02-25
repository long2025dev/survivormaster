using System;
using System.Collections.Generic;
using SurvivorMaster.Player;
using SurvivorMaster.UI;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class XPManager : MonoBehaviour
    {
        public static XPManager Instance { get; private set; }

        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private LevelUpUI levelUpUI;
        [SerializeField] private UpgradeDefinition[] availableUpgrades;

        [SerializeField] private float baseXpToLevel = 20f;
        [SerializeField] private float xpGrowth = 1.28f;

        private readonly UpgradeDefinition[] _choiceBuffer = new UpgradeDefinition[3];
        private readonly List<int> _uniqueIndices = new List<int>(3);

        public event Action<float, float> XpChanged;
        public event Action<int> LevelChanged;

        public int Level { get; private set; } = 1;
        public float CurrentXp { get; private set; }
        public float XpToNextLevel { get; private set; }

        private int _pendingLevelUps;
        private bool _isChoosing;

        private void Awake()
        {
            Instance = this;
            XpToNextLevel = baseXpToLevel;
        }

        public void AddXP(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentXp += amount;

            while (CurrentXp >= XpToNextLevel)
            {
                CurrentXp -= XpToNextLevel;
                Level++;
                XpToNextLevel = Mathf.Ceil(XpToNextLevel * xpGrowth);
                _pendingLevelUps++;
                LevelChanged?.Invoke(Level);
            }

            XpChanged?.Invoke(CurrentXp, XpToNextLevel);

            if (!_isChoosing && _pendingLevelUps > 0)
            {
                ShowNextLevelUp();
            }
        }

        private void ShowNextLevelUp()
        {
            if (levelUpUI == null || availableUpgrades == null || availableUpgrades.Length == 0)
            {
                _pendingLevelUps = 0;
                return;
            }

            _isChoosing = true;
            Time.timeScale = 0f;
            BuildUpgradeChoices();
            levelUpUI.Show(_choiceBuffer, OnUpgradeSelected);
        }

        private void BuildUpgradeChoices()
        {
            _uniqueIndices.Clear();

            int count = Mathf.Min(3, availableUpgrades.Length);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableUpgrades.Length);
                int guard = 0;
                while (_uniqueIndices.Contains(randomIndex) && guard < 20)
                {
                    randomIndex = UnityEngine.Random.Range(0, availableUpgrades.Length);
                    guard++;
                }

                _uniqueIndices.Add(randomIndex);
                _choiceBuffer[i] = availableUpgrades[randomIndex];
            }

            for (int i = count; i < _choiceBuffer.Length; i++)
            {
                _choiceBuffer[i] = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Length)];
            }
        }

        private void OnUpgradeSelected(UpgradeDefinition selected)
        {
            if (selected != null && playerStats != null)
            {
                playerStats.ApplyUpgrade(selected);
            }

            _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);

            if (_pendingLevelUps > 0)
            {
                ShowNextLevelUp();
                return;
            }

            _isChoosing = false;
            Time.timeScale = 1f;
            levelUpUI.Hide();
            XpChanged?.Invoke(CurrentXp, XpToNextLevel);
        }
    }
}
