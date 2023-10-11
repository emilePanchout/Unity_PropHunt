using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    protected MovementController _movementController;
    public Camera Camera;
    protected ClassController _currentController;
    public NetworkVariable<bool> isHunter;

    public int baseHealth = 100;
    public int health = 100;
    public TMP_Text healthText;

    public GameObject blinder;
    public TMP_Text counterText;
    public PlayerInput inputs;

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

        healthText = GameObject.Find("HealthText").GetComponent<TMP_Text>();

        NetworkManager.Singleton.SceneManager.OnSceneEvent += Blinder;

        
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

        // Si un player a changé d'équipe avant la connexion de ce joueur 
        if(IsClient && !IsOwner && isHunter.Value != false)
        {
            SwapTeam(false, true);
        }

        if(SceneManager.GetActiveScene().name == "Game")
        {
            blinder.SetActive(true);
            Debug.Log("blind");
        }

        Camera.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwapTeamServerRPC()
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            isHunter.Value = !isHunter.Value;

        }
    }

    public void SwapTeam(bool previousIsHunterValue, bool newIsHunterValue)
    {
        if (newIsHunterValue)
        {
            _movementController.ClassController = _hunterController;
            _actionInput.SetClassInput(_hunterController.ClassInput);
            _propController.Deactivate();
            _hunterController.Activate();
            ResetHealth();
            return;
        }
        _movementController.ClassController = _propController;
        _actionInput.SetClassInput(_propController.ClassInput);
        _hunterController.Deactivate();
        _propController.Activate();
        ResetHealth();
    }

    public void ToggleCursorLock()
    {
        bool isLocked = !_movementController.cursorLocked;
        Cursor.lockState = isLocked? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }

    public void ResetHealth()
    {
        if (IsOwner)
        {
            health = baseHealth;
            healthText.text = baseHealth.ToString();
        }

    }

    //[ServerRpc(RequireOwnership = false)]

    public void UpdateHealth(int value)
    {
        if (IsOwner)
        {
            health += value;
            healthText.text = health.ToString();
        }

    }

    public void Blinder(SceneEvent sceneEvent)
    {
        if(IsOwner && isHunter.Value)
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {
                blinder.SetActive(true);
                inputs.enabled = false;

                StartCoroutine(BlinderTimer(10));
            }
        }

    }
    IEnumerator BlinderTimer(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            counterText.text = counter.ToString();
            yield return new WaitForSeconds(1);

            counter--;
            
        }
        blinder.SetActive(false);
        inputs.enabled = true;
    }

}
