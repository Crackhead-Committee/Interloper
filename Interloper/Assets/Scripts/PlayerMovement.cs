using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // --- Enums ---
    public enum SurfaceType { Forest, Metal, Wood, Unknown }
    public enum StaminaDisplay { ValueSlashMax, Percent, RawValueOneDecimal }

    // --- Serialized Fields ---
    [Header("Movement")]
    [FormerlySerializedAs("moveSpeed")] public float WalkSpeed = 5f;
    [FormerlySerializedAs("airControl")] [Range(0f, 1f)] public float AirControlFactor = 0.1f;

    [Header("Sprint")]
    public float SprintSpeed = 8f;
    public float MaxStamina = 5f;
    public float StaminaDrainPerSec = 1.5f;
    public float StaminaRegenPerSec = 1.0f;
    public float StaminaRegenDelay = 0.25f;
    [Range(0f,1f)] public float MinStaminaToSprint = 0f;
    public TMP_Text staminaText;
    public StaminaDisplay staminaDisplay = StaminaDisplay.ValueSlashMax;

    [Header("Jump")]
    public float JumpHeight = 1.0f;
    public float RiseGravity = -20f;
    public float FallMultiplier = 2.2f;
    public float CoyoteTime = 0.10f;
    public float JumpBuffer = 0.10f;
    public float JumpCooldown = 2.0f;
    public float JumpCost = 0.5f;

    [Header("Mouse Look")]
    public float MouseSensitivity = 1.0f;
    public Transform PlayerCamera;

    [Header("Ground Check")]
    public Transform GroundCheck;
    public float GroundRadius = 0.25f;
    public LayerMask GroundMask = ~0;

    [Header("Ground Info")]
    public float GroundProbeDistance = 0.75f;
    Vector3 _groundNormal = Vector3.up;

    [Header("Audio")]
    public AudioSource SfxSource;
    public AudioSource BreathSource;
    public AudioClip LandingClip;
    public AudioClip BreathLoop;

    [Header("Surface Tags (non-terrain)")]
    public string TagForest = "GroundForest";
    public string TagMetal  = "GroundMetal";
    public string TagWood   = "GroundWood";

    [Header("Per-Surface Footsteps (used for walk & sprint)")]
    public AudioClip[] FootstepsForest;
    public AudioClip[] FootstepsMetal;
    public AudioClip[] FootstepsWood;

    [Header("Per-Surface Landing Clips")]
    public AudioClip LandingForest;
    public AudioClip LandingMetal;
    public AudioClip LandingWood;

    // (Audio Settings)
    [Range(0f,1f)] public float FootstepVolume = 0.55f;
    [Range(0f,1f)] public float LandingVolume = 0.7f;
    [Range(0f,1f)] public float BreathVolume = 0.6f;
    public float StepIntervalWalk = 0.5f;
    public float StepIntervalSprint = 0.35f;
    public float MinStepSpeed = 1.0f;
    [Range(0f,0.2f)] public float FootstepPitchJitter = 0.06f;
    public float BreathFadeSpeed = 5f;
    
    [Header("Head Bob")]
    public bool EnableHeadBob = true;
    public float BobFrequencyWalk = 1.8f;
    public float BobFrequencySprint = 2.6f;
    public float BobAmplitudeWalk = 0.03f;
    public float BobAmplitudeSprint = 0.06f;
    public float BobSwayAmplitude = 0.02f;
    public float BobSmooth = 10f;

    [Header("Landing Dip")]
    public bool EnableLandingDip = true;
    public float LandMinSpeed = 3f;
    public float LandDipAmount = 0.06f;
    public float LandDipTime = 0.10f;
    public float LandRecoverTime = 0.12f;

    // --- Public Methods ---
    public bool IsSprinting() => _isSprinting;
    
    // --- Private Fields ---
    // --- privates ---
    Rigidbody _rb;
    Vector3 _moveDir;
    float _xRotation;

    InputAction _moveAction, _lookAction, _jumpAction, _sprintAction;

    Vector3 _camDefaultLocalPos;
    float _bobTimer;

    bool _wasMovingForSteps;

    bool _isGrounded;
    float _lastGroundedTime;
    float _lastJumpPressedTime;
    float _lastJumpTime = -999f;
    bool _jumping;

    float _prevYVel;
    bool _wasGrounded;
    float _landingOffsetY;
    Coroutine _landingCR;

    float _stepTimer;

    float _stamina;
    bool _sprintHeld;
    bool _isSprinting;
    float _lastSprintEndTime;

    Collider[] _groundHits = new Collider[8];
    CapsuleCollider _capsule;

    // --- Unity Methods ---
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.useGravity = false;
        Cursor.lockState = CursorLockMode.Locked;

        _stamina = MaxStamina;
        _capsule = GetComponent<CapsuleCollider>();

        // input
        _moveAction = new InputAction("Move");
        var wasd = _moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w");
        wasd.With("Down", "<Keyboard>/s");
        wasd.With("Left", "<Keyboard>/a");
        wasd.With("Right", "<Keyboard>/d");

        _lookAction = new InputAction("Look");
        _lookAction.AddBinding("<Mouse>/delta");

        _jumpAction = new InputAction("Jump");
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.performed += _ => _lastJumpPressedTime = Time.time;

        _sprintAction = new InputAction("Sprint");
        _sprintAction.AddBinding("<Keyboard>/leftShift");
        _sprintAction.performed += _ => _sprintHeld = true;
        _sprintAction.canceled  += _ => { _sprintHeld = false; StopSprint(); };

        if (PlayerCamera) _camDefaultLocalPos = PlayerCamera.localPosition;
    }

    void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
    }

    void FixedUpdate()
    {
        Vector3 v = _rb.linearVelocity;

        float speed = _isSprinting ? SprintSpeed : WalkSpeed;

        Vector3 moveAlongGround = _moveDir;
        if (_isGrounded)
            moveAlongGround = Vector3.ProjectOnPlane(_moveDir, _groundNormal).normalized;

        Vector3 desiredXZ = moveAlongGround * speed;
        float control = _isGrounded ? 1f : AirControlFactor;
        Vector3 xz = Vector3.Lerp(new Vector3(v.x, 0, v.z), desiredXZ, control);

        float g = RiseGravity;
        if (v.y <= 0.001f) g *= FallMultiplier;
        v.x = xz.x;
        v.z = xz.z;
        v.y += g * Time.fixedDeltaTime;

        if (_isGrounded && v.y < 0f) v.y = -2f;

        _rb.linearVelocity = v;

        if (_isGrounded && _jumping && (Time.time - _lastJumpTime) > 0.05f)
            _jumping = false;

        _prevYVel = _rb.linearVelocity.y;
    }

    void Update()
    {
        Vector2 look = _lookAction.ReadValue<Vector2>() * MouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * look.x);
        _xRotation = Mathf.Clamp(_xRotation - look.y, -90f, 90f);
        if (PlayerCamera) PlayerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        Vector2 move = _moveAction.ReadValue<Vector2>();
        _moveDir = (transform.right * move.x + transform.forward * move.y).normalized;

        _isGrounded = IsGrounded();
        UpdateGroundInfo();

        bool canCoyote = (Time.time - _lastGroundedTime) <= CoyoteTime;
        bool buffered  = (Time.time - _lastJumpPressedTime) <= JumpBuffer;
        bool cooled    = (Time.time - _lastJumpTime) >= JumpCooldown;
        if (!_jumping && buffered && (_isGrounded || canCoyote) && cooled)
            StartJump();

        float inputMag = move.magnitude;
        bool wantsSprint = _sprintHeld && _isGrounded && inputMag > 0.1f;

        if (_isSprinting)
        {
            _stamina -= StaminaDrainPerSec * Time.deltaTime;
            if (_stamina <= 0f || !wantsSprint)
                StopSprint();
        }
        else
        {
            float startThreshold = MinStaminaToSprint * MaxStamina;
            if (wantsSprint && _stamina > startThreshold)
                _isSprinting = true;

            if (Time.time - _lastSprintEndTime >= StaminaRegenDelay)
                _stamina += StaminaRegenPerSec * Time.deltaTime;
        }

        _stamina = Mathf.Clamp(_stamina, 0f, MaxStamina);

        if (staminaText)
        {
            int percent = Mathf.RoundToInt((_stamina / Mathf.Max(0.0001f, MaxStamina)) * 100f);
            staminaText.text = percent + "";
        }

        if (EnableHeadBob && PlayerCamera)
            HeadBobUpdate();

        bool justLanded = !_wasGrounded && _isGrounded;
        float impactSpeed = -_prevYVel;

        float airTime = Time.time - _lastGroundedTime;

        if (justLanded && impactSpeed >= LandMinSpeed && airTime > 0.1f)
        {
            PlayLanding(impactSpeed);
            if (EnableLandingDip)
            {
                if (_landingCR != null) StopCoroutine(_landingCR);
                _landingCR = StartCoroutine(LandingDipRoutine(LandDipAmount));
            }
            ApplyFallDamage(impactSpeed);
        }
        
        if (_isGrounded) _lastGroundedTime = Time.time;
        _wasGrounded = _isGrounded;

        Vector3 planar = Vector3.ProjectOnPlane(_rb.linearVelocity, _groundNormal);
        float horizSpeed = planar.magnitude;
        UpdateFootstepsAndBreath(horizSpeed);
    }
    
    void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
    }
    
    void OnDrawGizmosSelected()
    {
        if (GroundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GroundCheck.position, GroundRadius);
        }
    }

    // --- Movement Helpers ---

    void StartJump()
    {
        float upVel = Mathf.Sqrt(2f * Mathf.Abs(RiseGravity) * Mathf.Max(0.01f, JumpHeight));
        Vector3 v = _rb.linearVelocity;
        v.y = upVel;
        _rb.linearVelocity = v;

        _jumping = true;
        _lastJumpTime = Time.time;

        _stamina = Mathf.Max(0f, _stamina - JumpCost);
    }
    
    void StopSprint()
    {
        if (_isSprinting)
        {
            _isSprinting = false;
            _lastSprintEndTime = Time.time;
        }
    }

    // --- Ground & Physics Helpers ---
    
    bool IsGrounded()
    {
        Vector3 pos = GroundCheck
            ? GroundCheck.position
            : (transform.position + Vector3.down * ((_capsule ? (_capsule.height * 0.5f - _capsule.radius) : 0.6f) + 0.02f));

        int count = Physics.OverlapSphereNonAlloc(pos, GroundRadius, _groundHits, GroundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            var c = _groundHits[i];
            if (!c) continue;
            if (c.transform.IsChildOf(transform)) continue;
            return true;
        }
        return false;
    }
    
    void UpdateGroundInfo()
    {
        Vector3 origin = GroundCheck ? GroundCheck.position + Vector3.up * 0.05f
                                       : transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out var hit, GroundProbeDistance, GroundMask, QueryTriggerInteraction.Ignore))
            _groundNormal = hit.normal;
        else
            _groundNormal = Vector3.up;
    }
    
    void ApplyFallDamage(float impactSpeed)
    {
        const float SAFE_SPEED = 20f;    // No damage below this
        const float LETHAL_SPEED = 30f;  // Auto death speed
        float damage = 0f;

        if (impactSpeed < SAFE_SPEED) 
            return;
        else if (impactSpeed >= LETHAL_SPEED)
            damage = 100f;   // full HP
        else
        {
            float t = (impactSpeed - SAFE_SPEED) / (LETHAL_SPEED - SAFE_SPEED);
            damage = t * t * 100f;   // quadratic scaling = smoother
        }

        // Optional surface scaling → softer forest, harder metal
        var surface = GetSurfaceUnderfoot();
        switch (surface)
        {
            case SurfaceType.Forest: damage *= 0.8f; break;
            case SurfaceType.Metal:  damage *= 1.1f; break;
            case SurfaceType.Wood:   damage *= 1.0f; break;
            default:                 damage *= 1.0f; break;
        }

        // Deal damage
        var playerLife = GetComponent<PlayerLife>();
        if (playerLife != null)
            playerLife.ApplyDamage(damage);
    }

    // --- Audio Helpers ---

    void UpdateFootstepsAndBreath(float horizSpeed)
    {
        bool canCoyote = (Time.time - _lastGroundedTime) <= CoyoteTime;
        bool functionallyGrounded = _isGrounded || canCoyote;
        bool movingOnGround = functionallyGrounded && horizSpeed > MinStepSpeed && _moveDir.sqrMagnitude > 0.01f;

        if (!movingOnGround) _stepTimer = 0f;
            else
            {
                float targetInterval = _isSprinting ? StepIntervalSprint : StepIntervalWalk;
                
                float interval = targetInterval; 

                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    PlayFootstep();
                    _stepTimer = interval; 
                }
            }

        if (BreathSource && BreathLoop)
        {
            if (!BreathSource.isPlaying)
            {
                BreathSource.clip = BreathLoop;
                BreathSource.loop = true;
                BreathSource.volume = 0f;
                BreathSource.Play();
            }
            float targetVol = (_isSprinting && _isGrounded) ? BreathVolume : 0f;
            BreathSource.volume = Mathf.MoveTowards(BreathSource.volume, targetVol, BreathFadeSpeed * Time.deltaTime);
        }
    }

    void PlayFootstep()
    {
        if (!SfxSource) return;

        SurfaceType s = GetSurfaceUnderfoot();
        AudioClip[] set = GetFootstepSet(s);

        if (set == null || set.Length == 0) return;

        AudioClip clip = set[Random.Range(0, set.Length)];
        float pitch = 1f + Random.Range(-FootstepPitchJitter, FootstepPitchJitter);

        SfxSource.pitch = pitch;
        SfxSource.PlayOneShot(clip, FootstepVolume);
    }
    
    void PlayLanding(float impactSpeed)
    {
        if (!SfxSource) return;

        SurfaceType s = GetSurfaceUnderfoot();
        AudioClip clip = GetLandingClip(s);
        if (!clip) return;

        float scale = Mathf.InverseLerp(1f, 8f, impactSpeed);
        float vol = LandingVolume * Mathf.Lerp(0.6f, 1f, scale);

        SfxSource.pitch = 1f;
        SfxSource.PlayOneShot(clip, vol);
    }
    
    SurfaceType GetSurfaceUnderfoot()
    {
        Vector3 origin = GroundCheck ? GroundCheck.position + Vector3.up * 0.05f
                                       : transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, GroundProbeDistance, GroundMask, QueryTriggerInteraction.Ignore))
        {
            var t = hit.collider.tag;
            if (t == TagForest) return SurfaceType.Forest;
            if (t == TagMetal)  return SurfaceType.Metal;
            if (t == TagWood)   return SurfaceType.Wood;
        }
        return SurfaceType.Unknown;
    }

    AudioClip[] GetFootstepSet(SurfaceType s)
    {
        switch (s)
        {
            case SurfaceType.Metal:  return FootstepsMetal;
            case SurfaceType.Wood:   return FootstepsWood;
            case SurfaceType.Forest: return FootstepsForest;
            default:                 return FootstepsForest;
        }
    }

    AudioClip GetLandingClip(SurfaceType s)
    {
        switch (s)
        {
            case SurfaceType.Metal:  return LandingMetal ? LandingMetal : LandingClip;
            case SurfaceType.Wood:   return LandingWood  ? LandingWood  : LandingClip;
            case SurfaceType.Forest: return LandingForest? LandingForest: LandingClip;
            default:                 return LandingClip;
        }
    }
    
    // --- Camera Effect Helpers ---

    void HeadBobUpdate()
    {
        Vector3 vel = _rb.linearVelocity;
        float horizSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
        bool moving = _isGrounded && horizSpeed > 0.15f && _moveDir.sqrMagnitude > 0.01f;

        float maxSpeed = Mathf.Max(WalkSpeed, SprintSpeed);
        float t = Mathf.InverseLerp(0f, Mathf.Max(0.01f, maxSpeed), horizSpeed);
        float freq = Mathf.Lerp(BobFrequencyWalk, BobFrequencySprint, t);
        float ampY = Mathf.Lerp(BobAmplitudeWalk, BobAmplitudeSprint, t);
        float ampX = Mathf.Lerp(BobSwayAmplitude * 0.6f, BobSwayAmplitude, t);

        Vector3 offset = Vector3.zero;
        if (moving)
        {
            _bobTimer += Time.deltaTime * freq * 2f * Mathf.PI;
            float sin = Mathf.Sin(_bobTimer);
            float cos = Mathf.Cos(_bobTimer);
            offset.x = cos * ampX;
            offset.y = -Mathf.Abs(sin) * ampY;
        }
        else
        {
            _bobTimer = Mathf.Lerp(_bobTimer, 0f, Time.deltaTime * BobSmooth);
        }

        Vector3 desired = _camDefaultLocalPos + offset + new Vector3(0f, _landingOffsetY, 0f);
        PlayerCamera.localPosition = Vector3.Lerp(PlayerCamera.localPosition, desired, BobSmooth * Time.deltaTime);
    }
    
    // --- Coroutines ---
    
    IEnumerator LandingDipRoutine(float amount)
    {
        float t = 0f;
        while (t < LandDipTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / LandDipTime);
            _landingOffsetY = -Mathf.SmoothStep(0f, amount, p);
            yield return null;
        }

        t = 0f;
        while (t < LandRecoverTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / LandRecoverTime);
            _landingOffsetY = -Mathf.Lerp(amount, 0f, Mathf.SmoothStep(0f, 1f, p));
            yield return null;
        }

        _landingOffsetY = 0f;
        _landingCR = null;
    }
}