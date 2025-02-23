using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class StaffrollTranslationManager : IStaffrollTranslationManager
    {
        private const int StaffRollEndRow_ = 636;
        private const string TableName_ = "Staffroll";

        private readonly ISheetManager _sheetManager;

        private (uint, int)[]? _staffRollInfos;
        private TranslatedTextData<TextData>[]? _staffRollTexts;

        public StaffrollTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<StaffrollTextData[]> GetStaffRolls()
        {
            if (_staffRollInfos == null || _staffRollTexts == null)
            {
                await PopulateStaffrollData();

                if (_staffRollInfos == null || _staffRollTexts == null)
                    return Array.Empty<StaffrollTextData>();
            }

            var result = new List<StaffrollTextData>();

            for (var i = 0; i < _staffRollInfos.Length; i++)
            {
                StaffrollTextData? staffrollData = CreateStaffrollText(i);
                if (staffrollData == null)
                    continue;

                result.Add(staffrollData);
            }

            return result.ToArray();
        }

        public async Task UpdateStaffRoles()
        {
            if (_staffRollTexts == null)
                return;

            var updateRange = new List<UpdateStaffRollTextRangeData>();
            foreach (TranslatedTextData<TextData> translatedText in _staffRollTexts.OrderBy(x => x.Row))
            {
                updateRange.Add(new UpdateStaffRollTextRangeData
                {
                    Translation = translatedText.Text.Text
                });
            }

            CellIdentifier textCellStart = CellIdentifier.Parse("D2");
            CellIdentifier textCellEnd = CellIdentifier.Parse($"D{StaffRollEndRow_}");

            await _sheetManager.UpdateRangeAsync(updateRange, TableName_, textCellStart, textCellEnd);
        }

        private async Task PopulateStaffrollData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"D{StaffRollEndRow_}");

            StaffRollTextRangeData[]? range = await _sheetManager.GetRangeAsync<StaffRollTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _staffRollTexts = new TranslatedTextData<TextData>[range.Length];
            for (var i = 0; i < range.Length; i++)
                _staffRollTexts[i] = CreateTranslatedText(i + 2, range[i].Hash, range[i].Translation);

            _staffRollInfos = new (uint, int)[range.Length];
            for (var i = 0; i < range.Length; i++)
                _staffRollInfos[i] = (range[i].Hash, range[i].Id);
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, uint hash, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = $"{hash}",
                    Text = text
                }
            };
        }

        private StaffrollTextData? CreateStaffrollText(int index)
        {
            if (_staffRollInfos!.Length <= index
                || _staffRollTexts!.Length <= index)
                return null;

            return new StaffrollTextData
            {
                Hash = _staffRollInfos[index].Item1,
                Flag = _staffRollInfos[index].Item2,
                Text = _staffRollTexts[index].Text
            };
        }
    }
}
