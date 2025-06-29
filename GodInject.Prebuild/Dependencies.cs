using DryIoc;
internal class Dependencies
{
    static Dependencies()
    {
        Container = new Container();    
    }
    internal static IContainer Container { get; private set; }
}
