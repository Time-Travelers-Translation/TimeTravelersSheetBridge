namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class ExtendedValueData
    {
        public string StringValue { get; set; }
        public long? NumberValue { get; set; }
        public bool? BoolValue { get; set; }
        public string FormulaValue { get; set; }
        public ErrorValueData ErrorValue { get; set; }
    }
}
