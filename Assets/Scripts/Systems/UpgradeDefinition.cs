using UnityEngine;

namespace SurvivorMaster.Systems
{
    public enum UpgradeType
    {
        Damage = 0,
        AttackSpeed = 1,
        MoveSpeed = 2,
        MaxHp = 3
    }

    [CreateAssetMenu(fileName = "Upgrade", menuName = "SurvivorMaster/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "New Upgrade";
        [SerializeField] private string description = "Upgrade description";
        [SerializeField] private UpgradeType upgradeType = UpgradeType.Damage;
        [SerializeField] private float value = 1f;

        public string DisplayName => displayName;
        public string Description => description;
        public UpgradeType UpgradeType => upgradeType;
        public float Value => value;
    }
}
