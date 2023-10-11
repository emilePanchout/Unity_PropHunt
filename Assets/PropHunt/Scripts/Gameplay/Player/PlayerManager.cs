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
    public NetworkVariable<int> health;
    public TMP_Text healthText;
    public GameObject propBody;
    public GameObject hunterBody;

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
        health.OnValueChanged += UpdateHealth;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += Blinder;


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
            ResetHealthServerRpc();
            return;
        }
        _movementController.ClassController = _propController;
        _actionInput.SetClassInput(_propController.ClassInput);
        _hunterController.Deactivate();
        _propController.Activate();
        ResetHealthServerRpc();
    }

    public void ToggleCursorLock()
    {
        bool isLocked = !_movementController.cursorLocked;
        Cursor.lockState = isLocked? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetHealthServerRpc()
    {
        health.Value = baseHealth;
        healthText.text = baseHealth.ToString();
    }



    public void UpdateHealth(int prevVal, int newVal)
    {
        if (IsOwner)
        {
            healthText.text = newVal.ToString();

            if(newVal <= 0)
            {
                playerDeathClientRpc();
            }
        }
    }

    [ClientRpc]
    public void playerDeathClientRpc()
    {
        if (IsOwner)
        {
            //propBody.SetActive(false);
            //hunterBody.SetActive(false);
        }

    }

    public void Blinder(SceneEvent sceneEvent)
    {
        if (IsServer && SceneManager.GetActiveScene().name == "Game")
        {
            BlinderServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BlinderServerRpc()
    {
        SendBlinderClientRpc();
        StartCoroutine(BlinderTimer(10));
    }

    [ClientRpc]
    public void SendBlinderClientRpc()
    {
        if (IsOwner && isHunter.Value)
        {
            blinder.SetActive(true);
            inputs.enabled = false;
        }
    }

    [ClientRpc]
    public void RemoveBlinderClientRpc()
    {
        if (IsOwner && isHunter.Value)
        {
            blinder.SetActive(false);
            inputs.enabled = true;
        }
    }

    [ClientRpc]
    public void BlinderTimerClientRpc(int seconds)
    {
        counterText.text = seconds.ToString();
    }

    

    IEnumerator BlinderTimer(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            BlinderTimerClientRpc(counter);
            yield return new WaitForSeconds(1);

            counter--;
        }

        RemoveBlinderClientRpc();

    }

}
