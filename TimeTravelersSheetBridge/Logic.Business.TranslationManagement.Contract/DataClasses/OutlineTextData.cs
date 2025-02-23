using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class OutlineTextData
    {
        public string Route { get; set; }
        public TextData[] Texts { get; set; }
    }
}
