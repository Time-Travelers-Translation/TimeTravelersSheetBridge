using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class TipTranslationManager : ITipTranslationManager
    {
        private const int TipEndRow_ = 448;
        private const string TableName_ = "TIPS";

        private readonly ISheetManager _sheetManager;

        private IDictionary<int, (string, int)>? _tipInfoLookup;
        private IDictionary<int, TranslatedTextData<TextData>>? _tipTitlesLookup;
        private IDictionary<int, TranslatedTextData<TextData>>? _tipTextsLookup;

        public TipTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<TipTextData[]> GetTips()
        {
            if (_tipInfoLookup == null || _tipTitlesLookup == null || _tipTextsLookup == null)
            {
                await PopulateTipData();

                if (_tipInfoLookup == null || _tipTitlesLookup == null || _tipTextsLookup == null)
                    return Array.Empty<TipTextData>();
            }

            var result = new List<TipTextData>();

            foreach (int index in _tipTitlesLookup.Keys)
            {
                TipTextData? tipData = CreateTipText(index);
                if (tipData == null)
                    continue;

                result.Add(tipData);
            }

            return result.ToArray();
        }

        public async Task<TipTextData?> GetTip(int index)
        {
            if (_tipInfoLookup == null || _tipTitlesLookup == null || _tipTextsLookup == null)
            {
                await PopulateTipData();

                if (_tipInfoLookup == null || _tipTitlesLookup == null || _tipTextsLookup == null)
                    return null;
            }

            return CreateTipText(index);
        }

        public async Task UpdateTips(int[] indexes)
        {
            if (_tipTextsLookup == null || _tipTitlesLookup == null)
                return;

            var tips = new List<(int row, TranslatedTextData<TextData> title, TranslatedTextData<TextData> text)>();
            foreach (int index in indexes)
            {
                bool hasTitle = _tipTitlesLookup.TryGetValue(index, out TranslatedTextData<TextData>? title);
                bool hasText = _tipTextsLookup.TryGetValue(index, out TranslatedTextData<TextData>? text);

                if (!hasTitle && !hasText)
                    continue;

                if (title!.Row != text!.Row)
                    continue;

                tips.Add((title.Row, title, text));
            }

            var ranges = new List<IList<UpdateTipTextRangeData>>();
            foreach ((int row, TranslatedTextData<TextData> title, TranslatedTextData<TextData> text) tip in tips.OrderBy(x => x.row))
            {
                if (ranges.Count <= 0 || ranges[^1][^1].Row + 1 != tip.row)
                    ranges.Add(new List<UpdateTipTextRangeData>());

                ranges[^1].Add(new UpdateTipTextRangeData
                {
                    Row = tip.row,
                    TranslatedTitle = tip.title.Text.Text,
                    Translation = tip.text.Text.Text,
                });
            }

            foreach (IList<UpdateTipTextRangeData> textRange in ranges)
            {
                if (textRange.Count <= 0)
                    continue;

                CellIdentifier titleCellStart = CellIdentifier.Parse($"I{textRange[0].Row}");
                CellIdentifier titleCellEnd = CellIdentifier.Parse($"I{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(textRange, TableName_, titleCellStart, titleCellEnd);

                CellIdentifier textCellStart = CellIdentifier.Parse($"L{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"L{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(textRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateTipData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"L{TipEndRow_}");

            TipTextRangeData[]? range = await _sheetManager.GetRangeAsync<TipTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _tipInfoLookup = new Dictionary<int, (string, int)>();
            for (var i = 0; i < range.Length; i++)
                _tipInfoLookup[range[i].Index] = (range[i].Category, range[i].Type);

            _tipTitlesLookup = new Dictionary<int, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
                _tipTitlesLookup[range[i].Index] = CreateTranslatedText(i + 2, range[i].Index, range[i].TranslatedTitle);

            _tipTextsLookup = new Dictionary<int, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
                _tipTextsLookup[range[i].Index] = CreateTranslatedText(i + 2, range[i].Index, range[i].Translation);

        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, int index, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = $"TIP{index:000}",
                    Text = text
                }
            };
        }

        private TipTextData? CreateTipText(int index)
        {
            if (!_tipTitlesLookup!.TryGetValue(index, out TranslatedTextData<TextData>? title)
                || !_tipTextsLookup!.TryGetValue(index, out TranslatedTextData<TextData>? text)
                || !_tipInfoLookup!.TryGetValue(index, out (string, int) info))
                return null;

            return new TipTextData
            {
                Id = index,
                Category = info.Item1,
                Type = info.Item2,
                Title = title.Text,
                Text = text.Text
            };
        }
    }
}
