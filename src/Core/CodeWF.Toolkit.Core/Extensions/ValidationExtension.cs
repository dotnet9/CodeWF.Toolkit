using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CodeWF.Toolkit.Core.Extensions;

public static class ValidationExtension
{
    [RequiresUnreferencedCode("DataAnnotations validation reflects over models and attributes. Use generated or explicit validation for trimming and NativeAOT.")]
    public static bool TryValidateObject<T>(this T beValidInstance, out List<ValidationResult>? results) where T : class
    {
        var context = new ValidationContext(beValidInstance);
        var validResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(beValidInstance, context, validResults, true);
        if (isValid)
        {
            validResults = null;
            results = null;
            return true;
        }

        results = validResults;
        return false;
    }
}
