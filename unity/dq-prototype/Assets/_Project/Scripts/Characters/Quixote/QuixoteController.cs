using UnityEngine;

namespace DonQuixote
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class QuixoteController : MonoBehaviour
    {
        // --- Action State Machine ---
        public enum ActionState
        {
            Idle, Walking, Riding, Charging, Attacking,
            Stunned, KnockedDown, Recovering
        }

        // --- Perception State Machine ---
        public enum PerceptionState { Delusion, Lucidity }

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _chargeSpeed = 10f;

        [Header("Timers")]
        [SerializeField] private float _stunnedDuration = 1.5f;
        [SerializeField] private float _knockedDownDuration = 2f;
        [SerializeField] private float _recoveringDuration = 1f;

        public ActionState CurrentActionState { get; private set; } = ActionState.Idle;
        public PerceptionState CurrentPerceptionState { get; private set; } = PerceptionState.Delusion;

        public event System.Action<ActionState> OnActionStateChanged;
        public event System.Action<PerceptionState> OnPerceptionStateChanged;

        private Rigidbody2D _rb;
        private float _stateTimer;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            HandleActionTimers();
            HandleMovementInput();
        }

        // ---- Input ----

        private void HandleMovementInput()
        {
            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = 0f;

            switch (CurrentActionState)
            {
                case ActionState.Idle:
                    if (_moveInput.x != 0f) SetActionState(ActionState.Walking);
                    break;
                case ActionState.Walking:
                    _rb.linearVelocity = new Vector2(_moveInput.x * _walkSpeed, _rb.linearVelocity.y);
                    if (_moveInput.x == 0f) SetActionState(ActionState.Idle);
                    break;
                case ActionState.Riding:
                    _rb.linearVelocity = new Vector2(_moveInput.x * _walkSpeed * 1.5f, _rb.linearVelocity.y);
                    if (Input.GetButtonDown("Jump")) SetActionState(ActionState.Charging);
                    break;
                case ActionState.Charging:
                    _rb.linearVelocity = new Vector2(transform.localScale.x > 0 ? _chargeSpeed : -_chargeSpeed, _rb.linearVelocity.y);
                    break;
            }
        }

        // ---- Timer-driven state exits ----

        private void HandleActionTimers()
        {
            switch (CurrentActionState)
            {
                case ActionState.Stunned:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) SetActionState(ActionState.Recovering);
                    break;
                case ActionState.KnockedDown:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) SetActionState(ActionState.Recovering);
                    break;
                case ActionState.Recovering:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) SetActionState(ActionState.Idle);
                    break;
            }
        }

        // ---- Public transition entry points (called by other systems/entities) ----

        public void Mount(Rocinante rocinante)
        {
            if (CurrentActionState == ActionState.Idle || CurrentActionState == ActionState.Walking)
                SetActionState(ActionState.Riding);
        }

        public void TriggerStunned()
        {
            SetActionState(ActionState.Stunned);
        }

        public void TriggerKnockedDown()
        {
            SetActionState(ActionState.KnockedDown);
        }

        public void TriggerAttack()
        {
            if (CurrentActionState == ActionState.Charging || CurrentActionState == ActionState.Riding)
                SetActionState(ActionState.Attacking);
        }

        // Called by SanchoPanza behaviour machine or by LuciditySystem
        public void TriggerLucidity()
        {
            if (CurrentPerceptionState == PerceptionState.Delusion)
                SetPerceptionState(PerceptionState.Lucidity);
        }

        public void EndLucidity()
        {
            if (CurrentPerceptionState == PerceptionState.Lucidity)
                SetPerceptionState(PerceptionState.Delusion);
        }

        // ---- State setters ----

        private void SetActionState(ActionState newState)
        {
            if (CurrentActionState == newState) return;
            CurrentActionState = newState;

            switch (newState)
            {
                case ActionState.Stunned:
                    _stateTimer = _stunnedDuration;
                    _rb.linearVelocity = Vector2.zero;
                    SetPerceptionState(PerceptionState.Lucidity);
                    break;
                case ActionState.KnockedDown:
                    _stateTimer = _knockedDownDuration;
                    _rb.linearVelocity = Vector2.zero;
                    SetPerceptionState(PerceptionState.Lucidity);
                    break;
                case ActionState.Recovering:
                    _stateTimer = _recoveringDuration;
                    break;
                case ActionState.Idle:
                case ActionState.Walking:
                    break;
            }

            OnActionStateChanged?.Invoke(CurrentActionState);
        }

        private void SetPerceptionState(PerceptionState newState)
        {
            if (CurrentPerceptionState == newState) return;
            CurrentPerceptionState = newState;

            if (newState == PerceptionState.Lucidity)
                LuciditySystem.Instance?.TriggerLucidity();

            OnPerceptionStateChanged?.Invoke(CurrentPerceptionState);
        }

        // LuciditySystem calls back here when timer expires
        private void OnEnable()
        {
            if (LuciditySystem.Instance != null)
                LuciditySystem.Instance.OnLucidityEnded += EndLucidity;
        }

        private void OnDisable()
        {
            if (LuciditySystem.Instance != null)
                LuciditySystem.Instance.OnLucidityEnded -= EndLucidity;
        }
    }
}
