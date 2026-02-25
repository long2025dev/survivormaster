using SurvivorMaster.Core;
using SurvivorMaster.Player;
using SurvivorMaster.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorMaster.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private XPManager xpManager;

        [SerializeField] private SimpleBar hpBar;
        [SerializeField] private SimpleBar xpBar;
        [SerializeField] private Text levelText;
        [SerializeField] private Text enemyCountText;
        [SerializeField] private Text timerText;

        private void Start()
        {
            if (playerStats != null)
            {
                playerStats.HealthChanged += OnHealthChanged;
                OnHealthChanged(playerStats.CurrentHp, playerStats.MaxHp);
            }

            if (xpManager != null)
            {
                xpManager.XpChanged += OnXpChanged;
                xpManager.LevelChanged += OnLevelChanged;
                OnXpChanged(xpManager.CurrentXp, xpManager.XpToNextLevel);
                OnLevelChanged(xpManager.Level);
            }
        }

        private void Update()
        {
            if (EnemyManager.Instance != null && enemyCountText != null)
            {
                enemyCountText.text = $"Enemies: {EnemyManager.Instance.ActiveCount}";
            }

            if (timerText != null && GameTimer.Instance != null)
            {
                float total = GameTimer.Instance.ElapsedSeconds;
                int minutes = Mathf.FloorToInt(total / 60f);
                int seconds = Mathf.FloorToInt(total % 60f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }

        private void OnDestroy()
        {
            if (playerStats != null)
            {
                playerStats.HealthChanged -= OnHealthChanged;
            }

            if (xpManager != null)
            {
                xpManager.XpChanged -= OnXpChanged;
                xpManager.LevelChanged -= OnLevelChanged;
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            hpBar?.SetFill(max > 0f ? current / max : 0f);
        }

        private void OnXpChanged(float current, float max)
        {
            xpBar?.SetFill(max > 0f ? current / max : 0f);
        }

        private void OnLevelChanged(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv {level}";
            }
        }
    }
}
