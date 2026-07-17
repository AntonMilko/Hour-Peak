using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool isopen;

    public void open() 
    {
        isopen=true;
        gameObject.SetActive(true);
    }

    public void close()
    {
        isopen = false;
        gameObject.SetActive(false);
    }
}
