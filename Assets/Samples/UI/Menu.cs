using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool isopen;
    private SaveSlotSystem _saveSlotSystem;

    private void Awake()
    {
        Debug.Log($"Menu.Awake: gameObject={gameObject.name}, transform={transform.name}");
        
        // Ищем SaveManagerUI на этом же GameObject или в дочерних
        _saveSlotSystem = GetComponent<SaveSlotSystem>();
        if (_saveSlotSystem == null)
        {
            _saveSlotSystem = GetComponentInChildren<SaveSlotSystem>();
            if (_saveSlotSystem != null)
            {
                Debug.Log($"Menu.Awake: Found SaveSlotSystem in children: {_saveSlotSystem.transform.name}");
            }
        }
        else
        {
            Debug.Log($"Menu.Awake: Found SaveSlotSystem on same GameObject");
        }
    }

    public void open() 
    {
        isopen=true;
        gameObject.SetActive(true);
        
        Debug.Log("Menu.open(): Calling SaveSlotSystem.OpenMenu()");
        
        if (_saveSlotSystem != null)
        {
            _saveSlotSystem.OpenMenu();
        }
        else
        {
            Debug.LogWarning("Menu.open(): SaveSlotSystem not found!");
        }
    }

    public void close()
    {
        isopen = false;
        gameObject.SetActive(false);
        
        if (_saveSlotSystem != null)
        {
            _saveSlotSystem.CloseMenu();
        }
    }
}
