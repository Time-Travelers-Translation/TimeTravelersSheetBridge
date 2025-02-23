﻿using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Domain.Level5Management.Contract.DataClasses.ConfigBinary
{
    public class ConfigurationEntryValue
    {
        public ValueType Type { get; set; }
        public object? Value { get; set; }
    }
}
