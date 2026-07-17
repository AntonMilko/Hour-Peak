using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MetroTransitionController : MonoBehaviour
{
    [SerializeField] 
    private string targetSceneName;
    [SerializeField] 
    private string MessageOnEnter;
    [SerializeField] 
    private float DelayBeforeTransition;
    [SerializeField] 
    private Rigidbody Rigidbody;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log(MessageOnEnter);
            }
            Invoke("TransitionToTarget", DelayBeforeTransition);
        }
    }
    private void TransitionToTarget() 
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else 
        {
            Debug.LogError("SceneEmpty");      
        }
    }
}