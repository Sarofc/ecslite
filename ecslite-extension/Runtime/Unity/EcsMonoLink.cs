using UnityEngine;

namespace Saro.Entities.Extension
{
    public class EcsMonoLink : MonoBehaviour, IEcsMonoLink
    {
        public ref EcsEntity Entity => ref m_Entity;

        public bool IsAlive => m_Entity.IsAlive();

        private EcsEntity m_Entity;

        public void Link(in EcsEntity ent) => m_Entity = ent;
    }
}
