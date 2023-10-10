using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LaunchGameButton : NetworkBehaviour
{
    private GameObject button;
    void Start()
    {
        button = gameObject;

        if (IsHost)
        {
            button.SetActive(true);
        }
        else if(IsClient)
        {
            button.SetActive(false);
        }
    }

    public void LaunchGame()

    {
        NetworkManager.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
