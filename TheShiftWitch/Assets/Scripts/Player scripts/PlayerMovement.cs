using UnityEngine;
using Unity.Cinemachine;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssets.StarterAssetsInputs))]
    public class BaseMovementAndCamera : MonoBehaviour
{
    public enum MoveState
    {
        Normal,
        Dashing
    }

    [Header("Movement & Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.12f;
    public float moveSpeed = 5.5f;
    public float sprintSpeed = 8.5f;
    public float speedChangeRate = 12.0f;
    [Tooltip("Optional hook if your combat script wants to freeze walking during swings.")]
    public bool lockMovement;
    public bool lockRotation;

    [Header("Jump & Gravity")]
    public float jumpHeight = 3.0f;
    public float gravity = -35.0f;
    public float fallGravityMultiplier = 2.0f;
    public float jumpTimeout = 0.0f;
    public float fallTimeout = 0.0f;

    [Header("Grounded Check")]
    public bool isGrounded = true;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;

    [Header("Dash")]
    public float dashSpeed = 16.0f;
    public float dashDuration = 0.22f;
    public float dashCooldown = 0.35f;

    [Header("Cinemachine Camera")]
    public CinemachineCamera vCam;
    public Transform cameraTarget;
    public float baseCameraDistance = 4.5f;
    public float cameraTargetHeight = 1.35f;
    public float dashCamDampDuration = 0.28f;

    private MoveState _state = MoveState.Normal;
    private float _speed;
    private float _verticalVelocity;
    private const float TerminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _targetRotation;
    private float _rotationVelocity;

    private float _dashTimer;
    private float _dashCooldownTimer;
    private float _dashCamDampTimer;
    private Vector3 _dashDirection;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;

    private Animator _animator;
    private CharacterController _controller;
    private StarterAssets.StarterAssetsInputs _input;
    private Camera _mainCam;
    private bool _hasAnimator;

    private void Awake() {
        _mainCam = Camera.main;
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssets.StarterAssetsInputs>();
        _hasAnimator = TryGetComponent(out _animator);
    }

    private void Start() {
        if (_mainCam == null) _mainCam = Camera.main;

        if (cameraTarget != null && cameraTarget.transform.parent != null) {
            cameraTarget.transform.SetParent(null);
        }

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");

        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;
    }

    private void Update() {
        if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;

        if (!lockMovement) {
            if (_state == MoveState.Normal) {
                Move();
                JumpAndGravity();
                CheckDash();
            }
            else if (_state == MoveState.Dashing) {
                DashStep();
            }
        }

        GroundedCheck();
        UpdateCamera();
    }

    private void Move() {
        if (_controller == null || !_controller.enabled || _input == null) return;

        float targetSpeed = _input.move == Vector2.zero
            ? 0.0f
            : (_input.sprint ? sprintSpeed : moveSpeed);

        float currentHorizSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
        float inputMag = _input.analogMovement ? _input.move.magnitude : 1.0f;
        _speed = Mathf.Lerp(currentHorizSpeed, targetSpeed * inputMag, Time.deltaTime * speedChangeRate);

        Vector3 camFwd = _mainCam != null
            ? Vector3.ProjectOnPlane(_mainCam.transform.forward, Vector3.up).normalized
            : Vector3.forward;
        Vector3 camRight = _mainCam != null
            ? Vector3.ProjectOnPlane(_mainCam.transform.right, Vector3.up).normalized
            : Vector3.right;

        if (!lockRotation) {
            if (_input.move != Vector2.zero) {
                Vector3 inputDir = new Vector3(_input.move.x, 0f, _input.move.y).normalized;
                float camYaw = _mainCam != null ? _mainCam.transform.eulerAngles.y : transform.eulerAngles.y;
                _targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + camYaw;
            }

            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        Vector3 moveDir = _input.move != Vector2.zero
            ? (Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward)
            : Vector3.zero;

        _controller.Move(moveDir * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));

        if (_hasAnimator) {
            _animator.SetFloat(_animIDSpeed, _speed);
        }
    }

    private void JumpAndGravity() {
        if (_input == null) return;

        if (isGrounded) {
            _fallTimeoutDelta = fallTimeout;
            if (_verticalVelocity < 0f) _verticalVelocity = -2f;

            if (_input.jump && _jumpTimeoutDelta <= 0f) {
                isGrounded = false;
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (_hasAnimator) {
                    _animator.SetBool(_animIDGrounded, false);
                    _animator.ResetTrigger(_animIDJump);
                    _animator.SetTrigger(_animIDJump);
                }
            }

            if (_jumpTimeoutDelta >= 0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else {
            _jumpTimeoutDelta = jumpTimeout;
            if (_fallTimeoutDelta >= 0f) _fallTimeoutDelta -= Time.deltaTime;
            _input.jump = false;
        }

        if (_verticalVelocity < TerminalVelocity) {
            float mult = (_verticalVelocity < 0f) ? fallGravityMultiplier : 1.0f;
            _verticalVelocity += gravity * Time.deltaTime * mult;
        }
    }

    private void CheckDash() {
        if (_input == null) return;

        bool dashPressed = _input.sprint && _input.move != Vector2.zero;
        if (dashPressed && _dashCooldownTimer <= 0f) {
            StartDash();
        }
    }

    private void StartDash() {
        _state = MoveState.Dashing;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        _dashCamDampTimer = dashCamDampDuration;

        Vector3 camFwd = _mainCam != null
            ? Vector3.ProjectOnPlane(_mainCam.transform.forward, Vector3.up).normalized
            : Vector3.forward;
        Vector3 camRight = _mainCam != null
            ? Vector3.ProjectOnPlane(_mainCam.transform.right, Vector3.up).normalized
            : Vector3.right;

        Vector2 move = _input != null ? _input.move : Vector2.zero;
        Vector3 dir = (camRight * move.x + camFwd * move.y).normalized;
        _dashDirection = dir != Vector3.zero ? dir : transform.forward;

        Vector3 flatDir = new Vector3(_dashDirection.x, 0f, _dashDirection.z);
        if (flatDir.sqrMagnitude > 0.0001f) {
            transform.rotation = Quaternion.LookRotation(flatDir.normalized);
        }
    }

    private void DashStep() {
        _dashTimer -= Time.deltaTime;

        Vector3 vel = _dashDirection * dashSpeed;
        if (!isGrounded) vel.y = _verticalVelocity;

        if (_controller != null && _controller.enabled) {
            _controller.Move(vel * Time.deltaTime);
        }

        if (_dashTimer <= 0f) {
            _state = MoveState.Normal;
        }
    }

    private void GroundedCheck() {
        Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (_hasAnimator) {
            _animator.SetBool(_animIDGrounded, isGrounded);
        }
    }

    private void UpdateCamera() {
        if (vCam == null || cameraTarget == null) return;

        var orbital = vCam.GetComponent<CinemachineOrbitalFollow>();
        if (orbital == null) return;

        if (vCam.Follow != cameraTarget) vCam.Follow = cameraTarget;
        if (vCam.LookAt != cameraTarget) vCam.LookAt = cameraTarget;

        if (_dashCamDampTimer > 0f) {
            _dashCamDampTimer -= Time.deltaTime;
            orbital.TrackerSettings.PositionDamping = new Vector3(0.35f, 0.25f, 0.35f);
        }
        else {
            orbital.TrackerSettings.PositionDamping = new Vector3(0.08f, 0.08f, 0.08f);
        }

        Vector3 desiredFocus = transform.position + Vector3.up * cameraTargetHeight;
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, desiredFocus, Time.deltaTime * 14f);
        orbital.Radius = Mathf.Lerp(orbital.Radius, baseCameraDistance, Time.deltaTime * 8f);
    }
}