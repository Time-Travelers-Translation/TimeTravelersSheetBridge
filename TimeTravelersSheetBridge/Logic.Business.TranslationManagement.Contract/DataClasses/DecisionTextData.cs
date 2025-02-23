using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class DecisionTextData
    {
        public string Scene { get; set; }
        public TextData[] Texts { get; set; }
    }
}
