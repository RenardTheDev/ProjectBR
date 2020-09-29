using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI current;

    [Header("UI Packets")]
    public GameObject uip_mainmenu;
    public GameObject uip_pause;
    public GameObject uip_gameplay;
    public GameObject uip_always;
    public GameObject uip_controls;

    [Header("UI Canvases")]
    public Canvas c_mainmenu;
    public Canvas c_customization;
    public Canvas c_pause;
    public Canvas c_effects;
    public Canvas c_msg;
    public Canvas c_player;
    public Canvas c_inv;
    public Canvas c_eqp;
    public Canvas c_settings;
    public Canvas c_debug;

    public bool invOpened;
    public bool eqpOpened;

    private void Awake()
    {
        if (current != null)
        {
            Destroy(gameObject);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        DisableAllCanvas();

        SceneManager.activeSceneChanged += ActiveSceneChanged;

        if (GameManager.current.gameState == GameState.mainmenu)
        {
            uip_mainmenu.SetActive(true);
            c_mainmenu.enabled = true;
        }
    }

    private void ActiveSceneChanged(Scene from, Scene to)
    {
        Debug.Log("ActiveSceneChanged: from.buildIndex = " + from.buildIndex + " | to.buildIndex = " + to.buildIndex);
        DisableAllCanvas();
        if (to.buildIndex == 0)
        {
            //EnableUICanvas("mainmenu");
            uip_mainmenu.SetActive(true);
            c_mainmenu.enabled = true;
        }
    }

    public void DisableAllCanvas()
    {
        uip_mainmenu.SetActive(false);
        uip_gameplay.SetActive(false);
        uip_controls.SetActive(false);
        uip_pause.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.current.gameState == GameState.gameplay)
        {
            if (TouchController.current.button_inventory.state == bindState.down)
            {
                ToggleInventory(!invOpened);
            }

            if (!invOpened && TouchController.current.button_equipment.state == bindState.down)
            {
                ToggleEquipment(!eqpOpened);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (GameManager.current.gameState)
            {
                case GameState.loading:
                    // nothing to do here
                    break;
                case GameState.mainmenu:
                    {
                        if (c_settings.enabled)
                        {
                            ToggleSettings(false);
                        }
                        else if (c_customization.enabled)
                        {
                            ToggleCustomization(false);
                        }
                        break;
                    }
                case GameState.gameplay:
                    {
                        if (invOpened)
                        {
                            invOpened = false;

                            c_inv.enabled = invOpened;
                            c_eqp.enabled = invOpened;
                        }
                        else
                        {
                            uip_gameplay.SetActive(false);
                            uip_controls.SetActive(false);
                            uip_pause.SetActive(true);

                            GameManager.current.gameState = GameState.pause;
                        }
                        break;
                    }
                case GameState.pause:
                    {
                        if (c_settings.enabled)
                        {
                            ToggleSettings(false);
                        }
                        else
                        {
                            uip_gameplay.SetActive(true);
                            uip_controls.SetActive(true);
                            uip_pause.SetActive(false);

                            GameManager.current.gameState = GameState.gameplay;
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }

    //--- Windows togglers ---
    public void ToggleInventory(bool toggle)
    {
        if (GameManager.current.gameState == GameState.gameplay)
        {
            invOpened = toggle;
            eqpOpened = invOpened;

            c_inv.enabled = invOpened;
            c_eqp.enabled = eqpOpened;

            InventoryUI.current.OnToggleInventory(toggle);
        }
    }

    public void ToggleEquipment(bool toggle)
    {
        if (!invOpened && GameManager.current.gameState == GameState.gameplay)
        {
            eqpOpened = toggle;
            c_eqp.enabled = eqpOpened;

            InventoryUI.current.OnToggleInventory(toggle);
        }
    }

    public void TogglePause(bool toggle)
    {
        if (toggle && GameManager.current.gameState == GameState.gameplay)
        {
            GameManager.current.gameState = GameState.pause;

            uip_gameplay.SetActive(false);
            uip_controls.SetActive(false);
            uip_pause.SetActive(true);
        }
        else if (!toggle && GameManager.current.gameState == GameState.pause)
        {
            GameManager.current.gameState = GameState.gameplay;

            uip_gameplay.SetActive(true);
            uip_controls.SetActive(true);
            uip_pause.SetActive(false);
        }
    }

    public void ToggleCustomization(bool state)
    {
        if (GameManager.current.gameState == GameState.mainmenu)
        {
            c_customization.enabled = state;
            c_mainmenu.enabled = !state;

            CustomizationMenu.current.ToggleCustomization(state);
        }
    }

    public void ToggleSettings(bool state)
    {
        if (GameManager.current.gameState == GameState.mainmenu)
        {
            c_settings.enabled = state;
            c_mainmenu.enabled = !state;
        }
        else
        {
            c_settings.enabled = state;
            c_pause.enabled = !state;
        }
    }
}