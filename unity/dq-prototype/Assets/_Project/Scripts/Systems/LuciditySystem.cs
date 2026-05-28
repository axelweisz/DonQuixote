using UnityEngine;

namespace DonQuixote
{
    // Owns the Lucidity timer. QuixoteController triggers it; WindmillEntity listens to it.
    public class LuciditySystem : MonoBehaviour
    {
        public static LuciditySystem Instance { get; private set; }

        [SerializeField] private float _lucidityDuration = 4f;

        public bool IsLucid { get; private set; }
        private float _lucidityTimer;

        public event System.Action OnLucidityBegan;
        public event System.Action OnLucidityEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!IsLucid) return;

            _lucidityTimer -= Time.deltaTime;
            if (_lucidityTimer <= 0f)
                EndLucidity();
        }

        public void TriggerLucidity()
        {
            IsLucid = true;
            _lucidityTimer = _lucidityDuration;
            OnLucidityBegan?.Invoke();
        }

        private void EndLucidity()
        {
            IsLucid = false;
            OnLucidityEnded?.Invoke();
        }
    }
}
