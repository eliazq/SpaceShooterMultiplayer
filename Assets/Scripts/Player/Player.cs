using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float motorForce = 350f;
    [SerializeField] private float brakeForce = 9f;
    [SerializeField] private float turningSpeed = 200f;
    [SerializeField] private float autoBrakeForce = 3f;

    private void Start()
    {
        if (!IsOwner) return;
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMotor();        
    }

    private void HandleMotor()
    {
        float movementInput = Input.GetAxisRaw("Vertical");
        float turn = Input.GetAxisRaw("Horizontal");

        /* 
         * Handle Forward Movement
        */
        if (movementInput > 0.1f)
        {
            rb.AddForce(transform.up * movementInput * motorForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
        // Braking Or Moving Backwards
        else if (movementInput < -0.1f)
        {
            // Moving Backwards
            if (rb.velocity.y <= 0)
                rb.AddForce(transform.up * movementInput * motorForce * Time.fixedDeltaTime, ForceMode2D.Force);

            // Breaking
            else
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, Time.fixedDeltaTime * brakeForce), Mathf.MoveTowards(rb.velocity.y, 0, Time.fixedDeltaTime * brakeForce));

        }
        else
        {
            // Not using motor
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, Time.fixedDeltaTime * autoBrakeForce), Mathf.MoveTowards(rb.velocity.y, 0, Time.fixedDeltaTime * autoBrakeForce));
        }

        /*
         * Handle Turning
        */
        if (turn != 0)
        {
            rb.angularVelocity = -turn * turningSpeed;
        }
        else
        {
            rb.angularVelocity = 0;
        }
    }
}
