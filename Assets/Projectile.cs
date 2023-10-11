using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 && IsServer)
        {
            if(collision.gameObject.GetComponent<PlayerManager>().health.Value >=10)
            {
                collision.gameObject.GetComponent<PlayerManager>().health.Value -= 10;
                DestroyProjectileServerRpc();
            }  
        }


    }

    [ServerRpc]
    public void DestroyProjectileServerRpc()
    {
        Destroy(gameObject);
    }
}
