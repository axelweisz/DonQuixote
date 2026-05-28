using UnityEngine;

namespace DonQuixote
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Score")]
        [SerializeField] private int _dragonHitPoints = 100;
        [SerializeField] private int _windmillHitPenalty = -50;

        public int Score { get; private set; }

        public enum LevelState { Playing, Paused, Won, Lost }
        public LevelState CurrentLevelState { get; private set; }

        public event System.Action<int> OnScoreChanged;
        public event System.Action<LevelState> OnLevelStateChanged;
        public event System.Action OnDragonHit;
        public event System.Action OnWindmillHit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterDragonHit()
        {
            AddScore(_dragonHitPoints);
            OnDragonHit?.Invoke();
        }

        public void RegisterWindmillHit()
        {
            AddScore(_windmillHitPenalty);
            OnWindmillHit?.Invoke();
        }

        private void AddScore(int delta)
        {
            Score += delta;
            OnScoreChanged?.Invoke(Score);
        }

        public void SetLevelState(LevelState state)
        {
            if (CurrentLevelState == state) return;
            CurrentLevelState = state;
            OnLevelStateChanged?.Invoke(state);
        }
    }
}
