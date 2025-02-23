using System.Text;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.Cryptography;
using Logic.Domain.Level5Management.Contract.DataClasses.Archive;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.InjectionManagement
{
    internal abstract class BaseInjectStoryWorkflow
    {
        private readonly InjectionManagementConfiguration _config;
        private readonly IPckReader _pckReader;
        private readonly IPckWriter _pckWriter;
        private readonly IFullWidthConverter _fullWidthConverter;
        private readonly IChecksum<uint> _crc;
        private readonly IEventTextParser _eventParser;
        private readonly IEventTextComposer _eventComposer;

        public BaseInjectStoryWorkflow(InjectionManagementConfiguration config,
            IPckReader pckReader, IPckWriter pckWriter,
            IFullWidthConverter fullWidthConverter, IChecksumFactory checksumFactory,
            IEventTextParser eventParser, IEventTextComposer eventComposer)
        {
            _config = config;
            _pckReader = pckReader;
            _pckWriter = pckWriter;
            _fullWidthConverter = fullWidthConverter;
            _crc = checksumFactory.CreateCrc32();
            _eventParser = eventParser;
            _eventComposer = eventComposer;
        }

        protected void InjectChapterPck(string pckName, IDictionary<string, StoryTextData[]> chapterEntries)
        {
            NormalizeTranslationData(chapterEntries);

            string pckFileName = Path.Combine(_config.InputFolder, pckName + ".pck");

            Stream pckStream = File.OpenRead(pckFileName);
            PckArchiveData pckData = _pckReader.Read(pckStream);

            InjectTranslationData(pckData, chapterEntries);

            string pckOutFileName = pckFileName + ".out";

            Stream pckOutStream = File.Create(pckOutFileName);
            _pckWriter.Write(pckData, pckOutStream);

            pckStream.Close();
            pckOutStream.Close();

            File.Replace(pckOutFileName, pckFileName, null);
        }

        private void NormalizeTranslationData(IDictionary<string, StoryTextData[]> chapterEntries)
        {
            foreach (string sceneName in chapterEntries.Keys)
            {
                // Remove invalid translations
                chapterEntries[sceneName] = chapterEntries[sceneName].Where(IsValidStoryText).ToArray();

                // Normalize indexes
                foreach (IGrouping<string, StoryTextData> eventGroup in chapterEntries[sceneName].GroupBy(x => x.Event))
                {
                    var index = 0;
                    var isStart = true;
                    foreach (StoryTextData eventEntry in eventGroup)
                    {
                        if (!isStart && eventEntry.Index == 0)
                        {
                            index = 1;
                            continue;
                        }

                        eventEntry.Index = index++;
                        isStart = false;
                    }
                }
            }
        }

        private bool IsValidStoryText(StoryTextData storyText)
        {
            if (storyText.Event.StartsWith("0x"))
                return false;

            return storyText.Text.Text != "<remove>";
        }

        private void InjectTranslationData(PckArchiveData pckData, IDictionary<string, StoryTextData[]> chapterEntries)
        {
            IDictionary<uint, HashArchiveEntry> pckLookup = pckData.Files.ToDictionary(x => x.Hash);

            foreach (string sceneName in chapterEntries.Keys)
            {
                StoryTextData[] textEntries = chapterEntries[sceneName];

                uint sceneHash = _crc.ComputeValue(sceneName);
                if (!pckLookup.TryGetValue(sceneHash, out HashArchiveEntry? pckEntry))
                    continue;

                var eventTexts = new List<EventText>();

                foreach (StoryTextData textEntry in textEntries)
                {
                    eventTexts.Add(new EventText
                    {
                        Hash = ~_crc.ComputeValue(textEntry.Event),
                        SubId = textEntry.Index,
                        Text = GetEntryText(textEntry)
                    });
                }

                EventTextConfiguration pckEntries = _eventParser.Parse(pckEntry.Content, StringEncoding.Sjis);
                pckEntries.Texts = eventTexts.ToArray();

                var ms = new MemoryStream();
                _eventComposer.Compose(pckEntries, ms);

                pckEntry.Content = ms;
            }
        }

        private string GetEntryText(StoryTextData textEntry)
        {
            string? convertedText = _fullWidthConverter.Convert(textEntry.Text.Text);

            if (textEntry.Speaker?.StartsWith('＊') ?? false)
                return textEntry.Speaker + convertedText;

            string? convertedSpeaker = _fullWidthConverter.Convert(textEntry.Speaker);
            return $"{convertedSpeaker}: “{convertedText}”";
        }

        private void InjectChapterEntries(string chapterId, IList<HashArchiveEntry> pckFiles, IList<StoryTextData> chapterEntries)
        {
            var events = pckFiles.Select(f => new { PckFile = f, EventText = _eventParser.Parse(f.Content, StringEncoding.Sjis), f.Hash });
            var hashedChapterEntries = chapterEntries
                .Where(ce => ce.Scene.StartsWith(chapterId))
                .Select(ce => new { ChapterEntry = ce, Hash = _crc.ComputeValue(Encoding.ASCII.GetBytes(ce.Scene)) })
                .Join(events, ce => ce.Hash, cb => cb.Hash, (ce, cb) => new { ce, cb });

            var list2 = hashedChapterEntries.GroupBy(x => x.cb.EventText, y => new { y.ce.ChapterEntry, y.cb.PckFile });
            //foreach (var el in list2)
            //{
            //    el.Key;
            //    el.FirstOrDefault().

            //    el.Key.Texts.RemoveAll(x =>
            //        (uint?)x.MetaInfo.ElementAtOrDefault(2)?.Value == 0xffffffff);

            //    int textCount = el.Key.Entries.Count(x => x.Text != null);

            //    var copy = (CfgEntry)el.Key.Entries[4].Clone();
            //    el.Key.Entries.RemoveRange(4, textCount - 3);

            //    var index = 4;
            //    var textIdCorrection = -el.First().ChapterEntry.TextId;
            //    foreach (var entry in el)
            //    {
            //        // Correct wrong TextId's due to entries that are marked as removed
            //        if (entry.ChapterEntry.TextId == 0)
            //            textIdCorrection = 0;

            //        string translatedText = string.IsNullOrEmpty(entry.ChapterEntry.TranslatedText2)
            //            ? entry.ChapterEntry.TranslatedText1
            //            : entry.ChapterEntry.TranslatedText2;

            //        if (translatedText == "<remove>")
            //        {
            //            textIdCorrection--;
            //            continue;
            //        }

            //        var toInsert = (CfgEntry)copy.Clone();

            //        if (entry.ChapterEntry.ContentOriginal.StartsWith('＊'))
            //            toInsert.Text = new string(entry.ChapterEntry.ContentOriginal.TakeWhile(c => c == '＊').ToArray()) + ConvertText(translatedText);
            //        else
            //            toInsert.Text = $"{ConvertText(entry.ChapterEntry.TranslatedSpeaker)}: “" + ConvertText(translatedText) + "”";
            //        toInsert.MetaInfo[0].UpdateValue(~crc32.Compute(Encoding.ASCII.GetBytes(entry.ChapterEntry.Event)));
            //        toInsert.MetaInfo[1].UpdateValue((uint)(entry.ChapterEntry.TextId + textIdCorrection));

            //        el.Key.Entries.Insert(index++, toInsert);
            //    }

            //    var savedCfgBin = new MemoryStream();
            //    el.Key.Save(savedCfgBin);
            //    el.First().PckFile.Data = savedCfgBin;
            //}
        }
    }
}
