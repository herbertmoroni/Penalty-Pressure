using UnityEngine;
using TMPro;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("Goal Guide")]
    public Transform goalGuide;

    [Header("Speed")]
    public float minShootSpeed = 5f;
    public float maxShootSpeed = 12f;
    public float maxPowerDrag  = 3f;

    [Header("Settings")]
    public float minDragDistance    = 0.3f;
    public float ballDragRadius     = 1.0f;
    public float keeperSaveDistance = 1.2f;

    [Header("UI")]
    public TextMeshProUGUI goalText;

    [Header("Audio")]
    public AudioSource goalSound;

    [Header("References")]
    public GoalkeeperController goalkeeper;

    private float goalLineY;
    private float goalXLeft;
    private float goalXRight;

    private Vector3 startPosition;
    private bool isDragging;
    private bool isShot;
    private bool isResetting;
    private Vector3 shotDirection;
    private float currentSpeed;
    private SpriteRenderer ballRenderer;
    private Vector3 originalScale;

    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        ballRenderer  = GetComponent<SpriteRenderer>();

        if (goalText != null)
            goalText.gameObject.SetActive(false);

        SetupGoalBounds();
    }

    void SetupGoalBounds()
    {
        if (goalGuide == null)
        {
            Debug.LogWarning("BallController: goalGuide not assigned.");
            goalLineY  = 1.5f;
            goalXLeft  = -5f;
            goalXRight =  5f;
            return;
        }

        goalLineY  = goalGuide.position.y - Mathf.Abs(goalGuide.lossyScale.y) * 0.5f;
        goalXLeft  = goalGuide.position.x - Mathf.Abs(goalGuide.lossyScale.x) * 0.5f;
        goalXRight = goalGuide.position.x + Mathf.Abs(goalGuide.lossyScale.x) * 0.5f;

        Debug.Log($"Ball Y={startPosition.y:F3} | goalLineY={goalLineY:F3}  X=[{goalXLeft:F3}, {goalXRight:F3}]");
    }

    void Update()
    {
        if (isShot)
        {
            transform.position += shotDirection * currentSpeed * Time.deltaTime;

            // Shrink ball as it travels forward
            float t = Mathf.InverseLerp(startPosition.y, goalLineY, transform.position.y);
            transform.localScale = originalScale * Mathf.Lerp(1f, 0.6f, t);

            if (!isResetting && transform.position.y >= goalLineY)
                CheckGoalOrSave();

            return;
        }

        if (isResetting) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Vector3.Distance(GetMouseWorldPos(), transform.position) <= ballDragRadius)
                isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector3 toMouse = GetMouseWorldPos() - transform.position;
            if (toMouse.magnitude > minDragDistance && toMouse.y > 0)
            {
                // Horizontal drag controls left/right aim — ball always travels toward the goal
                float aimX    = Mathf.Clamp(transform.position.x + toMouse.x * 2f, goalXLeft, goalXRight);
                Vector3 target = new Vector3(aimX, goalLineY, 0f);
                shotDirection  = (target - transform.position).normalized;

                float power   = Mathf.Clamp01(toMouse.magnitude / maxPowerDrag);
                currentSpeed  = Mathf.Lerp(minShootSpeed, maxShootSpeed, power);
                isShot = true;
            }
        }
    }

    void CheckGoalOrSave()
    {
        isResetting = true;
        isShot = false;

        float ballX   = transform.position.x;
        float keeperX = goalkeeper.transform.position.x;

        // Wide — outside the posts
        if (ballX < goalXLeft || ballX > goalXRight)
        {
            Debug.Log($"WIDE — ball X={ballX:F3}");
            isShot = false;
            StartCoroutine(WaitForClick());
            return;
        }

        Debug.Log($"ON TARGET — ball X={ballX:F3}  keeper X={keeperX:F3}");

        if (Mathf.Abs(ballX - keeperX) < keeperSaveDistance)
        {
            goalkeeper.ShowSaved();
            ballRenderer.enabled = false;
            isShot = false;
            StartCoroutine(WaitForClick());
        }
        else
        {
            StartCoroutine(GoalSequence());
        }
    }

    IEnumerator GoalSequence()
    {
        if (goalSound != null) goalSound.Play();

        if (goalText != null)
        {
            goalText.gameObject.SetActive(true);
            goalText.text = "GOAAALLLL!";
            goalText.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < 0.45f)
            {
                elapsed += Time.deltaTime;
                goalText.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, elapsed / 0.45f);
                yield return null;
            }
            goalText.transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(1.5f);

        if (goalText != null)
        {
            goalText.gameObject.SetActive(false);
            goalText.transform.localScale = Vector3.one;
        }

        ResetBall();
        goalkeeper.ResetKeeper();
    }

    IEnumerator WaitForClick()
    {
        // Ball stops wherever it is — click anywhere to reset
        yield return null;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        ResetBall();
        goalkeeper.ResetKeeper();
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        return pos;
    }

    void ResetBall()
    {
        transform.position = startPosition;
        transform.localScale = originalScale;
        ballRenderer.enabled = true;
        isShot      = false;
        isDragging  = false;
        isResetting = false;
    }
}
