using CrossCutting.Core.Contract.DependencyInjection;
using Logic.Business.InjectionManagement.InternalContract.Scene;

namespace Logic.Business.InjectionManagement.Scene
{
    internal class SceneStateFactory : ISceneStateFactory
    {
        private readonly ICoCoKernel _kernel;

        public SceneStateFactory(ICoCoKernel kernel)
        {
            _kernel = kernel;
        }

        public ISceneState Create()
        {
            return _kernel.Get<ISceneState>();
        }
    }
}
