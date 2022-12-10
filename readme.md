# ecslite

-----------------
## 单例组件

```csharp
struct YourSingletonComponent : IEcsComponentSingleton
{
    public int someValue;
}

ref var singletonComp = ref world.GetSingleton<YourSingletonComponent>();
```

-----------------
## transform层级系统

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