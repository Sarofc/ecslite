using UnityEngine;

namespace Leopotam.EcsLite.Authoring
{
    // TODO 这个貌似不太容易支持 entity 层级化
    // 可能还是得搞个 authoring 的东西，来序列化，充分利用monobehaviour
    // authoring 以entity为单位？这样方便嵌套entity？

    // 需要想办法将层级关系自动化

    // 这个就可以当作是 prefab 来用了
    // 数据表的话，可以考虑 生成代码，一个api搞定entity数据赋值。复杂的赋值，依然是手动写代码

    // 补充，华佗也支持直接挂载脚本，所以这个应该不是啥问题了

    public interface IEcsConvertToEntity
    {
        int ConvertToEntity(EcsWorld world);
    }

    public abstract class MonoEntityAuthoring : MonoBehaviour, IEcsConvertToEntity
    {
        public abstract int ConvertToEntity(EcsWorld world);
    }

    public abstract class SOEntityAuthoring : ScriptableObject, IEcsConvertToEntity
    {
        public abstract int ConvertToEntity(EcsWorld world);
    }
}