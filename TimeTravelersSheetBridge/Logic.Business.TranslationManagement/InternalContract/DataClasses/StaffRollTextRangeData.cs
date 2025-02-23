using Logic.Domain.GoogleSheetsManagement.Contract.Aspects;

namespace Logic.Business.TranslationManagement.InternalContract.DataClasses
{
    internal class StaffRollTextRangeData
    {
        [Column("A")]
        public uint Hash { get; set; }
        [Column("B")]
        public int Id { get; set; }
        [Column("D")]
        public string Translation { get; set; }
    }
}
