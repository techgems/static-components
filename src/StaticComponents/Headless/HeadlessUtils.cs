using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// Utility methods for working with headless components in Razor views.
/// </summary>
public static class HeadlessUtils
{
    /// <summary>
    /// The text rendered inside the &lt;label&gt;. Uses the <see cref="System.ComponentModel.DisplayNameAttribute"/>
    /// if available, otherwise falls back to the property name.
    /// </summary>
    /// <param name="modelExpression">The model expression to get the label content for.</param>
    /// <returns>The label content.</returns>
    public static string GetLabelContent(ModelExpression modelExpression)
    {
        return modelExpression?.Metadata.DisplayName
            ?? modelExpression?.Metadata.PropertyName
            ?? string.Empty;
    }
}
