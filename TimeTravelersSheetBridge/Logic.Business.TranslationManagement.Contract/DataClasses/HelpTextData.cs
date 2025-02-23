using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class HelpTextData
    {
        public int Id { get; set; }
        public TextData Title { get; set; }
        public TextData[] Texts { get; set; }
    }
}
