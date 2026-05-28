using UnityEngine;

namespace DonQuixote
{
    public class Rocinante : MonoBehaviour
    {
        public enum HorseState
        {
            Grazing, Trotting, Charging, Spooked,
            Stumbling, Exhausted, Knocked
        }

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _chargeStaminaDrain = 20f;    // per second
        [SerializeField] private float _staminaRecoveryRate = 10f;   // per second

        [Header("Chance")]
        [SerializeField] [Range(0f, 1f)] private float _stumbleChancePerTick = 0.05f;
        [SerializeField] [Range(0f, 1f)] private float _spookChance = 0.4f;

        [Header("Timers")]
        [SerializeField] private float _spookedDuration = 1.5f;
        [SerializeField] private float _stumblingDuration = 1f;
        [SerializeField] private float _exhaustedRestDuration = 3f;
        [SerializeField] private float _knockedRecoveryDuration = 2.5f;

        public HorseState CurrentState { get; private set; } = HorseState.Grazing;
        public float Stamina { get; private set; }

        public event System.Action<HorseState> OnStateChanged;

        private QuixoteController _quixote;
        private float _stateTimer;

        private void Awake()
        {
            Stamina = _maxStamina;
        }

        private void Start()
        {
            _quixote = FindFirstObjectByType<QuixoteController>();
        }

        private void Update()
        {
            HandleTimers();
            HandleStamina();
        }

        // ---- Stamina ----

        private void HandleStamina()
        {
            if (CurrentState == HorseState.Charging)
            {
                Stamina -= _chargeStaminaDrain * Time.deltaTime;
                if (Stamina <= 0f)
                {
                    Stamina = 0f;
                    TransitionTo(HorseState.Exhausted);
                }

                // Random stumble chance — checked once per frame (low probability)
                if (Random.value < _stumbleChancePerTick * Time.deltaTime)
                    TransitionTo(HorseState.Stumbling);
            }
            else if (CurrentState == HorseState.Exhausted || CurrentState == HorseState.Grazing)
            {
                Stamina = Mathf.Min(_maxStamina, Stamina + _staminaRecoveryRate * Time.deltaTime);
            }
        }

        // ---- Timer exits ----

        private void HandleTimers()
        {
            switch (CurrentState)
            {
                case HorseState.Spooked:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(HorseState.Trotting);
                    break;
                case HorseState.Stumbling:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(HorseState.Trotting);
                    break;
                case HorseState.Exhausted:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(HorseState.Trotting);
                    break;
                case HorseState.Knocked:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(HorseState.Grazing);
                    break;
            }
        }

        // ---- Public entry points ----

        public void OnQuixoteMounts()
        {
            if (CurrentState == HorseState.Grazing)
                TransitionTo(HorseState.Trotting);
        }

        public void OnChargeInput()
        {
            if (CurrentState == HorseState.Trotting && Stamina > 0f)
                TransitionTo(HorseState.Charging);
        }

        // Called by WindmillEntity when it hits Rocinante
        public void OnWindmillImpact()
        {
            TransitionTo(HorseState.Knocked);
        }

        // Called by WindmillEntity when it transforms to Dragon nearby
        public void OnDragonNearby()
        {
            if (CurrentState == HorseState.Trotting && Random.value < _spookChance)
                TransitionTo(HorseState.Spooked);
        }

        // ---- State transitions ----

        private void TransitionTo(HorseState newState)
        {
            if (CurrentState == newState) return;

            HorseState previous = CurrentState;
            CurrentState = newState;

            switch (newState)
            {
                case HorseState.Spooked:
                    _stateTimer = _spookedDuration;
                    break;
                case HorseState.Stumbling:
                    _stateTimer = _stumblingDuration;
                    _quixote?.TriggerKnockedDown();
                    break;
                case HorseState.Exhausted:
                    _stateTimer = _exhaustedRestDuration;
                    break;
                case HorseState.Knocked:
                    _stateTimer = _knockedRecoveryDuration;
                    _quixote?.TriggerKnockedDown();
                    break;
            }

            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
