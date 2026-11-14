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
    [SerializeField] private bool fourDirectionMovement = true;
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

    void Update()
    {
        if (fourDirectionMovement)
        {
            if (Math.Abs(_movement.x) > movementThreshold)
            {
                _movement.y = 0;
            }
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
