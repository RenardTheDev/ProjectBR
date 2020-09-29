using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TouchController : MonoBehaviour
{
    public static TouchController current;

    public FixedTouchField field_look;

    public SlidingButton button_fire;
    public FixedButton button_reload;
    public FixedButton button_aim;

    public FixedButton button_crouch;
    public FixedButton button_sprint;

    public FixedButton button_equipment;

    public FixedButton button_inventory;

    public FixedJoystick stick_movement;

    public InputMethod inputMethod;

    public float sens = 2.0f;

    private void Awake()
    {
        current = this;
    }

    private void Update()
    {
        Controls.move = stick_movement.InputVector;
        Controls.look = field_look.TouchDist;

        Controls.fire.state = button_fire.state;
        Controls.fire.delta = button_fire.TouchDist;

        Controls.reload.state = button_reload.state;
        Controls.aim.state = button_aim.state;

        Controls.eqipment.state = button_equipment.state;

        Controls.inventory.state = button_inventory.state;

        Controls.sens = sens;

        Controls.crouch.state = button_crouch.state;
        Controls.sprint.state = button_sprint.state;

        if (inputMethod == InputMethod.keysAndMouse)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void LateUpdate()
    {
        
    }

    private void OnValidate()
    {

    }
}

public static class Controls
{
    public static float sens = 2.0f;

    public static Vector2 move;
    public static Vector2 look;

    public static ControlsBind fire = new ControlsBind("fire");

    public static ControlsBind reload = new ControlsBind("reload");
    public static ControlsBind aim = new ControlsBind("aim");

    public static ControlsBind eqipment = new ControlsBind("equipment");

    public static ControlsBind inventory = new ControlsBind("inventory");

    public static ControlsBind crouch = new ControlsBind("crouch");
    public static ControlsBind sprint = new ControlsBind("sprint");
}

public class ControlsBind
{
    public string name;
    public bindState state;
    public Vector2 delta;       //for mobile sliding fire button

    public ControlsBind(string name)
    {
        this.name = name;
    }
}

public delegate void ButtonStateHandler(bindState state);
public delegate void ButtonRepeatHandler(int clicks);

public enum bindState
{
    down,
    hold,
    up,
    none
}
public enum InputMethod
{
    touch,
    keysAndMouse,
    gamepad
}