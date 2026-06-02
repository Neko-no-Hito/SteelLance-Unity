using System;
using SteelLance.Combat;
using SteelLance.Mech;
using TMPro;
using UnityEngine;

namespace SteelLance.Mission
{
    /// <summary>
    /// 敵全滅でクリア、プレイヤー撃破で失敗。Phase1 ミッション管理。
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        private const string StatusFormat = "Enemies: {0}";
        private const string ClearMessage = "MISSION CLEAR";
        private const string FailedMessage = "MISSION FAILED";

        [SerializeField] private TextMeshProUGUI statusText;

        private int _remainingEnemies;
        private bool _missionEnded;

        public bool IsMissionEnded => _missionEnded;

        public event Action<bool> MissionEnded;

        private void Start()
        {
            RegisterEnemies();
            RegisterPlayer();
            RefreshStatus();
        }

        private void RegisterEnemies()
        {
            var enemies = FindObjectsByType<EnemyTank>(FindObjectsInactive.Exclude);
            _remainingEnemies = 0;

            foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<Health>();
                if (health == null)
                {
                    continue;
                }

                _remainingEnemies++;
                health.Died += OnEnemyDied;
            }
        }

        private void RegisterPlayer()
        {
            var mech = FindAnyObjectByType<MechController>();
            if (mech == null)
            {
                return;
            }

            var health = mech.GetComponent<Health>();
            if (health != null)
            {
                health.Died += OnPlayerDied;
            }
        }

        private void OnEnemyDied(Health _)
        {
            if (_missionEnded)
            {
                return;
            }

            _remainingEnemies = Mathf.Max(0, _remainingEnemies - 1);
            RefreshStatus();

            if (_remainingEnemies <= 0)
            {
                EndMission(true);
            }
        }

        private void OnPlayerDied(Health _)
        {
            if (!_missionEnded)
            {
                EndMission(false);
            }
        }

        private void EndMission(bool victory)
        {
            _missionEnded = true;
            var message = victory ? ClearMessage : FailedMessage;
            Debug.Log($"[SteelLance] {message}");
            SetStatusText(message);
            MissionEnded?.Invoke(victory);
        }

        private void RefreshStatus()
        {
            if (!_missionEnded)
            {
                SetStatusText(string.Format(StatusFormat, _remainingEnemies));
            }
        }

        private void SetStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
