#region ========================================================================= USING =====================================================================================
using Avalonia.Data.Converters;
using Avalonia.Media;
using DatasetTag.Common.Controls;
using DatasetTag.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace DatasetTag.Common.Converters;

/// <summary>
/// Converter for brushes from tag categories
/// </summary>
/// <remarks>
/// Creation Date: 07th of January, 2024
/// </remarks>
public class TagCategoryToBackgroundConverter : IValueConverter
{
    #region ================================================================= METHODS ===================================================================================
    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TagCategory tagCategory && targetType == typeof(IBrush))
        {
            return tagCategory switch
            {
                TagCategory.TriggerWord => new SolidColorBrush(Color.FromRgb(255, 209, 220), 0.5),
                TagCategory.Type => new SolidColorBrush(Color.FromRgb(255, 229, 180), 0.5),
                TagCategory.Subject => new SolidColorBrush(Color.FromRgb(204, 204, 255), 0.5),
                TagCategory.Shot => new SolidColorBrush(Color.FromRgb(170, 255, 195), 0.5),
                TagCategory.Perspective => new SolidColorBrush(Color.FromRgb(172, 229, 238), 0.5),
                TagCategory.Pose => new SolidColorBrush(Color.FromRgb(255, 250, 205), 0.5),
                TagCategory.Location => new SolidColorBrush(Color.FromRgb(200, 191, 231), 0.5),
                TagCategory.Action => new SolidColorBrush(Color.FromRgb(208, 240, 192), 0.5),
                TagCategory.Gaze => new SolidColorBrush(Color.FromRgb(176, 224, 230), 0.5),
                TagCategory.Mouth => new SolidColorBrush(Color.FromRgb(255, 182, 193), 0.5),
                TagCategory.MouthAction => new SolidColorBrush(Color.FromRgb(255, 218, 185), 0.5),
                TagCategory.Hair => new SolidColorBrush(Color.FromRgb(230, 190, 255), 0.5),
                TagCategory.Limbs => new SolidColorBrush(Color.FromRgb(255, 127, 80), 0.5),
                TagCategory.SubjectDescription => new SolidColorBrush(Color.FromRgb(135, 206, 235), 0.5),
                TagCategory.Scenery => new SolidColorBrush(Color.FromRgb(160, 255, 224), 0.5),
                TagCategory.SceneDescription => new SolidColorBrush(Color.FromRgb(159, 226, 191), 0.5),
                TagCategory.Lighting => new SolidColorBrush(Color.FromRgb(224, 176, 255), 0.5),
                TagCategory.Miscellaneous => new SolidColorBrush(Color.FromRgb(188, 143, 143), 0.5),
                _ => new SolidColorBrush(Color.FromRgb(255, 255, 255), 0.5)
            };
        }
        else if (value == null)
            return null;
        else
            throw new NotSupportedException();
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
    #endregion
}
