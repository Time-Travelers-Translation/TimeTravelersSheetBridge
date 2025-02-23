using CrossCutting.Abstract.DataClasses;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.InjectionManagement.InternalContract.Scene;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Font;
using Logic.Domain.Level5Management.Contract.Font;

namespace Logic.Business.InjectionManagement
{
    internal class InjectScnWorkflow : IInjectScnWorkflow
    {
        private readonly string[] _scnFiles = new[]
        {
            "tt1.scn",
            "tt1_A01A_0000.scn",
            "tt1_P01A_0000.scn",
            "tt1_I02A_0000.scn",
            "tt1_I03A_0000.scn",
            "tt1_I04A_0000.scn",
            "tt1_C05A_0020.scn",
            "tt1_I06A_0000.scn",
            "tt1_M07A_0000.scn"
        };

        private readonly InjectionManagementConfiguration _config;
        private readonly ITitleTranslationManager _titleTranslationManager;
        private readonly IDecisionTranslationManager _decisionTranslationManager;
        private readonly IHintTranslationManager _hintTranslationManager;
        private readonly ISceneStateFactory _sceneFactory;
        private readonly IFontParser _fontParser;
        private readonly IFullWidthConverter _fullWidthConverter;

        public InjectScnWorkflow(InjectionManagementConfiguration config, ITitleTranslationManager titleTranslationManager,
            IDecisionTranslationManager decisionTranslationManager, IHintTranslationManager hintTranslationManager,
            ISceneStateFactory sceneFactory, IFontParser fontParser, IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _titleTranslationManager = titleTranslationManager;
            _decisionTranslationManager = decisionTranslationManager;
            _hintTranslationManager = hintTranslationManager;
            _sceneFactory = sceneFactory;
            _fontParser = fontParser;
            _fullWidthConverter = fullWidthConverter;
        }

        public async Task Run()
        {
            TitleTextData[] titles = await _titleTranslationManager.GetSceneTitles();
            DecisionTextData[] decisions = await _decisionTranslationManager.GetDecisions();
            HintTextData[] hints = await _hintTranslationManager.GetHints();

            await using Stream fontStream = File.OpenRead(_config.ModeArguments[0]);
            FontImageData? font = _fontParser.Parse(fontStream);

            ProcessTitleTexts(titles);
            ProcessDecisionTexts(decisions);
            ProcessHintTexts(hints, font);

            foreach (string scnFile in _scnFiles)
            {
                string scnFilePath = Path.Combine(_config.InputFolder, scnFile);
                Stream scnStream = File.OpenRead(scnFilePath);

                ISceneState sceneState = _sceneFactory.Create();
                sceneState.Load(scnStream);

                sceneState.UpdateTitles(titles);
                sceneState.UpdateDecisionTexts(decisions);
                sceneState.UpdateHints(hints);

                string scnOutFilePath = scnFilePath + ".new";
                Stream scnOutStream = File.Create(scnOutFilePath);

                sceneState.Save(scnStream, scnOutStream);

                scnStream.Close();
                scnOutStream.Close();

                File.Replace(scnOutFilePath, scnFilePath, null);
            }
        }

        private void ProcessTitleTexts(TitleTextData[] titles)
        {
            foreach (TitleTextData title in titles)
            {
                string? convertedText = ProcessTitleText(title);
                if (string.IsNullOrEmpty(convertedText))
                    continue;

                title.Text.Text = convertedText;
            }
        }

        private void ProcessDecisionTexts(DecisionTextData[] decisions)
        {
            foreach (DecisionTextData decision in decisions)
            {
                foreach (TextData decisionText in decision.Texts)
                {
                    string? convertedText = _fullWidthConverter.Convert(decisionText.Text);
                    if (string.IsNullOrEmpty(convertedText))
                        continue;

                    decisionText.Text = convertedText;
                }
            }
        }

        private void ProcessHintTexts(HintTextData[] hints, FontImageData? font)
        {
            foreach (HintTextData hint in hints)
            {
                string? convertedText = ProcessHintText(hint, font);
                if (string.IsNullOrEmpty(convertedText))
                    continue;

                hint.Text.Text = convertedText;
            }
        }

        private string? ProcessTitleText(TitleTextData? titleEntry)
        {
            if (titleEntry == null || string.IsNullOrEmpty(titleEntry.Text.Text))
                return null;

            string text = titleEntry.Text.Text;
            bool isBadEnd = text.StartsWith("No.");

            if (isBadEnd)
                text = text[..3] + text[3..].Trim();

            string? convertedText = _fullWidthConverter.Convert(text);
            if (string.IsNullOrEmpty(convertedText))
                return null;

            if (!isBadEnd)
                return convertedText;

            int index = convertedText.IndexOf('　') + 1;
            return convertedText[..index] + convertedText[index..].Replace('　', ' ');
        }

        private string? ProcessHintText(HintTextData hintData, FontImageData? font)
        {
            string[] lines = hintData.Text.Text?.Replace("\r\n", "\n").Split('\n') ?? Array.Empty<string>();

            string composedLines;
            if (font == null || !font.Font.LargeFont.Glyphs.TryGetValue(' ', out GlyphData? spaceGlyph))
            {
                composedLines = string.Join('\n', lines);
                return _fullWidthConverter.Convert(composedLines);
            }

            var lineWidths = new int[lines.Length];
            for (var i = 0; i < lines.Length; i++)
                for (var j = 0; j < lines[i].Length; j++)
                    lineWidths[i] += font.Font.LargeFont.Glyphs[lines[i][j]].Width + (j + 1 >= lines[i].Length ? 0 : 1);

            int maxLength = lineWidths.Max();
            for (var i = 0; i < lineWidths.Length; i++)
            {
                if (lineWidths[i] <= 0)
                    continue;

                int lineDiff = (maxLength - lineWidths[i]) / 2;
                if (lineDiff <= 0)
                    continue;

                int spaceCount = lineDiff / (spaceGlyph.Width + 1);
                lines[i] = new string(' ', spaceCount) + lines[i];
            }

            composedLines = string.Join('\n', lines);
            return _fullWidthConverter.Convert(composedLines);
        }
    }
}
