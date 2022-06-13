using System.Runtime.CompilerServices;
using Saro.Entities.Transforms;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        internal EcsPool<Destroy> DestroyPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_DestroyPool ??= GetPool<Destroy>();
        }

        private EcsPool<Destroy> m_DestroyPool;

        /// <summary>
        /// entity �Ƿ񽫱�����
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsEntityMarkDestroy(int entity)
        {
            return !DestroyPool.Has(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelEntity(int entity)
        {
            // TODO ����
            //  1. DelEntity �ڵ�ʱ���Ƿ���ȷɾ��
            // 2. world.OnDestroy ʱ���Ƿ���ȷɾ��
            // 3. ���� DelEntity �ĵط���Ҫע�⣡
            // 4. �㼶��entity��������ӵ��Children�����ʱ�򣬵���DelEntity�������޷��ݹ�ɾ���ӽڵ�

            // ��children�����entity����Ҫ�ݹ���ã���֤�㼶��ȷ
            if (ChildrenPool.Has(entity))
            {
                EcsTransformUtility.OnEntityDestroy(this.Pack(entity));
            }

            // ֻ��ǣ�����ɾ������ system ��
            DestroyPool.Add(entity);
            //DelEntity_Internal(entity);
        }
    }
}