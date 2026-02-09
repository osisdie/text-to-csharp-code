namespace TextToCode.Core.Interfaces;

public interface IPromptTemplateFactory
{
    IPromptTemplate GetCodeGenerationTemplate();
    IPromptTemplate GetSelfHealingTemplate();
}
