using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class HelpTranslationManager : IHelpTranslationManager
    {
        private const int HelpEndRow_ = 18;
        private const string TableName_ = "Help";

        private readonly ISheetManager _sheetManager;

        private IDictionary<int, TranslatedTextData<TextData>>? _helpTitlesLookup;
        private IDictionary<int, IList<TranslatedTextData<TextData>>>? _helpTextsLookup;

        public HelpTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<HelpTextData[]> GetHelps()
        {
            if (_helpTitlesLookup == null || _helpTextsLookup == null)
            {
                await PopulateHelpData();

                if (_helpTitlesLookup == null || _helpTextsLookup == null)
                    return Array.Empty<HelpTextData>();
            }

            var result = new List<HelpTextData>();

            foreach (int index in _helpTitlesLookup.Keys)
            {
                HelpTextData? helpData = CreateHelpText(index);
                if (helpData == null)
                    continue;

                result.Add(helpData);
            }

            return result.ToArray();
        }

        public async Task<HelpTextData?> GetHelp(int index)
        {
            if (_helpTitlesLookup == null || _helpTextsLookup == null)
            {
                await PopulateHelpData();

                if (_helpTitlesLookup == null || _helpTextsLookup == null)
                    return null;
            }

            return CreateHelpText(index);
        }

        public async Task UpdateHelps(int[] indexes)
        {
            if (_helpTextsLookup == null || _helpTitlesLookup == null)
                return;

            var helps = new List<(int row, TranslatedTextData<TextData> title, IList<TranslatedTextData<TextData>> texts)>();
            foreach (int index in indexes)
            {
                bool hasTitle = _helpTitlesLookup.TryGetValue(index, out TranslatedTextData<TextData>? title);
                bool hasTexts = _helpTextsLookup.TryGetValue(index, out IList<TranslatedTextData<TextData>>? texts);

                if (!hasTitle && !hasTexts)
                    continue;

                if (texts!.Count == 0)
                    continue;

                if (title!.Row != texts[0].Row)
                    continue;

                helps.Add((title.Row, title, texts));
            }

            var ranges = new List<IList<UpdateHelpTextRangeData>>();
            foreach ((int row, TranslatedTextData<TextData> title, IList<TranslatedTextData<TextData>> texts) help in helps.OrderBy(x => x.row))
            {
                if (ranges.Count <= 0 || ranges[^1][^1].Row + 1 != help.row)
                    ranges.Add(new List<UpdateHelpTextRangeData>());

                ranges[^1].Add(new UpdateHelpTextRangeData
                {
                    Row = help.row,
                    TranslatedTitle = help.title.Text.Text,
                    Translation = string.Join("\n\n", help.texts.Select(t => t.Text.Text))
                });
            }

            foreach (IList<UpdateHelpTextRangeData> textRange in ranges)
            {
                if (textRange.Count <= 0)
                    continue;

                CellIdentifier titleCellStart = CellIdentifier.Parse($"D{textRange[0].Row}");
                CellIdentifier titleCellEnd = CellIdentifier.Parse($"D{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(textRange, TableName_, titleCellStart, titleCellEnd);

                CellIdentifier textCellStart = CellIdentifier.Parse($"G{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"G{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(textRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateHelpData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"G{HelpEndRow_}");

            HelpTextRangeData[]? range = await _sheetManager.GetRangeAsync<HelpTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _helpTitlesLookup = new Dictionary<int, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
                _helpTitlesLookup[range[i].Index] = CreateTranslatedText(i + 2, range[i].Index, range[i].TranslatedTitle);
            
            _helpTextsLookup = new Dictionary<int, IList<TranslatedTextData<TextData>>>();
            for (var i = 0; i < range.Length; i++)
            {
                _helpTextsLookup[range[i].Index] = new List<TranslatedTextData<TextData>>();

                string[] translatedTexts = range[i].Translation?.Split("\n\n") ?? Array.Empty<string>();
                if (translatedTexts.Length <= 0)
                {
                    _helpTextsLookup[range[i].Index].Add(CreateTranslatedText(i + 2, range[i].Index, string.Empty));
                    continue;
                }

                foreach (string translatedText in translatedTexts)
                    _helpTextsLookup[range[i].Index].Add(CreateTranslatedText(i + 2, range[i].Index, translatedText));
            }
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, int index, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = $"HELP{index:000}",
                    Text = text
                }
            };
        }

        private HelpTextData? CreateHelpText(int index)
        {
            if (!_helpTitlesLookup!.TryGetValue(index, out TranslatedTextData<TextData>? title)
                || !_helpTextsLookup!.TryGetValue(index, out IList<TranslatedTextData<TextData>>? texts))
                return null;

            return new HelpTextData
            {
                Id = index,
                Title = title.Text,
                Texts = texts.Select(t => t.Text).ToArray()
            };
        }
    }
}
