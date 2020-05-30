using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bindState state = bindState.none;

    protected RectTransform Background;
    protected int PointerId;
    public RectTransform Handle;
    [Range(0f, 2f)]
    public float HandleRange = 1f;

    public int repeatClicks;
    public float repeatThreshold = 0.1f;
    float lastClick;
    float holdTime;

    //[HideInInspector]
    public Vector2 InputVector = Vector2.zero;
    public Vector2 AxisNormalized { get { return InputVector.magnitude > 0.25f ? InputVector.normalized : (InputVector.magnitude < 0.01f ? Vector2.zero : InputVector * 4f); } }


    private void Awake()
    {
        if (Handle == null)
            Handle = transform.GetChild(0).GetComponent<RectTransform>();
        Background = GetComponent<RectTransform>();
        Background.pivot = new Vector2(0.5f, 0.5f);
    }

    private void Update()
    {

        switch (state)
        {
            case bindState.down:
                StateChange(bindState.hold);
                break;

            case bindState.hold:
                holdTime += Time.deltaTime;
                break;

            case bindState.up:
                if (holdTime < repeatThreshold)
                {
                    lastClick = Time.time;
                }
                else
                {
                    repeatClicks = 1;
                }
                holdTime = 0;
                StateChange(bindState.none);
                break;
        }

        if (state == bindState.hold)
        {
            Vector2 direction = (PointerId >= 0 && PointerId < Input.touches.Length) ? Input.touches[PointerId].position - new Vector2(Background.position.x, Background.position.y) : new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(Background.position.x, Background.position.y);
            InputVector = (direction.magnitude > Background.sizeDelta.x / 2f) ? direction.normalized : direction / (Background.sizeDelta.x / 2f);
            Handle.anchoredPosition = (InputVector * Background.sizeDelta.x / 2f) * HandleRange;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StateChange(bindState.down);

        PointerId = eventData.pointerId;

        if (Time.time < lastClick + repeatThreshold)
        {
            repeatClicks++;
            OnClickRepeated?.Invoke(repeatClicks);
        }
        else
        {
            repeatClicks = 1;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StateChange(bindState.up);

        InputVector = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        StateChange(bindState.none);

        InputVector = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
    }

    void StateChange(bindState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(state);
    }

    public event ButtonStateHandler OnStateChanged;
    public event ButtonRepeatHandler OnClickRepeated;
}
