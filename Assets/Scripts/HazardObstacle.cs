using UnityEngine;

/// <summary>
/// 實體危險障礙物 (石柱、邊框或地形)
/// 飛行器撞擊後會觸發墜機音效與重置
/// </summary>
public class HazardObstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        AirplaneController plane = collision.gameObject.GetComponentInParent<AirplaneController>();
        if (plane != null && !plane.isCrashed)
        {
            plane.OnCrash();
        }
    }
}
