#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace DatasetTag.Common.Configuration;

/// <summary>
/// Data transfer object for category of images of an image
/// </summary>
/// <remarks>
/// Creation Date: 09th of January, 2024
/// </remarks>
public class ImageInfoDto
{
    public string? ImageName { get; set; }
    public string? TriggerWord { get; set; }
    public List<CategoryDto>? Categories { get; set; }
}

/// <summary>
/// Data transfer object for tags of categories of an image
/// </summary>
/// <remarks>
/// Creation Date: 09th of January, 2024
/// </remarks>
public class CategoryDto
{
    public string? CategoryName { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Data transfer object for images in a directory
/// </summary>
/// <remarks>
/// Creation Date: 09th of January, 2024
/// </remarks>
public class DirectoryImagesDto
{
    public List<ImageInfoDto>? Images { get; set; }
}