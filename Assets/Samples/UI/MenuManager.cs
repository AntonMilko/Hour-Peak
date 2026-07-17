using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Menu[] menus;
    public static MenuManager instance;
    private void Awake()
    {
        instance = this;
    }
    public void OpenMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].name == menuName)
            {
                menus[i].open();
            }
            else if (menus[i].isopen) 
            {
                CloseMenu(menus[i]); 
            }
        }
    }
    public void CloseMenu(Menu menu)
    {
        menu.close();
    }
    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++) 
        {
            if (menus[i].isopen)
            {
                CloseMenu (menus[i]);
            }
        }
        menu.open();
    }
    public void Exit()
    {
        Application.Quit();
    }
}
