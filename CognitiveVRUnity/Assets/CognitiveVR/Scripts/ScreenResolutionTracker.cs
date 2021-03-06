﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Adds screen resolution to device info
/// </summary>

namespace CognitiveVR
{
    public class ScreenResolutionTracker : CognitiveVRAnalyticsComponent
    {
        public override void CognitiveVR_Init(Error initError)
        {
            base.CognitiveVR_Init(initError);

            CognitiveVR.Instrumentation.updateDeviceState(new Dictionary<string, object>() { { "cvr.vr.screenresolution", Screen.height + " x " + Screen.width } });
        }

        public static string GetDescription()
        {
            return "Include Screen Resolution in Device Info. Probaby only useful for mobile";
        }
    }
}