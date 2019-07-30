using UnityEngine;

namespace Atuvu.Lifecycle
{
    [CreateAssetMenu(fileName = "NewControllerGroup", menuName = "Atuvu/Controller Group")]
    public sealed class ControllerGroup : ScriptableObject
    {
        [SerializeField]
        string m_DisplayName = "Controller Group";

        [SerializeField]
        GameObject[] m_Controllers;

        public string displayName { get { return m_DisplayName; } }
        internal GameObject[] controllers { get { return m_Controllers; } }

        void OnValidate()
        {
            for (var i = 0; i < m_Controllers.Length; ++i)
            {
                var controller = m_Controllers[i];
                if (controller == null)
                    continue;
                
                if (controller.GetComponent(typeof(IController)) == null)
                {
                    m_Controllers[i] = null;
                    Debug.LogError("GameObject in controller group must have a Controller<T> component");
                }
            }
        }
    }
}