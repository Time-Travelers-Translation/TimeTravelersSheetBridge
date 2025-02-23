using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.Cryptography;

namespace Logic.Business.InjectionManagement
{
    internal class InjectChapter3Workflow : BaseInjectStoryWorkflow, IInjectChapter3Workflow
    {
        private readonly IStoryTranslationManager _storyTranslationManager;

        public InjectChapter3Workflow(InjectionManagementConfiguration config,
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
            IDictionary<string, StoryTextData[]> storyTexts = await _storyTranslationManager.GetStoryTexts(3);

            InjectChapterPck("C03", storyTexts);
            InjectChapterPck("H03", storyTexts);
            InjectChapterPck("I03", storyTexts);
            InjectChapterPck("P03", storyTexts);
            InjectChapterPck("R03", storyTexts);
            InjectChapterPck("S03", storyTexts);
        }
    }
}
