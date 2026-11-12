using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float characterSpeed = 5;
    [SerializeField] private float movementThreshold = 0.01f;
    [Space(10)]
    [Tooltip("If false the character can move diagonally.")]
    [SerializeField] private bool fourDirectionMovement = true;
    [Space(10)]
    [Tooltip("If false only horizontal animations are played.")]
    [SerializeField] private bool fourDirectionAnimation = true;
    [Header("Input Settings")]
    [SerializeField] private InputActionReference moveAction;

    // #region AnimatorHashValues
    // private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
    // private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
    // private static readonly int Moving = Animator.StringToHash("Moving");
    // private static readonly int PreviousHorizontalMovement = Animator.StringToHash("PreviousHorizontalMovement");
    // private static readonly int PreviousVerticalMovement = Animator.StringToHash("PreviousVerticalMovement");
    // private static readonly int Die = Animator.StringToHash("Die");
    // #endregion
    
    private Vector2 _movement;
    private Vector2 _previousMovement;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()  => moveAction.action.Enable();
    private void OnDisable() => moveAction.action.Disable();

    void Update()
    {
        _movement = moveAction.action.ReadValue<Vector2>();

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
        float ds = characterSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + ds*_movement);
    }

    // private void LateUpdate()
    // {
    //     if (fourDirectionAnimation)
    //     {
    //         UpdateAnimator(_movement);
    //     }
    //     else
    //     {
    //         UpdateAnimatorTwoDirections(_movement);
    //     }
    // }
    
    // private void UpdateAnimatorTwoDirections(Vector2 movement)
    // {
    //     bool moving = movement.x!=0 || movement.y!=0;

    //     if (moving)
    //     {
    //         if (movement.y != 0)
    //         {
    //             // it is moving vertically so we have to use previous x movement
    //             _animator.SetFloat(HorizontalMovement,_previousMovement.x);
    //             print(_previousMovement);
    //         }
    //         else
    //         {
    //             _animator.SetFloat(HorizontalMovement,movement.x);    
    //             _previousMovement = movement;
    //         }
            
    //     }
    //     else
    //     {
    //         _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
    //         _animator.SetFloat(PreviousVerticalMovement, 0f);
    //     }

    //     _animator.SetBool(Moving,moving);
    // }

    // private void UpdateAnimator(Vector2 movement)
    // {
    //     bool moving = movement.x!=0 || movement.y!=0;

    //     if (moving)
    //     {
            
    //         _animator.SetFloat(HorizontalMovement,movement.x);
    //         _animator.SetFloat(VerticalMovement,movement.y);
    //         _previousMovement = movement;
    //     }
    //     else
    //     {
    //         _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
    //         _animator.SetFloat(PreviousVerticalMovement, _previousMovement.y);
    //     }

    //     _animator.SetBool(Moving,moving);
    // }

    public void Death()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
