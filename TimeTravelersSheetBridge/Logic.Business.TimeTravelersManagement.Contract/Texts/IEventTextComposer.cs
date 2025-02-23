using Logic.Business.TimeTravelersManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.ConfigBinary;

namespace Logic.Business.TimeTravelersManagement.Contract.Texts
{
    public interface IEventTextComposer
    {
        void Compose(EventTextConfiguration config, string filePath);
        void Compose(EventTextConfiguration config, Stream output);
        Configuration<RawConfigurationEntry> Compose(EventTextConfiguration config);
    }
}
