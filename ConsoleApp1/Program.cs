using Castle.DynamicProxy;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

var defaultProxyBuilder = new DefaultProxyBuilder();
Type interfaceProxyTypeWithTarget = defaultProxyBuilder.CreateInterfaceProxyTypeWithTarget(typeof(INestedObject),
    Type.EmptyTypes, typeof(NestedObject), ProxyGenerationOptions.Default);
INestedObject nestedObject = (INestedObject)Activator.CreateInstance(interfaceProxyTypeWithTarget,
    Array.Empty<IInterceptor>(),
    new NestedObject()); // replace this with new NestedObject() to see it's working fine
var parentObject = new ParentObject();

nestedObject.Input = "1"; // reported, all fine
parentObject.NestedObject = nestedObject;
nestedObject.Input = "2"; // todo: not reported
nestedObject.Input = "3"; // todo: not reported
nestedObject.Input = "4"; // todo: sometimes reported probably due disposing

public interface INestedObject : IReactiveObject
{
    string Input { get; set; }
}

class NestedObject : ReactiveObject, INestedObject
{
    [Reactive] public string Input { get; set; }
}

class ParentObject : ReactiveObject
{
    [Reactive] public INestedObject NestedObject { get; set; }

    public ParentObject()
    {
        this.WhenAnyValue(parent => parent.NestedObject,
                parent => parent.NestedObject.Input,
                (parent, _) => parent?.Input)
            .Subscribe(str =>
            {
                Console.WriteLine(str);
            });
    }
}