using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.W;
    [SerializeField] private float moveForce = 1f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private KeyCode leftKnockbackKey = KeyCode.J;
    [SerializeField] private KeyCode rightKnockbackKey = KeyCode.L;
    [SerializeField] private KeyCode upKnockbackKey = KeyCode.I;
    [SerializeField] private KeyCode downKnockbackKey = KeyCode.K;
    [SerializeField] private float maxHitDistance = 2f;
    [SerializeField] private float minKnockbackForce = 2f;
    [SerializeField] private float maxKnockbackForce = 10f;
    [SerializeField] private Player enemy;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject knockbackEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private string winMessage;
    [SerializeField] private Color winColor;

    private int jumps = 0;
    private bool canKnockback = true;
    private Vector2 movement;

    private new Rigidbody2D rigidbody2D;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        Time.timeScale = 1f;
    }

    private void Update()
    {
        int left = Input.GetKey(leftKey) ? -1 : 0;
        int right = Input.GetKey(rightKey) ? 1 : 0;
        int horizontal = left + right;

        if (Input.GetKeyDown(jumpKey) && jumps < maxJumps)
        {
            rigidbody2D.velocity = Vector2.right * rigidbody2D.velocity;
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumps++;
        }

        if (Input.GetKeyDown(leftKnockbackKey) && canKnockback)
        {
            StartCoroutine(KnockbackCooldown());

            Instantiate(knockbackEffectPrefab, transform.position, Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, Vector2.left), Vector3.forward));

            if (HasTarget(-1, 0))
            {
                enemy.Knockback(RegisterKnockback(Vector2.left));
            }
        }
        else if (Input.GetKeyDown(rightKnockbackKey) && canKnockback)
        {
            StartCoroutine(KnockbackCooldown());

            Instantiate(knockbackEffectPrefab, transform.position, Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, Vector2.right), Vector3.forward));

            if (HasTarget(1, 0))
            {
                enemy.Knockback(RegisterKnockback(Vector2.right));
            }
        }
        else if (Input.GetKeyDown(upKnockbackKey) && canKnockback)
        {
            StartCoroutine(KnockbackCooldown());

            Instantiate(knockbackEffectPrefab, transform.position, Quaternion.AngleAxis(0, Vector3.forward));

            if (HasTarget(0, 1))
            {
                enemy.Knockback(RegisterKnockback(Vector2.up));
            }
        }
        else if (Input.GetKeyDown(downKnockbackKey) && canKnockback)
        {
            StartCoroutine(KnockbackCooldown());

            Instantiate(knockbackEffectPrefab, transform.position, Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, Vector2.down), Vector3.forward));

            if (HasTarget(0, -1))
            {
                enemy.Knockback(RegisterKnockback(Vector2.down));
            }
        }

        float moveMult = 1f;
        if (Mathf.Sign(rigidbody2D.velocity.x) == Mathf.Sign(horizontal))
        {
            moveMult = 1f - Mathf.Clamp01(Mathf.Abs(rigidbody2D.velocity.x) / maxMoveSpeed);
        }
        movement = Vector2.right * horizontal * moveForce * moveMult;
    }

    private void FixedUpdate()
    {
        rigidbody2D.AddForce(movement);
    }

    public void Die()
    {
        enemy.Win();
        Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void Win()
    {
        winText.text = winMessage;
        winText.color = winColor;
        winText.enabled = true;
        Time.timeScale = 0.2f;
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f * 0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Knockback(Vector2 force)
    {
        rigidbody2D.AddForce(force, ForceMode2D.Impulse);
    }

    private Vector2 RegisterKnockback(Vector2 dir)
    {
        float enemyDist = Vector2.Distance(transform.position, enemy.transform.position);
        float knockbackForce = Mathf.Lerp(maxKnockbackForce, minKnockbackForce, enemyDist / maxHitDistance);
        return dir * knockbackForce;
    }

    private bool HasTarget(int dirX, int dirY)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 2f, 0, new Vector2(dirX, dirY), maxHitDistance, enemyLayer);
        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            jumps = 0;
        }
    }

    private IEnumerator KnockbackCooldown()
    {
        canKnockback = false;
        yield return new WaitForSeconds(0.2f);
        canKnockback = true;
    }
}
