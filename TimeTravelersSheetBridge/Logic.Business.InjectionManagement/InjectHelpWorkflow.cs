using CrossCutting.Abstract.DataClasses;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract.Cryptography;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.InjectionManagement
{
    internal class InjectHelpWorkflow : IInjectHelpWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IChecksum<uint> _checksum;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;
        private readonly IHelpTranslationManager _helpTranslationManager;
        private readonly IFullWidthConverter _fullWidthConverter;

        public InjectHelpWorkflow(InjectionManagementConfiguration config, IChecksumFactory checksumFactory,
            IEventTextParser eventParser, IEventTextComposer eventComposer, IHelpTranslationManager helpTranslationManager,
            IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _checksum = checksumFactory.CreateCrc32Jam();
            _eventParser = eventParser;
            _eventComposer = eventComposer;
            _helpTranslationManager = helpTranslationManager;
            _fullWidthConverter = fullWidthConverter;
        }

        public async Task Run()
        {
            HelpTextData[] helps = await _helpTranslationManager.GetHelps();

            InjectHelpTexts(helps);
            InjectHelpTitles(helps);
        }

        private void InjectHelpTexts(HelpTextData[] helps)
        {
            foreach (HelpTextData help in helps)
            {
                string helpFilePath = Path.Combine(_config.InputFolder, $"HELP{help.Id:000}_ja.cfg.bin");

                if (!File.Exists(helpFilePath))
                    continue;

                var result = new List<string>();
                foreach (TextData helpText in help.Texts)
                {
                    if (string.IsNullOrEmpty(helpText.Text))
                        continue;

                    result.Add(helpText.Text);
                }

                EventTextConfiguration eventConfig = _eventParser.Parse(helpFilePath, StringEncoding.Sjis);

                eventConfig.Texts = result.Select((x, i) => new EventText
                {
                    Hash = _checksum.ComputeValue($"HELP{help.Id:000}"),
                    SubId = i,
                    Text = _fullWidthConverter.Convert(x)
                }).ToArray();

                _eventComposer.Compose(eventConfig, helpFilePath);
            }
        }

        private void InjectHelpTitles(HelpTextData[] helps)
        {
            string helpTitleFilePath = Path.Combine(_config.InputFolder, "Help_List_ja.cfg.bin");

            if (!File.Exists(helpTitleFilePath))
                return;

            EventTextConfiguration eventConfig = _eventParser.Parse(helpTitleFilePath, StringEncoding.Sjis);

            eventConfig.Texts = helps.Select(x => new EventText
            {
                Hash = _checksum.ComputeValue($"HELP{x.Id:000}"),
                SubId = 0,
                Text = _fullWidthConverter.Convert(x.Title.Text)
            }).ToArray();

            _eventComposer.Compose(eventConfig, helpTitleFilePath);
        }
    }
}
