using SteelLance.Combat;
using SteelLance.Mech;
using TMPro;
using UnityEngine;

namespace SteelLance.UI
{
    /// <summary>
    /// プレイヤー Torso HP を TMP に表示。Phase2 HUD。
    /// </summary>
    public class PlayerHealthHud : MonoBehaviour
    {
        private const string HpFormat = "CORE: {0:0} / {1:0}";

        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private MechPartSystem playerPartSystem;
        [SerializeField] private Health fallbackHealth;

        private void Start()
        {
            if (playerPartSystem == null)
            {
                var mech = FindAnyObjectByType<MechController>();
                if (mech != null)
                {
                    playerPartSystem = mech.GetComponent<MechPartSystem>();
                    if (fallbackHealth == null)
                    {
                        fallbackHealth = mech.GetComponent<Health>();
                    }
                }
            }

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (hpText == null)
            {
                return;
            }

            if (playerPartSystem != null)
            {
                hpText.text = string.Format(
                    HpFormat,
                    playerPartSystem.TorsoCurrentHP,
                    playerPartSystem.TorsoMaxHP);
                return;
            }

            if (fallbackHealth != null)
            {
                hpText.text = string.Format(
                    "HP: {0:0} / {1:0}",
                    fallbackHealth.CurrentHealth,
                    fallbackHealth.MaxHealth);
            }
        }
    }
}
