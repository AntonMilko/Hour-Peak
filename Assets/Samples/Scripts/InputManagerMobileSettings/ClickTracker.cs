using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClickTracker : MonoBehaviour
{
    public string buttonName = ""; //This should be an unique name of the button
    public bool isJoystick = false; //Is this a joystick or a button?

    public float movementLimit = 1; //How far the joystick can be moved (n x Joystick Width)
    public float movementThreshold = 0.1f; //Minimum distance (n x Joystick Width) that the Joystick need to be moved to trigger inputAxis (Must be less than movementLimit)
    public float lookLimit = 0.5f; //How far the joystick can be moved (n x Joystick Width)
    public float lookThreshold = 0.1f; //Minimum distance (n x Joystick Width) that the Joystick need to be moved to trigger inputAxis (Must be less than lookLimit)
    
    //Reference variables
    RectTransform rt;
    Vector3 startPos;
    Vector2 clickPos;

    //Input variables
    Vector2 inputAxis = Vector2.zero;
    bool holding = false;
    bool clicked = false;

    void Start()
    {
        //Add this button to the list
        MobileControls.instance.AddButton(this);

        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition3D;
    }

    //Do this when the mouse is clicked over the selectable object this script is attached to.
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " Was Clicked.");

        holding = true;

        if (!isJoystick)
        {
            clicked = true;
            StartCoroutine(StopClickEvent());
        }
        else
        {
            //Initialize Joystick movement
            clickPos = eventData.pressPosition;
        }
    }

    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private object lookVector;

    //Wait for next update then release the click event
    IEnumerator StopClickEvent()
    {
        yield return waitForEndOfFrame;

        clicked = false;
    }

    //Joystick movement
    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " The element is being dragged");

        if (isJoystick)
        {
            Vector3 movementVector = Vector3.ClampMagnitude((eventData.position - clickPos) / MobileControls.instance.canvas.scaleFactor, (rt.sizeDelta.x * movementLimit * lookLimit) + (rt.sizeDelta.x * movementThreshold * lookThreshold));
            Vector3 movePos = startPos + movementVector;
            rt.anchoredPosition = movePos;

            //Update inputAxis
            float inputX = 0;
            float inputY = 0;
            if (Mathf.Abs(movementVector.x) > rt.sizeDelta.x * movementThreshold * lookThreshold)
            {
                inputX = (movementVector.x - (rt.sizeDelta.x * movementThreshold * lookThreshold * (movementVector.x > 0 ? 1 : -1))) / (rt.sizeDelta.x * movementLimit * lookLimit);
            }
            if (Mathf.Abs(movementVector.y) > rt.sizeDelta.x * movementThreshold * lookThreshold)
            {
                inputY = (movementVector.y - (rt.sizeDelta.x * movementThreshold * lookThreshold * (movementVector.y > 0 ? 1 : -1))) / (rt.sizeDelta.x * movementLimit * lookLimit);
            }
            inputAxis = new Vector2(inputX, inputY);
        }
    }

    //Do this when the mouse click on this selectable UI object is released.
    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " The mouse click was released");

        holding = false;

        if (isJoystick)
        {
            //Reset Joystick position
            rt.anchoredPosition = startPos;
            inputAxis = Vector2.zero;
        }
    }

    public Vector2 GetInputAxis()
    {
        return inputAxis;
    }

    public bool GetClickedStatus()
    {
        return clicked;
    }

    public bool GetHoldStatus()
    {
        return holding;
    }
}

#if UNITY_EDITOR
//Custom Editor
[CustomEditor(typeof(ClickTracker))]
public class ClickTracker_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        ClickTracker script = (ClickTracker)target;

        script.buttonName = EditorGUILayout.TextField("Button Name", script.buttonName);
        script.isJoystick = EditorGUILayout.Toggle("Is Joystick", script.isJoystick);
    }
}
#endif