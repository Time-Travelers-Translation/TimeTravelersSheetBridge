using CrossCutting.Abstract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.InternalContract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract.DataClasses;
using Logic.Domain.GoogleSheetsManagement.Contract;

namespace Logic.Business.TranslationManagement
{
    internal class TutorialTranslationManager : ITutorialTranslationManager
    {
        private const int TutorialEndRow_ = 37;
        private const string TableName_ = "Tutorial";

        private readonly ISheetManager _sheetManager;

        private IDictionary<int, TranslatedTextData<TextData>>? _tutorialTitlesLookup;
        private IDictionary<int, IList<TranslatedTextData<TextData>>>? _tutorialTextsLookup;

        public TutorialTranslationManager(TranslationManagementConfiguration config, IGoogleApiConnector apiConnector)
        {
            _sheetManager = apiConnector.CreateSheetManager(config.SheetId);
        }

        public async Task<TutorialTextData[]> GetTutorials()
        {
            if (_tutorialTitlesLookup == null || _tutorialTextsLookup == null)
            {
                await PopulateTutorialData();

                if (_tutorialTitlesLookup == null || _tutorialTextsLookup == null)
                    return Array.Empty<TutorialTextData>();
            }

            var result = new List<TutorialTextData>();

            foreach (int index in _tutorialTitlesLookup.Keys)
            {
                TutorialTextData? tutorialData = CreateTutorialText(index);
                if (tutorialData == null)
                    continue;

                result.Add(tutorialData);
            }

            return result.ToArray();
        }

        public async Task<TutorialTextData?> GetTutorial(int index)
        {
            if (_tutorialTitlesLookup == null || _tutorialTextsLookup == null)
            {
                await PopulateTutorialData();

                if (_tutorialTitlesLookup == null || _tutorialTextsLookup == null)
                    return null;
            }

            return CreateTutorialText(index);
        }

        public async Task UpdateTutorials(int[] indexes)
        {
            if (_tutorialTextsLookup == null || _tutorialTitlesLookup == null)
                return;

            var tutorials = new List<(int row, TranslatedTextData<TextData> title, IList<TranslatedTextData<TextData>> texts)>();
            foreach (int index in indexes)
            {
                bool hasTitle = _tutorialTitlesLookup.TryGetValue(index, out TranslatedTextData<TextData>? title);
                bool hasTexts = _tutorialTextsLookup.TryGetValue(index, out IList<TranslatedTextData<TextData>>? texts);

                if (!hasTitle && !hasTexts)
                    continue;

                if (texts!.Count == 0)
                    continue;

                if (title!.Row != texts[0].Row)
                    continue;

                tutorials.Add((title.Row, title, texts));
            }

            var ranges = new List<IList<UpdateTutorialTextRangeData>>();
            foreach ((int row, TranslatedTextData<TextData> title, IList<TranslatedTextData<TextData>> texts) tutorial in tutorials.OrderBy(x => x.row))
            {
                if (ranges.Count <= 0 || ranges[^1][^1].Row + 1 != tutorial.row)
                    ranges.Add(new List<UpdateTutorialTextRangeData>());

                ranges[^1].Add(new UpdateTutorialTextRangeData
                {
                    Row = tutorial.row,
                    TranslatedTitle = tutorial.title.Text.Text,
                    Translation = string.Join("\n\n", tutorial.texts.Select(t => t.Text.Text))
                });
            }

            foreach (IList<UpdateTutorialTextRangeData> textRange in ranges)
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

        private async Task PopulateTutorialData()
        {
            CellIdentifier startCell = CellIdentifier.Parse("A2");
            CellIdentifier endCell = CellIdentifier.Parse($"G{TutorialEndRow_}");

            TutorialTextRangeData[]? range = await _sheetManager.GetRangeAsync<TutorialTextRangeData>(TableName_, startCell, endCell);
            if (range == null)
                return;

            _tutorialTitlesLookup = new Dictionary<int, TranslatedTextData<TextData>>();
            for (var i = 0; i < range.Length; i++)
                _tutorialTitlesLookup[range[i].Index] = CreateTranslatedText(i + 2, range[i].Index, range[i].TranslatedTitle);

            _tutorialTextsLookup = new Dictionary<int, IList<TranslatedTextData<TextData>>>();
            for (var i = 0; i < range.Length; i++)
            {
                _tutorialTextsLookup[range[i].Index] = new List<TranslatedTextData<TextData>>();

                string[] translatedTexts = range[i].Translation?.Split("\n\n") ?? Array.Empty<string>();
                if (translatedTexts.Length <= 0)
                {
                    _tutorialTextsLookup[range[i].Index].Add(CreateTranslatedText(i + 2, range[i].Index, string.Empty));
                    continue;
                }

                foreach (string translatedText in translatedTexts)
                    _tutorialTextsLookup[range[i].Index].Add(CreateTranslatedText(i + 2, range[i].Index, translatedText));
            }
        }

        private TranslatedTextData<TextData> CreateTranslatedText(int row, int index, string text)
        {
            return new TranslatedTextData<TextData>
            {
                Row = row,
                Text = new TextData
                {
                    Name = $"TUTO{index:000}",
                    Text = text
                }
            };
        }

        private TutorialTextData? CreateTutorialText(int index)
        {
            if (!_tutorialTitlesLookup!.TryGetValue(index, out TranslatedTextData<TextData>? title)
                || !_tutorialTextsLookup!.TryGetValue(index, out IList<TranslatedTextData<TextData>>? texts))
                return null;

            return new TutorialTextData
            {
                Id = index,
                Title = title.Text,
                Texts = texts.Select(t => t.Text).ToArray()
            };
        }
    }
}
