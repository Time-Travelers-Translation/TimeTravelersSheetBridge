using CrossCutting.Abstract.DataClasses;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.InjectionManagement
{
    internal class InjectStaffrollWorkflow : IInjectStaffrollWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;
        private readonly IStaffrollTranslationManager _staffrollTranslationManager;

        public InjectStaffrollWorkflow(InjectionManagementConfiguration config, IEventTextParser eventParser,
            IEventTextComposer eventComposer, IStaffrollTranslationManager staffrollTranslationManager,
            IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _eventParser = eventParser;
            _eventComposer = eventComposer;
            _staffrollTranslationManager = staffrollTranslationManager;
        }

        public async Task Run()
        {
            StaffrollTextData[] staffrolls = await _staffrollTranslationManager.GetStaffRolls();

            InjectStaffrollTexts(staffrolls);
        }

        private void InjectStaffrollTexts(StaffrollTextData[] staffrolls)
        {
            string staffrollFilePath = Path.Combine(_config.InputFolder, "staffroll_ja.cfg.bin");

            var result = new List<EventText>();
            foreach (StaffrollTextData staffroll in staffrolls)
            {
                result.Add(new EventText
                {
                    Hash = staffroll.Hash,
                    SubId = staffroll.Flag,
                    Text = staffroll.Text.Text
                });
            }

            EventTextConfiguration eventConfig = _eventParser.Parse(staffrollFilePath, StringEncoding.Sjis);

            eventConfig.Texts = result.ToArray();

            _eventComposer.Compose(eventConfig, staffrollFilePath);
        }
    }
}
