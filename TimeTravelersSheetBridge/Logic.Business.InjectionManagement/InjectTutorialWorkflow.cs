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
    internal class InjectTutorialWorkflow : IInjectTutorialWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IChecksum<uint> _checksum;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;
        private readonly ITutorialTranslationManager _tutorialTranslationManager;
        private readonly IFullWidthConverter _fullWidthConverter;

        public InjectTutorialWorkflow(InjectionManagementConfiguration config, IChecksumFactory checksumFactory,
            IEventTextParser eventParser, IEventTextComposer eventComposer, ITutorialTranslationManager tutorialTranslationManager,
            IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _checksum = checksumFactory.CreateCrc32Jam();
            _eventParser = eventParser;
            _eventComposer = eventComposer;
            _tutorialTranslationManager = tutorialTranslationManager;
            _fullWidthConverter = fullWidthConverter;
        }

        public async Task Run()
        {
            TutorialTextData[] tutorials = await _tutorialTranslationManager.GetTutorials();

            InjectTutorialTexts(tutorials);
            InjectTutorialTitles(tutorials);
        }

        private void InjectTutorialTexts(TutorialTextData[] tutorials)
        {
            foreach (TutorialTextData tutorial in tutorials)
            {
                string tutorialFilePath = Path.Combine(_config.InputFolder, $"TUTO{tutorial.Id:000}_ja.cfg.bin");

                if (!File.Exists(tutorialFilePath))
                    continue;

                var result = new List<string>();
                foreach (TextData tutorialText in tutorial.Texts)
                {
                    if (string.IsNullOrEmpty(tutorialText.Text))
                        continue;

                    result.Add(tutorialText.Text);
                }

                EventTextConfiguration eventConfig = _eventParser.Parse(tutorialFilePath, StringEncoding.Sjis);

                eventConfig.Texts = result.Select((x, i) => new EventText
                {
                    Hash = _checksum.ComputeValue($"TUTO{tutorial.Id:000}"),
                    SubId = i,
                    Text = ProcessTutorialText(x)
                }).ToArray();

                _eventComposer.Compose(eventConfig, tutorialFilePath);
            }
        }

        private string? ProcessTutorialText(string text)
        {
            if (!text.EndsWith("<I>"))
                text += "<I>";

            return _fullWidthConverter.Convert(text);
        }

        private void InjectTutorialTitles(TutorialTextData[] tutorials)
        {
            string tutorialTitleFilePath = Path.Combine(_config.InputFolder, "Tuto_List_ja.cfg.bin");

            if (!File.Exists(tutorialTitleFilePath))
                return;

            EventTextConfiguration eventConfig = _eventParser.Parse(tutorialTitleFilePath, StringEncoding.Sjis);

            eventConfig.Texts = tutorials.Select(x => new EventText
            {
                Hash = _checksum.ComputeValue($"TUTO{x.Id:000}"),
                SubId = 0,
                Text = _fullWidthConverter.Convert(x.Title.Text)
            }).ToArray();

            _eventComposer.Compose(eventConfig, tutorialTitleFilePath);
        }
    }
}
