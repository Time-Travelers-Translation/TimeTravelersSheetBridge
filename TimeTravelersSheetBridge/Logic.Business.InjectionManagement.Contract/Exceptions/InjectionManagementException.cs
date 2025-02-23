using System.Runtime.Serialization;

namespace Logic.Business.InjectionManagement.Contract.Exceptions
{
    [Serializable]
    public class InjectionManagementException : Exception
    {
        public InjectionManagementException()
        {
        }

        public InjectionManagementException(string message) : base(message)
        {
        }

        public InjectionManagementException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InjectionManagementException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
