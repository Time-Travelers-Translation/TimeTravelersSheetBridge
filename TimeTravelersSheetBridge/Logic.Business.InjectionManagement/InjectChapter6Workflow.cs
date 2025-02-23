using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.Cryptography;

namespace Logic.Business.InjectionManagement
{
    internal class InjectChapter6Workflow : BaseInjectStoryWorkflow, IInjectChapter6Workflow
    {
        private readonly IStoryTranslationManager _storyTranslationManager;

        public InjectChapter6Workflow(InjectionManagementConfiguration config,
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
            IDictionary<string, StoryTextData[]> storyTexts = await _storyTranslationManager.GetStoryTexts(6);

            InjectChapterPck("C06", storyTexts);
            InjectChapterPck("H06", storyTexts);
            InjectChapterPck("I06", storyTexts);
            InjectChapterPck("P06", storyTexts);
            InjectChapterPck("R06", storyTexts);
            InjectChapterPck("S06", storyTexts);
        }
    }
}
