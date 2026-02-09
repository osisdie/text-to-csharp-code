namespace TextToCode.Core.Interfaces;

public interface IPromptTemplate
{
    string Name { get; }
    string Render(IDictionary<string, string> variables);
}
