### My set of scripts that will be useful to everyone...
> GameLoop.cs 
> > The script is useful for optimizing a large number of update calls
> > 
> > Example
> > ```cs
> >    public class Example : MonoBehaviour, IFixedUpdate
> >    {
> >         private void OnEnable()
> >         {
> >             GameLoop.Register(this);
> >         }
> > 
> >         private void OnDisable()
> >         {
> >             GameLoop.Unregister(this);
> >         }
> >         
> >         void IFixedUpdate.FixedUpdate()
> >         {
> >             Debug.Log("IFixedUpdate");
> >         }
> >    }
> > ```
> StaticMonoBehaviour.cs 
> > The script will simplify the use of singletons
> > 
> > To initialize the script, you need to call the Get() method
> > ```cs
> > ExampleStatic.Get();
> > ```
> >
> > Example
> > ```cs
> >    public class ExampleStatic : StaticMonoBehaviour<ExampleStatic>
> >    {
> >         void Start()
> >         {
> >             Debug.Log("Example static");
> >         }
> >    }
> > ```
> FixedList.cs 
> > The same as List, but when deleting or adding elements, the array does not change in any way
> >
> FixedQueue.cs 
> > The same as Queue, but when adding to the queue or pulling out of the queue, the array does not change in any way
> >
> DebugEx.cs 
> > Drawing primitive geometry using Debug.DrawLine
> >
> GameBuilder.cs 
> > A simple game collector, it is possible to specify the manifest of asset bundles to exclude assets
![](https://github.com/redheadgektor/UnityExtensions/blob/main/gamebuilder_demo.gif)
> >
> AssetBundleBuilder.cs
> > A linker of assets into bundles without explicitly labeling assets to bundles...
> >
