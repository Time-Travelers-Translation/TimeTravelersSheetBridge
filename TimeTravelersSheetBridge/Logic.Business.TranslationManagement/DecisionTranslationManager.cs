using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class DecisionTranslationManager : IDecisionTranslationManager
    {
        private const int DecisionEndRow_ = 222;
        private const string TableName_ = "Decisions";

        private readonly ISheetManager _sheetManager;

        private IDictionary<string, IList<TranslatedTextData<TextData>>>? _sceneDecisionLookup;

        public DecisionTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<DecisionTextData[]> GetDecisions()
        {
            if (_sceneDecisionLookup == null)
            {
                await PopulateDecisionData();

                if (_sceneDecisionLookup == null)
                    return Array.Empty<DecisionTextData>();
            }

            var result = new List<DecisionTextData>();

            foreach (string scene in _sceneDecisionLookup.Keys)
            {
                DecisionTextData? decisionData = CreateDecisionData(scene);
                if (decisionData == null)
                    continue;

                result.Add(decisionData);
            }

            return result.ToArray();
        }

        public async Task<DecisionTextData?> GetDecisions(string sceneName)
        {
            if (_sceneDecisionLookup == null)
            {
                await PopulateDecisionData();

                if (_sceneDecisionLookup == null)
                    return null;
            }

            return CreateDecisionData(sceneName);
        }

        public Task UpdateDecisionText(string sceneName)
        {
            return UpdateDecisionText(new[] { sceneName });
        }

        public async Task UpdateDecisionText(string[] sceneNames)
        {
            if (_sceneDecisionLookup == null)
                return;

            var texts = new List<TranslatedTextData<TextData>>();
            foreach (string sceneName in sceneNames)
            {
                if (_sceneDecisionLookup.TryGetValue(sceneName, out IList<TranslatedTextData<TextData>>? sceneTexts))
                    texts.AddRange(sceneTexts);
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

                UpdateDecisionTextRangeData[] updateRange = textRange.Select(r => new UpdateDecisionTextRangeData
                {
                    Translation = r.Text.Text
                }).ToArray();

                CellIdentifier textCellStart = CellIdentifier.Parse($"D{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"D{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(updateRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateDecisionData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"D{DecisionEndRow_}");

            DecisionTextRangeData[]? range = await _sheetManager.GetRangeAsync<DecisionTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _sceneDecisionLookup = new Dictionary<string, IList<TranslatedTextData<TextData>>>();
            for (var i = 0; i < range.Length; i++)
            {
                if (!_sceneDecisionLookup.TryGetValue(range[i].SceneName, out IList<TranslatedTextData<TextData>>? texts))
                    _sceneDecisionLookup[range[i].SceneName] = texts = new List<TranslatedTextData<TextData>>();

                texts.Add(CreateTranslatedText(i + 2, range[i].SceneName, range[i].Translation));
            }
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, string scene, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = scene,
                    Text = text
                }
            };
        }

        private DecisionTextData? CreateDecisionData(string scene)
        {
            if (!_sceneDecisionLookup!.TryGetValue(scene, out IList<TranslatedTextData<TextData>>? texts))
                return null;

            return new DecisionTextData
            {
                Scene = scene,
                Texts = texts.Select(t => t.Text).ToArray()
            };
        }
    }
}
