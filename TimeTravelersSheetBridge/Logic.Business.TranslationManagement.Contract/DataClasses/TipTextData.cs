using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class TipTextData
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Type { get; set; }
        public TextData Title { get; set; }
        public TextData Text { get; set; }
    }
}
