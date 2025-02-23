using System.Text;
using Logic.Business.InjectionManagement.DataClasses.Scene;
using Logic.Business.InjectionManagement.InternalContract.Scene;
using Logic.Business.TranslationManagement.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;

namespace Logic.Business.InjectionManagement.Scene
{
    internal class SceneState : ISceneState
    {
        private readonly IBinaryFactory _binaryFactory;

        private ScnHeader _header;

        private List<(int offset, string value)> _flags;
        private List<(int offset, string value)> _counters;
        private List<(int offset, string value)> _selections;
        private List<(int offset, string value)> _quickTimes;

        private long _sceneEntryOffset;
        private IList<SceneEntry> _sceneEntries;
        private IList<SceneName> _sceneNames;

        private List<(int offset, string value)> _decisionTexts;
        private List<(int offset, string value)> _badEnds;
        private List<(int offset, string value)> _hints;

        public SceneState(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public void Load(Stream input)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjis = Encoding.GetEncoding("Shift-JIS");

            using IBinaryReaderX reader = _binaryFactory.CreateReader(input, sjis, true);

            // Read header
            _header = ReadHeader(reader);

            // Read preset values
            input.Position = 0x60;
            int count = reader.ReadInt32();
            input.Position += count;

            input.Position += 4;
            count = reader.ReadInt32();
            input.Position += count;

            input.Position += 4;
            count = reader.ReadInt32();
            input.Position += count;

            // Read value strings
            _flags = ReadStrings(reader).ToList();
            _counters = ReadStrings(reader).ToList();
            _selections = ReadStrings(reader).ToList();
            _quickTimes = ReadStrings(reader).ToList();

            // Read scenes
            _sceneEntryOffset = input.Position;
            _sceneEntries = ReadSceneEntries(reader, _header.sceneEntryCount);
            _sceneNames = ReadSceneNames(reader, _header.sceneEntryCount);

            // Read texts
            _decisionTexts = ReadStringSection(reader, _header.decisionOffset, _header.badEndOffset).ToList();
            _badEnds = ReadStringSection(reader, _header.badEndOffset, _header.hintOffset).ToList();
            _hints = ReadStringSection(reader, _header.hintOffset, input.Length).ToList();
        }

        public void Save(Stream input, Stream output)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjis = Encoding.GetEncoding("Shift-JIS");

            using IBinaryReaderX reader = _binaryFactory.CreateReader(input, sjis, true);
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(output, sjis, true);

            // Copy original file content
            var buffer = new byte[0x4000];
            for (var i = 0x5c; i < _sceneEntryOffset; i += 0x4000)
            {
                var bufferSize = (int)Math.Min(0x4000, _sceneEntryOffset - i);

                input.Position = output.Position = i;

                _ = input.Read(buffer, 0, bufferSize);
                output.Write(buffer, 0, bufferSize);
            }

            // Write scene entries
            WriteSceneEntries(_sceneEntries, writer);

            // Write scene names
            WriteSceneNames(_sceneNames, writer);

            // Write decision texts
            _header.decisionOffset = (int)output.Position;
            foreach ((int offset, string value) decisionText in _decisionTexts)
                writer.WriteString(decisionText.value, sjis, false);

            // Write bad end texts
            _header.badEndOffset = (int)output.Position;
            foreach ((int offset, string value) badEndText in _badEnds)
                writer.WriteString(badEndText.value, sjis, false);

            // Write hint texts
            _header.hintOffset = (int)output.Position;
            foreach ((int offset, string value) text in _hints)
                writer.WriteString(text.value, sjis, false);

            // Write header
            output.Position = 0;
            WriteHeader(_header, writer);
        }

        public void UpdateDecisionTexts(DecisionTextData[] decisionTexts)
        {
            var stringIndex = 0;
            var stringOffset = 0;

            var sjis = Encoding.GetEncoding("Shift-JIS");
            foreach (var decision in decisionTexts)
            {
                var affectedScene = _sceneEntries.FirstOrDefault(x =>
                    sjis.GetString(_sceneNames[x.sceneId].sceneName.TakeWhile(c => c != 0).ToArray()) ==
                    decision.Scene);

                if (affectedScene == null || affectedScene.metaId != 1)
                    continue;

                var decisionData = affectedScene.metaData as DecisionData;
                decisionData.decisionTextOffset = stringOffset;

                foreach (var decisionText in decision.Texts)
                {
                    var finalText = !string.IsNullOrEmpty(decisionText.Text)
                        ? decisionText.Text
                        : _decisionTexts[stringIndex].value;

                    _decisionTexts[stringIndex] = (stringOffset, finalText);
                    stringIndex++;

                    stringOffset += sjis.GetByteCount(finalText) + 1;
                }
            }
        }

