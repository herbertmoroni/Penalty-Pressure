using UnityEngine;

public class GoalkeeperController : MonoBehaviour
{
    [Header("Movement")]
    public float minMoveSpeed = 3f;
    public float maxMoveSpeed = 7f;
    public float moveRange    = 2.5f;

    [Header("Sprites")]
    public Sprite idle1;
    public Sprite idle2;
    public Sprite saved;
    public float spriteSwapInterval = 0.3f;

    private SpriteRenderer sr;
    private float spriteTimer;
    private bool showingIdle1 = true;
    private bool isSaved;

    private float targetX;
    private float currentSpeed;
    private bool isFrozen;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.position = new Vector3(0f, transform.position.y, 0f);
        sr.sprite = idle1;
        PickNewTarget();
    }

    void Update()
    {
        if (isSaved || isFrozen) return;
        Patrol();
        AnimateSprite();
    }

    // Random destination and speed each move — prevents the player from timing the keeper
    void PickNewTarget()
    {
        targetX      = Random.Range(-moveRange, moveRange);
        currentSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
    }

    void Patrol()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetX, transform.position.y, 0f),
            currentSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
            PickNewTarget();
    }

    void AnimateSprite()
    {
        spriteTimer += Time.deltaTime;
        if (spriteTimer >= spriteSwapInterval)
        {
            spriteTimer  = 0f;
            showingIdle1 = !showingIdle1;
            sr.sprite    = showingIdle1 ? idle1 : idle2;
        }
    }

    public void Freeze() { isFrozen = true; }

    public void ShowSaved()
    {
        isSaved   = true;
        sr.sprite = saved;
    }

    public void ResetKeeper()
    {
        isSaved      = false;
        isFrozen     = false;
        showingIdle1 = true;
        spriteTimer  = 0f;
        transform.position = new Vector3(0f, transform.position.y, 0f);
        sr.sprite = idle1;
        PickNewTarget();
    }
}
