using UnityEngine;

/// <summary>
/// 遊戲主管理器 (含 HUD 介面與跨平台手機觸控介面)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("玩家與場景物件")]
    public AirplaneController playerAirplane;
    public RingObstacle[] allRings;

    [Header("遊戲數據")]
    public int totalRings = 0;
    public int passedRings = 0;
    public int crashCount = 0;
    public float elapsedTime = 0f;
    public bool isGameWon = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerAirplane == null)
        {
            playerAirplane = FindFirstObjectByType<AirplaneController>();
        }

        FindAllRings();
    }

    private void Start()
    {
        FindAllRings();
    }

    private void Update()
    {
        if (!isGameWon)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public void FindAllRings()
    {
        allRings = FindObjectsByType<RingObstacle>(FindObjectsSortMode.None);
        totalRings = allRings.Length;
    }

    /// <summary>
    /// 當玩家通過一個光圈障礙物
    /// </summary>
    public void OnRingPassed(RingObstacle ring)
    {
        passedRings++;
        if (passedRings >= totalRings && totalRings > 0)
        {
            isGameWon = true;
        }
    }

    /// <summary>
    /// 當玩家撞到障礙物墜機
    /// </summary>
    public void OnPlayerCrashed()
    {
        crashCount++;
    }

    /// <summary>
    /// 重新開始本關
    /// </summary>
    public void RestartGame()
    {
        passedRings = 0;
        crashCount = 0;
        elapsedTime = 0f;
        isGameWon = false;

        foreach (var r in allRings)
        {
            if (r != null) r.ResetRing();
        }

        if (playerAirplane != null)
        {
            playerAirplane.Respawn();
        }
    }

    /// <summary>
    /// 即時繪製 HUD 與手機觸控按鈕
    /// </summary>
    private void OnGUI()
    {
        // 1. 頂部資訊看板
        GUI.Box(new Rect(20, 20, 260, 130), "<b>3D 飛行過關挑戰 (HW4)</b>");
        float speed = playerAirplane != null ? playerAirplane.currentSpeed : 0f;
        GUI.Label(new Rect(30, 45, 240, 25), $"飛行時速: {speed:F1} km/h");
        GUI.Label(new Rect(30, 68, 240, 25), $"通過障礙: {passedRings} / {totalRings}");
        GUI.Label(new Rect(30, 91, 240, 25), $"失誤撞擊次數: {crashCount}");
        GUI.Label(new Rect(30, 114, 240, 25), $"飛行時間: {elapsedTime:F1} 秒");

        if (GUI.Button(new Rect(20, 160, 110, 35), "重置位置/關卡"))
        {
            RestartGame();
        }

        // 2. 通關慶祝大標題
        if (isGameWon)
        {
            GUIStyle winStyle = new GUIStyle(GUI.skin.box);
            winStyle.fontSize = 24;
            winStyle.fontStyle = FontStyle.Bold;
            winStyle.normal.textColor = Color.yellow;
            winStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Box(new Rect(Screen.width / 2f - 200, Screen.height / 2f - 70, 400, 110), 
                $"🏆 恭喜成功穿越所有障礙！\n花費時間: {elapsedTime:F1} 秒\n撞擊次數: {crashCount}", winStyle);
        }

        // 3. 跨平台/手機觸控介面 (支援在手機螢幕上或在電腦上點擊操作)
        DrawMobileControls();
    }

    /// <summary>
    /// 繪製跨平台虛擬搖桿與觸控按鈕 (支援手機輸出測試)
    /// </summary>
    private void DrawMobileControls()
    {
        if (playerAirplane == null) return;

        float screenH = Screen.height;
        float screenW = Screen.width;

        // 姿態方向控制 (左下角方向十字按鍵)
        float btnSize = 65f;
        float padX = 30f;
        float padY = screenH - 180f;

        // 向上抬頭 (Pitch Up)
        bool pitchUp = GUI.RepeatButton(new Rect(padX + btnSize, padY, btnSize, btnSize), "▲\n抬頭");
        // 向下低頭 (Pitch Down)
        bool pitchDown = GUI.RepeatButton(new Rect(padX + btnSize, padY + btnSize * 1.4f, btnSize, btnSize), "▼\n俯衝");
        // 左轉 (Yaw Left)
        bool yawLeft = GUI.RepeatButton(new Rect(padX, padY + btnSize * 0.7f, btnSize, btnSize), "◄\n左轉");
        // 右轉 (Yaw Right)
        bool yawRight = GUI.RepeatButton(new Rect(padX + btnSize * 2f, padY + btnSize * 0.7f, btnSize, btnSize), "►\n右轉");

        // 油門控制 (右下角加速與減速)
        float rightPadX = screenW - 170f;
        float rightPadY = screenH - 170f;
        bool boost = GUI.RepeatButton(new Rect(rightPadX, rightPadY, 70f, 65f), "⚡\n加速");
        bool brake = GUI.RepeatButton(new Rect(rightPadX + 80f, rightPadY, 70f, 65f), "🛑\n減速");

        // 滾轉翻滾 (右上方側滾按鍵)
        bool rollLeft = GUI.RepeatButton(new Rect(screenW - 170f, rightPadY - 75f, 70f, 60f), "↺\n左滾");
        bool rollRight = GUI.RepeatButton(new Rect(screenW - 90f, rightPadY - 75f, 70f, 60f), "↻\n右滾");

        // 寫入飛行器輸入
        float pitch = 0f;
        if (pitchUp) pitch += 1f;
        if (pitchDown) pitch -= 1f;

        float yaw = 0f;
        if (yawRight) yaw += 1f;
        if (yawLeft) yaw -= 1f;

        float roll = 0f;
        if (rollLeft) roll += 1f;
        if (rollRight) roll -= 1f;

        float throttle = 0f;
        if (boost) throttle += 1f;
        if (brake) throttle -= 1f;

        playerAirplane.mobilePitchInput = pitch;
        playerAirplane.mobileYawInput = yaw;
        playerAirplane.mobileRollInput = roll;
        playerAirplane.mobileThrottleInput = throttle;
    }
}
