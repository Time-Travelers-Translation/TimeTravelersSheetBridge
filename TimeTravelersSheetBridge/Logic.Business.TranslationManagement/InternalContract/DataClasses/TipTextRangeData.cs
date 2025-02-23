using Logic.Domain.GoogleSheetsManagement.Contract.Aspects;

namespace Logic.Business.TranslationManagement.InternalContract.DataClasses
{
    internal class TipTextRangeData
    {
        [Column("A")]
        public int Index { get; set; }
        [Column("B")]
        public string Category { get; set; }
        [Column("C")]
        public int Type { get; set; }
        [Column("I")]
        public string TranslatedTitle { get; set; }
        [Column("L")]
        public string Translation { get; set; }
    }
}
