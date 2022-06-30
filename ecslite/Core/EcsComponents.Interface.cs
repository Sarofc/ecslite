namespace Saro.Entities
{
    /// <summary>
    /// Ecs组件必须 *继承/间接继承* 此接口
    /// <code>struct 持有 class引用，是浅拷贝，只拷贝引用</code>
    /// </summary>
    public interface IEcsComponent { }

    /// <summary>
    /// 单例组件继承这个！
    /// </summary>
    public interface IEcsComponentSingleton : IEcsComponent { }
}