using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Domain.GoogleSheetsManagement.InternalContract
{
    public interface IDataRangeParser
    {
        IReadOnlyCollection<TType> Parse<TType>(List<List<string>?>? list, CellIdentifier start, CellIdentifier end);
    }
}
