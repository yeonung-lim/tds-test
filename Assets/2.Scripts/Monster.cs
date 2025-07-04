using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class Monster : MonoBehaviour
{
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");

    public enum State { Walking, Climbing, Attacking, Dead, PushedBack }
    public State currentState = State.Walking;
    public static System.Action<Vector3, int> OnDamageDealt;

    [Header("Movement")]
    public float walkSpeed = 1f;
    public float climbSpeed = 2f;

    [Header("Climb")]
    public float climbYOffset = 0.15f;
    public float climbCheckCooldown = 0.5f;
    public float climbRayLength = 0.5f;

    [Header("Health")]
    public int initialHP = 3;

    private LayerMask groundMask;
    private LayerMask monsterMask;
    private LayerMask groundOrMonsterMask;
    private float _climbCheckTimer = 0f;
    private float _originalGravityScale;
    private Rigidbody2D _rb;
    private Animator _animator;
    private Monster _climbBase;
    private int _currentHp;
    private Collider2D _selfCollider;

    private readonly Vector2 _left = Vector2.left;
    private readonly Vector2 _pushOffset = new Vector2(1f, 0f);

    private const float ClimbVerticalThreshold = 0.01f;
    private const float ClimbHorizontalThreshold = 0.01f;
    private const int Damage = 1;
    private const float AttackBoxSize = 1f;

    private LayerMask TruckMask;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _selfCollider = GetComponent<Collider2D>();
        _originalGravityScale = _rb.gravityScale;
        TruckMask = LayerMask.GetMask("Truck");
    }

    private void OnEnable()
    {
        ResetState();
    }

    public void ResetState()
    {
        currentState = State.Walking;
        _rb.gravityScale = _originalGravityScale;
        _rb.velocity = Vector2.zero;
        _climbBase = null;
        _currentHp = initialHP;
        _climbCheckTimer = 0f;
        enabled = true;
    }

    private void Update()
    {
        if (currentState == State.Dead) return;

        _animator.SetBool(IsIdle, currentState != State.Dead);
        _animator.SetBool(IsAttacking, currentState == State.Attacking);

        _rb.gravityScale = (currentState == State.Climbing) ? 0f : _originalGravityScale;

        if (currentState == State.Walking)
        {
            _climbCheckTimer -= Time.deltaTime;
            if (_climbCheckTimer <= 0f)
            {
                Monster candidate = RaycastClimbTarget();
                if (IsValidClimbCandidate(candidate))
                {
                    StartClimbing(candidate);
                }
                _climbCheckTimer = climbCheckCooldown;
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentState == State.Dead || currentState == State.PushedBack) return;

        switch (currentState)
        {
            case State.Walking:
                if (IsTouchingTruck())
                {
                    _rb.velocity = Vector2.zero;
                    currentState = State.Attacking;
                }
                else
                {
                    _rb.velocity = _left * walkSpeed;
                }
                break;

            case State.Climbing:
                HandleClimbMovement();
                break;

            case State.Attacking:
                _rb.velocity = Vector2.zero;
                if (!IsTouchingTruck())
                {
                    currentState = State.Walking;
                }
                break;
        }
    }

    private void HandleClimbMovement()
    {
        if (_climbBase == null || !IsValidClimbTargetForClimbing(_climbBase))
        {
            currentState = State.Walking;
            return;
        }

        Vector3 targetPos = _climbBase.GetClimbTopPosition();
        targetPos.y += climbYOffset;

        Vector3 currentPos = transform.position;
        Vector3 newPos = currentPos;

        bool moved = false;

        if (currentPos.y < targetPos.y - ClimbVerticalThreshold)
        {
            newPos.y = Mathf.MoveTowards(currentPos.y, targetPos.y, climbSpeed * Time.fixedDeltaTime);
            moved = true;
        }
        else if (currentPos.x > targetPos.x + ClimbHorizontalThreshold)
        {
            newPos.x = Mathf.MoveTowards(currentPos.x, targetPos.x, climbSpeed * Time.fixedDeltaTime);
            moved = true;
        }

        if (moved)
        {
            _rb.MovePosition(newPos);
        }
        else
        {
            _climbBase.EnterPushedBackStateSmooth(_pushOffset);
            currentState = State.Walking;
            _climbBase = null;
        }
    }

    private void StartClimbing(Monster target)
    {
        _rb.velocity = Vector2.zero;
        _climbBase = target;
        currentState = State.Climbing;
    }

    private bool IsTouchingTruck()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(TruckMask);
        filter.useTriggers = false;

        Collider2D[] results = new Collider2D[1];
        return _selfCollider.OverlapCollider(filter, results) > 0;
    }

    private Monster RaycastClimbTarget()
    {
        Vector2 origin = new Vector2(_selfCollider.bounds.min.x, _selfCollider.bounds.center.y);
        origin.x -= 0.01f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.left, climbRayLength, monsterMask);

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.left * climbRayLength, hit.collider ? Color.green : Color.red, 0.1f);
#endif

        return hit.collider && hit.collider.gameObject != gameObject ? hit.collider.GetComponent<Monster>() : null;
    }

    private bool IsValidClimbCandidate(Monster target)
    {
        return target != null &&
               target != this &&
               Mathf.Abs(transform.position.x - target.transform.position.x) < 1f &&
               Mathf.Abs(transform.position.y - target.transform.position.y) < 0.5f &&
               target.HasLanded() &&
               target.currentState != State.Climbing &&
               target.currentState != State.PushedBack &&
               target.currentState != State.Dead &&
               !target.HasMonsterAbove();
    }

    private bool IsValidClimbTargetForClimbing(Monster target)
    {
        return target != null &&
               target != this &&
               target.HasLanded() &&
               target.currentState != State.Climbing &&
               target.currentState != State.Dead &&
               target.currentState != State.PushedBack &&
               !target.HasMonsterAbove(gameObject);
    }

    public void EnterPushedBackStateSmooth(Vector2 offset)
    {
        if (currentState == State.Dead || currentState == State.PushedBack) return;
        currentState = State.PushedBack;
        StartCoroutine(PushBackCoroutine(offset));
    }

    private IEnumerator PushBackCoroutine(Vector2 offset)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector2 start = _rb.position;
        Vector2 end = start + offset;

        while (elapsed < duration)
        {
            _rb.MovePosition(Vector2.Lerp(start, end, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _rb.MovePosition(end);
        currentState = State.Walking;
    }

    public bool HasLanded()
    {
        Vector2 origin = new Vector2(_selfCollider.bounds.center.x, _selfCollider.bounds.min.y - 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.05f, groundOrMonsterMask);
        return hit.collider != null && hit.collider != _selfCollider;
    }

    public bool HasMonsterAbove(GameObject except = null)
    {
        Vector2 origin = new Vector2(_selfCollider.bounds.center.x, _selfCollider.bounds.max.y + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, 0.1f, groundOrMonsterMask);

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.up * 0.1f, hit.collider ? Color.magenta : Color.gray, 0.1f);
#endif

        return hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.gameObject != except;
    }

    public Vector3 GetClimbTopPosition()
    {
        return new Vector3(transform.position.x, _selfCollider.bounds.max.y, transform.position.z);
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;

        _currentHp -= damage;
        ShowDamage(damage);

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentState = State.Dead;
        _rb.velocity = Vector2.zero;
        gameObject.SetActive(false); // 풀링 기반 비활성화
    }

    private void ShowDamage(int amount)
    {
        OnDamageDealt?.Invoke(transform.position + Vector3.up * 0.5f, amount);
    }

    public void SetGroundAndMonsterMask(LayerMask groundLayerMask, LayerMask monsterLayerMask)
    {
        groundMask = groundLayerMask;
        monsterMask = monsterLayerMask;
        groundOrMonsterMask = groundLayerMask | monsterLayerMask;
    }
    
    private void OnAttack()
    {
        var hit = Physics2D.OverlapBox(_selfCollider.bounds.center, Vector2.one * AttackBoxSize, 0f, TruckMask);
        if (hit != null)
        {
            Truck truck = hit.GetComponent<Truck>();
            truck?.TakeDamage(Damage);
        }
    }
}
