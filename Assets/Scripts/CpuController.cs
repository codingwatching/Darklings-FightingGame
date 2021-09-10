using System.Collections;
using UnityEngine;

public class CpuController : MonoBehaviour
{
    [SerializeField] private Transform _otherPlayer = default;
    private Player _player;
    private PlayerMovement _playerMovement;
    private bool _isControllerEnabled = true;
    private float _movementInputX;
    private float _distance;


    void Awake()
    {
        _player = GetComponent<Player>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

	private void Start()
	{
        StartCoroutine(MovementCoroutine());
        StartCoroutine(AttackCoroutine());
    }

    private void Update()
    {
        _distance = Mathf.Abs(_otherPlayer.transform.position.x - transform.position.x);
        _playerMovement.MovementInput = new Vector2(_movementInputX, 0.0f);
    }

    IEnumerator MovementCoroutine()
    {
        while (_isControllerEnabled)
        {
            float waitTime = 0.0f;
            int movementRandom = Random.Range(0, 4);
            int jumpRandom = Random.Range(0, 8);
            int crouchRandom = Random.Range(0, 12);
            int standingRandom = Random.Range(0, 4);
            switch (movementRandom)
            {
                case 1:
                    _movementInputX = 0.0f;
                    waitTime = Random.Range(0.2f, 0.35f);
                    break;
                case 2:
                    _movementInputX = -1.0f;
                    waitTime = Random.Range(0.5f, 1.2f);
                    break;
                case 3:
                    _movementInputX = 1.0f;
                    waitTime = Random.Range(0.5f, 1.2f);
                    break;
            }
            if (jumpRandom == 2)
            {
                _playerMovement.JumpAction();
            }
            if (crouchRandom == 2)
            {
                _playerMovement.CrouchAction();
            }
            if (_playerMovement.IsCrouching)
            {
                if (standingRandom == 2)
                {
                    _playerMovement.StandUpAction();
                }
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator AttackCoroutine()
    {
        while (_isControllerEnabled)
        {
            if (_distance <= 7.0f)
            {
                if (GameManager.Instance.HasGameStarted)
                {
                    float attackWaitRandom = Random.Range(0.25f, 1.0f);
                    int movementRandom = Random.Range(0, 4);
                    switch (movementRandom)
                    {
                        case 1:
                            _player.AttackAction();
                            break;
                        case 2:
                            _player.AttackAction();
                            break;
                        case 3:
                            _player.AttackAction();
                            break;
                    }
                    yield return new WaitForSeconds(attackWaitRandom);
                }
            }
            yield return null;
        }
    }

    public void ActivateInput()
    {
        _isControllerEnabled = true;
    }

    public void DeactivateInput()
    {
        _isControllerEnabled = false;
    }
}