using UnityEngine;

namespace DonQuixote
{
    // Drives the oscillation timing broadcast to all WindmillEntities.
    // Each windmill subscribes and uses its own RNG to decide whether to flip on each tick.
    public class TransformationSystem : MonoBehaviour
    {
        public static TransformationSystem Instance { get; private set; }

        [Header("Oscillation")]
        [SerializeField] private float _baseInterval = 3f;
        [SerializeField] private float _minInterval = 0.4f;
        [SerializeField] private float _accelerationRate = 0.05f;  // subtracted per tick

        private float _currentInterval;
        private float _nextTickTime;

        public event System.Action OnOscillationTick;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _currentInterval = _baseInterval;
            _nextTickTime = Time.time + _currentInterval;
        }

        private void Update()
        {
            if (LuciditySystem.Instance != null && LuciditySystem.Instance.IsLucid)
                return;

            if (Time.time >= _nextTickTime)
            {
                OnOscillationTick?.Invoke();
                _currentInterval = Mathf.Max(_minInterval, _currentInterval - _accelerationRate);
                _nextTickTime = Time.time + _currentInterval;
            }
        }

        public void ResetAcceleration()
        {
            _currentInterval = _baseInterval;
            _nextTickTime = Time.time + _currentInterval;
        }
    }
}
