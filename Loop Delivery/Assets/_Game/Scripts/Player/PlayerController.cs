using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float bikeSpeed = 10;
    [SerializeField] private float movementThreshold = 0.01f;
    [Space(10)]
    [Tooltip("If false the character can move diagonally.")]
    [SerializeField] private bool fourDirectionMovement = false;
    [Space(10)]
    [Tooltip("If false only horizontal animations are played.")]
    [SerializeField] private bool fourDirectionAnimation = true;

    #region AnimatorHashValues
    private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
    private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int PreviousHorizontalMovement = Animator.StringToHash("PreviousHorizontalMovement");
    private static readonly int PreviousVerticalMovement = Animator.StringToHash("PreviousVerticalMovement");
    private static readonly int OnBike = Animator.StringToHash("OnBike");
    #endregion
    
    private Vector2 _movement;
    private Vector2 _previousMovement;
    
    private Rigidbody2D _rb;
    private Animator _animator;

    [SerializeField] private bool _isOnBike = false;

    public Vector2 Facing {get; private set;} = Vector2.right;

    private static Vector2 SnapToCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) >= Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y)); 
    }
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnMove(InputValue value)
    {
        _movement = value.Get<Vector2>();
    }

    private void OnToggleBike(InputValue value)
    {
        _isOnBike = !_isOnBike;
        _animator.SetBool(OnBike, _isOnBike);
    }

    private void Update()
    {
        if (fourDirectionMovement)
        {
            if (Math.Abs(_movement.x) > movementThreshold)
            {
                _movement.y = 0;
            }
        }

        if (_movement.sqrMagnitude > movementThreshold * movementThreshold)
        {
            Facing = fourDirectionMovement ? SnapToCardinal(_movement) : _movement.normalized;
        }
    }

    private void FixedUpdate()
    {
        float speed = _isOnBike ? bikeSpeed : walkingSpeed;
        float ds = speed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + ds*_movement);
    }

    private void LateUpdate()
    {
        if (fourDirectionAnimation)
        {
            UpdateAnimator(_movement);
        }
        else
        {
            UpdateAnimatorTwoDirections(_movement);
        }
    }
    
    private void UpdateAnimatorTwoDirections(Vector2 movement)
    {
        bool moving = movement.x!=0 || movement.y!=0;

        if (moving)
        {
            if (movement.y != 0)
            {
                // it is moving vertically so we have to use previous x movement
                _animator.SetFloat(HorizontalMovement,_previousMovement.x);
                print(_previousMovement);
            }
            else
            {
                _animator.SetFloat(HorizontalMovement,movement.x);    
                _previousMovement = movement;
            }
            
        }
        else
        {
            _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
            _animator.SetFloat(PreviousVerticalMovement, 0f);
        }

        _animator.SetBool(Moving,moving);
    }

    private void UpdateAnimator(Vector2 movement)
    {
        bool moving = movement.x!=0 || movement.y!=0;

        if (moving)
        {
            
            _animator.SetFloat(HorizontalMovement,movement.x);
            _animator.SetFloat(VerticalMovement,movement.y);
            _previousMovement = movement;
        }
        else
        {
            _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
            _animator.SetFloat(PreviousVerticalMovement, _previousMovement.y);
        }

        _animator.SetBool(Moving,moving);
    }

    public void Death()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
