using CrossCutting.Core.Contract.Aspects;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.Exceptions;

namespace Logic.Business.TranslationManagement.Contract
{
    [MapException(typeof(TranslationManagementException))]
    public interface ITipTranslationManager
    {
        Task<TipTextData[]> GetTips();
        Task<TipTextData?> GetTip(int index);
        Task UpdateTips(int[] indexes);
    }
}