        public void UpdateTitles(TitleTextData[] badEndTexts)
        {
            var badEndIndex = 0;
            var badEndStringOffset = 0;

            var sjis = Encoding.GetEncoding("Shift-JIS");
            foreach (var hint in badEndTexts)
            {
                var affectedScene = _sceneEntries.FirstOrDefault(x =>
                    sjis.GetString(_sceneNames[x.sceneId].sceneName.TakeWhile(c => c != 0).ToArray()) ==
                    hint.Scene);

                if (affectedScene == null || affectedScene.metaId != 4)
                    continue;

                var badEnd = badEndTexts.FirstOrDefault(x => x.Scene == hint.Scene);
                var finalBadEndText = !string.IsNullOrEmpty(badEnd.Text.Text)
                    ? badEnd.Text.Text
                    : _badEnds[badEndIndex].value;

                var badEndData = affectedScene.metaData as BadEndData;

                badEndData!.titleOffset = badEndStringOffset;
                _badEnds[badEndIndex++] = (badEndStringOffset, finalBadEndText);
                badEndStringOffset += sjis.GetByteCount(finalBadEndText) + 1;
            }
        }

        public void UpdateHints(HintTextData[] hintTexts)
        {
            var hintIndex = 0;
            var hintStringOffset = 0;

            var sjis = Encoding.GetEncoding("Shift-JIS");
            foreach (var hint in hintTexts)
            {
                var affectedScene = _sceneEntries.FirstOrDefault(x =>
                    sjis.GetString(_sceneNames[x.sceneId].sceneName.TakeWhile(c => c != 0).ToArray()) ==
                    hint.Scene);

                if (affectedScene == null || affectedScene.metaId != 4)
                    continue;

                var finalHintText = !string.IsNullOrEmpty(hint.Text.Text)
                    ? hint.Text.Text
                    : _hints[hintIndex].value;

                var badEndData = affectedScene.metaData as BadEndData;

                badEndData!.hintOffset = hintStringOffset;
                _hints[hintIndex++] = (hintStringOffset, finalHintText);
                hintStringOffset += sjis.GetByteCount(finalHintText) + 1;
            }
        }

