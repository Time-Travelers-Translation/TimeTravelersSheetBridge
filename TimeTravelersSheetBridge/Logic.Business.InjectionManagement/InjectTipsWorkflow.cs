using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Business.TranslationManagement.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract.Cryptography;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;
using System.Text.RegularExpressions;

namespace Logic.Business.InjectionManagement
{
    internal partial class InjectTipsWorkflow : IInjectTipsWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IChecksum<uint> _checksum;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;
        private readonly ITipTranslationManager _tipTranslationManager;
        private readonly IFullWidthConverter _fullWidthConverter;

        public InjectTipsWorkflow(InjectionManagementConfiguration config, IChecksumFactory checksumFactory,
            IEventTextParser eventParser, IEventTextComposer eventComposer, ITipTranslationManager tipTranslationManager,
            IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _checksum = checksumFactory.CreateCrc32Jam();
            _eventParser = eventParser;
            _eventComposer = eventComposer;
            _tipTranslationManager = tipTranslationManager;
            _fullWidthConverter = fullWidthConverter;
        }

        public async Task Run()
        {
            TipTextData[] tips = await _tipTranslationManager.GetTips();

            InjectTipText(tips);
            InjectTipTitles(tips);
        }

        private void InjectTipText(TipTextData[] tips)
        {
            foreach (TipTextData tip in tips)
            {
                string tipFilePath = Path.Combine(_config.InputFolder, $"TIP{tip.Id:000}_ja.cfg.bin");

                if (!File.Exists(tipFilePath))
                    continue;

                EventTextConfiguration eventConfig = _eventParser.Parse(tipFilePath, StringEncoding.Sjis);

                eventConfig.Texts = new[]
                {
                    new EventText
                    {
                        Hash = _checksum.ComputeValue($"TIP{tip.Id:000}"),
                        SubId = 0,
                        Text = ProcessTutorialText(tip.Text.Text)
                    }
                };

                _eventComposer.Compose(eventConfig, tipFilePath);
            }
        }

        [GeneratedRegex(@"^:texture=#\/(.*):pos=(\d+),(\d+):")]
        private static partial Regex TextureRegex();

        [GeneratedRegex(@"^:model=#\/(.*):pos=(\d+),(\d+):")]
        private static partial Regex ModelRegex();

        [GeneratedRegex(@"^:movie=#\/(.*):pos=(\d+),(\d+):")]
        private static partial Regex MovieRegex();

        private string? ProcessTutorialText(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var prefix = "";
            if (TextureRegex().IsMatch(text) || ModelRegex().IsMatch(text) || MovieRegex().IsMatch(text))
            {
                prefix = TextureRegex().IsMatch(text) ? TextureRegex().Match(text).Value :
                    ModelRegex().IsMatch(text) ? ModelRegex().Match(text).Value :
                    MovieRegex().Match(text).Value;
                text = MovieRegex().Replace(ModelRegex().Replace(TextureRegex().Replace(text, ""), ""), "").TrimStart('\n');
            }

            text = _fullWidthConverter.Convert(text);
            if (string.IsNullOrEmpty(text))
                return text;

            return prefix + string.Join('\n', text.Split('\n').Select(x => $"[{x}/]"));
        }

        private void InjectTipTitles(TipTextData[] tips)
        {
            string tipTitleFilePath = Path.Combine(_config.InputFolder, "Tip_List_ja.cfg.bin");

            if (!File.Exists(tipTitleFilePath))
                return;

            EventTextConfiguration eventConfig = _eventParser.Parse(tipTitleFilePath, StringEncoding.Sjis);

            eventConfig.Texts = tips.Select(x => new EventText
            {
                Hash = _checksum.ComputeValue($"TIP{x.Id:000}"),
                SubId = GetSubId(x),
                Text = _fullWidthConverter.Convert(x.Title.Text)
            }).ToArray();

            _eventComposer.Compose(eventConfig, tipTitleFilePath);
        }

        private int GetSubId(TipTextData tip)
        {
            int categoryId = GetCategoryId(tip.Category);
            return tip.Type * 100 + categoryId;
        }

        private int GetCategoryId(string category)
        {
            if (category.Equals("Society", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (category.Equals("Science", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (category.Equals("Trivia", StringComparison.OrdinalIgnoreCase))
                return 2;

            if (category.Equals("Popular", StringComparison.OrdinalIgnoreCase))
                return 3;

            if (category.Equals("Character", StringComparison.OrdinalIgnoreCase))
                return 4;

            if (category.Equals("Other", StringComparison.OrdinalIgnoreCase))
                return 5;

            throw new InvalidOperationException($"Unknown category '{category}'.");
        }
    }
}
