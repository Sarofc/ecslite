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

    /// <summary>
    /// component 实现这个接口，才能被 <see cref="GenericEntityAuthoring"/> 所使用
    /// </summary>
    public interface IEcsComponentAuthoring : IEcsComponent
    { }
}