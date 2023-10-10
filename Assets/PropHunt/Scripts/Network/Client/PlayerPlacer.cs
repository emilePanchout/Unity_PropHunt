using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PropHunt.Gameplay
{
    public class PlayerPlacer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            //Place the player on the position 0,0,0. Only orks because the position is client authoritative
            if(NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = new Vector3(0, 1f, 0);
            }

            Placer();
        }

        IEnumerator Placer()
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = new Vector3(0, 1f, 0);
            yield return new WaitForEndOfFrame();

        }

        void Start()
        {

        }

    }
}