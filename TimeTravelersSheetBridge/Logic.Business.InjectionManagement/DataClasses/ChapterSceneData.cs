using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract.DataClasses;

namespace Logic.Business.InjectionManagement.DataClasses
{
    internal class ChapterSceneData
    {
        public EventTextConfiguration Events { get; set; }
        public StoryTextData[] Translations { get; set; }
    }
}
