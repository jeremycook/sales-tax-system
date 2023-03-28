using System;
using System.Runtime.Serialization;

namespace Cohub.Data.Fin.Deposits
{
    [Serializable]
    public class DepositServiceException : Exception
    {
        public DepositServiceException()
        {
        }

        public DepositServiceException(string message) : base(message)
        {
        }

        public DepositServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DepositServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}