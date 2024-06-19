using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("Reference Components")]
    [SerializeField] private Rigidbody2D rb;
    [Header("Reference Prefabs & GameObjects")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] public Transform shootingPoint;
    private int health = 100;

    [Header("Settings")]
    [SerializeField] private float motorForce = 350f;
    [SerializeField] private float turningSpeed = 200f;
    [SerializeField] private float autoBrakeForce = 3f;

    [Header("Public Settings")]
    public State state;
    public MovingSpeedMeter speedMeter 
    { 
        get 
        {
            switch (movingSpeed)
            {
                case >= 20:
                    return MovingSpeedMeter.extreamelyFast;
                case >= 12:
                    return MovingSpeedMeter.fast;
                case >= 7:
                    return MovingSpeedMeter.normal;
                case <= 0:
                    return MovingSpeedMeter.none;
                default:
                    return MovingSpeedMeter.slow;
            } 
        }
    }
    public bool isTurning { get; private set; }
    public bool isTurningRight { get; private set; }
    public bool isMoving { get { return state != State.Running; } }
    public float movingSpeed
    {
        get
        {
            return Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.y);
        }
    }
    public enum State
    {
        Running,
        Forward,
        Backwards,
        AutoBreaking,
    }

    public enum MovingSpeedMeter
    {
        none,
        slow,
        normal,
        fast,
        extreamelyFast,
    }

    private void Start()
    {
        if (!IsOwner) return;
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBulletServerRpc(new ServerRpcParams());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        // Find the NetworkObject associated with the ClientId
        NetworkObject senderNetworkObject = GetNetworkObjectByClientId(senderClientId);

        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = senderNetworkObject.GetComponent<Player>().shootingPoint.position;
        bullet.transform.rotation = transform.rotation;
        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.GetComponent<Bullet>().ownerId = senderClientId;
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMotor();
    }

    private NetworkObject GetNetworkObjectByClientId(ulong clientId)
    {
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (networkObject.OwnerClientId == clientId)
            {
                return networkObject;
            }
        }
        return null;
    }

    private void HandleMotor()
    {
        float movementInput = Input.GetAxisRaw("Vertical");
        float turn = Input.GetAxisRaw("Horizontal");

        /* 
         * Handle Forward Movement
        */
        // Moving Forward
        if (movementInput > 0.1f)
        {
            rb.AddForce(transform.up * movementInput * motorForce * Time.fixedDeltaTime, ForceMode2D.Force);
            state = State.Forward;
        }
 
        else if (movementInput < -0.1f)
        {
            // Moving Backwards
            rb.AddForce(transform.up * movementInput * motorForce * Time.fixedDeltaTime, ForceMode2D.Force);
            state = State.Backwards;
        }

        else
        {
            // Not using motor
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, Time.fixedDeltaTime * autoBrakeForce), Mathf.MoveTowards(rb.velocity.y, 0, Time.fixedDeltaTime * autoBrakeForce));
            if (rb.velocity != Vector2.zero) state = State.AutoBreaking;
            else state = State.Running;
        }

        /*
         * Handle Turning
        */
        if (turn != 0)
        {
            if (turn > 0) isTurningRight = true;
            else isTurningRight = false;

            rb.angularVelocity = -turn * turningSpeed;
            isTurning = true;
        }
        else
        {
            rb.angularVelocity = 0;
            isTurning = false;
            isTurningRight = false;
        }
    }
    
    public void Damage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Destroy(gameObject);
        }
    }
}
