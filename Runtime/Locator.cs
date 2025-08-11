namespace AbstractServiceLocator
{
	// A simple service locator, using an injected interface as the implementation.
	public static class Locator
	{
		private static IServiceLocator ServiceLocator;

		private static System.Func<IServiceLocator> LocatorDelegate;

		// Using a delegate allows a private method to be injected,
		// so it can only be removed by the class that injected it.
		public static void Register(System.Func<IServiceLocator> locatorDelegate)
		{
			if(locatorDelegate == null)
				throw new System.ArgumentNullException("locatorDelegate");

			// The delegate must immediately return the service locator.
			// It is cached to avoid invoking the delegate.
			IServiceLocator serviceLocator = locatorDelegate();
			if(serviceLocator == null)
				throw new System.ArgumentException(
					"The service locator is null.", "locatorDelegate");

			// There can be only one.
			if(LocatorDelegate != null)
				throw new System.InvalidOperationException(
					"Service locator registration conflict.");

			ServiceLocator = serviceLocator;
			LocatorDelegate = locatorDelegate;
		}

		public static void Remove(System.Func<IServiceLocator> locatorDelegate)
		{
			// A matching delegate must be provided, or it will not be removed.
			if(LocatorDelegate == locatorDelegate)
			{
				LocatorDelegate = null;
				ServiceLocator = null;
			}
		}

		public static T Get<T>() where T : class
		{
			T instance = null;
			if(ServiceLocator != null)
				instance = ServiceLocator.Get<T>();
			return instance;
		}

		public static bool TryGet<T>(out T instance) where T : class
		{
			instance = null;
			if(ServiceLocator != null)
				instance = ServiceLocator.Get<T>();
			return instance != null;
		}
	}
}
