using UnityEngine;

/// <summary>
/// 音效管理器 (單例模式)
/// 管理飛行引擎聲、障礙物撞擊音效、穿過光圈成功音效
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("用於循環播放飛行引擎聲的 AudioSource")]
    [SerializeField] private AudioSource engineSource;
    [Tooltip("用於播放單次音效 (撞擊、通關) 的 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip engineFlyLoopClip;
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private AudioClip passRingClip;

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

        // 自動確保 AudioSource 存在
        if (engineSource == null)
        {
            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.playOnAwake = false;
            engineSource.loop = true;
            engineSource.spatialBlend = 0f; // 2D 背景音
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        // 自動嘗試載入預設音效
        if (engineFlyLoopClip == null) engineFlyLoopClip = Resources.Load<AudioClip>("Audio/Airplane_Fly_Loop");
        if (crashClip == null) crashClip = Resources.Load<AudioClip>("Audio/Obstacle_Crash");
        if (passRingClip == null) passRingClip = Resources.Load<AudioClip>("Audio/Pass_Ring_Success");
    }

    private void Start()
    {
        StartEngineSound();
    }

    /// <summary>
    /// 開始播放飛行引擎聲 (循環)
    /// </summary>
    public void StartEngineSound()
    {
        if (engineSource != null && engineFlyLoopClip != null)
        {
            engineSource.clip = engineFlyLoopClip;
            engineSource.volume = 0.5f;
            if (!engineSource.isPlaying)
            {
                engineSource.Play();
            }
        }
    }

    /// <summary>
    /// 依據飛行速度動態調整引擎音調 (Pitch)
    /// </summary>
    /// <param name="normalizedSpeed">速度百分比 0.0 ~ 1.0</param>
    public void UpdateEnginePitch(float normalizedSpeed)
    {
        if (engineSource != null && engineSource.isPlaying)
        {
            engineSource.pitch = Mathf.Lerp(0.85f, 1.45f, normalizedSpeed);
        }
    }

    /// <summary>
    /// 停止引擎音效
    /// </summary>
    public void StopEngineSound()
    {
        if (engineSource != null && engineSource.isPlaying)
        {
            engineSource.Stop();
        }
    }

    /// <summary>
    /// 播放撞到障礙物音效
    /// </summary>
    public void PlayCrashSound()
    {
        if (sfxSource != null && crashClip != null)
        {
            sfxSource.PlayOneShot(crashClip, 1.0f);
        }
    }

    /// <summary>
    /// 播放順利通過障礙物 (光圈) 音效
    /// </summary>
    public void PlayPassRingSound()
    {
        if (sfxSource != null && passRingClip != null)
        {
            sfxSource.PlayOneShot(passRingClip, 0.9f);
        }
    }
}
