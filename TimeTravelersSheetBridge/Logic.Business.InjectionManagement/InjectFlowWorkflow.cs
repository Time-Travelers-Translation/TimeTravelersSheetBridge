using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TranslationManagement.Contract;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using System.Text;

namespace Logic.Business.InjectionManagement
{
    internal class InjectFlowWorkflow : IInjectFlowWorkflow
    {
        // HINT: As composed from all buffers, this represents the random data per full entry
        private static readonly byte[] FillBytes = new byte[]
        {
            0x00, 0x97, 0x55, 0x89, 0xfb, 0x2f, 0x82, 0xe4, 0x82, 0xa4, 0x82, 0xa9, 0x82, 0xa2, 0x5d, 0x82,
            0xb3, 0x82, 0xea, 0x82, 0xbd, 0x5b, 0x8d, 0xc8, 0x8e, 0x71, 0x2f, 0x82, 0xb3, 0x82, 0xa2, 0x82,
            0xb5, 0x5d, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x70, 0xeb, 0x12, 0x00, 0x5a, 0xbc, 0xf6, 0x79,
            0xc2, 0x5a, 0xea, 0x79, 0x7c, 0x42, 0xeb, 0x03, 0x80, 0xeb, 0x12, 0x00, 0x51, 0x5e, 0xea, 0x79,
            0x7c, 0x42, 0xeb, 0x03, 0x08, 0x42, 0x2b, 0x01, 0x19, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x5f, 0x43, 0xea, 0x79, 0xe3, 0xeb, 0x48, 0xee, 0x7c, 0x42, 0xeb, 0x03
        };

        private readonly InjectionManagementConfiguration _config;
        private readonly ITitleTranslationManager _titleTranslationManager;
        private readonly IFullWidthConverter _fullWidthConverter;
        private readonly IBinaryFactory _binaryFactory;

        public InjectFlowWorkflow(InjectionManagementConfiguration config, ITitleTranslationManager titleTranslationManager,
            IBinaryFactory binaryFactory, IFullWidthConverter fullWidthConverter)
        {
            _config = config;
            _titleTranslationManager = titleTranslationManager;
            _fullWidthConverter = fullWidthConverter;
            _binaryFactory = binaryFactory;
        }

        public async Task Run()
        {
            TitleTextData[] titles = await _titleTranslationManager.GetSceneTitles();

            InjectTitleTexts(titles);
        }

        private void InjectTitleTexts(TitleTextData[] titles)
        {
            string floFilePath = Path.Combine(_config.InputFolder, "tt1.flo");
            using Stream openedFile = File.Open(floFilePath, FileMode.Open);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjis = Encoding.GetEncoding("Shift-JIS");

            using IBinaryReaderX br = _binaryFactory.CreateReader(openedFile, sjis);
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(openedFile, sjis);

            br.BaseStream.Position = 0xc;
            int table3Count = br.ReadInt32();
            int table3Offset = br.ReadInt32();

            for (var i = 0; i < table3Count; i++)
            {
                br.BaseStream.Position = table3Offset + i * 0x200;
                br.BaseStream.Position += 0x5c;
                string sceneName = br.ReadNullTerminatedString();

                if (string.IsNullOrEmpty(sceneName))
                    continue;

                TitleTextData? titleEntry = titles.FirstOrDefault(x => x.Scene == sceneName);

                string? processedText = ProcessTitleText(titleEntry);
                if (string.IsNullOrEmpty(processedText))
                    continue;

                // Write title
                long entryOffset = table3Offset + i * 0x200;

                bw.BaseStream.Position = entryOffset;
                bw.Write(FillBytes);

                bw.BaseStream.Position = entryOffset;
                int textLength = Math.Min(processedText.Length, 0x2E);
                bw.WriteString(processedText[..textLength], sjis, false);
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
            convertedText = convertedText[..index] + convertedText[index..].Replace('　', ' ');

            return convertedText;
        }
    }
}
