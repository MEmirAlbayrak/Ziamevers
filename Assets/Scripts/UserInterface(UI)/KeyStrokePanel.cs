using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyStrokePanel : MonoBehaviour
{
   public Canvas cv;

        public bool isOpen;
    void Start()
    {
        cv.enabled = false;
        isOpen = false;
    }

    
    void Update()
    {
         if(Input.GetKeyDown(KeyCode.P)) {
        if(isOpen){
            cv.enabled = false;
            isOpen = false;
        }
        else{
            cv.enabled = true;
            isOpen = true;
        }
    }
    }
}
