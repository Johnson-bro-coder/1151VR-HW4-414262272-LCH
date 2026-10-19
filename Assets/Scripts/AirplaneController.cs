using UnityEngine;

/// <summary>
/// 3D 飛行器控制器
/// 支援 PC 鍵盤操控與手機觸控/重力感應，兼具真實空氣動力轉向與順暢手感
/// </summary>
public class AirplaneController : MonoBehaviour
{
    [Header("飛行速度設定")]
    [Tooltip("基礎前進速度")]
    public float baseSpeed = 15f;
    [Tooltip("加速極限速度")]
    public float maxSpeed = 28f;
    [Tooltip("減速最低速度")]
    public float minSpeed = 8f;
    [Tooltip("油門加速度")]
    public float acceleration = 12f;

    [Header("機動轉向設定")]
    [Tooltip("俯仰速度 (Pitch - 抬頭/低頭)")]
    public float pitchSensitivity = 50f;
    [Tooltip("是否反轉俯仰操作 (預設取消勾選：W / ↑ 為抬頭；若勾選則 W / ↑ 為俯衝)")]
    public bool invertPitch = false;
    [Tooltip("偏航轉向速度 (Yaw - 左右轉)")]
    public float yawSensitivity = 35f;
    [Tooltip("滾轉速度 (Roll - 機身側滾)")]
    public float rollSensitivity = 60f;
    [Tooltip("自動姿態回正力道")]
    public float autoLevelForce = 1.5f;

    [Header("手機虛擬搖桿與輸入注入")]
    [HideInInspector] public float mobilePitchInput = 0f;
    [HideInInspector] public float mobileYawInput = 0f;
    [HideInInspector] public float mobileRollInput = 0f;
    [HideInInspector] public float mobileThrottleInput = 0f; // -1 to 1

    [Header("狀態監控")]
    public float currentSpeed;
    public bool isCrashed = false;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // 飛行器使用動力學計算，關閉預設重力以保持平穩巡航
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        if (isCrashed) return;

        HandleInput();
        UpdateEngineAudio();
    }

    private void FixedUpdate()
    {
        if (isCrashed) return;

        ApplyFlightPhysics();
    }

    /// <summary>
    /// 讀取 PC 鍵盤與手機虛擬輸入
    /// </summary>
    private void HandleInput()
    {
        // 鍵盤輸入 (PC)
        float keyPitch = Input.GetAxis("Vertical");       // W/S 或 方向鍵上下
        float keyYaw = Input.GetAxis("Horizontal");       // A/D 或 方向鍵左右
        float keyRoll = 0f;
        if (Input.GetKey(KeyCode.Q)) keyRoll += 1f;
        if (Input.GetKey(KeyCode.E)) keyRoll -= 1f;

        float keyThrottle = 0f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) keyThrottle += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) keyThrottle -= 1f;

        // 整合手機觸控輸入 (若有虛擬搖桿或觸控按鈕)
        float pitch = Mathf.Clamp(keyPitch + mobilePitchInput, -1f, 1f);
        float yaw = Mathf.Clamp(keyYaw + mobileYawInput, -1f, 1f);
        float roll = Mathf.Clamp(keyRoll + mobileRollInput, -1f, 1f);
        float throttle = Mathf.Clamp(keyThrottle + mobileThrottleInput, -1f, 1f);

        // 如果手機支援重力感應 (Tilt)
        if (SystemInfo.supportsAccelerometer && Application.isMobilePlatform)
        {
            Vector3 accel = Input.acceleration;
            // 橫向手持：X 軸為左右側滾，Y 軸為前後俯仰
            pitch = Mathf.Clamp(pitch + accel.y * 1.5f, -1f, 1f);
            roll = Mathf.Clamp(roll - accel.x * 2.0f, -1f, 1f);
        }

        // 調節速度
        if (throttle > 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        else if (throttle < 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, minSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed, (acceleration * 0.5f) * Time.deltaTime);
        }

        // 旋轉計算 (預設 -pitch 為抬頭，符合一般玩家操作直覺)
        float pitchDir = invertPitch ? 1f : -1f;
        float pitchAmount = pitch * pitchDir * pitchSensitivity * Time.deltaTime;
        float yawAmount = yaw * yawSensitivity * Time.deltaTime;
        // 左右轉動時自動輔助傾斜 (Banking)
        float bankAmount = -yaw * rollSensitivity * 0.5f * Time.deltaTime;
        float rollAmount = (roll * rollSensitivity * Time.deltaTime) + bankAmount;

        transform.Rotate(pitchAmount, yawAmount, rollAmount, Space.Self);

        // 機身水平自動回正
        if (Mathf.Abs(roll) < 0.05f && Mathf.Abs(yaw) < 0.05f)
        {
            Vector3 currentEuler = transform.localEulerAngles;
            float zAngle = currentEuler.z > 180 ? currentEuler.z - 360 : currentEuler.z;
            float targetZ = Mathf.Lerp(zAngle, 0f, autoLevelForce * Time.deltaTime);
            transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, targetZ);
        }
    }

    /// <summary>
    /// 物理推力驅動
    /// </summary>
    private void ApplyFlightPhysics()
    {
        // 始終沿著機身正前方 (transform.forward) 飛行
        Vector3 forwardVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = forwardVelocity;
    }

    /// <summary>
    /// 動態更新引擎聲音頻率
    /// </summary>
    private void UpdateEngineAudio()
    {
        if (AudioManager.Instance != null)
        {
            float normalized = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
            AudioManager.Instance.UpdateEnginePitch(normalized);
        }
    }

    /// <summary>
    /// 撞擊障礙物時觸發
    /// </summary>
    public void OnCrash()
    {
        if (isCrashed) return;
        isCrashed = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrashSound();
            AudioManager.Instance.StopEngineSound();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerCrashed();
        }

        // 停止動力
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2 秒後重置回起點或檢查點
        Invoke(nameof(Respawn), 2.0f);
    }

    /// <summary>
    /// 重置重生
    /// </summary>
    public void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        currentSpeed = baseSpeed;
        isCrashed = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartEngineSound();
        }
    }
}
