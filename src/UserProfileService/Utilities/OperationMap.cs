namespace UserProfileService.Utilities;

public class OperationMap
{
    internal string ActionName { get; set; }
    internal Type Controller { get; set; }
    internal string Operation { get; set; }

    public OperationMap(string operation, Type controllerType, string methodName)
    {
        Operation = operation;
        Controller = controllerType;
        ActionName = methodName;
    }
}
