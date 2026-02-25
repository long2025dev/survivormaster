using UnityEngine;

namespace SurvivorMaster.Core
{
    public sealed class GameTimer : MonoBehaviour
    {
        public static GameTimer Instance { get; private set; }

        public float ElapsedSeconds { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            ElapsedSeconds += Time.deltaTime;
        }
    }
}
