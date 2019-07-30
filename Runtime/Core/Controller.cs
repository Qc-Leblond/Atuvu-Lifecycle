#define PROTECTED_INSTANCE

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Atuvu.Lifecycle
{
    internal interface IController
    {
        bool enabled { get; }
        string controllerName { get; }

        void AddSceneObject(SceneObject obj);
        void RemoveSceneObject(SceneObject obj);
        void SetInstance(GameObject instance);

        void DoControllerStarted();
        void DoControllerStopped();

        void DoRegisterCallbacks();
        void DoUnregisterCallbacks();

        void DoPreUpdate(float deltaTime);
        void DoUpdate(float deltaTime);
        void DoPostUpdate(float deltaTime);
        void DoEndOfFrame(float deltaTime);
        void DoFixedUpdate();
    }

    [DisallowMultipleComponent]
    public abstract class Controller<T> : MonoBehaviour, IController
        where T : Controller<T>
    {
        static T s_Instance;

        string m_ControllerName = null;
        List<SceneObject> m_BoundObjects = new List<SceneObject>(32);

        void IController.SetInstance(GameObject instance)
        {
            instance.GetComponents(TempBuffers.monoBehaviours);
            var components = TempBuffers.monoBehaviours;

            foreach (var component in components)
            {
                var controller = component as T;
                if (controller != null)
                {
                    s_Instance = controller;
                    return;
                }
            }

            Debug.LogAssertion($"Failed to find controller component of type {GetType()} on object {instance.name}.");
        }

        void IController.AddSceneObject(SceneObject obj)
        {
            m_BoundObjects.Add(obj);
        }

        void IController.RemoveSceneObject(SceneObject obj)
        {
            m_BoundObjects.Remove(obj);
        }

        string IController.controllerName
        {
            get
            {
                if (m_ControllerName == null)
                    m_ControllerName = GetType().Name;

                return m_ControllerName;
            }
        }

        void IController.DoControllerStarted() { OnControllerStarted(); }
        void IController.DoControllerStopped() { OnControllerStopped(); }

        void IController.DoRegisterCallbacks()
        {
            RegisterCallbacks();
        }

        void IController.DoUnregisterCallbacks()
        {
            UnregisterCallbacks();
        }

        void IController.DoPreUpdate(float deltaTime)
        {
            OnPreUpdate(deltaTime);
        }

        void IController.DoUpdate(float deltaTime)
        {
            OnUpdate(deltaTime);
        }

        void IController.DoPostUpdate(float deltaTime)
        {
            OnPostUpdate(deltaTime);
        }

        void IController.DoEndOfFrame(float deltaTime)
        {
            OnEndOfFrame(deltaTime);
        }

        void IController.DoFixedUpdate()
        {
            OnFixedUpdate();
        }
        
        protected virtual void OnControllerStarted() { }
        protected virtual void OnControllerStopped() { }
        protected virtual void RegisterCallbacks() { }
        protected virtual void UnregisterCallbacks() { }
        protected virtual void OnPreUpdate(float deltaTime) { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnPostUpdate(float deltaTime) { }
        protected virtual void OnEndOfFrame(float deltaTime) { }
        protected virtual void OnFixedUpdate() { }
    }
}