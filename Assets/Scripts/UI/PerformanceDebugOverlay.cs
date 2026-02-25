using System.Text;
using SurvivorMaster.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorMaster.UI
{
    public sealed class PerformanceDebugOverlay : MonoBehaviour
    {
        [SerializeField] private Text output;
        [SerializeField] private float refreshInterval = 0.25f;

        private readonly StringBuilder _builder = new StringBuilder(256);
        private float _refreshTimer;
        private float _smoothedMs = 16.6f;

        private void Update()
        {
            _smoothedMs = Mathf.Lerp(_smoothedMs, Time.unscaledDeltaTime * 1000f, 0.1f);

            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer > 0f || output == null)
            {
                return;
            }

            _refreshTimer = refreshInterval;

            int enemyActive = EnemyManager.Instance != null ? EnemyManager.Instance.ActiveCount : 0;
            int enemyPool = EnemyManager.Instance != null ? EnemyManager.Instance.PoolTotalCount : 0;
            int projectileActive = ProjectileManager.Instance != null ? ProjectileManager.Instance.ActiveCount : 0;
            int projectilePool = ProjectileManager.Instance != null ? ProjectileManager.Instance.PoolTotalCount : 0;
            int xpActive = XPOrbManager.Instance != null ? XPOrbManager.Instance.ActiveCount : 0;
            int xpPool = XPOrbManager.Instance != null ? XPOrbManager.Instance.PoolTotalCount : 0;

            _builder.Length = 0;
            _builder.Append("Frame: ").Append(_smoothedMs.ToString("F2")).Append(" ms\n");
            _builder.Append("Enemies: ").Append(enemyActive).Append(" / Pool ").Append(enemyPool).Append('\n');
            _builder.Append("Projectiles: ").Append(projectileActive).Append(" / Pool ").Append(projectilePool).Append('\n');
            _builder.Append("XP Orbs: ").Append(xpActive).Append(" / Pool ").Append(xpPool);

            output.text = _builder.ToString();
        }
    }
}
