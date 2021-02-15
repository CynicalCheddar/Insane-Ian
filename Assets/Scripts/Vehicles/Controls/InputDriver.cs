using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDriver : MonoBehaviour
{
    public InterfaceCarDrive ICD;
    void FixedUpdate() {
        Steer();
        Accellerate();
        Reverse();
        Brake();
        UpdateWheelPoses();
    }

    private void UpdateWheelPoses() {
        var CarDriver = ICD.GetComponent<IDrivable>();
        CarDriver.UpdateWheelPoses();
    }

    private void Brake() {
        var CarDriver = ICD.GetComponent<IDrivable>();
        if (Input.GetKey(KeyCode.LeftShift)) {
            CarDriver.Brake();
        } 
    }

    private void Reverse() {
        var CarDriver = ICD.GetComponent<IDrivable>();
        if (Input.GetKey(KeyCode.S)) {
            CarDriver.Reverse();
        }
    }

    private void Accellerate() {
        var CarDriver = ICD.GetComponent<IDrivable>();
        if (Input.GetKey(KeyCode.W)) {
            CarDriver.Accellerate();
        } else {
            CarDriver.StopAccellerate();
        }
    }

    private void Steer() {
        int TargetDirection = 0;
        var CarDriver = ICD.GetComponent<IDrivable>();
        if (Input.GetKey(KeyCode.A)) {
            TargetDirection -= 1;
        } else if (Input.GetKey(KeyCode.D)) {
            TargetDirection += 1;
        }
        CarDriver.Steer(TargetDirection);
    }
}
