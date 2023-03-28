using System;
using System.Runtime.Serialization;

namespace Cohub.Data.Fin.Deposits
{
    [Serializable]
    public class MissingFilingsDepositServiceException : DepositServiceException
    {
        public MissingFilingsDepositServiceException()
        {
        }

        public MissingFilingsDepositServiceException(string message) : base(message)
        {
        }

        public MissingFilingsDepositServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingFilingsDepositServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}