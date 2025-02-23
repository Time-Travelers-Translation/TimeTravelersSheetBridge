using CrossCutting.Core.Contract.Aspects;
using Logic.Business.InjectionManagement.Contract.Exceptions;

namespace Logic.Business.InjectionManagement.Contract
{
    [MapException(typeof(InjectionManagementException))]
    public interface IInjectionWorkflow
    {
        Task<int> Execute();
    }
}
