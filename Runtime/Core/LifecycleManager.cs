using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Atuvu.Lifecycle
{
    internal sealed class LifecycleManager : MonoBehaviour
    {
        static readonly Action<IController> k_Update = (controller) => { controller.DoUpdate(deltaTime); };
        static readonly Action<IController> k_PreUpdate = (controller) => { controller.DoPreUpdate(deltaTime); };
        static readonly Action<IController> k_PostUpdate = (controller) => { controller.DoPostUpdate(deltaTime); };
        static readonly Action<IController> k_FixedUpdate = (controller) => { controller.DoFixedUpdate(); };
        static readonly Action<IController> k_EndOfFrame = (controller) => { controller.DoEndOfFrame(deltaTime); };
        static readonly Action<IController> k_ControllerStarted = (controller) => { controller.DoControllerStarted(); };
        static readonly Action<IController> k_RegisterCallbacks = (controller) => { controller.DoRegisterCallbacks(); };
        static readonly Action<IController> k_UnregisterCallbacks = (controller) => { controller.DoUnregisterCallbacks(); };
        static readonly Action<IController> k_ControllerStopped = (controller) => { controller.DoControllerStopped(); };

        static readonly WaitForEndOfFrame k_EndOfFrameYield = new WaitForEndOfFrame();

        static LifecycleManager s_Instance;

        ControllerBatch[] m_ControllerBatches;

        static float deltaTime => Time.deltaTime;

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            if (s_Instance != null)
                return;

            s_Instance = new GameObject("[Controllers]").AddComponent<LifecycleManager>();
            DontDestroyOnLoad(s_Instance.gameObject);
        }

        #region INIT / DESTROY
        void Awake()
        {
            m_ControllerBatches = LoadAllControllerBatch();
            InitBatches();

            StartCoroutine(EndOfFrame());
        }

        ControllerBatch[] LoadAllControllerBatch()
        {
            var groups = Resources.LoadAll<ControllerGroup>("");
            List<ControllerBatch> batches = new List<ControllerBatch>(groups.Length);
            List<IController> controllers = new List<IController>();

            foreach (var controllerGroup in groups)
            {
                controllers.Clear();
                foreach (var controller in controllerGroup.controllers)
                {
                    if (controller == null)
                        continue;

                    controller.GetComponents(TempBuffers.monoBehaviours);
                    foreach (var component in TempBuffers.monoBehaviours)
                    {
                        if (component is IController c)
                        {
                            c.SetInstance(controller);
                            controllers.Add(c);
                        }
                    }
                }

                if (controllers.Count > 0)
                    batches.Add(new ControllerBatch(controllerGroup.displayName, controllers.ToArray()));
            }

            return batches.ToArray();
        }

        void InitBatches()
        {
            OperateOnAllController("[Controller] Start Controller", k_ControllerStarted);
            OperateOnAllController("[Controller] Register Callbacks", k_RegisterCallbacks);
        }

        void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
                DisposeOfBatches();
            }
        }

        void DisposeOfBatches()
        {
            OperateOnAllController("[Controller] Unregister Callbacks", k_UnregisterCallbacks);
            OperateOnAllController("[Controller] Stop Controller", k_ControllerStopped);
        }
        #endregion

        #region LIFECYCLE



        void Update()
        {
            OperateOnAllController("[Controller] Pre Update", k_PreUpdate);
            OperateOnAllController("[Controller] - Update", k_Update);
            OperateOnAllController("[Controller] - Post Update", k_PostUpdate);
        }

        void FixedUpdate()
        {
            OperateOnAllController("[Controller] - Fixed Update", k_FixedUpdate);
        }

        IEnumerator EndOfFrame()
        {
            while (true)
            {
                yield return k_EndOfFrameYield;
                OperateOnAllController("[Controller] - End Of Frame", k_EndOfFrame);
            }
        }

        #endregion

        #region UTILITY

        void OperateOnAllController(string operationName, Action<IController> operation)
        {
            Profiler.BeginSample(operationName);
            foreach (var batch in m_ControllerBatches)
            {
                Profiler.BeginSample("Group: " + batch.name);
                foreach (var controller in batch.controllers)
                {
                    if (controller == null || !controller.enabled)
                        continue;

                    Profiler.BeginSample(controller.controllerName);
                    operation.Invoke(controller);
                    Profiler.EndSample();
                }
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }
        #endregion
    }
}