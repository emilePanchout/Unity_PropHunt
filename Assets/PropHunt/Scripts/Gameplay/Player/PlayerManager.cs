using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    protected MovementController _movementController;
    public Camera Camera;
    protected ClassController _currentController;
    public NetworkVariable<bool> isHunter;

    public ActionInput _actionInput;
    public Animator _animator;
    [SerializeField] PropController _propController;
    [SerializeField] HunterController _hunterController;

    private void Awake()
    {
        _movementController = GetComponent<MovementController>();
        isHunter.OnValueChanged += SwapTeam;
        if (_propController == null)
        {
            _propController = GetComponentInChildren<PropController>();
        }
        if(_hunterController == null)
        {
            _hunterController = GetComponentInChildren<HunterController>();
        }
        if(_actionInput == null)
        {
            _actionInput = GetComponent<ActionInput>();
        }
        if (Camera == null) Camera = GetComponentInChildren<Camera>(true);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SwapTeam(true, false);
        if (IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
            _movementController.enabled = true;
            Camera.gameObject.SetActive(true);
            _movementController.SetAnimator(GetComponent<Animator>());
            return;
        }
        Camera.gameObject.SetActive(false);
    }

    [ServerRpc]
    public void SwapTeamServerRPC()
    {
        isHunter.Value = !isHunter.Value;
    }

    public void SwapTeam(bool previousIsHunterValue, bool newIsHunterValue)
    {
        if (newIsHunterValue)
        {
            _movementController.ClassController = _hunterController;
            _actionInput.SetClassInput(_hunterController.ClassInput);
            _propController.Deactivate();
            _hunterController.Activate();
            return;
        }
        _movementController.ClassController = _propController;
        _actionInput.SetClassInput(_propController.ClassInput);
        _hunterController.Deactivate();
        _propController.Activate();
    }

    public void ToggleCursorLock()
    {
        bool isLocked = !_movementController.cursorLocked;
        Cursor.lockState = isLocked? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }
}
