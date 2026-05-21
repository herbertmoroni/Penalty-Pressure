using UnityEngine;

public class GoalkeeperController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveRange = 2.5f;

    [Header("Sprites")]
    public Sprite idle1;
    public Sprite idle2;
    public Sprite saved;
    public float spriteSwapInterval = 0.3f;

    private SpriteRenderer sr;
    private float spriteTimer;
    private bool showingIdle1 = true;
    private bool isSaved;

    // Patrol: center(0) → left → center(0) → right → repeat
    private float[] waypoints;
    private int waypointIndex;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        waypoints = new float[] { 0f, -moveRange, 0f, moveRange };
        waypointIndex = 0;
        transform.position = new Vector3(0f, transform.position.y, 0f);
        sr.sprite = idle1;
    }

    void Update()
    {
        if (isSaved) return;
        Patrol();
        AnimateSprite();
    }

    void Patrol()
    {
        float targetX = waypoints[waypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetX, transform.position.y, 0f),
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void AnimateSprite()
    {
        spriteTimer += Time.deltaTime;
        if (spriteTimer >= spriteSwapInterval)
        {
            spriteTimer = 0f;
            showingIdle1 = !showingIdle1;
            sr.sprite = showingIdle1 ? idle1 : idle2;
        }
    }

    public void ShowSaved()
    {
        isSaved = true;
        sr.sprite = saved;
    }

    public void ResetKeeper()
    {
        isSaved = false;
        showingIdle1 = true;
        spriteTimer = 0f;
        waypointIndex = 0;
        transform.position = new Vector3(0f, transform.position.y, 0f);
        sr.sprite = idle1;
    }
}
