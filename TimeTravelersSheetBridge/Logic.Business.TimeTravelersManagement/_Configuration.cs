using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.TimeTravelersManagement
{
    public class TimeTravelersManagementConfiguration
    {
        [ConfigMap("Logic.Business.TimeTravelersManagement", "PatchMapPath")]
        public virtual string PatchMapPath { get; set; }
    }
}