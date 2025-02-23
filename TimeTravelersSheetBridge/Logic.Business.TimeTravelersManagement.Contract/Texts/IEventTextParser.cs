using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.ConfigBinary;
using Logic.Domain.Level5Management.Contract.Enums.ConfigBinary;

namespace Logic.Business.TimeTravelersManagement.Contract.Texts
{
    public interface IEventTextParser
    {
        EventTextConfiguration Parse(string filepath, StringEncoding encoding);
        EventTextConfiguration Parse(Stream input, StringEncoding encoding);
        EventTextConfiguration Parse(Configuration<RawConfigurationEntry> config);
    }
}
