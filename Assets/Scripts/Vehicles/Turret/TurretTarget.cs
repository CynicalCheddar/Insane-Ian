using UnityEngine;

[VehicleScript(ScriptType.playerGunnerScript)]
[VehicleScript(ScriptType.aiGunnerScript)]
public class TurretTarget : MonoBehaviour
{
    public float pitch = 0;
    public float yaw = 0;
    public float upTraverse = 75;
    public float downTraverse = 30;

    // Update is called once per frame
    public void UpdateAngle()
    {
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        float localPitch = transform.localEulerAngles.x;
        if (localPitch > 180) localPitch -= 360;
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(localPitch, -upTraverse, downTraverse), transform.localEulerAngles.y, transform.localEulerAngles.z);
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }
}
