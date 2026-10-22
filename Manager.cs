namespace ManagerNM;

public class Manager
{
    public void Start(string[]? args = null)
    {
        var application = new RadApp.Cli.RadiationCalculatorCli();
        application.Run(args ?? []);
    }
}
