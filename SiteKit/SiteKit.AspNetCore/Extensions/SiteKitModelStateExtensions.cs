using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitModelStateExtensions
    {
        public static void AddModelErrors(this ModelStateDictionary modelState, string key, IEnumerable<string> errorMessages)
        {
            if (errorMessages is null)
                throw new ArgumentNullException(nameof(errorMessages));

            foreach (var errorMessage in errorMessages)
            {
                modelState.AddModelError(key ?? string.Empty, errorMessage);
            }
        }

        public static void AddModelErrors(this ModelStateDictionary modelState, IEnumerable<ValidationResult> validationResults)
        {
            if (validationResults is null)
                throw new ArgumentNullException(nameof(validationResults));

            foreach (var result in validationResults)
            {
                modelState.AddModelError(result.MemberNames.FirstOrDefault() ?? string.Empty, result.ErrorMessage ?? string.Empty);
            }
        }
    }
}
