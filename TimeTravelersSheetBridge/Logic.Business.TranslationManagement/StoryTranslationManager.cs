using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;
using System;

namespace Logic.Business.TranslationManagement
{
    internal class StoryTranslationManager : IStoryTranslationManager
    {
        private static readonly int[] ChapterEndRows =
        {
            1005,
            1642,
            2106,
            1891,
            1868,
            849,
            305
        };

        private readonly ISheetManager _sheetManager;
        private readonly ISpeakerTranslationManager _speakerTranslationManager;

        private IDictionary<string, IList<(string eventName, int index, string? speaker)>>? _sceneInfoLookup;
        private IDictionary<string, IList<TranslatedTextData<TextData>>>? _sceneStoryLookup;
        private IDictionary<int, HashSet<string>>? _chapterScenesLookup;

        public StoryTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector,
            ISpeakerTranslationManager speakerTranslationManager)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
            _speakerTranslationManager = speakerTranslationManager;
        }

        public async Task<IDictionary<string, StoryTextData[]>> GetStoryTexts(int chapter)
        {
            var result = new Dictionary<string, StoryTextData[]>();

            if (_chapterScenesLookup == null || !_chapterScenesLookup.ContainsKey(chapter))
                await PopulateChapterData(chapter);

            if (_chapterScenesLookup == null || !_chapterScenesLookup.TryGetValue(chapter, out HashSet<string>? scenes))
                return result;

            foreach (string scene in scenes)
            {
                StoryTextData[]? sceneTexts = await GetStoryTexts(scene);
                if (sceneTexts == null)
                    continue;

                result[scene] = sceneTexts;
            }

            return result;
        }

        public async Task<StoryTextData[]?> GetStoryTexts(string sceneName)
        {
            string identifier = sceneName[1..3];
            if (!int.TryParse(identifier, out int chapter))
                return null;

            if (_chapterScenesLookup != null && _chapterScenesLookup.ContainsKey(chapter))
            {
                if (_sceneStoryLookup != null && _sceneStoryLookup.ContainsKey(sceneName))
                    return CreateStoryData(sceneName);

                return null;
            }

            await PopulateChapterData(chapter);

            if (_sceneStoryLookup == null || !_sceneStoryLookup.ContainsKey(sceneName))
                return null;

            return CreateStoryData(sceneName);
        }

        public Task UpdateStoryText(string sceneName)
        {
            return UpdateStoryText(new[] { sceneName });
        }

        public async Task UpdateStoryText(string[] sceneNames)
        {
            if (_sceneStoryLookup == null || _sceneInfoLookup == null)
                return;

            var texts = new List<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>();
            foreach (string sceneName in sceneNames)
            {
                if (!_sceneStoryLookup.TryGetValue(sceneName, out IList<TranslatedTextData<TextData>>? sceneTexts)
                    || !_sceneInfoLookup.TryGetValue(sceneName, out IList<(string eventName, int index, string? speaker)>? sceneInfos))
                    continue;

                for (var i = 0; i < sceneTexts.Count; i++)
                    texts.Add((sceneTexts[i], sceneInfos[i]));
            }

            var chapters = new Dictionary<int, IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>>();
            foreach ((TranslatedTextData<TextData>, (string eventName, int index, string? speaker)) translatedTextData in texts)
            {
                string identifier = translatedTextData.Item1.Text.Name[1..3];
                if (!int.TryParse(identifier, out int chapter))
                    continue;

                if (!chapters.TryGetValue(chapter, out IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>? group))
                    chapters[chapter] = group = new List<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>();

                group.Add(translatedTextData);
            }

            var chapterRanges = new Dictionary<int, IList<IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>>>();
            foreach (int chapter in chapters.Keys)
            {
                if (!chapterRanges.TryGetValue(chapter, out IList<IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>>? rangeGroups))
                    chapterRanges[chapter] = rangeGroups = new List<IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>>();

                foreach ((TranslatedTextData<TextData>, (string eventName, int index, string? speaker)) translatedTextData in chapters[chapter].OrderBy(g => g.Item1.Row))
                {
                    if (rangeGroups.Count <= 0 || rangeGroups[^1][^1].Item1.Row + 1 != translatedTextData.Item1.Row)
                        rangeGroups.Add(new List<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>());

                    rangeGroups[^1].Add(translatedTextData);
                }
            }

            foreach (int chapter in chapterRanges.Keys)
            {
                IList<IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))>> chapterRange = chapterRanges[chapter];

                foreach (IList<(TranslatedTextData<TextData>, (string eventName, int index, string? speaker))> chapterTexts in chapterRange)
                {
                    if (chapterTexts.Count <= 0)
                        continue;

                    UpdateStoryTextRangeData[] updateRange = chapterTexts.Select(r => new UpdateStoryTextRangeData
                    {
                        Index = r.Item2.index,
                        Translation = r.Item1.Text.Text
                    }).ToArray();

                    CellIdentifier indexCellStart = CellIdentifier.Parse($"C{chapterTexts[0].Item1.Row}");
                    CellIdentifier indexCellEnd = CellIdentifier.Parse($"C{chapterTexts[^1].Item1.Row}");

                    await _sheetManager.UpdateRangeAsync(updateRange, $"{chapter}", indexCellStart, indexCellEnd);

                    CellIdentifier textCellStart = CellIdentifier.Parse($"I{chapterTexts[0].Item1.Row}");
                    CellIdentifier textCellEnd = CellIdentifier.Parse($"I{chapterTexts[^1].Item1.Row}");

                    await _sheetManager.UpdateRangeAsync(updateRange, $"{chapter}", textCellStart, textCellEnd);
                }
            }
        }

        private async Task PopulateChapterData(int chapter)
        {
            if (_chapterScenesLookup?.ContainsKey(chapter) ?? false)
                return;

            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"I{ChapterEndRows[chapter - 1]}");

            StoryTextRangeData[]? range = await _sheetManager.GetRangeAsync<StoryTextRangeData>($"{chapter}", startCell, endCell);
            if (range == null)
                return;

            _sceneInfoLookup ??= new Dictionary<string, IList<(string eventName, int index, string? speaker)>>();
            _sceneStoryLookup ??= new Dictionary<string, IList<TranslatedTextData<TextData>>>();
            _chapterScenesLookup ??= new Dictionary<int, HashSet<string>>();

            if (!_chapterScenesLookup.TryGetValue(chapter, out HashSet<string>? chapterScenes))
                _chapterScenesLookup[chapter] = chapterScenes = new HashSet<string>();

            for (var i = 0; i < range.Length; i++)
            {
                if (!_sceneInfoLookup.TryGetValue(range[i].SceneName, out IList<(string eventName, int index, string? speaker)>? sceneInfos))
                    _sceneInfoLookup[range[i].SceneName] = sceneInfos = new List<(string eventName, int index, string? speaker)>();

                if (!_sceneStoryLookup.TryGetValue(range[i].SceneName, out IList<TranslatedTextData<TextData>>? sceneTexts))
                    _sceneStoryLookup[range[i].SceneName] = sceneTexts = new List<TranslatedTextData<TextData>>();

                string? speaker = await GetTranslatedSpeaker(range[i].Original);

                sceneInfos.Add((range[i].EventName, range[i].Index, speaker));
                sceneTexts.Add(CreateTranslatedText(i + 2, range[i].EventName, range[i].Translation));
                chapterScenes.Add(range[i].SceneName);
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

        private StoryTextData[] CreateStoryData(string scene)
        {
            if (!_sceneInfoLookup!.TryGetValue(scene, out IList<(string eventName, int index, string? speaker)>? infos)
                || !_sceneStoryLookup!.TryGetValue(scene, out IList<TranslatedTextData<TextData>>? texts))
                return Array.Empty<StoryTextData>();

            var result = new List<StoryTextData>();

            for (var i = 0; i < infos.Count; i++)
            {
                result.Add(new StoryTextData
                {
                    Scene = scene,
                    Event = infos[i].eventName,
                    Index = infos[i].index,
                    Speaker = infos[i].speaker,
                    Text = texts[i].Text
                });
            }

            return result.ToArray();
        }

        private async Task<string?> GetTranslatedSpeaker(string originalText)
        {
            if (originalText.StartsWith("＊＊"))
                return (await _speakerTranslationManager.GetSpeaker("＊＊"))?.Text;

            if (originalText.StartsWith("＊"))
                return (await _speakerTranslationManager.GetSpeaker("＊"))?.Text;

            int speakerIndex = originalText.IndexOf('「');
            if (speakerIndex < 0)
                return null;

            return (await _speakerTranslationManager.GetSpeaker(originalText[..speakerIndex]))?.Text;
        }
    }
}
