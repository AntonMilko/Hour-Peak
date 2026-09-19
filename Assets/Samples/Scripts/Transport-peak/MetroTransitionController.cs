using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MetroTransitionController : MonoBehaviour
{
    [SerializeField] 
    private string targetSceneName;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}