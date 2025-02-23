using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.InjectionManagement.Contract;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.InjectionManagement.InternalContract.Scene;
using Logic.Business.InjectionManagement.Scene;

namespace Logic.Business.InjectionManagement
{
    public class InjectionManagementActivator : IComponentActivator
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
            kernel.Register<IInjectionWorkflow, InjectionWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter1Workflow, InjectChapter1Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter2Workflow, InjectChapter2Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter3Workflow, InjectChapter3Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter4Workflow, InjectChapter4Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter5Workflow, InjectChapter5Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter6Workflow, InjectChapter6Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectChapter7Workflow, InjectChapter7Workflow>(ActivationScope.Unique);
            kernel.Register<IInjectFlowWorkflow, InjectFlowWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectHelpWorkflow, InjectHelpWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectOutlineWorkflow, InjectOutlineWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectScnWorkflow, InjectScnWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectStaffrollWorkflow, InjectStaffrollWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectTipsWorkflow, InjectTipsWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectTutorialWorkflow, InjectTutorialWorkflow>(ActivationScope.Unique);

            kernel.Register<IFullWidthConverter, FullWidthConverter>(ActivationScope.Unique);

            kernel.Register<ISceneState, SceneState>(ActivationScope.Request);
            kernel.Register<ISceneStateFactory, SceneStateFactory>(ActivationScope.Unique);

            kernel.Register<IConfigurationValidator, ConfigurationValidator>(ActivationScope.Unique);

            kernel.RegisterConfiguration<InjectionManagementConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
