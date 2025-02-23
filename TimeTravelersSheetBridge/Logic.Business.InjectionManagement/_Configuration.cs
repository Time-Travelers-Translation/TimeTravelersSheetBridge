using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.InjectionManagement
{
    public class InjectionManagementConfiguration
    {
        [ConfigMap("CommandLine", new[] { "h", "help" })]
        public virtual bool ShowHelp { get; set; } = false;

        [ConfigMap("CommandLine", new[] { "i", "input" })]
        public virtual string InputFolder { get; set; }

        [ConfigMap("CommandLine", new[] { "m", "mode" })]
        public virtual string Mode { get; set; }

        [ConfigMap("CommandLine", new[] { "a", "args" })]
        public virtual string[] ModeArguments { get; set; }
    }
}