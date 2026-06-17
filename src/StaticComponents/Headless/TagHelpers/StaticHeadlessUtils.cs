using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless.TagHelpers;

internal static class StaticHeadlessUtils
{

    internal static void SetGeneralInputAttributes(TagHelperOutput output, string propertyName, ModelMetadata propertyMetadata)
    {
        output.Attributes.SetAttribute("name", propertyName);
        output.Attributes.SetAttribute("id", SanitizeId(propertyName));

        SetValidationAttributes(output, propertyMetadata);
    }

    internal static void SetValidationAttributes(TagHelperOutput output, ModelMetadata propertyMetadata)
    {
        var inputExpressionAsDefault = (DefaultModelMetadata)propertyMetadata;
        var attributesToCheck = new List<object>(inputExpressionAsDefault.Attributes.PropertyAttributes!);

        var requiredAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(RequiredAttribute)) as RequiredAttribute;
        var displayNameAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(DisplayNameAttribute)) as DisplayNameAttribute;
        var emailAddressAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(EmailAddressAttribute)) as EmailAddressAttribute;
        var urlAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(UrlAttribute)) as UrlAttribute;
        var phoneAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(PhoneAttribute)) as PhoneAttribute;
        var regularExpressionAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(RegularExpressionAttribute)) as RegularExpressionAttribute;
        var rangeAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(RangeAttribute)) as RangeAttribute;
        var stringLengthAttribute = attributesToCheck.FirstOrDefault(x => x.GetType() == typeof(StringLengthAttribute)) as StringLengthAttribute;

        var displayNameForProperty = GetDisplayNameFromProperty(displayNameAttribute, inputExpressionAsDefault.PropertyName!);
        var containsAnyValidationAttributes = attributesToCheck.Any(x => IsValidationAttribute(x.GetType()));

        if(containsAnyValidationAttributes)
        {
            output.Attributes.SetAttribute("data-val", "true");
        }

        if (propertyMetadata.IsRequired)
        {
            var defaultErrorMessage = $"The {displayNameForProperty} field is required.";
            var customErrorMessage = requiredAttribute?.ErrorMessage?.Replace("{0}", displayNameForProperty);
            var errorMessage = requiredAttribute?.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-required", errorMessage);
        }

        if(emailAddressAttribute != null)
        {
            var defaultErrorMessage = $"The {displayNameForProperty} field must be a valid email address.";
            var customErrorMessage = emailAddressAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty);
            var errorMessage = emailAddressAttribute.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-email", errorMessage);
        }

        if (urlAttribute != null)
        {
            var defaultErrorMessage = $"The {displayNameForProperty} field is not a valid fully-qualified http, https, or ftp URL.";
            var customErrorMessage = urlAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty);
            var errorMessage = urlAttribute.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-url", errorMessage);
        }

        if(phoneAttribute != null)
        {
            var defaultErrorMessage = $"The {displayNameForProperty} field is not a valid phone number.";
            var customErrorMessage = phoneAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty);
            var errorMessage = phoneAttribute.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-phone", errorMessage);
        }

        if(regularExpressionAttribute != null)
        {
            var defaultErrorMessage = $"The field {displayNameForProperty} must match the regular expression '{regularExpressionAttribute.Pattern}'.";
            var customErrorMessage = regularExpressionAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty).Replace("{1}", regularExpressionAttribute.Pattern);
            var errorMessage = regularExpressionAttribute.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-regex", errorMessage);
            output.Attributes.SetAttribute("data-val-regex-pattern", regularExpressionAttribute.Pattern);
        }

        if(rangeAttribute != null)
        {
            var minimum = rangeAttribute.Minimum;
            var maximum = rangeAttribute.Maximum;

            var defaultErrorMessage = $"The field {displayNameForProperty} must be between {minimum} and {maximum}.";
            var customErrorMessage = rangeAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty).Replace("{1}", minimum.ToString()).Replace("{2}", maximum.ToString());
            var errorMessage = rangeAttribute.ErrorMessage is null ? defaultErrorMessage : rangeAttribute.ErrorMessage;
            output.Attributes.SetAttribute("data-val-range", errorMessage);
            output.Attributes.SetAttribute("data-val-range-min", rangeAttribute.Minimum);
            output.Attributes.SetAttribute("data-val-range-max", rangeAttribute.Maximum);
        }

        if(stringLengthAttribute != null)
        {
            var minLength = stringLengthAttribute.MinimumLength;
            var maxLength = stringLengthAttribute.MaximumLength;

            var customErrorMessage = stringLengthAttribute.ErrorMessage?.Replace("{0}", displayNameForProperty).Replace("{1}", maxLength.ToString()).Replace("{2}", minLength.ToString());

            output.Attributes.SetAttribute("data-val-length-max", stringLengthAttribute.MaximumLength);
            output.Attributes.SetAttribute("maxlength", stringLengthAttribute.MaximumLength);

            var errorMessage = "";
            var hasMinLength = minLength > 0;

            var defaultErrorMessage = hasMinLength ? $"The field {displayNameForProperty} must be between {minLength} and {maxLength} characters long." : $"The field {displayNameForProperty} must be at most {stringLengthAttribute.MaximumLength} characters long.";

            if (hasMinLength)
            {
                output.Attributes.SetAttribute("data-val-length-min", minLength);                
            }

            errorMessage = stringLengthAttribute.ErrorMessage is null ? defaultErrorMessage : customErrorMessage;
            output.Attributes.SetAttribute("data-val-length", errorMessage);
        }
    }

    private static bool IsValidationAttribute(Type? current)
    {
        if(current == typeof(ValidationAttribute))
            return true;

        while (current is not null)
        {
            if(current?.BaseType == typeof(ValidationAttribute))
            {
                return true;
            }

            current = current?.BaseType;
        }

        return false;
    }

    private static string GetDisplayNameFromProperty(DisplayNameAttribute? displayNameAttribute, string propertyName)
    {
        if (displayNameAttribute is not null)
        {
            return displayNameAttribute.DisplayName;
        }

        return propertyName ?? string.Empty;
    }

    internal static string FormatModelValue(object value)
    {
        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    internal static void SetValueAttribute(TagHelperOutput output, ModelExpression inputExpression)
    {
        if (!output.Attributes.ContainsName("value"))
        {
            var modelValue = inputExpression.Model;
            if (modelValue is not null)
            {
                output.Attributes.SetAttribute("value", FormatModelValue(modelValue));
            }
        }
    }

    private static string SanitizeId(string name)
    {
        // asp-for replaces characters that are invalid in HTML ids — most commonly '.' from
        // nested property paths and '['/']' from collection indexers.
        return name
            .Replace('.', '_')
            .Replace('[', '_')
            .Replace(']', '_');
    }
}
