using Logic.Business.InjectionManagement.Contract;
using Logic.Business.InjectionManagement.InternalContract;

namespace Logic.Business.InjectionManagement
{
    internal class InjectionWorkflow : IInjectionWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IConfigurationValidator _configValidator;
        private readonly IInjectTutorialWorkflow _tutoWorkflow;
        private readonly IInjectTipsWorkflow _tipsWorkflow;
        private readonly IInjectHelpWorkflow _helpWorkflow;
        private readonly IInjectFlowWorkflow _flowWorkflow;
        private readonly IInjectScnWorkflow _scnWorkflow;
        private readonly IInjectOutlineWorkflow _outlineWorkflow;
        private readonly IInjectChapter1Workflow _chapter1Workflow;
        private readonly IInjectChapter2Workflow _chapter2Workflow;
        private readonly IInjectChapter3Workflow _chapter3Workflow;
        private readonly IInjectChapter4Workflow _chapter4Workflow;
        private readonly IInjectChapter5Workflow _chapter5Workflow;
        private readonly IInjectChapter6Workflow _chapter6Workflow;
        private readonly IInjectChapter7Workflow _chapter7Workflow;
        private readonly IInjectStaffrollWorkflow _staffrollWorkflow;

        public InjectionWorkflow(InjectionManagementConfiguration config, IConfigurationValidator configValidator,
            IInjectTutorialWorkflow tutoWorkflow, IInjectTipsWorkflow tipsWorkflow, IInjectHelpWorkflow helpWorkflow,
            IInjectFlowWorkflow flowWorkflow, IInjectScnWorkflow scnWorkflow, IInjectOutlineWorkflow outlineWorkflow,
            IInjectChapter1Workflow chapter1Workflow, IInjectChapter2Workflow chapter2Workflow, IInjectChapter3Workflow chapter3Workflow,
            IInjectChapter4Workflow chapter4Workflow, IInjectChapter5Workflow chapter5Workflow, IInjectChapter6Workflow chapter6Workflow,
            IInjectChapter7Workflow chapter7Workflow, IInjectStaffrollWorkflow staffrollWorkflow)
        {
            _config = config;
            _configValidator = configValidator;
            _tutoWorkflow = tutoWorkflow;
            _tipsWorkflow = tipsWorkflow;
            _helpWorkflow = helpWorkflow;
            _flowWorkflow = flowWorkflow;
            _scnWorkflow = scnWorkflow;
            _outlineWorkflow = outlineWorkflow;
            _chapter1Workflow = chapter1Workflow;
            _chapter2Workflow = chapter2Workflow;
            _chapter3Workflow = chapter3Workflow;
            _chapter4Workflow = chapter4Workflow;
            _chapter5Workflow = chapter5Workflow;
            _chapter6Workflow = chapter6Workflow;
            _chapter7Workflow = chapter7Workflow;
            _staffrollWorkflow = staffrollWorkflow;
        }

        public async Task<int> Execute()
        {
            if (_config.ShowHelp || Environment.GetCommandLineArgs().Length <= 1)
            {
                PrintHelp();
                return 0;
            }

            if (!IsValidConfig())
            {
                PrintHelp();
                return 0;
            }

            await InjectTexts();

            return 0;
        }

        private async Task InjectTexts()
        {
            switch (_config.Mode)
            {
                case "tuto":
                    await _tutoWorkflow.Run();
                    break;

                case "tips":
                    await _tipsWorkflow.Run();
                    break;

                case "help":
                    await _helpWorkflow.Run();
                    break;

                case "outline":
                    await _outlineWorkflow.Run();
                    break;

                case "staffroll":
                    await _staffrollWorkflow.Run();
                    break;

                case "flow":
                    await _flowWorkflow.Run();
                    break;

                case "scn":
                    await _scnWorkflow.Run();
                    break;

                case "c1":
                    await _chapter1Workflow.Run();
                    break;

                case "c2":
                    await _chapter2Workflow.Run();
                    break;

                case "c3":
                    await _chapter3Workflow.Run();
                    break;

                case "c4":
                    await _chapter4Workflow.Run();
                    break;

                case "c5":
                    await _chapter5Workflow.Run();
                    break;

                case "c6":
                    await _chapter6Workflow.Run();
                    break;

                case "c7":
                    await _chapter7Workflow.Run();
                    break;
            }
        }

        private bool IsValidConfig()
        {
            try
            {
                _configValidator.Validate(_config);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Input parameters are incorrect: {GetInnermostException(e).Message}");
                Console.WriteLine();

                return false;
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("Following commands exist:");
            Console.WriteLine("  -h, --help\tShows this help message.");
            Console.WriteLine("  -m, --mode\tThe type of text file to inject:");
            Console.WriteLine("    'tuto'\tInjects all tutorial texts");
            Console.WriteLine("    'tips'\tInjects all tip texts");
            Console.WriteLine("    'help'\tInjects all help texts");
            Console.WriteLine("    'outline'\tInjects all outline texts");
            Console.WriteLine("    'flow'\tInjects all scene titles");
            Console.WriteLine("    'scn'\tInjects all bad end titles, hints, and decisions");
            Console.WriteLine("    'c1'\tInjects all story texts related to chapter 1");
            Console.WriteLine("    'c2'\tInjects all story texts related to chapter 2");
            Console.WriteLine("    'c3'\tInjects all story texts related to chapter 3");
            Console.WriteLine("    'c4'\tInjects all story texts related to chapter 4");
            Console.WriteLine("    'c5'\tInjects all story texts related to chapter 5");
            Console.WriteLine("    'c6'\tInjects all story texts related to chapter 6");
            Console.WriteLine("    'c7'\tInjects all story texts related to chapter 7");
            Console.WriteLine("    'staffroll'\tInjects all entries in the staffroll");
            Console.WriteLine("  -a, --args\tAdditional arguments for each type of file.");
        }

        private Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null)
                e = e.InnerException;

            return e;
        }
    }
}
