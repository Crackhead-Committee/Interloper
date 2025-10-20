using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Tuning")]
    public float returnSpeed = 10f;   // how fast it recenters
    public float snap = 15f;          // how fast it absorbs new kick

    // internal
    private Vector2 target;           // target offset (x=pitch up, y=yaw right)
    private Vector2 current;          // smoothed offset
    private Vector2 vel;
    private Quaternion lastApplied = Quaternion.identity;

    /// <summary>Adds recoil in degrees (pitch up is positive, yaw right can be +/-).</summary>
    public void AddRecoil(float upDegrees, float sideDegrees)
    {
        // absorb new impulse quickly
        target += new Vector2(upDegrees, sideDegrees);
    }

    void LateUpdate()
    {
        // remove last frame's offset so we don't compound
        transform.localRotation = Quaternion.Inverse(lastApplied) * transform.localRotation;

        // smooth target back to zero
        target = Vector2.Lerp(target, Vector2.zero, returnSpeed * Time.deltaTime);
        // smooth current towards target (snappy intake)
        current = Vector2.Lerp(current, target, snap * Time.deltaTime);

        // apply new offset
        lastApplied = Quaternion.Euler(-current.x, current.y, 0f);
        transform.localRotation = lastApplied * transform.localRotation;
    }
}
