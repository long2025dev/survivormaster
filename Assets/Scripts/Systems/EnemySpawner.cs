using SurvivorMaster.Player;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private EnemyManager enemyManager;

        [Header("Spawn")]
        [SerializeField] private float initialSpawnRate = 8f;
        [SerializeField] private float spawnRateMultiplierPerStep = 1.15f;
        [SerializeField] private float difficultyStepSeconds = 30f;
        [SerializeField] private float spawnMinDistance = 18f;
        [SerializeField] private float spawnMaxDistance = 24f;

        [Header("Scaling")]
        [SerializeField] private float hpGrowthPerStep = 0.18f;
        [SerializeField] private float speedGrowthPerStep = 0.05f;
        [SerializeField] private float damageGrowthPerStep = 0.08f;
        [SerializeField] private float xpGrowthPerStep = 0.08f;

        private float _spawnRate;
        private float _spawnAccumulator;
        private float _stepTimer;
        private int _difficultyStep;

        private void Awake()
        {
            _spawnRate = initialSpawnRate;
            TryResolveReferences();
        }

        private void Update()
        {
            if (player == null || enemyManager == null)
            {
                TryResolveReferences();
            }

            if (player == null || enemyManager == null)
            {
                return;
            }

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= difficultyStepSeconds)
            {
                _stepTimer -= difficultyStepSeconds;
                _difficultyStep++;
                _spawnRate *= spawnRateMultiplierPerStep;
            }

            _spawnAccumulator += _spawnRate * Time.deltaTime;

            while (_spawnAccumulator >= 1f)
            {
                _spawnAccumulator -= 1f;
                SpawnOne();
            }
        }

        private void SpawnOne()
        {
            float angle = Random.value * Mathf.PI * 2f;
            float dist = Random.Range(spawnMinDistance, spawnMaxDistance);
            Vector3 center = player.transform.position;
            Vector3 spawnPosition = new Vector3(
                center.x + Mathf.Cos(angle) * dist,
                0.5f,
                center.z + Mathf.Sin(angle) * dist);

            float hpMul = 1f + (_difficultyStep * hpGrowthPerStep);
            float speedMul = 1f + (_difficultyStep * speedGrowthPerStep);
            float dmgMul = 1f + (_difficultyStep * damageGrowthPerStep);
            float xpMul = 1f + (_difficultyStep * xpGrowthPerStep);

            enemyManager.SpawnEnemy(spawnPosition, hpMul, speedMul, dmgMul, xpMul);
        }

        private void TryResolveReferences()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>();
            }

            if (enemyManager == null)
            {
                enemyManager = EnemyManager.Instance != null ? EnemyManager.Instance : FindFirstObjectByType<EnemyManager>();
            }
        }
    }
}
