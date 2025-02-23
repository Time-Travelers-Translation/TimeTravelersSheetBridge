using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class TitleTranslationManager : ITitleTranslationManager
    {
        private const int FlowEndRow_ = 393;
        private const string TableName_ = "Titles";

        private readonly ISheetManager _sheetManager;

        private IDictionary<string, TranslatedTextData<TextData>>? _sceneTitleLookup;

        public TitleTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<TitleTextData[]> GetSceneTitles()
        {
            if (_sceneTitleLookup == null)
            {
                await PopulateSceneTitles();

                if (_sceneTitleLookup == null)
                    return Array.Empty<TitleTextData>();
            }

            var result = new List<TitleTextData>();

            foreach (string sceneName in _sceneTitleLookup.Keys)
            {
                TitleTextData? titleData = CreateTitleText(sceneName);
                if (titleData == null)
                    continue;

                result.Add(titleData);
            }

            return result.ToArray();
        }

        public async Task<TitleTextData?> GetSceneTitle(string sceneName)
        {
            if (_sceneTitleLookup == null)
            {
                await PopulateSceneTitles();

                if (_sceneTitleLookup == null)
                    return null;
            }

            return CreateTitleText(sceneName);
        }

        public Task UpdateSceneTitle(string sceneName)
        {
            return UpdateSceneTitle(new[] { sceneName });
        }

        public async Task UpdateSceneTitle(string[] sceneNames)
        {
            if (_sceneTitleLookup == null)
                return;

            var texts = new List<TranslatedTextData<TextData>>();
            foreach (string sceneName in sceneNames)
            {
                if (_sceneTitleLookup.TryGetValue(sceneName, out TranslatedTextData<TextData>? titleText))
                    texts.Add(titleText);
            }

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

                UpdateFlowTextRangeData[] updateRange = textRange.Select(r => new UpdateFlowTextRangeData
                {
                    Translation = r.Text.Text
                }).ToArray();

                CellIdentifier textCellStart = CellIdentifier.Parse($"D{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"D{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(updateRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateSceneTitles()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"D{FlowEndRow_}");

            FlowTextRangeData[]? range = await _sheetManager.GetRangeAsync<FlowTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _sceneTitleLookup = new Dictionary<string, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
            {
                if (_sceneTitleLookup.ContainsKey(range[i].SceneName))
                    continue;

                _sceneTitleLookup[range[i].SceneName] = CreateTranslatedText(i + 2, range[i].SceneName, range[i].Translation);
            }
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, string name, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = name,
                    Text = text
                }
            };
        }

        private TitleTextData? CreateTitleText(string sceneName)
        {
            if (!_sceneTitleLookup!.TryGetValue(sceneName, out TranslatedTextData<TextData>? title))
                return null;

            return new TitleTextData
            {
                Scene = sceneName,
                Text = title.Text
            };
        }
    }
}
