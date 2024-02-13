using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCCarMenu : MonoBehaviour
{
    public Animation[] WheelAnimations;
    public Animator DoorAnimator;
    bool IsDoorOpened;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void WheelAnimation()
    {
        Debug.Log("Wheel Click");
        foreach (Animation anim in WheelAnimations)
        {
            anim.enabled = !anim.enabled;
            if(anim.isActiveAndEnabled)
                anim.Play();
        }
    }

    public void DoorAnimation()
    {
        Debug.Log("Door Click");
        if (!IsDoorOpened)
        {
            DoorAnimator.Play("OpenDoor");
            IsDoorOpened = true;
        }
        else
        {
            DoorAnimator.Play("CloseDoor");
            IsDoorOpened = false;
        }
    }
}
