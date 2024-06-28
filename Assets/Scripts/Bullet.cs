using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damageAmount = 35;
    [SerializeField] private float destroyTime = 8f;
    private NetworkVariable<ulong> ownerClientID = new NetworkVariable<ulong>(42);
    public ulong ownerId { private get { return ownerClientID.Value; } set { ownerClientID.Value = value; } }

    private void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner) return;
        if (ownerId == 42) return;
        // if not player, don't continue
        if (!collision.TryGetComponent(out Player player)) return;
        // don't damage yourself
        if (ownerId == player.OwnerClientId) return;

        // DAMAGE ENEMY PLAYER
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        DamagePlayerServerRpc(playerNetworkObject, damageAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DamagePlayerServerRpc(NetworkObjectReference networkObjectReference, int damageAmount)
    {
        DamagePlayerClientRpc(networkObjectReference, damageAmount);
    }

    [ClientRpc(RequireOwnership = false)]
    private void DamagePlayerClientRpc(NetworkObjectReference networkObjectReference, int damageAmount)
    {
        if (!networkObjectReference.TryGet(out NetworkObject playerNetworkObject)) Debug.LogError("Couldnt Get NetworkObject From Player");

        Player player = playerNetworkObject.GetComponent<Player>();
        player.Damage(damageAmount);

    }
}
