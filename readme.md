# ecslite


-----------------
## 单例组件

```csharp
private int m_SingletonEntityId = -1;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public int GetSingletonEntity()
{
    if (m_SingletonEntityId < 0)
        m_SingletonEntityId = NewEntity();

    return m_SingletonEntityId;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref T GetSingleton<T>() where T : struct, IEcsComponent
{
    var singletonID = GetSingletonEntity();

    return ref GetPool<T>(0, 1).Add(singletonID);
}
```

-----------------
## 数据蓝图，可编辑entity资源，通过代码一键生成想要的实体

<img src="https://github.com/Sarofc/ecslite/blob/main/doc/pic2.jpg" width=50%>

-----------------
## transfrom层级系统

```csharp
EcsEntity root = CreateNode(world, "root");

EcsEntity child1 = CreateNode(world, "child1");
child1.SetParent(root);

EcsEntity child2 = CreateNode(world, "child2");
child2.SetParent(root);

EcsEntity child3 = CreateNode(world, "child3");
child3.SetParent(root);
child3.SetLocalPosition(new Vector3(-6, 0, 0));

EcsEntity child4 = CreateNode(world, "child4");
child4.SetParent(child3);
```

-----------------
## UnityEditor增强

<img src="https://github.com/Sarofc/ecslite/blob/main/doc/pic0.jpg" width=50%>
<img src="https://github.com/Sarofc/ecslite/blob/main/doc/pic1.jpg" width=40%>