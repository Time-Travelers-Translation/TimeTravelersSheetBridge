using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class OutlineTranslationManager : IOutlineTranslationManager
    {
        private const int OutlineEndRow_ = 63;
        private const string TableName_ = "Outlines";

        private readonly ISheetManager _sheetManager;

        private IDictionary<string, IList<TranslatedTextData<TextData>>>? _routeOutlineLookup;

        public OutlineTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<OutlineTextData[]> GetOutlines()
        {
            if (_routeOutlineLookup == null)
            {
                await PopulateOutlineData();

                if (_routeOutlineLookup == null)
                    return Array.Empty<OutlineTextData>();
            }

            var result = new List<OutlineTextData>();

            foreach (string route in _routeOutlineLookup.Keys)
            {
                OutlineTextData? helpData = CreateOutlineText(route);
                if (helpData == null)
                    continue;

                result.Add(helpData);
            }

            return result.ToArray();
        }

        public async Task<OutlineTextData?> GetOutline(string route)
        {
            if (_routeOutlineLookup == null)
            {
                await PopulateOutlineData();

                if (_routeOutlineLookup == null)
                    return null;
            }

            return CreateOutlineText(route);
        }

        public async Task UpdateOutlines(string route)
        {
            if (_routeOutlineLookup == null)
                return;

            if (!_routeOutlineLookup.TryGetValue(route, out IList<TranslatedTextData<TextData>>? texts))
                return;

            var ranges = new List<IList<TranslatedTextData<TextData>>>();
            foreach (TranslatedTextData<TextData> translatedText in texts.OrderBy(x => x.Row))
            {
                if (ranges.Count <= 0 || ranges[^1][^1].Row + 1 != translatedText.Row)
                    ranges.Add(new List<TranslatedTextData<TextData>>());

                ranges[^1].Add(translatedText);
            }

            foreach (IList<TranslatedTextData<TextData>> textRange in ranges)
            {
                if (textRange.Count <= 0)
                    continue;

                UpdateOutlineTextRangeData[] updateRange = textRange.Select(r => new UpdateOutlineTextRangeData
                {
                    Translation = r.Text.Text
                }).ToArray();

                CellIdentifier textCellStart = CellIdentifier.Parse($"D{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"D{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(updateRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateOutlineData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"D{OutlineEndRow_}");

            OutlineTextRangeData[]? range = await _sheetManager.GetRangeAsync<OutlineTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _routeOutlineLookup = new Dictionary<string, IList<TranslatedTextData<TextData>>>();
            for (var i = 0; i < range.Length; i++)
            {
                if (!_routeOutlineLookup.TryGetValue(range[i].Route, out IList<TranslatedTextData<TextData>>? routeTexts))
                    _routeOutlineLookup[range[i].Route] = routeTexts = new List<TranslatedTextData<TextData>>();

                routeTexts.Add(CreateTranslatedText(i + 2, range[i].Route, range[i].Translation));
            }
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, string route, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = route,
                    Text = text
                }
            };
        }

        private OutlineTextData? CreateOutlineText(string route)
        {
            if (!_routeOutlineLookup!.TryGetValue(route, out IList<TranslatedTextData<TextData>>? texts))
                return null;

            return new OutlineTextData
            {
                Route = route,
                Texts = texts.Select(t => t.Text).ToArray()
            };
        }
    }
}
