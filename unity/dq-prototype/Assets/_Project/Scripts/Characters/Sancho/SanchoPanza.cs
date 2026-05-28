using UnityEngine;

namespace DonQuixote
{
    public class SanchoPanza : MonoBehaviour
    {
        // ---- Action State Machine ----
        public enum ActionState { Idle, Walking, Riding, Running, Cowering, Helping }

        // ---- Behaviour State Machine ----
        public enum BehaviourState { Idle, Speaking, Worried }

        [Header("Follow")]
        [SerializeField] private float _followDistance = 2.5f;
        [SerializeField] private float _runTriggerDistance = 5f;
        [SerializeField] private float _speakingSuppressionDistance = 8f;

        [Header("Debug")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Timers")]
        [SerializeField] private float _speakingDuration = 2f;
        [SerializeField] private float _speakingCooldownMin = 5f;
        [SerializeField] private float _speakingCooldownMax = 12f;
        [SerializeField] private float _cowerDuration = 3f;

        public ActionState CurrentActionState { get; private set; } = ActionState.Idle;
        public BehaviourState CurrentBehaviourState { get; private set; } = BehaviourState.Idle;

        public event System.Action<ActionState> OnActionStateChanged;
        public event System.Action<BehaviourState> OnBehaviourStateChanged;

        private QuixoteController _quixote;
        private float _behaviourTimer;
        private float _cowerTimer;
        private bool _speakingCooldownActive;

        private void Start()
        {
            _quixote = FindFirstObjectByType<QuixoteController>();

            if (_quixote != null)
                _quixote.OnActionStateChanged += HandleQuixoteActionChanged;

            ScheduleNextSpeakingWindow();
        }

        private void OnDestroy()
        {
            if (_quixote != null)
                _quixote.OnActionStateChanged -= HandleQuixoteActionChanged;
        }

        private void Update()
        {
            UpdateActionState();
            UpdateBehaviourTimer();
            UpdateCowerTimer();
        }

        // ---- Action state logic ----

        private void UpdateActionState()
        {
            if (_quixote == null) return;
            if (CurrentActionState == ActionState.Helping) return;
            if (CurrentActionState == ActionState.Cowering) return;

            float dist = Vector2.Distance(transform.position, _quixote.transform.position);

            if (dist > _runTriggerDistance)
                SetActionState(ActionState.Running);
            else if (dist > _followDistance)
                SetActionState(ActionState.Walking);
            else
                SetActionState(ActionState.Idle);
        }

        private void UpdateCowerTimer()
        {
            if (CurrentActionState != ActionState.Cowering) return;
            _cowerTimer -= Time.deltaTime;
            if (_cowerTimer <= 0f)
                SetActionState(ActionState.Walking);
        }

        public void TriggerCower()
        {
            _cowerTimer = _cowerDuration;
            SetActionState(ActionState.Cowering);
        }

        private void HandleQuixoteActionChanged(QuixoteController.ActionState actionState)
        {
            if (actionState == QuixoteController.ActionState.KnockedDown)
            {
                SetActionState(ActionState.Helping);
                SetBehaviourState(BehaviourState.Worried);
            }
            else if (actionState == QuixoteController.ActionState.Idle &&
                     CurrentActionState == ActionState.Helping)
            {
                SetActionState(ActionState.Idle);
                SetBehaviourState(BehaviourState.Idle);
            }
        }

        // ---- Behaviour state logic ----

        private void UpdateBehaviourTimer()
        {
            if (CurrentBehaviourState == BehaviourState.Speaking)
            {
                _behaviourTimer -= Time.deltaTime;
                if (_behaviourTimer <= 0f)
                {
                    SetBehaviourState(BehaviourState.Idle);
                    ScheduleNextSpeakingWindow();
                }
                return;
            }

            if (_speakingCooldownActive)
            {
                _behaviourTimer -= Time.deltaTime;
                if (_behaviourTimer <= 0f)
                    _speakingCooldownActive = false;
                return;
            }

            // Attempt to speak — suppressed if cowering or too far from Quixote
            if (CurrentActionState == ActionState.Cowering) return;
            if (_quixote != null &&
                Vector2.Distance(transform.position, _quixote.transform.position) > _speakingSuppressionDistance)
                return;

            SetBehaviourState(BehaviourState.Speaking);
        }

        private void ScheduleNextSpeakingWindow()
        {
            _behaviourTimer = Random.Range(_speakingCooldownMin, _speakingCooldownMax);
            _speakingCooldownActive = true;
        }

        // ---- State setters ----

        private void SetActionState(ActionState newState)
        {
            if (CurrentActionState == newState) return;
            CurrentActionState = newState;
            ApplyDebugTint();
            OnActionStateChanged?.Invoke(CurrentActionState);
        }

        private void SetBehaviourState(BehaviourState newState)
        {
            if (CurrentBehaviourState == newState) return;
            CurrentBehaviourState = newState;

            if (newState == BehaviourState.Speaking)
            {
                _behaviourTimer = _speakingDuration;
                _quixote?.TriggerLucidity();
            }

            ApplyDebugTint();
            OnBehaviourStateChanged?.Invoke(CurrentBehaviourState);
        }

        private void ApplyDebugTint()
        {
            if (_spriteRenderer == null) return;

            Color baseColor = CurrentActionState switch
            {
                ActionState.Idle     => new Color(0.2f, 0.75f, 0.3f),
                ActionState.Walking  => new Color(0.3f, 0.85f, 0.35f),
                ActionState.Riding   => new Color(0.1f, 0.6f, 0.2f),
                ActionState.Running  => new Color(0.6f, 0.9f, 0.1f),
                ActionState.Cowering => new Color(0.3f, 0.5f, 0.1f),
                ActionState.Helping  => new Color(0.1f, 1f, 0.5f),
                _                    => Color.green
            };

            // Behaviour overlay: Speaking = flash yellow, Worried = shift orange
            baseColor = CurrentBehaviourState switch
            {
                BehaviourState.Speaking => Color.Lerp(baseColor, new Color(1f, 0.95f, 0.1f), 0.6f),
                BehaviourState.Worried  => Color.Lerp(baseColor, new Color(1f, 0.4f, 0.1f), 0.5f),
                _                       => baseColor
            };

            _spriteRenderer.color = baseColor;
        }
    }
}
