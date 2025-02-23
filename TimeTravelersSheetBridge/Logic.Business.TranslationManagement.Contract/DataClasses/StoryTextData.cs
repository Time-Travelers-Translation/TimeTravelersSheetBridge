using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class StoryTextData
    {
        public string Scene { get; set; }
        public string Event { get; set; }
        public int Index { get; set; }
        public string? Speaker { get; set; }
        public TextData Text { get; set; }
    }
}
