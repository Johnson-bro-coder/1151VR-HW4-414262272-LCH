using UnityEngine;

/// <summary>
/// 第三人稱平滑追隨攝影機
/// </summary>
public class SmoothFollowCamera : MonoBehaviour
{
    [Header("目標對象")]
    public Transform target;

    [Header("相對位置與偏移")]
    [Tooltip("相對於飛機的後上方偏移向量")]
    public Vector3 offset = new Vector3(0f, 1.8f, -6f);

    [Header("跟隨阻尼參數")]
    public float positionSmoothSpeed = 10f;
    public float rotationSmoothSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null) return;

        // 計算期望位置 (依據飛機的世界座標與旋轉角度)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed * Time.deltaTime);

        // 平滑計算相機朝向
        Vector3 lookTarget = target.position + target.forward * 4f + target.up * 0.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
