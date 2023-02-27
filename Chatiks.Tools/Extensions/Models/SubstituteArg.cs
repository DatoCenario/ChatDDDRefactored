namespace Chatiks.Tools.Extensions.Models;

public class SubstituteArg<T>
{
    public SubstituteArg(string argName)
    {
        ArgName = argName;
    }

    public string ArgName { get; }

    public T GetValue()
    {
        return default;
    }
}