using Saro.Pool;
using Saro.Collections;
using System.Collections.Generic;

namespace Saro.Entities.Extension
{
    /// <summary>
    /// TODO 类似 unity dynamic buffer?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentArray<T> : IEcsAutoReset<ComponentArray<T>>, IEcsComponent
        where T : struct
    {
        //public int length;

        //private T[] m_Values;

        //public ComponentArray(int length)
        //{
        //    m_Values = ArrayPool<T>.Shared.Rent(length);
        //    this.length = m_Values.Length;
        //}

        public void AutoReset(ref ComponentArray<T> c)
        {
            //if (c.m_Values != null)
            //{
            //    ArrayPool<T>.Shared.Return(c.m_Values);
            //    c.m_Values = null;
            //}
        }
    }

    /// <summary>
    /// TODO 实现一个 dynamic buffer 代替
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentList<T> : IEcsAutoReset<ComponentList<T>>, IEcsComponent
    where T : struct
    {
        public List<T> Value => m_Value ??= ListPool<T>.Rent();
        private List<T> m_Value;

        public void AutoReset(ref ComponentList<T> c)
        {
            if (c.m_Value != null)
            {
                ListPool<T>.Return(c.m_Value);
                c.m_Value = null;
            }
        }
    }

    //public struct ComponentStack<T> where T : struct, IEcsComponent
    //{
    //    public Stack<T> Value => m_Value ??= new Stack<T>();
    //    private Stack<T> m_Value;
    //}

    //public struct ComponentQueue<T> where T : struct, IEcsComponent
    //{
    //    public Queue<T> Value => m_Value ??= new Queue<T>();
    //    private Queue<T> m_Value;
    //}
}
