using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.TimeTravelersManagement.Contract;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TimeTravelersManagement.Texts;

namespace Logic.Business.TimeTravelersManagement
{
    public class TimeTravelersManagementActivator : IComponentActivator
    {
        public void Activating()
        {
        }

        public void Activated()
        {
        }

        public void Deactivating()
        {
        }

        public void Deactivated()
        {
        }

        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<IEventTextParser, EventTextParser>(ActivationScope.Unique);
            kernel.Register<IEventTextComposer, EventTextComposer>(ActivationScope.Unique);

            kernel.Register<ICharacterProvider, CharacterProvider>(ActivationScope.Unique);

            kernel.RegisterConfiguration<TimeTravelersManagementConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
