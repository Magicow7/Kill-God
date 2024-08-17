using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float cycleRate;

    public Light dirLight;
    private float currAngle = 0;

    // Update is called once per frame
    void Update()
    {
        if(currAngle > 180){
            dirLight.enabled = false;
        }else{
            dirLight.enabled = true;
        }
        if(currAngle > 360){
            currAngle -= 360;
        }
        transform.Rotate(new Vector3(cycleRate*Time.deltaTime, 0, 0));
        currAngle += cycleRate*Time.deltaTime;
    }
}
