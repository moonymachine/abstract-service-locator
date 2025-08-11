# Abstract Service Locator

This is an abstract service locator with no concrete implementation, used to facilitate dependency injection when constructors are unavailable. It was created for use with Unity applications, but it can be applied to any development environment that prohibits constructor based dependency injection.

The Service Locator pattern is often considered an antipattern, to be avoided. However, arguments against using a service locator typically point to constructor based dependency injection as the appropriate alternative. In Unity, code is generally added as MonoBehaviour components. MonoBehaviour scripts do not support constructor based dependency injection. In an environment where constructor based dependency injection is impossible, many arguments against the Service Locator pattern become invalid.

### Requirements

A proper, concrete `IServiceLocator` implementation that can be injected and removed from the static `Locator` at the appropriate time requires Unity 2018. That is because Unity 2018 is the earliest version that contains both the `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` attribute (added in version 5.2) and the `Application.quitting` event. Together they provide the best way to initialize and shut down a concrete `IServiceLocator`.

### Installation

This package can be installed via Unity's Package Manager.

- Open the Package Manager window.
- Open the Add (+) menu in the toolbar.
- Select the "Install package from git URL" button.
- Enter this URL: https://github.com/moonymachine/abstract-service-locator.git
- Select Install.

### How to Use

Use `Locator.Get<T>()` or `Locator.TryGet<T>(out T)` to resolve dependencies in your MonoBehaviour components.

```csharp
using AbstractServiceLocator;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
	private IService Service;
	private ILog Logger;

	// Get services on Awake.
	private void Awake()
	{
		Logger = Locator.Get<ILog>();
		// or
		Locator.TryGet(out Logger);

		// TryGet can be used as a condition.
		if(!Locator.TryGet(out Service))
			Logger?.LogWarning("A required service is unavailable!");
	}

	private void OnEnable()
	{
		if(Service == null)
		{
			// You can set enabled = false and return early to emulate a constructor exception.
			Logger?.LogError("A required service is unavailable!");
			enabled = false;
			return;
		}

		Service.Foo += Foo;
		Service.Bar();
	}

	// OnApplicationQuit happens just before shut down. Set service references to null.
	// The logger reference may be a rare exception to keep.
	// Any logger service should be static, and you should null check logger calls.
	private void OnApplicationQuit()
	{
		if(Service != null)
		{
			Service.Foo -= Foo;
			Service = null;
		}
	}

	// OnDisable and OnDestroy can be called before or after OnApplicationQuit, so check for null.
	private void OnDisable()
	{
		if(Service != null)
		{
			Service.Foo -= Foo;
		}
	}

	private void Foo() { }
}
```

### How to Implement

This package does not include a concrete implementation of the `IServiceLocator` interface. Therefore, tight coupling is limited to the empty static `Locator` in this package. Here is an example of how you can create and inject an actual service locator implementation for Unity. For a full implementation, see n Dependents: https://github.com/moonymachine/n-dependents

```csharp
using System;
using AbstractServiceLocator;
using UnityEngine;

public static class CompositionRoot
{
	private static IServiceContainer ServiceContainer;
	private static IServiceLocator GetServiceLocator()
	{
		return ServiceContainer;
	}

	static CompositionRoot()
	{
		Application.quitting += Quitting;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		CompositionRootAsset compositionRootAsset = Resources.Load<CompositionRootAsset>(CompositionRootAsset.FileName);
		if(compositionRootAsset == null)
		{
			Debug.LogError("Failed to load the required composition root resource. Please create the required asset in a Resources directory.");
		}
		else
		{
			IServiceContainerAsset serviceContainerAsset = compositionRootAsset.ServiceContainer as IServiceContainerAsset;
			if(serviceContainerAsset == null)
			{
				Debug.LogError("No service container asset has been assigned to the composition root.");
			}
			else
			{
				try
				{
					ServiceContainer = serviceContainerAsset.InitializeServiceContainer();
					Locator.Register(GetServiceLocator);
				}
				catch(Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	private static void Quitting()
	{
		Locator.Remove(GetServiceLocator);

		if(ServiceContainer != null)
		{
			try
			{
				ServiceContainer.ShutDown();
			}
			catch(Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				ServiceContainer = null;
			}
		}
	}
}
```
