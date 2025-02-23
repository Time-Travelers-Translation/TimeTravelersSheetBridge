using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract.ConfigBinary;
using Logic.Domain.Level5Management.Contract.Cryptography;
using Logic.Domain.Level5Management.Contract.DataClasses.ConfigBinary;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.TimeTravelersManagement.Texts
{
    internal class EventTextParser : IEventTextParser
    {
        private readonly IConfigurationReader<RawConfigurationEntry> _configReader;

        private readonly uint _lastUpdateDatetimeChecksum;
        private readonly uint _lastUpdateUserChecksum;
        private readonly uint _lastUpdateMachineChecksum;
        private readonly uint _textInfoBeginChecksum;
        private readonly uint _textInfoChecksum;
        private readonly uint _textInfoEndChecksum;
        private readonly uint _textIndexBeginChecksum;
        private readonly uint _textIndexChecksum;
        private readonly uint _textIndexEndChecksum;

        public EventTextParser(IChecksumFactory checksumFactory, IConfigurationReader<RawConfigurationEntry> configReader)
        {
            IChecksum<uint> jamCrc = checksumFactory.CreateCrc32Jam();

            _configReader = configReader;

            _lastUpdateDatetimeChecksum = jamCrc.ComputeValue("LAST_UPDATE_DATE_TIME");
            _lastUpdateUserChecksum = jamCrc.ComputeValue("LAST_UPDATE_USER");
            _lastUpdateMachineChecksum = jamCrc.ComputeValue("LAST_UPDATE_MACHINE");
            _textInfoBeginChecksum = jamCrc.ComputeValue("EVENT_TEXT_INFO_BEGIN");
            _textInfoChecksum = jamCrc.ComputeValue("EVENT_TEXT_INFO");
            _textInfoEndChecksum = jamCrc.ComputeValue("EVENT_TEXT_INFO_END");
            _textIndexBeginChecksum = jamCrc.ComputeValue("EVENT_TEXT_INDEX_BEGIN");
            _textIndexChecksum = jamCrc.ComputeValue("EVENT_TEXT_INDEX");
            _textIndexEndChecksum = jamCrc.ComputeValue("EVENT_TEXT_INDEX_END");
        }

        public EventTextConfiguration Parse(string filepath, StringEncoding encoding)
        {
            using Stream fileStream = File.OpenRead(filepath);

            return Parse(fileStream, encoding);
        }

        public EventTextConfiguration Parse(Stream input, StringEncoding encoding)
        {
            Configuration<RawConfigurationEntry> config = _configReader.Read(input, encoding);

            return Parse(config);
        }

        public EventTextConfiguration Parse(Configuration<RawConfigurationEntry> config)
        {
            var result = new EventTextConfiguration();

            for (var i = 0; i < config.Entries.Length; i++)
            {
                RawConfigurationEntry entry = config.Entries[i];

                if (entry.Hash == _lastUpdateDatetimeChecksum)
                {
                    if (entry.Values[0].Value == null)
                        continue;

                    result.LastUpdateDateTime = DateTime.Parse((string)entry.Values[0].Value!);
                    continue;
                }

                if (entry.Hash == _lastUpdateUserChecksum)
                {
                    result.LastUpdateUser = (string?)entry.Values[0].Value;
                    continue;
                }

                if (entry.Hash == _lastUpdateMachineChecksum)
                {
                    result.LastUpdateMachine = (string?)entry.Values[0].Value;
                    continue;
                }

                if (entry.Hash == _textInfoBeginChecksum)
                {
                    result.Texts = ReadEventTextInfos(config.Entries, ref i);
                    continue;
                }

                if (entry.Hash == _textIndexBeginChecksum)
                {
                    if (result.Texts == null)
                        throw new InvalidOperationException("Texts were not read yet.");

                    _ = ReadEventTextIndexes(config.Entries, result.Texts.Length, ref i);
                }
            }

            return result;
        }

        private EventText[] ReadEventTextInfos(RawConfigurationEntry[] entries, ref int index)
        {
            if (entries[index].Hash != _textInfoBeginChecksum)
                throw new InvalidOperationException("TextInfos are not properly opened.");

            var count = (int)entries[index++].Values[0].Value!;
            int endIndex = index + count;

            var result = new EventText[count];

            int startIndex = index;
            for (; index < Math.Min(entries.Length, endIndex); index++)
            {
                RawConfigurationEntry entry = entries[index];

                if (entry.Hash == _textInfoChecksum)
                {
                    result[index - startIndex] = new EventText
                    {
                        Hash = (uint)(int)entry.Values[0].Value!,
                        SubId = (int)entry.Values[1].Value!,
                        Text = (string?)entry.Values[2].Value
                    };
                    continue;
                }

                if (entry.Hash == _textInfoEndChecksum)
                    break;
            }

            if (entries[index].Hash != _textInfoEndChecksum)
                throw new InvalidOperationException("TextInfos are not properly closed.");

            return result;
        }

        private EventTextIndex[] ReadEventTextIndexes(RawConfigurationEntry[] entries, int count, ref int index)
        {
            if (entries[index].Hash != _textIndexBeginChecksum)
                throw new InvalidOperationException("TextIndexes are not properly opened.");

            var result = new EventTextIndex[count];

            index++;
            int startIndex = index;

            for (; index < entries.Length; index++)
            {
                RawConfigurationEntry entry = entries[index];

                if (entry.Hash == _textIndexChecksum)
                {
                    result[index - startIndex] = new EventTextIndex
                    {
                        Hash = (uint)(int)entry.Values[0].Value!,
                        SubId = (int)entry.Values[1].Value!,
                        Index = (int)entry.Values[2].Value!
                    };
                    continue;
                }

                if (entry.Hash == _textIndexEndChecksum)
                    break;
            }

            if (entries[index].Hash != _textIndexEndChecksum)
                throw new InvalidOperationException("TextIndexes are not properly closed.");

            return result;
        }
    }
}
