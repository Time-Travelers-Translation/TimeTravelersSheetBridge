using Logic.Business.TranslationManagement.Contract.DataClasses;

namespace Logic.Business.InjectionManagement.InternalContract.Scene
{
    internal interface ISceneState
    {
        void Load(Stream input);
        void Save(Stream input, Stream output);

        void UpdateDecisionTexts(DecisionTextData[] decisionTexts);
        void UpdateTitles(TitleTextData[] badEndTexts);
        void UpdateHints(HintTextData[] hintTexts);
    }
}
