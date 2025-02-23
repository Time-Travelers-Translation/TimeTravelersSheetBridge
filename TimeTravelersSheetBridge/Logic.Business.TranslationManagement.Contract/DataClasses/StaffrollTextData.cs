using CrossCutting.Abstract.DataClasses;

namespace Logic.Business.TranslationManagement.Contract.DataClasses
{
    public class StaffrollTextData
    {
        public uint Hash { get; set; }
        public int Flag { get; set; }
        public TextData Text { get; set; }
    }
}
