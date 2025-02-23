using CrossCutting.Core.Contract.Aspects;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.Exceptions;

namespace Logic.Business.TranslationManagement.Contract
{
    [MapException(typeof(TranslationManagementException))]
    public interface IHelpTranslationManager
    {
        Task<HelpTextData[]> GetHelps();
        Task<HelpTextData?> GetHelp(int index);
        Task UpdateHelps(int[] helps);
    }
}
