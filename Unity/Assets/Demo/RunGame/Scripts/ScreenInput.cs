using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScreenInput : MonoBehaviour
{
    public RectTransform panelFullScreen;
    public RectTransform joystickBkg;
    public Image joystickButton;
    public Text txtFps;
    //private MyPlayerController player;
    private Vector2 positionDown;
    private bool isMoving;
    private bool isDraging;
    private float movingAngle;

    private Vector2 lastDragCameraPosition;

    private float m_timeCountStart = 0.0f;
    private float INTERVAL = 0.5f;
    private float fps = 0.0f;
    private int m_count = 0;
    private float m_timeElapse;

    // Use this for initialization
    void Start () {
        //player = GameObject.FindObjectOfType<MyPlayerController>();

        UIEvent.BindBeginDrag(joystickBkg.gameObject, JoystickOnBeginDrag);
        UIEvent.BindDrag(joystickBkg.gameObject, JoystickOnDrag);
        UIEvent.BindEndDrag(joystickBkg.gameObject, JoystickOnEndDrag);

        UIEvent.BindBeginDrag(panelFullScreen.gameObject, ScreenOnBeginDrag);
        UIEvent.BindDrag(panelFullScreen.gameObject, ScreenOnDrag);
        UIEvent.BindEndDrag(panelFullScreen.gameObject, ScreenOnEndDrag);

        UIEvent.BindClick(transform.Find("Main/Skills/Skill0").gameObject, () => OnClickSkill(0));
        UIEvent.BindClick(transform.Find("Main/Skills/Skill1").gameObject, () => OnClickSkill(1));
    }

    void OnClickSkill(int id)
    {
        MessageSystem.Publish("ui_use_skill", id);
    }

    // Update is called once per frame
    void Update()
    {

        m_count++;
        m_timeElapse = Time.realtimeSinceStartup - m_timeCountStart;
        if (m_timeElapse >= INTERVAL) {
            float newFps = 1.0f / m_timeElapse * m_count;
            m_timeCountStart = Time.realtimeSinceStartup;
            m_count = 0;
            if (Mathf.Abs(newFps - fps) >= 3) {
                fps = newFps;
                txtFps.text = ((int)fps).ToString();
            }
        }


        if (isMoving) {
            Vector3 camDir = Camera.main.transform.TransformDirection(Vector3.forward);
            Vector3 leftDir = Vector3.Cross(camDir, Vector3.up);
            Vector3 forwardDir = Vector3.Cross(leftDir, Vector3.down);
            float worldAngle = Mathf.PI + get2DAngle(Vector2.right, new Vector2(forwardDir.x, forwardDir.z));
            worldAngle += movingAngle;

            MessageSystem.Publish("ui_move_angle", worldAngle * Mathf.Rad2Deg);
            MessageSystem.Publish("ui_move_speed", 1.0f);
        }
        else {
            MessageSystem.Publish("ui_move_speed", 0.0f);
        }


        if (!isDraging) {
            MoveDirection dir = getKeyboardDirection();
            if (dir != MoveDirection.Stand) {
                isMoving = true;
                movingAngle = (((int)dir) / 8.0f) * Mathf.PI * 2 - Mathf.PI / 2;
            }
            else {
                isMoving = false;
            }
        }

        if(Input.GetAxis("Mouse ScrollWheel") != 0) {
            MessageSystem.Publish("ui_mouse_scroll", Input.GetAxis("Mouse ScrollWheel"));
        }
    }
    static public float get2DAngle(Vector2 from, Vector2 to)
    {
        float angle = Vector2.Angle(from, to) * (Mathf.PI / 180);
        Vector3 cross = Vector3.Cross(from, to);
        if (cross.z > 0) {
            angle = Mathf.PI * 2 - angle;
        }
        return angle;
    }

    private Vector2 GetLocalPos(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelFullScreen, eventData.position, eventData.pressEventCamera, out pos)) {
            return pos;
        }
        return Vector2.zero;
    }

    void JoystickOnBeginDrag(PointerEventData eventData)
    {
        positionDown = GetLocalPos(eventData);
    }

    void JoystickOnDrag(PointerEventData eventData)
    {
        Vector2 pos = GetLocalPos(eventData);
        Vector2 move = (pos - positionDown);
        if (move.magnitude < 70) {
            joystickButton.transform.localPosition = move;
        }
        else {
            move = move.normalized * 70;
            joystickButton.transform.localPosition = move;
        }
        isMoving = move.magnitude > 30;
        isDraging = true;
        movingAngle = get2DAngle(Vector2.right, move.normalized);
    }
    
    void JoystickOnEndDrag(PointerEventData eventData)
    {
        isMoving = false;
        isDraging = false;
        joystickButton.transform.localPosition = Vector2.zero;
    }

    void ScreenOnBeginDrag(PointerEventData eventData)
    {
        lastDragCameraPosition = GetLocalPos(eventData);
    }

    void ScreenOnDrag(PointerEventData eventData)
    {
        Vector2 pos = GetLocalPos(eventData);
        Vector2 move = (pos - lastDragCameraPosition);
        lastDragCameraPosition = pos;
        MessageSystem.Publish("ui_change_yaw_pitch", move.x * 0.3f, -move.y * 0.3f);
    }

    void ScreenOnEndDrag(PointerEventData eventData)
    {

    }



    private enum MoveDirection
    {
        Stand = -1,
        Up = 0,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    };
    private MoveDirection getKeyboardDirection()
    {
        MoveDirection moveDir = MoveDirection.Stand;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)) {
            moveDir = MoveDirection.UpLeft;
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)) {
            moveDir = MoveDirection.UpRight;
        }
        else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S)) {
            moveDir = MoveDirection.DownLeft;
        }
        else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S)) {
            moveDir = MoveDirection.DownRight;
        }
        else if (Input.GetKey(KeyCode.W)) {
            moveDir = MoveDirection.Up;
        }
        else if (Input.GetKey(KeyCode.A)) {
            moveDir = MoveDirection.Left;
        }
        else if (Input.GetKey(KeyCode.D)) {
            moveDir = MoveDirection.Right;
        }
        else if (Input.GetKey(KeyCode.S)) {
            moveDir = MoveDirection.Down;
        }
        return moveDir;
    }
}
