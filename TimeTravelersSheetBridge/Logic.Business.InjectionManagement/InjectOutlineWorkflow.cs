using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.InjectionManagement
{
    internal class InjectOutlineWorkflow : IInjectOutlineWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;
        private readonly IOutlineTranslationManager _outlineTranslationManager;
        private readonly IFullWidthConverter _fullWidthConverter;

        public InjectOutlineWorkflow(InjectionManagementConfiguration config, IEventTextParser eventParser,
            IEventTextComposer eventComposer, IOutlineTranslationManager outlineTranslationManager,
            IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _eventParser = eventParser;
            _eventComposer = eventComposer;
            _outlineTranslationManager = outlineTranslationManager;
            _fullWidthConverter = fullWidthConverter;
        }

        public async Task Run()
        {
            OutlineTextData[] outlines = await _outlineTranslationManager.GetOutlines();

            InjectOutlineTexts(outlines);
        }

        private void InjectOutlineTexts(OutlineTextData[] outlines)
        {
            foreach (OutlineTextData outline in outlines)
            {
                string outlineFilePath = Path.Combine(_config.InputFolder, $"OUTLINE_{outline.Route}_ja.cfg.bin");

                if (!File.Exists(outlineFilePath))
                    continue;

                EventTextConfiguration eventConfig = _eventParser.Parse(outlineFilePath, StringEncoding.Sjis);

                for (var i = 0; i < Math.Min(outline.Texts.Length, eventConfig.Texts.Length); i++)
                    eventConfig.Texts[i].Text = _fullWidthConverter.Convert(outline.Texts[i].Text);

                _eventComposer.Compose(eventConfig, outlineFilePath);
            }
        }
    }
}
