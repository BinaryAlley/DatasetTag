namespace DatasetTag.Common.Configuration;

public class ApplicationConfigDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    public double ImagePreviewsPanelHeight { get; set; }
    public double ImagePreviewPanelWidth { get; set; }
    public double TagCategoriesPanelWidth { get; set; }
    public bool IsFirstRun { get; set; }
    public bool IsMaximized { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public int WindowPositionX { get; set; }
    public int WindowPositionY { get; set; }
    #endregion
}