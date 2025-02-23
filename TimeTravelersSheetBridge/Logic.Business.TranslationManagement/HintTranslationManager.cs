﻿using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;

namespace Logic.Business.TranslationManagement
{
    internal class HintTranslationManager : IHintTranslationManager
    {
        private const int HintEndRow_ = 75;
        private const string TableName_ = "Hints";

        private readonly ISheetManager _sheetManager;

        private IDictionary<string, TranslatedTextData<TextData>>? _sceneHintLookup;

        public HintTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<HintTextData[]> GetHints()
        {
            if (_sceneHintLookup == null)
            {
                await PopulateHintData();

                if (_sceneHintLookup == null)
                    return Array.Empty<HintTextData>();
            }

            var result = new List<HintTextData>();

            foreach (string scene in _sceneHintLookup.Keys)
            {
                HintTextData? helpData = CreateHintText(scene);
                if (helpData == null)
                    continue;

                result.Add(helpData);
            }

            return result.ToArray();
        }

        public async Task<HintTextData?> GetHints(string sceneName)
        {
            if (_sceneHintLookup == null)
            {
                await PopulateHintData();

                if (_sceneHintLookup == null)
                    return null;
            }

            return CreateHintText(sceneName);
        }

        public Task UpdateSceneHint(string sceneName)
        {
            return UpdateSceneHint(new[] { sceneName });
        }

        public async Task UpdateSceneHint(string[] sceneNames)
        {
            if (_sceneHintLookup == null)
                return;

            var texts = new List<TranslatedTextData<TextData>>();
            foreach (string sceneName in sceneNames)
            {
                if (_sceneHintLookup.TryGetValue(sceneName, out TranslatedTextData<TextData>? titleText))
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

                UpdateHintTextRangeData[] updateRange = textRange.Select(r => new UpdateHintTextRangeData
                {
                    Translation = r.Text.Text
                }).ToArray();

                CellIdentifier textCellStart = CellIdentifier.Parse($"D{textRange[0].Row}");
                CellIdentifier textCellEnd = CellIdentifier.Parse($"D{textRange[^1].Row}");

                await _sheetManager.UpdateRangeAsync(updateRange, TableName_, textCellStart, textCellEnd);
            }
        }

        private async Task PopulateHintData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"D{HintEndRow_}");

            HintTextRangeData[]? range = await _sheetManager.GetRangeAsync<HintTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _sceneHintLookup = new Dictionary<string, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
            {
                if (_sceneHintLookup.ContainsKey(range[i].SceneName))
                    continue;

                _sceneHintLookup[range[i].SceneName] = CreateTranslatedText(i + 2, range[i].SceneName, range[i].Translation);
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

        private HintTextData? CreateHintText(string scene)
        {
            if (!_sceneHintLookup!.TryGetValue(scene, out TranslatedTextData<TextData>? text))
                return null;

            return new HintTextData
            {
                Scene = scene,
                Text = text.Text
            };
        }
    }
}
