using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.InputSystem.InputAction;

public class HunterController : ClassController
{
    public float burstSpeed;
    public GameObject projectile;

    public override void Activate()
    {
        gameObject.SetActive(true);
        _camera.transform.SetParent(transform);
        _camera.transform.localPosition = new Vector3(0, 1, 0);
        ResetAnimator();
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    [ServerRpc]
    public void FireServerRpc(Vector3 camPos,Quaternion camRot,Vector3 camForw )
    {
        var newProjectile = Instantiate(projectile, camPos + camForw * 0.6f, camRot);
        newProjectile.GetComponent<NetworkObject>().Spawn();
        //newProjectile.transform.position = camPos + camForw * 0.6f;
        //newProjectile.transform.rotation = camRot;
        const int size = 1;
        newProjectile.transform.localScale *= size;
        newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
        newProjectile.GetComponent<Rigidbody>().AddForce(camForw * 20f, ForceMode.Impulse);
        newProjectile.GetComponent<MeshRenderer>().material.color =
        new Color(Random.value, Random.value, Random.value, 1.0f);
    }

    public void Fire()
    {
        if(IsOwner)
        {
            FireServerRpc(_camera.transform.position, _camera.transform.rotation, _camera.transform.forward);
        }

    }

}
