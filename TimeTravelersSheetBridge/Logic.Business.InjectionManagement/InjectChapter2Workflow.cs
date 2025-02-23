using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.Cryptography;

namespace Logic.Business.InjectionManagement
{
    internal class InjectChapter2Workflow : BaseInjectStoryWorkflow, IInjectChapter2Workflow
    {
        private readonly IStoryTranslationManager _storyTranslationManager;

        public InjectChapter2Workflow(InjectionManagementConfiguration config,
            IPckReader pckReader, IPckWriter pckWriter,
            IFullWidthConverter fullWidthConverter, IChecksumFactory checksumFactory,
            IEventTextParser eventParser, IEventTextComposer eventComposer,
            IStoryTranslationManager storyTranslationManager)
            : base(config, pckReader, pckWriter, fullWidthConverter, checksumFactory, eventParser, eventComposer)
        {
            _storyTranslationManager = storyTranslationManager;
        }

        public async Task Run()
        {
            IDictionary<string, StoryTextData[]> storyTexts = await _storyTranslationManager.GetStoryTexts(2);
            
            InjectChapterPck("C02", storyTexts);
            InjectChapterPck("H02", storyTexts);
            InjectChapterPck("I02", storyTexts);
            InjectChapterPck("P02", storyTexts);
            InjectChapterPck("R02", storyTexts);
            InjectChapterPck("S02", storyTexts);
        }
    }
}
