using UnityEngine;

namespace DonQuixote
{
    public class Rucio : MonoBehaviour
    {
        public enum DonkeyState { Grazing, Plodding, Stopping, Braying, Bolting }

        [Header("Chance")]
        [SerializeField] [Range(0f, 1f)] private float _stoppingChancePerSecond = 0.02f;
        [SerializeField] [Range(0f, 1f)] private float _brayingChance = 0.2f;

        [Header("Timers")]
        [SerializeField] private float _stoppingCoaxDuration = 4f;
        [SerializeField] private float _brayingDuration = 2f;
        [SerializeField] private float _boltingDuration = 3f;

        public DonkeyState CurrentState { get; private set; } = DonkeyState.Grazing;

        public event System.Action<DonkeyState> OnStateChanged;

        private SanchoPanza _sancho;
        private float _stateTimer;

        private void Start()
        {
            _sancho = FindFirstObjectByType<SanchoPanza>();
        }

        private void Update()
        {
            HandleTimers();
            HandleRandomStopping();
        }

        // ---- Random Stopping ----

        private void HandleRandomStopping()
        {
            if (CurrentState != DonkeyState.Plodding) return;
            if (Random.value < _stoppingChancePerSecond * Time.deltaTime)
                TransitionTo(DonkeyState.Stopping);
        }

        // ---- Timer exits ----

        private void HandleTimers()
        {
            switch (CurrentState)
            {
                case DonkeyState.Stopping:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(DonkeyState.Plodding);
                    break;
                case DonkeyState.Braying:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(DonkeyState.Plodding);
                    break;
                case DonkeyState.Bolting:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) TransitionTo(DonkeyState.Plodding);
                    break;
            }
        }

        // ---- Public entry points ----

        public void OnSanchoMounts()
        {
            if (CurrentState == DonkeyState.Grazing)
                TransitionTo(DonkeyState.Plodding);
        }

        // Called when a nearby WindmillEntity transforms to Dragon
        public void OnDragonNearby()
        {
            if (CurrentState == DonkeyState.Plodding && Random.value < _brayingChance)
                TransitionTo(DonkeyState.Braying);
        }

        // Called when Rocinante also becomes Spooked — extreme chaos event
        public void OnExtremeEvent()
        {
            if (CurrentState == DonkeyState.Plodding || CurrentState == DonkeyState.Braying)
                TransitionTo(DonkeyState.Bolting);
        }

        // Player coaxes Rucio out of Stopping (can be wired to input or a button)
        public void OnCoaxed()
        {
            if (CurrentState == DonkeyState.Stopping)
                TransitionTo(DonkeyState.Plodding);
        }

        // ---- Transition ----

        private void TransitionTo(DonkeyState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;

            switch (newState)
            {
                case DonkeyState.Stopping:
                    _stateTimer = _stoppingCoaxDuration;
                    break;
                case DonkeyState.Braying:
                    _stateTimer = _brayingDuration;
                    break;
                case DonkeyState.Bolting:
                    _stateTimer = _boltingDuration;
                    // Dismount Sancho
                    break;
            }

            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
