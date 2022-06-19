using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.W;
    [SerializeField] private float moveForce = 1f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private LayerMask groundLayer;

    private int jumps = 0;

    private new Rigidbody2D rigidbody2D;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        int left = Input.GetKey(leftKey) ? -1 : 0;
        int right = Input.GetKey(rightKey) ? 1 : 0;
        int horizontal = left + right;

        if (Input.GetKeyDown(jumpKey) && jumps < maxJumps)
        {
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumps++;
        }

        rigidbody2D.velocity = Vector2.right * horizontal * moveForce + Vector2.up * rigidbody2D.velocity.y;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            jumps = 0;
        }
    }
}
