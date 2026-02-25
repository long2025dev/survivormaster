using UnityEngine;
using UnityEngine.UI;

namespace SurvivorMaster.UI
{
    public sealed class SimpleBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public void SetFill(float normalized)
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}
