using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SiteKit.Data
{
    public class DataChangeResult<T>
        where T : class
    {
        public bool IsSuccess { get; private set; }
        public T? Model { get; private set; }
        public IEnumerable<string> Info { get; private set; } = Enumerable.Empty<string>();
        public IEnumerable<ValidationResult> Errors { get; private set; } = Enumerable.Empty<ValidationResult>();

        public static DataChangeResult<T> Success(T model, string info)
        {
            return new DataChangeResult<T>()
            {
                IsSuccess = true,
                Model = model,
                Info = new[] { info },
            };
        }

        public static DataChangeResult<T> Success(T model, IEnumerable<string> info)
        {
            return new DataChangeResult<T>()
            {
                IsSuccess = true,
                Model = model,
            };
        }

        public static DataChangeResult<T> Fail(string errorMessage)
        {
            return new DataChangeResult<T>()
            {
                IsSuccess = false,
                Errors = new[] { new ValidationResult(errorMessage) },
            };
        }

        public static DataChangeResult<T> Fail(IEnumerable<ValidationResult> errors)
        {
            return new DataChangeResult<T>()
            {
                IsSuccess = false,
                Errors = errors.ToArray(),
            };
        }
    }
}
