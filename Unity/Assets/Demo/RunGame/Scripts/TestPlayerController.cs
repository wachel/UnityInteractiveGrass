using UnityEngine;
using System.Collections;

public class TestPlayerController : MonoBehaviour
{
    public Animator animator;
    public float maxSpeed = 7.0f;
    public AnimationCurve cameraDistance;
    public Grass.TerrainGrassManager grassManager;
    public float cameraDistanceScale = 1;
    public float cameraSmooth = 0.1f;
    public float pitchMin = -20;
    public float pitchMax = 80;
    public Vector3 lookOffset;
    float minCameraDistance = 8;//俯视距离滚轮调节范围
    float maxCameraDistance = 22;//俯视距离滚轮调节范围
    float cameraFollowDistance = 12;//摄像机俯视时的距离
    float cameraFollowDistanceMin = 1.5f;//摄像机平视时的距离
    float speed = 0f;
    float targetSpeed;
    float angle;
    float targetAngle;

    float yaw = 0;
    float pitch = 0;
    float targetYaw = 0;
    float targetPitch = 0;

    Subscriber subscriber;

    static public float lerpAngleDeg(float angle0, float angle1, float t) { return lerpAngleRad(angle0 * Mathf.Deg2Rad, angle1 * Mathf.Deg2Rad, t) * Mathf.Rad2Deg; }
    static public float lerpAngleRad(float angle0, float angle1, float t)
    {
        while (angle1 - angle0 > Mathf.PI) {
            angle1 -= Mathf.PI * 2;
        }
        while (angle1 - angle0 < -Mathf.PI) {
            angle1 += Mathf.PI * 2;
        }
        return Mathf.Lerp(angle0, angle1, t);
    }

    void Start()
    {
        //animator = GetComponent<Animator>();

        subscriber = new Subscriber();
        subscriber.Add("ui_move_speed", (float v) => { targetSpeed = v * maxSpeed; });
        subscriber.Add("ui_move_angle", (float v) => { targetAngle = v; });
        subscriber.Add("ui_change_yaw_pitch", (float y, float p) => {
            targetYaw += y;
            targetPitch = Mathf.Clamp(targetPitch + p, pitchMin, pitchMax);
        });
        subscriber.Add("ui_mouse_scroll", (float scroll) => {
            cameraFollowDistance *= (scroll < 0?1.3f : 1 / 1.3f);
            cameraFollowDistance = Mathf.Clamp(cameraFollowDistance, minCameraDistance, maxCameraDistance);
        });
        subscriber.Add("ui_use_skill", (int id) => {
            if (id == 0) {
                grassManager.AddDisturbance(transform.position, 3, 2);
            } else {
                grassManager.AddDisturbance(transform.position, 6, 2);
            }
        });
    }

    public void Update()
    {
        //平滑角度
        angle = lerpAngleDeg(angle, targetAngle, 0.3f);
        //平滑速度
        speed = Mathf.Lerp(speed, targetSpeed, 0.5f);

        this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 forward = Quaternion.AngleAxis(targetAngle,Vector3.up) * Vector3.forward;
        transform.position += forward * speed * Time.deltaTime;
        animator.SetFloat("Speed", speed / maxSpeed);

        yaw = Mathf.Lerp(yaw, targetYaw, cameraSmooth);
        pitch = Mathf.Lerp(pitch, targetPitch, cameraSmooth);

        Quaternion rotPicth = Quaternion.AngleAxis(pitch, Vector3.right);
        Quaternion rotYaw = Quaternion.AngleAxis(yaw, Vector3.up);
        Camera.main.transform.rotation = rotYaw * rotPicth;
        Vector3 lookDir = Camera.main.transform.rotation * Vector3.forward;
        float distance = Mathf.Lerp(cameraFollowDistanceMin, cameraFollowDistance, cameraDistance.Evaluate((pitch + 20) / 100)) * cameraDistanceScale;
        Camera.main.transform.position = this.transform.position + lookOffset + Vector3.up * 1.5f - lookDir * distance;

    }
}
