namespace Saro.Entities
{
    /// <summary>
    /// Ecs托管组件必须 *继承* 此接口
    /// </summary>
    public interface IEcsComponent { }

    public interface IEcsComponentSingleton : IEcsComponent { }

    ///// <summary>
    ///// Ecs托管组件必须 *继承* 此接口
    ///// </summary>
    //public interface IEcsManagedComponent<T> : IEcsComponent, IEcsAutoReset<T> where T : class, new() { }

    ///// <summary>
    ///// Ecs非托管组件必须 *继承* 此接口
    ///// </summary>
    //public interface IEcsUnmanagedComponent<T> : IEcsComponent where T : unmanaged { }

    ///// <summary>
    ///// 托管单例组件继承这个！
    ///// </summary>
    //public interface IEcsManagedComponentSingleton<T> : IEcsManagedComponent<T> where T : class, new() { }

    ///// <summary>
    ///// 托管单例组件继承这个！
    ///// </summary>
    //public interface IEcsUnmanagedComponentSingleton<T> : IEcsUnmanagedComponent<T> where T : unmanaged { }
}