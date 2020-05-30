using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FixedTouchField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bindState state = bindState.none;

    public Vector2 TouchDist;
    public Vector2 PointerOld;
    public int PointerID;

    private void Awake()
    {

    }

    private void Update()
    {
        switch (state)
        {
            case bindState.down:
                StateChange(bindState.hold);
                break;

            case bindState.up:
                StateChange(bindState.none);
                break;
        }

        if (state == bindState.hold)
        {
            if (PointerID >= 0 && PointerID < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerID].position - PointerOld;
                PointerOld = Input.touches[PointerID].position;
            }
            else
            {
                TouchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                PointerOld = Input.mousePosition;
            }
        }
        else
        {
            //TouchDist = new Vector2();
        }
    }

    private void LateUpdate()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StateChange(bindState.down);

        PointerID = eventData.pointerId;
        PointerOld = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StateChange(bindState.up);

        TouchDist = new Vector2();
    }

    private void OnDisable()
    {
        StateChange(bindState.none);

        TouchDist = new Vector2();
    }

    void StateChange(bindState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(state);
    }

    public event ButtonStateHandler OnStateChanged;
}