        private static IEnumerable<(int offset, string value)> ReadStrings(IBinaryReaderX reader)
        {
            int count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                string name = reader.ReadNullTerminatedString();
                yield return ((int)(reader.BaseStream.Position - name.Length - 1), name);

                int length = name.Length + 1;
                if (length % 8 == 0)
                {
                    length++;
                    reader.BaseStream.Position++;
                }
                while (length % 8 != 0)
                {
                    length++;
                    reader.BaseStream.Position++;
                }
            }
        }

        private static IEnumerable<(int offset, string value)> ReadStringSection(IBinaryReaderX reader, long offset, long endOffset)
        {
            reader.BaseStream.Position = offset;

            while (reader.BaseStream.Position < endOffset)
                yield return ((int)reader.BaseStream.Position, reader.ReadNullTerminatedString());
        }

        #region Read

        private ScnHeader ReadHeader(IBinaryReaderX reader)
        {
            return new ScnHeader
            {
                magic = reader.ReadString(8),
                unk1 = reader.ReadInt32(),
                unk2 = reader.ReadInt32(),
                unk3 = reader.ReadInt16(),
                unk4 = reader.ReadInt16(),
                unk5 = reader.ReadInt16(),
                unk6 = reader.ReadInt16(),
                sceneEntryCount = reader.ReadInt32(),
                selectionCount = reader.ReadInt32(),
                quickTimeCount = reader.ReadInt32(),
                flagCount = reader.ReadInt32(),
                counterCount = reader.ReadInt32(),
                sceneOffset = reader.ReadInt32(),
                decisionOffset = reader.ReadInt32(),
                badEndOffset = reader.ReadInt32(),
                hintOffset = reader.ReadInt32()
            };
        }

        private IList<SceneEntry> ReadSceneEntries(IBinaryReaderX reader, int count)
        {
            reader.ReadInt32();
            reader.ReadInt32();

            var result = new List<SceneEntry>();

            for (var i = 0; i < count; i++)
                result.Add(ReadSceneEntry(reader));

            return result;
        }

        private IList<SceneName> ReadSceneNames(IBinaryReaderX reader, int count)
        {
            var result = new List<SceneName>();

            for (var i = 0; i < count; i++)
                result.Add(ReadSceneName(reader));

            return result;
        }

        private SceneEntry ReadSceneEntry(IBinaryReaderX reader)
        {
            var entry = new SceneEntry
            {
                sceneId = reader.ReadInt16(),
                metaId = reader.ReadInt32()
            };

            entry.metaData = ReadSceneMetaData(reader, entry.metaId);

            entry.branchCount = reader.ReadInt32();
            entry.branchCount2 = reader.ReadInt32();

            entry.branchEntries = new BranchEntry[entry.branchCount];
            for (var i = 0; i < entry.branchCount; i++)
                entry.branchEntries[i] = ReadBranchEntry(reader);

            return entry;
        }

        private SceneName ReadSceneName(IBinaryReaderX reader)
        {
            return new SceneName
            {
                sceneName = reader.ReadBytes(0x22),
                sceneId = reader.ReadInt16()
            };
        }

        private object ReadSceneMetaData(IBinaryReaderX reader, int metaId)
        {
            switch (metaId)
            {
                case 0:
                    return new NoData();

                case 1:
                    return new DecisionData
                    {
                        unk = reader.ReadInt16(),
                        timerIdent = reader.ReadByte(),
                        stringCount = reader.ReadByte(),
                        unk1 = reader.ReadInt32(),
                        decisionTextOffset = reader.ReadInt32()
                    };

                case 2:
                    return new MetaData2
                    {
                        unk1 = reader.ReadInt16(),
                        unk2 = reader.ReadInt16(),
                        unk3 = reader.ReadInt16()
                    };

                case 4:
                    return new BadEndData
                    {
                        unk1 = reader.ReadInt16(),
                        unk2 = reader.ReadInt16(),
                        titleOffset = reader.ReadInt32(),
                        hintOffset = reader.ReadInt32()
                    };

                default:
                    throw new InvalidOperationException($"Unsupported meta data id {metaId}.");
            }
        }

        private BranchEntry ReadBranchEntry(IBinaryReaderX reader)
        {
            var entry = new BranchEntry
            {
                sceneId = reader.ReadInt16(),
                unk0 = reader.ReadInt16(),

                count0 = reader.ReadInt32(),
                count1 = reader.ReadInt32()
            };

            entry.unk3 = new long[entry.count1];
            for (var i = 0; i < entry.count1; i++)
                entry.unk3[i] = reader.ReadInt64();

            entry.count2 = reader.ReadInt32();
            entry.count3 = reader.ReadInt32();

            entry.unk6 = new int[entry.count3];
            for (var i = 0; i < entry.count3; i++)
                entry.unk6[i] = reader.ReadInt32();

            entry.count4 = reader.ReadInt32();
            entry.count5 = reader.ReadInt32();

            entry.unk9 = new int[entry.count5];
            for (var i = 0; i < entry.count5; i++)
                entry.unk9[i] = reader.ReadInt32();

            return entry;
        }

        #endregion

        #region Write

        private void WriteHeader(ScnHeader header, IBinaryWriterX writer)
        {
            writer.WriteString(header.magic, Encoding.ASCII, false, false);
            writer.Write(header.unk1);
            writer.Write(header.unk2);
            writer.Write(header.unk3);
            writer.Write(header.unk4);
            writer.Write(header.unk5);
            writer.Write(header.unk6);
            writer.Write(header.sceneEntryCount);
            writer.Write(header.selectionCount);
            writer.Write(header.quickTimeCount);
            writer.Write(header.flagCount);
            writer.Write(header.counterCount);
            writer.Write(header.sceneOffset);
            writer.Write(header.decisionOffset);
            writer.Write(header.badEndOffset);
            writer.Write(header.hintOffset);
        }

        private void WriteSceneEntries(IList<SceneEntry> entries, IBinaryWriterX writer)
        {
            writer.Write(entries.Count);
            writer.Write(entries.Count);

            foreach (SceneEntry entry in entries)
                WriteSceneEntry(entry, writer);
        }

        private void WriteSceneNames(IList<SceneName> names, IBinaryWriterX writer)
        {
            foreach (SceneName name in names)
                WriteSceneName(name, writer);
        }

        private void WriteSceneEntry(SceneEntry entry, IBinaryWriterX writer)
        {
            writer.Write(entry.sceneId);
            writer.Write(entry.metaId);

            WriteSceneMetaData(entry.metaData, writer);

            writer.Write(entry.branchCount);
            writer.Write(entry.branchCount2);

            foreach (BranchEntry branch in entry.branchEntries)
                WriteBranchEntry(branch, writer);
        }

        private void WriteSceneName(SceneName name, IBinaryWriterX writer)
        {
            writer.Write(name.sceneName);
            writer.Write(name.sceneId);
        }

        private void WriteSceneMetaData(object metaData, IBinaryWriterX writer)
        {
            switch (metaData)
            {
                case NoData:
                    break;

                case DecisionData decision:
                    writer.Write(decision.unk);
                    writer.Write(decision.timerIdent);
                    writer.Write(decision.stringCount);
                    writer.Write(decision.unk1);
                    writer.Write(decision.decisionTextOffset);
                    break;

                case MetaData2 meta2:
                    writer.Write(meta2.unk1);
                    writer.Write(meta2.unk2);
                    writer.Write(meta2.unk3);
                    break;

                case BadEndData badEnd:
                    writer.Write(badEnd.unk1);
                    writer.Write(badEnd.unk2);
                    writer.Write(badEnd.titleOffset);
                    writer.Write(badEnd.hintOffset);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported meta data {metaData.GetType().Name}.");
            }
        }

        private void WriteBranchEntry(BranchEntry branch, IBinaryWriterX writer)
        {
            writer.Write(branch.sceneId);
            writer.Write(branch.unk0);
            writer.Write(branch.count0);
            writer.Write(branch.count1);

            foreach (long element in branch.unk3)
                writer.Write(element);

            writer.Write(branch.count2);
            writer.Write(branch.count3);

            foreach (int element in branch.unk6)
                writer.Write(element);

            writer.Write(branch.count4);
            writer.Write(branch.count5);

            foreach (int element in branch.unk9)
                writer.Write(element);
        }

        #endregion
    }
}
