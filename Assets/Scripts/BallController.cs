using UnityEngine;
using TMPro;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("Goal Guide")]
    public Transform goalGuide;

    [Header("Settings")]
    public float shootSpeed       = 14f;
    public float minDragDistance  = 0.3f;
    public float ballDragRadius   = 1.0f;
    public float keeperSaveDistance = 1.2f;
    public float maxPowerDrag     = 2.5f;   // drag Y distance that maps to crossbar height
    public float xSensitivity     = 2.5f;   // multiplier for left/right aim

    [Header("UI")]
    public TextMeshProUGUI goalText;

    [Header("Audio")]
    public AudioSource goalSound;

    [Header("References")]
    public GoalkeeperController goalkeeper;

    private float goalYBottom;
    private float goalYTop;
    private float goalXLeft;
    private float goalXRight;

    private Vector3 startPosition;
    private Vector3 dragStart;
    private Vector3 finalTarget;
    private bool isDragging;
    private bool isShot;
    private bool isResetting;
    private Vector3 shotDirection;
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
            Debug.LogWarning("BallController: goalGuide not assigned — using fallback bounds.");
            goalYBottom = -1.249f;
            goalYTop    =  2.722f;
            goalXLeft   = -5.216f;
            goalXRight  =  5.247f;
            return;
        }

        float halfH = Mathf.Abs(goalGuide.lossyScale.y) * 0.5f;
        float halfW = Mathf.Abs(goalGuide.lossyScale.x) * 0.5f;

        goalYBottom = goalGuide.position.y - halfH;
        goalYTop    = goalGuide.position.y + halfH;
        goalXLeft   = goalGuide.position.x - halfW;
        goalXRight  = goalGuide.position.x + halfW;

        Debug.Log($"[Setup] Goal X=[{goalXLeft:F3}, {goalXRight:F3}]  Y=[{goalYBottom:F3}, {goalYTop:F3}]  Ball Y={startPosition.y:F3}");
    }

    void Update()
    {
        if (isShot)
        {
            transform.position += shotDirection * shootSpeed * Time.deltaTime;

            // Shrink ball as it travels toward goal (perspective illusion)
            float progress = Mathf.InverseLerp(startPosition.y, finalTarget.y, transform.position.y);
            transform.localScale = originalScale * Mathf.Lerp(1f, 0.6f, progress);

            if (!isResetting && transform.position.y >= finalTarget.y)
            {
                transform.position = finalTarget;
                CheckGoalOrSave();
            }

            return;
        }

        if (isResetting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            if (Vector3.Distance(mouseWorld, transform.position) <= ballDragRadius)
            {
                dragStart  = mouseWorld;
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector3 dragVector = GetMouseWorldPos() - dragStart;

            if (dragVector.magnitude > minDragDistance && dragVector.y > 0)
            {
                // Y drag = power → height in goal. 1.4x multiplier lets max drag overshoot crossbar
                float dragPower = dragVector.y / maxPowerDrag;
                float aimX = transform.position.x + dragVector.x * xSensitivity;
                float aimY = goalYBottom + dragPower * (goalYTop - goalYBottom) * 1.4f;

                finalTarget   = new Vector3(aimX, aimY, 0f);
                shotDirection = (finalTarget - transform.position).normalized;

                Debug.Log($"[Shot] power={dragPower:F2}  aimX={aimX:F3}  aimY={aimY:F3}");

                isShot = true;
            }
        }
    }

    void CheckGoalOrSave()
    {
        isResetting = true;
        isShot      = false;

        float ballX   = transform.position.x;
        float ballY   = transform.position.y;
        float keeperX = goalkeeper.transform.position.x;

        bool inX = ballX >= goalXLeft  && ballX <= goalXRight;
        bool inY = ballY >= goalYBottom && ballY <= goalYTop;

        Debug.Log($"[Check] ball=({ballX:F3},{ballY:F3})  inX={inX} inY={inY}  keeperX={keeperX:F3}  keeperDiff={Mathf.Abs(ballX - keeperX):F3}");

        if (!inX || !inY)
        {
            string reason = !inX
                ? (ballX < goalXLeft ? "wide left" : "wide right")
                : (ballY < goalYBottom ? "below goal" : "over crossbar");
            Debug.Log($"[MISS] {reason}");
            StartCoroutine(ResetAfterDelay(0.5f));
            return;
        }

        if (Mathf.Abs(ballX - keeperX) < keeperSaveDistance)
        {
            Debug.Log($"[SAVED] keeper={keeperX:F3}  ball={ballX:F3}");
            goalkeeper.ShowSaved();
            ballRenderer.enabled = false;
            StartCoroutine(WaitForClick());
        }
        else
        {
            Debug.Log($"[GOAL] ball=({ballX:F3},{ballY:F3})  keeper={keeperX:F3}");
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
        yield return null;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        ResetBall();
        goalkeeper.ResetKeeper();
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        transform.position   = startPosition;
        transform.localScale = originalScale;
        ballRenderer.enabled = true;
        isShot      = false;
        isDragging  = false;
        isResetting = false;
    }
}
