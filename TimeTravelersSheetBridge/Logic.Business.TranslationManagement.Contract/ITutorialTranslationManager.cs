using Logic.Business.TranslationManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract
{
    public interface ITutorialTranslationManager
    {
        Task<TutorialTextData[]> GetTutorials();
        Task<TutorialTextData?> GetTutorial(int index);
        Task UpdateTutorials(int[] tutorials);
    }
}
