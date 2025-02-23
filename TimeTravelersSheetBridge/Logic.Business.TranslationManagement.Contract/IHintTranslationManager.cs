using CrossCutting.Core.Contract.Aspects;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.Exceptions;

namespace Logic.Business.TranslationManagement.Contract
{
    [MapException(typeof(TranslationManagementException))]
    public interface IHintTranslationManager
    {
        Task<HintTextData[]> GetHints();
        Task<HintTextData?> GetHints(string sceneName);
        Task UpdateSceneHint(string[] sceneNames);
    }
}
