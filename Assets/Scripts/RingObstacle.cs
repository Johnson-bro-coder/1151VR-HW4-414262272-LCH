using UnityEngine;

/// <summary>
/// 穿越型光圈障礙物
/// 當飛行器成功穿過光圈內部時觸發獎勵音效與計分
/// </summary>
public class RingObstacle : MonoBehaviour
{
    [Header("狀態設定")]
    [Tooltip("是否已經被穿過")]
    public bool isPassed = false;

    [Header("視覺反饋")]
    public Color normalColor = new Color(1f, 0.85f, 0.2f); // 亮黃色
    public Color passedColor = new Color(0.2f, 1f, 0.4f); // 翠綠色
    [SerializeField] private Renderer ringRenderer;

    private void Awake()
    {
        if (ringRenderer == null)
        {
            ringRenderer = GetComponent<Renderer>();
        }

        SetRingColor(normalColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPassed) return;

        // 偵測是否為飛行器進入觸發區
        AirplaneController plane = other.GetComponentInParent<AirplaneController>();
        if (plane != null && !plane.isCrashed)
        {
            isPassed = true;

            // 1. 播放順利穿過障礙物音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPassRingSound();
            }

            // 2. 變更外觀顏色
            SetRingColor(passedColor);

            // 3. 通知遊戲管理器
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRingPassed(this);
            }
        }
    }

    private void SetRingColor(Color col)
    {
        if (ringRenderer != null && ringRenderer.material != null)
        {
            ringRenderer.material.color = col;
        }
    }

    /// <summary>
    /// 重置光圈狀態
    /// </summary>
    public void ResetRing()
    {
        isPassed = false;
        SetRingColor(normalColor);
    }
}
