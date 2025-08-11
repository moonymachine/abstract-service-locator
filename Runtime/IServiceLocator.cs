namespace AbstractServiceLocator
{
	public interface IServiceLocator
	{
		T Get<T>() where T : class;
	}
}
