﻿using System;
using UnityEngine;

namespace Atuvu.Lifecycle
{
    public abstract class SceneObject : MonoBehaviour
    {
        internal virtual Type GetControllerOwner()
        {
            return typeof(ManagerlessController);
        }

        void OnEnableINTERNAL()
        {
            LifecycleManager.RegisterSceneObject(this);
        }

        void OnDisableINTERNAL()
        {
            LifecycleManager.UnregisterSceneObject(this);
        }
    }

    public abstract class SceneObject<T> : SceneObject
        where T : Controller<T>
    {
        static readonly Type s_CachedControllerType = typeof(T);

        internal sealed override Type GetControllerOwner()
        {
            return s_CachedControllerType;
        }
    }
}