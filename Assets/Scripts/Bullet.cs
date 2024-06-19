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
        player.Damage(damageAmount);
    }
}
