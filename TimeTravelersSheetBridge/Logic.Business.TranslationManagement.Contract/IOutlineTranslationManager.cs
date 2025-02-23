using CrossCutting.Core.Contract.Aspects;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.Exceptions;

namespace Logic.Business.TranslationManagement.Contract
{
    [MapException(typeof(TranslationManagementException))]
    public interface IOutlineTranslationManager
    {
        Task<OutlineTextData[]> GetOutlines();
        Task<OutlineTextData?> GetOutline(string route);
        Task UpdateOutlines(string route);
    }
}
