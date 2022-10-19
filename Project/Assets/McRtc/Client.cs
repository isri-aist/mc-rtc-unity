using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Action = System.Action;
using IntPtr = System.IntPtr;
using System.Linq;
using System.Runtime.InteropServices;

namespace McRtc
{
    [ExecuteAlways]
    public class Client : MonoBehaviour
    {
        public string host = "localhost";
        private Robot[] robots;
        static Client active_instance = null;

        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CreateClient(string host);
        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UpdateClient();
        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopClient();

        // This will be called when a new robot is seen by the GUI
        private delegate void OnRobotCallback(string id);
        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OnRobot(OnRobotCallback cb);

        // This will be called to place a robot mesh in the scene
        private delegate void OnRobotMeshCallback(string id, string name, string path, float scale, float qw, float qx, float qy, float qz, float tx, float ty, float tz);
        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OnRobotMesh(OnRobotMeshCallback cb);

        // This will be called when a robot is removed from the scene
        private delegate void OnRemoveRobotCallback(string id);
        [DllImport("McRtcPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OnRemoveRobot(OnRemoveRobotCallback cb);

        void Start()
        {
            CreateClient(host);
            if(active_instance != this)
            {
                active_instance = this;
            }
        }

        static void OnRobot(string id)
        {
            if (!active_instance)
            {
                return;
            }
            foreach (Robot robot in active_instance.robots)
            {
                if (robot.id == id)
                {
                    robot.UpdateRobot();
                }
            }
        }

        static void OnRobotMesh(string id, string name, string path, float scale, float qw, float qx, float qy, float qz, float tx, float ty, float tz)
        {
            if(!active_instance)
            {
                return;
            }
            foreach (Robot robot in active_instance.robots)
            {
                if (robot.id == id)
                {
                    robot.UpdateMesh(name, path, scale, qw, qx, qy, qz, tx, ty, tz);
                }
            }
        }

        static void OnRemoveRobot(string id)
        {
            if (!active_instance)
            {
                return;
            }
            foreach (Robot robot in active_instance.robots)
            {
                if (robot.id == id)
                {
                    robot.DeleteRobot();
                }
            }
        }

        void OnValidate()
        {
            CreateClient(host);
        }

        void OnDestroy()
        {
            if(active_instance == this)
            {
                active_instance = null;
            }
            StopClient();
        }

        void Awake()
        {
            if (Application.IsPlaying(gameObject))
            {
                SetupCallbacks();
            }
        }

        void SetupCallbacks()
        {
            if (active_instance != this)
            {
                active_instance = this;
            }
            robots = Object.FindObjectsOfType<Robot>();
            active_instance = this;
            OnRobot(Client.OnRobot);
            OnRobotMesh(Client.OnRobotMesh);
            OnRemoveRobot(Client.OnRemoveRobot);
        }

        void Update()
        {
            if (!Application.IsPlaying(gameObject))
            {
                SetupCallbacks();
            }
            OnRobot((string id) => OnRobot(id));
            OnRobotMesh((string id, string name, string path, float scale, float qw, float qx, float qy, float qz, float tx, float ty, float tz) => OnRobotMesh(id, name, path, scale, qw, qx, qy, qz, tx, ty, tz));
            OnRemoveRobot((string id) => OnRemoveRobot(id));
            UpdateClient();
        }

        // Force the scene to update frequently in editor mode
        // Based on https://forum.unity.com/threads/solved-how-to-force-update-in-edit-mode.561436/
        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...

#if UNITY_EDITOR
      // Ensure continuous Update calls.
      if (!Application.isPlaying)
      {
         UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
         UnityEditor.SceneView.RepaintAll();
      }
#endif
        }
    }
}