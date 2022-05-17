using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    [SerializeField] private GameObject _jumpPrefab = default;
    [SerializeField] private GameObject _groundedPrefab = default;
    private FallState _fallState;
    private JumpForwardState _jumpForwardState;
    private AirDashState _airDashState;

    void Awake()
    {
        _fallState = GetComponent<FallState>();
        _jumpForwardState = GetComponent<JumpForwardState>();
        _airDashState = GetComponent<AirDashState>();
    }

    public override void Enter()
    {
        base.Enter();
        Instantiate(_jumpPrefab, transform.position, Quaternion.identity);
        _audio.Sound("Jump").Play();
        _playerAnimator.Jump(true);
        _player.CanFlip = false;
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.AddForce(new Vector2(0.0f, _playerStats.PlayerStatsSO.jumpForce), ForceMode2D.Impulse);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        ToFallState();
        ToJumpState();
        ToJumpForwardState();
        ToAirDashState();
    }

    public void ToFallState()
    {
        if (_rigidbody.velocity.y <= 0.0f)
        {
            _stateMachine.ChangeState(_fallState);
        }
    }

    public void ToJumpState()
    {
        if (_playerStats.PlayerStatsSO.canDoubleJump && !_playerMovement.HasDoubleJumped)
        {
            if (_playerController.InputDirection.x == 0.0f)
            {
                if (_playerController.InputDirection.y > 0.0f && !_playerMovement.HasJumped)
                {
                    _playerMovement.HasDoubleJumped = true;
                    _playerMovement.HasJumped = true;
                    _stateMachine.ChangeState(this);
                }
                else if (_playerController.InputDirection.y <= 0.0f && _playerMovement.HasJumped)
                {
                    _playerMovement.HasJumped = false;
                }
            }
        }
    }

    public void ToJumpForwardState()
    {
        if (_playerStats.PlayerStatsSO.canDoubleJump && !_playerMovement.HasDoubleJumped)
        {
            if (_playerController.InputDirection.x != 0.0f)
            {
                if (_playerController.InputDirection.y > 0.0f && !_playerMovement.HasJumped)
                {
                    _playerMovement.HasDoubleJumped = true;
                    _playerMovement.HasJumped = true;
                    _stateMachine.ChangeState(_jumpForwardState);
                }
                else if (_playerController.InputDirection.y <= 0.0f && _playerMovement.HasJumped)
                {
                    _playerMovement.HasJumped = false;
                }
            }
        }
    }

    private void ToAirDashState()
    {
        if (!_playerMovement.HasAirDashed)
        {
            if (_playerController.DashForward())
            {
                _airDashState.DashDirection = 1;
                _stateMachine.ChangeState(_airDashState);
            }
            else if (_playerController.DashBackward())
            {
                _airDashState.DashDirection = -1;
                _stateMachine.ChangeState(_airDashState);
            }
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
    }
}
