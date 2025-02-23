using Logic.Business.InjectionManagement.InternalContract;

namespace Logic.Business.InjectionManagement
{
    internal class ConfigurationValidator : IConfigurationValidator
    {
        public void Validate(InjectionManagementConfiguration config)
        {
            if (config.ShowHelp)
                return;

            ValidateInput(config);
            ValidateMode(config);
            ValidateModeArguments(config);
        }

        private void ValidateInput(InjectionManagementConfiguration config)
        {
            if (!Directory.Exists(config.InputFolder))
                throw new InvalidOperationException($"Directory '{config.InputFolder}' does not exist.");
        }

        private void ValidateMode(InjectionManagementConfiguration config)
        {
            switch (config.Mode)
            {
                case "tuto":
                case "tips":
                case "help":
                case "outline":
                case "flow":
                case "c1":
                case "c2":
                case "c3":
                case "c4":
                case "c5":
                case "c6":
                case "c7":
                case "scn":
                case "staffroll":
                    return;

                default:
                    throw new InvalidOperationException($"Invalid mode '{config.Mode}'.");
            }
        }

        private void ValidateModeArguments(InjectionManagementConfiguration config)
        {
            if (config is { Mode: "scn", ModeArguments.Length: < 1 })
                throw new InvalidOperationException("Mode 'scn' requires one mode argument: 'Path/to/nrm_main.xf'");
        }
    }
}
