﻿using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Business.TimeTravelersManagement.Contract.Enums;
using Logic.Business.TimeTravelersManagement.Contract.Texts;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract.ConfigBinary;
using Logic.Domain.Level5Management.Contract.Cryptography;
using Logic.Domain.Level5Management.Contract.DataClasses.ConfigBinary;
using ValueType = Logic.Domain.Level5Management.Contract.Enums.ConfigBinary.ValueType;

namespace Logic.Business.TimeTravelersManagement.Texts
{
    internal class EventTextComposer : IEventTextComposer
    {
        private readonly IConfigurationWriter<RawConfigurationEntry> _configWriter;

        private readonly uint _lastUpdateDatetimeChecksum;
        private readonly uint _lastUpdateUserChecksum;
        private readonly uint _lastUpdateMachineChecksum;
        private readonly uint _textInfoBeginChecksum;
        private readonly uint _textInfoChecksum;
        private readonly uint _textInfoEndChecksum;
        private readonly uint _textIndexBeginChecksum;
        private readonly uint _textIndexChecksum;
        private readonly uint _textIndexEndChecksum;

        public EventTextComposer(IChecksumFactory checksumFactory, IConfigurationWriter<RawConfigurationEntry> configWriter)
        {
            IChecksum<uint> jamCrc = checksumFactory.CreateCrc32Jam();

            _configWriter = configWriter;

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

        public void Compose(EventTextConfiguration eventConfig, string filePath)
        {
            using Stream fileStream = File.Create(filePath);

            Compose(eventConfig, fileStream);
        }

        public void Compose(EventTextConfiguration eventConfig, Stream output)
        {
            Configuration<RawConfigurationEntry> config = Compose(eventConfig);

            using Stream configStream = _configWriter.Write(config);
            configStream.CopyTo(output);
        }

        public Configuration<RawConfigurationEntry> Compose(EventTextConfiguration config)
        {
            var result = new Configuration<RawConfigurationEntry>
            {
                Entries = CreateEntries(config),
                Encoding = config.StringEncoding
            };

            return result;
        }

        private RawConfigurationEntry[] CreateEntries(EventTextConfiguration config)
        {
            int entryCount = 7 + config.Texts.Length * 2;
            var result = new RawConfigurationEntry[entryCount];

            // Set user information
            result[0] = CreateLastUpdateDatetimeEntry(config.LastUpdateDateTime);
            result[1] = CreateLastUpdateUserEntry(config.LastUpdateUser);
            result[2] = CreateLastUpdateMachineEntry(config.LastUpdateMachine);

            // Set text information
            result[3] = CreateTextInfoBeginEntry(config.Texts.Length);
            SetTextInfoEntries(result, 4, config.Texts);
            result[4 + config.Texts.Length] = CreateTextInfoEndEntry();

            // Set text index information
            result[5 + config.Texts.Length] = CreateTextIndexBeginEntry(config.Texts.Length);
            SetTextIndexEntries(result, 6 + config.Texts.Length, config.Texts, EventSorting.Hash);
            result[6 + config.Texts.Length * 2] = CreateTextIndexEndEntry();

            return result;
        }

        private RawConfigurationEntry CreateLastUpdateDatetimeEntry(DateTime? updateTime)
        {
            return new RawConfigurationEntry
            {
                Hash = _lastUpdateDatetimeChecksum,
                Values = new[]
                {
                    new ConfigurationEntryValue
                    {
                        Type = ValueType.String,
                        Value = updateTime?.ToString("yyyy'/'MM'/'dd HH:mm:ss")
                    }
                }
            };
        }

        private RawConfigurationEntry CreateLastUpdateUserEntry(string? user)
        {
            return new RawConfigurationEntry
            {
                Hash = _lastUpdateUserChecksum,
                Values = new[]
                {
                    new ConfigurationEntryValue
                    {
                        Type = ValueType.String,
                        Value = user
                    }
                }
            };
        }

        private RawConfigurationEntry CreateLastUpdateMachineEntry(string? machine)
        {
            return new RawConfigurationEntry
            {
                Hash = _lastUpdateMachineChecksum,
                Values = new[]
                {
                    new ConfigurationEntryValue
                    {
                        Type = ValueType.String,
                        Value = machine
                    }
                }
            };
        }

        private RawConfigurationEntry CreateTextInfoBeginEntry(int count)
        {
            return new RawConfigurationEntry
            {
                Hash = _textInfoBeginChecksum,
                Values = new[]
                {
                    new ConfigurationEntryValue
                    {
                        Type = ValueType.Int,
                        Value = count
                    }
                }
            };
        }

        private void SetTextInfoEntries(RawConfigurationEntry[] entries, int index, EventText[] texts)
        {
            for (var i = 0; i < texts.Length; i++)
            {
                entries[index + i] = new RawConfigurationEntry
                {
                    Hash = _textInfoChecksum,
                    Values = new[]
                    {
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.Int,
                            Value = unchecked((int)texts[i].Hash)
                        },
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.Int,
                            Value = texts[i].SubId
                        },
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.String,
                            Value = texts[i].Text
                        }
                    }
                };
            }
        }

        private RawConfigurationEntry CreateTextInfoEndEntry()
        {
            return new RawConfigurationEntry
            {
                Hash = _textInfoEndChecksum,
                Values = Array.Empty<ConfigurationEntryValue>()
            };
        }

        private RawConfigurationEntry CreateTextIndexBeginEntry(int count)
        {
            return new RawConfigurationEntry
            {
                Hash = _textIndexBeginChecksum,
                Values = Array.Empty<ConfigurationEntryValue>()
            };
        }

        private void SetTextIndexEntries(RawConfigurationEntry[] entries, int index, EventText[] texts, EventSorting sorting)
        {
            switch (sorting)
            {
                case EventSorting.Hash:
                    SetTextIndexEntriesByHash(entries, index, texts);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported event text sorting {sorting}.");
            }
        }

        private void SetTextIndexEntriesByHash(RawConfigurationEntry[] entries, int index, EventText[] texts)
        {
            foreach (EventText text in texts.OrderBy(t => t.Hash).ThenBy(t => t.SubId))
            {
                int entryIndex = Array.IndexOf(texts, text);
                if (entryIndex < 0)
                    continue;

                entries[index++] = new RawConfigurationEntry
                {
                    Hash = _textIndexChecksum,
                    Values = new[]
                    {
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.Int,
                            Value = unchecked((int)text.Hash)
                        },
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.Int,
                            Value = text.SubId
                        },
                        new ConfigurationEntryValue
                        {
                            Type = ValueType.Int,
                            Value = entryIndex
                        }
                    }
                };
            }
        }

        private RawConfigurationEntry CreateTextIndexEndEntry()
        {
            return new RawConfigurationEntry
            {
                Hash = _textIndexEndChecksum,
                Values = Array.Empty<ConfigurationEntryValue>()
            };
        }
    }
}
