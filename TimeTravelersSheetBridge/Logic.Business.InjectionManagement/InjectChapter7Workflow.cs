using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.Cryptography;

namespace Logic.Business.InjectionManagement
{
    internal class InjectChapter7Workflow : BaseInjectStoryWorkflow, IInjectChapter7Workflow
    {
        private readonly IStoryTranslationManager _storyTranslationManager;

        public InjectChapter7Workflow(InjectionManagementConfiguration config,
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
            IDictionary<string, StoryTextData[]> storyTexts = await _storyTranslationManager.GetStoryTexts(7);

            InjectChapterPck("M07", storyTexts);
        }
    }
}
