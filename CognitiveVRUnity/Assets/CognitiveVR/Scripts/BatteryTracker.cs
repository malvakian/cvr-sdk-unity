﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WARNING - NOT FULLY TESTED!
/// 
/// send battery level of mobile device post initialization and on quit
/// on unsupported platforms (pc, laptop, vive, iOS) does not send battery level
/// </summary>

namespace CognitiveVR
{
    public class BatteryTracker : CognitiveVRAnalyticsComponent
    {
#if !CVR_OCULUS
        float batteryLevel; //0-100 battery level
#endif
        public override void CognitiveVR_Init(Error initError)
        {
            base.CognitiveVR_Init(initError);
            SendBatteryLevel();
            CognitiveVR_Manager.OnQuit += CognitiveVR_Manager_OnQuit;
        }

        void CognitiveVR_Manager_OnQuit()
        {
            SendBatteryLevel();
        }

        void SendBatteryLevel()
        {

#if CVR_OCULUS
            Util.logDebug("batterylevel " + OVRPlugin.batteryLevel);
            Instrumentation.Transaction("battery")
                .setProperty("batterylevel", OVRPlugin.batteryLevel)
                .setProperty("batterytemperature", OVRPlugin.batteryTemperature)
                .setProperty("batterystatus", OVRPlugin.batteryStatus)
                .beginAndEnd();
#else

            if (GetBatteryLevel())
            {
                Util.logDebug("batterylevel " + batteryLevel);

                Instrumentation.Transaction("battery").setProperty("batterylevel", batteryLevel).beginAndEnd();
            }
#endif
        }

#if !CVR_OCULUS
        public bool GetBatteryLevel()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                try
                {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        if (null != unityPlayer)
                        {
                            using (AndroidJavaObject currActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                            {
                                if (null != currActivity)
                                {
                                    using (AndroidJavaObject intentFilter = new AndroidJavaObject("android.content.IntentFilter", new object[] { "android.intent.action.BATTERY_CHANGED" }))
                                    {
                                        using (AndroidJavaObject batteryIntent = currActivity.Call<AndroidJavaObject>("registerReceiver", new object[] { null, intentFilter }))
                                        {
                                            int level = batteryIntent.Call<int>("getIntExtra", new object[] { "level", -1 });
                                            int scale = batteryIntent.Call<int>("getIntExtra", new object[] { "scale", -1 });

                                            // Error checking that probably isn't needed but I added just in case.
                                            if (level == -1 || scale == -1)
                                            {
                                                batteryLevel = 50f;
                                                return false;
                                            }
                                            batteryLevel =((float)level / (float)scale) * 100.0f;
                                            return true;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            batteryLevel = 100f;
            return false;
        }
#endif

        public static bool GetWarning()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return false;
#endif
            return true;
        }

        public static string GetDescription()
        {
            return "Send the battery level of Android device after initialization and on quit\nOculus Utilies also includes battery temperature and status" + (GetWarning() ? "\nPlatform not set to Android!": "");
        }
    }
}