using CrossCutting.Core.Contract.Aspects;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.Exceptions;

namespace Logic.Business.TranslationManagement.Contract
{
    [MapException(typeof(TranslationManagementException))]
    public interface IDecisionTranslationManager
    {
        Task<DecisionTextData[]> GetDecisions();
        Task<DecisionTextData?> GetDecisions(string sceneName);
        Task UpdateDecisionText(string sceneName);
        Task UpdateDecisionText(string[] sceneNames);
    }
}
