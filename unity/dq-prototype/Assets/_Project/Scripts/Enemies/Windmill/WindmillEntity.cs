using UnityEngine;

namespace DonQuixote
{
    public class WindmillEntity : MonoBehaviour
    {
        public enum WindmillState { Idle, Windmill, Dragon, Frozen }

        [Header("Proximity")]
        [SerializeField] private float _activationRadius = 8f;

        [Header("Visuals")]
        [SerializeField] private GameObject _windmillVisual;
        [SerializeField] private GameObject _dragonVisual;

        [Header("Hitbox")]
        [SerializeField] private Collider2D _dragonHitbox;

        [Header("Debug")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public WindmillState CurrentState { get; private set; } = WindmillState.Idle;
        private Transform _quixoteTransform;

        public event System.Action<WindmillState> OnStateChanged;

        private void Start()
        {
            var quixote = FindFirstObjectByType<QuixoteController>();
            if (quixote != null) _quixoteTransform = quixote.transform;

            if (LuciditySystem.Instance != null)
            {
                LuciditySystem.Instance.OnLucidityBegan += HandleLucidityBegan;
                LuciditySystem.Instance.OnLucidityEnded += HandleLucidityEnded;
            }

            if (TransformationSystem.Instance != null)
                TransformationSystem.Instance.OnOscillationTick += HandleOscillationTick;

            ApplyVisuals();
        }

        private void OnDestroy()
        {
            if (LuciditySystem.Instance != null)
            {
                LuciditySystem.Instance.OnLucidityBegan -= HandleLucidityBegan;
                LuciditySystem.Instance.OnLucidityEnded -= HandleLucidityEnded;
            }

            if (TransformationSystem.Instance != null)
                TransformationSystem.Instance.OnOscillationTick -= HandleOscillationTick;
        }

        private void Update()
        {
            if (CurrentState == WindmillState.Idle && IsQuixoteInRange())
                TransitionTo(WindmillState.Windmill);
        }

        private void HandleOscillationTick()
        {
            if (CurrentState == WindmillState.Windmill)
                TransitionTo(WindmillState.Dragon);
            else if (CurrentState == WindmillState.Dragon)
                TransitionTo(WindmillState.Windmill);
        }

        private void HandleLucidityBegan()
        {
            if (CurrentState == WindmillState.Windmill || CurrentState == WindmillState.Dragon)
                TransitionTo(WindmillState.Frozen);
        }

        private void HandleLucidityEnded()
        {
            if (CurrentState == WindmillState.Frozen)
                TransitionTo(WindmillState.Windmill);
        }

        private void TransitionTo(WindmillState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            ApplyVisuals();
            OnStateChanged?.Invoke(CurrentState);
        }

        private void ApplyVisuals()
        {
            bool isDragon = CurrentState == WindmillState.Dragon;
            if (_windmillVisual != null) _windmillVisual.SetActive(!isDragon);
            if (_dragonVisual != null) _dragonVisual.SetActive(isDragon);
            if (_dragonHitbox != null) _dragonHitbox.enabled = isDragon;
            ApplyDebugTint();
        }

        private void ApplyDebugTint()
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = CurrentState switch
            {
                WindmillState.Idle     => new Color(0.4f, 0.4f, 0.4f),
                WindmillState.Windmill => Color.white,
                WindmillState.Dragon   => new Color(0.9f, 0.15f, 0.1f),
                WindmillState.Frozen   => new Color(0.4f, 0.9f, 1f),
                _                      => Color.white
            };
        }

        public void OnHit()
        {
            if (CurrentState == WindmillState.Dragon)
                GameManager.Instance?.RegisterDragonHit();
            else if (CurrentState == WindmillState.Windmill)
                GameManager.Instance?.RegisterWindmillHit();
        }

        private bool IsQuixoteInRange()
        {
            if (_quixoteTransform == null) return false;
            return Vector2.Distance(transform.position, _quixoteTransform.position) <= _activationRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _activationRadius);
        }
    }
}
