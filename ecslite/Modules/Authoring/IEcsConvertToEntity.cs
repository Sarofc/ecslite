using System;
using Saro;

namespace Saro.Entities.Authoring
{
    /*
        1. authoring 以entity为单位，这样方便使用 GenericEntityAuthoring、ChildrenForAuthoring 嵌套
        2. 数据表的话，可以考虑 生成代码，一个api搞定entity数据赋值。
        3. 复杂的赋值，依然是手动写代码
        4. 这个就可以当作是 prefab 来用了

        补充，华佗也支持直接挂载脚本，所以这个应该不是啥问题了
    */
    public interface IEcsComponentAuthoring : IEcsComponent
    { }

    /// <summary>
    /// 此组件不会被添加！<see cref="GenericEntityAuthoring"/>
    /// </summary>
    public interface IEcsComponentNotAdd
    { }

    /// <summary>
    /// 添加全部组件后初始化！建议单个组件不要依赖过多组件！<see cref="GenericEntityAuthoring"/>
    /// </summary>
    public interface IEcsComponentPostInit
    {
        void PostInitialize(EcsWorld world, int entity);
    }

    /// <summary>
    /// Ecs转换接口
    /// </summary>
    public interface IEcsConvertToEntity
    {
        int ConvertToEntity(EcsWorld world);
    }
}