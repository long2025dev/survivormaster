using SurvivorMaster.Core;
using SurvivorMaster.Player;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class DemoSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private XPOrbManager xpOrbManager;

        private void Start()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>();
            }

            if (enemyManager == null)
            {
                enemyManager = EnemyManager.Instance;
            }

            if (xpOrbManager == null)
            {
                xpOrbManager = XPOrbManager.Instance;
            }

            if (player != null && cameraFollow != null)
            {
                cameraFollow.SetTarget(player.transform);
            }

            if (enemyManager != null)
            {
                enemyManager.RegisterPlayer(player);
            }

            if (xpOrbManager != null && player != null)
            {
                xpOrbManager.SetPlayer(player.transform);
            }
        }
    }
}
