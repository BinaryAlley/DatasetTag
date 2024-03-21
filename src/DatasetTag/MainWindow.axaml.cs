#region ========================================================================= USING =====================================================================================
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DatasetTag.Common.Configuration;
using DatasetTag.Common.Controls;
using DatasetTag.Common.Enums;
using DatasetTag.Common.MVVM;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#endregion

namespace DatasetTag;

/// <summary>
/// Code behind for the application's main window
/// </summary>
/// <remarks>
/// Creation Date: 06th of January, 2024
/// </remarks>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public new event PropertyChangedEventHandler? PropertyChanged;
    private readonly DispatcherTimer timer;
    private bool isWindowLoaded = false;
    private int tickCount = 0;
    #endregion

    #region ================================================================= BINDING COMMANDS ==============================================================================
    public IAsyncCommand BrowseInputAsync_Command { get; private set; }
    public IAsyncCommand RefreshInputAsync_Command { get; private set; }
    public IAsyncCommand AddNewTypeAsync_Command { get; private set; }
    public IAsyncCommand AddNewSubjectAsync_Command { get; private set; }
    public IAsyncCommand AddNewShotAsync_Command { get; private set; }
    public IAsyncCommand AddNewPerspectiveAsync_Command { get; private set; }
    public IAsyncCommand AddNewPoseAsync_Command { get; private set; }
    public IAsyncCommand AddNewLocationAsync_Command { get; private set; }
    public IAsyncCommand AddNewActionAsync_Command { get; private set; }
    public IAsyncCommand AddNewGazeAsync_Command { get; private set; }
    public IAsyncCommand AddNewMouthAsync_Command { get; private set; }
    public IAsyncCommand AddNewMouthActionAsync_Command { get; private set; }
    public IAsyncCommand AddNewHairAsync_Command { get; private set; }
    public IAsyncCommand AddNewLimbAsync_Command { get; private set; }
    public IAsyncCommand AddNewSubjectDescriptionAsync_Command { get; private set; }
    public IAsyncCommand AddNewSceneryAsync_Command { get; private set; }
    public IAsyncCommand AddNewSceneDescriptionAsync_Command { get; private set; }
    public IAsyncCommand AddNewLightingAsync_Command { get; private set; }
    public IAsyncCommand AddMiscellaneousAsync_Command { get; private set; }
    public IAsyncCommand SaveCaptionAsync_Command { get; private set; }
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    public static AppConfig? Configuration { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Default C-tor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        RefreshInputAsync_Command = new AsyncCommand(RefreshImagePreviewsAsync);
        BrowseInputAsync_Command = new AsyncCommand(BrowseInputAsync);
        AddNewTypeAsync_Command = new AsyncCommand(AddNewTypeAsync);
        AddNewSubjectAsync_Command = new AsyncCommand(AddNewSubjectAsync);
        AddNewShotAsync_Command = new AsyncCommand(AddNewShotAsync);
        AddNewPerspectiveAsync_Command = new AsyncCommand(AddNewPerspectiveAsync);
        AddNewPoseAsync_Command = new AsyncCommand(AddNewPoseAsync);
        AddNewLocationAsync_Command = new AsyncCommand(AddNewLocationAsync);
        AddNewActionAsync_Command = new AsyncCommand(AddNewActionAsync);
        AddNewGazeAsync_Command = new AsyncCommand(AddNewGazeAsync);
        AddNewMouthAsync_Command = new AsyncCommand(AddNewMouthAsync);
        AddNewMouthActionAsync_Command = new AsyncCommand(AddNewMouthActionAsync);
        AddNewHairAsync_Command = new AsyncCommand(AddNewHairAsync);
        AddNewLimbAsync_Command = new AsyncCommand(AddNewLimbAsync);
        AddNewSubjectDescriptionAsync_Command = new AsyncCommand(AddNewSubjectDescriptionAsync);
        AddNewSceneryAsync_Command = new AsyncCommand(AddNewSceneryAsync);
        AddNewSceneDescriptionAsync_Command = new AsyncCommand(AddNewSceneDescriptionAsync);
        AddNewLightingAsync_Command = new AsyncCommand(AddNewLightingAsync);
        AddMiscellaneousAsync_Command = new AsyncCommand(AddMiscellaneousAsync);
        SaveCaptionAsync_Command = new AsyncCommand(SaveCaptionAsync);
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        timer.Tick += Timer_Tick;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Displays a dialog for browsing the directory where the dataset input images are located
    /// </summary>
    private async Task BrowseInputAsync()
    {
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false, Title = "Choose dataset images directory" });
        if (result.Count > 0 && result[0].TryGetUri(out Uri? directory))
            txtInputPath.Text = directory.LocalPath;
    }

    /// <summary>
    /// Refreshes the list of detected image in the input folder, and displays them in the dragPanel
    /// </summary>
    private async Task RefreshImagePreviewsAsync()
    {
        // perform validations
        if (!await ValidateInputPathAsync())
            return;
        // clear previous image previews
        grdImages.Children.Clear();
        int column = 0;
        int margin = 5;
        await Task.Run(async () =>
        {
            // iterate all files in the input directory
            var filePaths = Directory.GetFiles(txtInputPath.Text!, "*.jpg")
                                     .Concat(Directory.GetFiles(txtInputPath.Text!, "*.jpeg"))
                                     .Concat(Directory.GetFiles(txtInputPath.Text!, "*.png"))
                                     .Concat(Directory.GetFiles(txtInputPath.Text!, "*.bmp"));
            var loadTasks = filePaths.Select(async file =>
            {
                using (var tempImage = await SixLabors.ImageSharp.Image.LoadAsync(file))
                {
                    var originalSize = new Avalonia.Size(tempImage.Width, tempImage.Height);
                    var resizedImage = await LoadResizedImageAsync(tempImage, (int)grdContainer.RowDefinitions[0].ActualHeight - 70, (int)grdContainer.RowDefinitions[0].ActualHeight - 70);
                    return new { FilePath = file, Image = resizedImage, OriginalSize = originalSize };
                }
            });

            var loadedImages = await Task.WhenAll(loadTasks);
            foreach (var loadedImage in loadedImages)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // for each image file, create a grid that contains an image and a drag panel, and add it to the previews list
                    Grid container = new();
                    container.Width = grdContainer.RowDefinitions[0].ActualHeight - 70;
                    container.Height = grdContainer.RowDefinitions[0].ActualHeight - 70;
                    container.Margin = new Thickness(column * ((grdContainer.RowDefinitions[0].ActualHeight - 70) + margin), 0, 0, 0);
                    container.HorizontalAlignment = HorizontalAlignment.Left;
                    container.VerticalAlignment = VerticalAlignment.Top;
                    container.Background = new SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 0, 0), 0.1);

                    Avalonia.Controls.Image image = new();
                    image.Source = loadedImage.Image;
                    image.Width = grdContainer.RowDefinitions[0].ActualHeight - 70;
                    image.Height = grdContainer.RowDefinitions[0].ActualHeight - 70;
                    image.Margin = new Thickness(0);
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    image.VerticalAlignment = VerticalAlignment.Top;
                    image.Cursor = new Cursor(StandardCursorType.Hand);
                    image.Tag = loadedImage.FilePath; // store the path of the original image file
                    image.PointerPressed += Thumbnail_PointerPressed;
                    ToolTip.SetTip(image, loadedImage.FilePath);
                    container.Children.Add(image);

                    grdImages.Children.Add(container);
                    column++;
                });
            }
        });
    }

    /// <summary>
    /// Loads an image and returns a scaled down version of it, as Bitmap
    /// </summary>
    /// <param name="image">The image to load</param>
    /// <param name="targetWidth">The width of the scaled down bitmap</param>
    /// <param name="targetHeight">The height of the scaled down bitmap</param>
    /// <returns>A scaled down bitmap of the original image</returns>
    public static async Task<Bitmap> LoadResizedImageAsync(SixLabors.ImageSharp.Image image, int targetWidth, int targetHeight)
    {
        // Calculate scale ratio to maintain aspect ratio
        var scale = Math.Min(targetWidth / (float)image.Width, targetHeight / (float)image.Height);

        image.Mutate(x => x.Resize((int)(image.Width * scale), (int)(image.Height * scale)));
        var memoryStream = new MemoryStream();
        await image.SaveAsBmpAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(memoryStream);
    }

    #region add new tags
    /// <summary>
    /// Adds a new type tag
    /// </summary>
    private async Task AddNewTypeAsync()
    {
        if (!await ValidateAddNewTagAsync(txtTypeName, grdTypes))
            return;
        grdTypes.Children.Add(CreateNewTag(txtTypeName.Text!, TagCategory.Type));
        Configuration!.DefaultCategories!.TypeCategory = FindControls<TagControl>(grdTypes).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtTypeName.Text = string.Empty;
        txtTypeName.Focus();
    }

    /// <summary>
    /// Adds a new subject tag
    /// </summary>
    private async Task AddNewSubjectAsync()
    {
        if (!await ValidateAddNewTagAsync(txtSubjectName, grdSubjects))
            return;
        grdSubjects.Children.Add(CreateNewTag(txtSubjectName.Text!, TagCategory.Subject));
        Configuration!.DefaultCategories!.SubjectCategory = FindControls<TagControl>(grdSubjects).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtSubjectName.Text = string.Empty;
        txtSubjectName.Focus();
    }

    /// <summary>
    /// Adds a new shot tag
    /// </summary>
    private async Task AddNewShotAsync()
    {
        if (!await ValidateAddNewTagAsync(txtShotName, grdShots))
            return;
        grdShots.Children.Add(CreateNewTag(txtShotName.Text!, TagCategory.Shot));
        Configuration!.DefaultCategories!.ShotCategory = FindControls<TagControl>(grdShots).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtShotName.Text = string.Empty;
        txtShotName.Focus();
    }

    /// <summary>
    /// Adds a new perspective tag
    /// </summary>
    private async Task AddNewPerspectiveAsync()
    {
        if (!await ValidateAddNewTagAsync(txtPerspectiveName, grdPerspectives))
            return;
        grdPerspectives.Children.Add(CreateNewTag(txtPerspectiveName.Text!, TagCategory.Perspective));
        Configuration!.DefaultCategories!.PerspectiveCategory = FindControls<TagControl>(grdPerspectives).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtPerspectiveName.Text = string.Empty;
        txtPerspectiveName.Focus();
    }

    /// <summary>
    /// Adds a new pose tag
    /// </summary>
    private async Task AddNewPoseAsync()
    {
        if (!await ValidateAddNewTagAsync(txtPoseName, grdPoses))
            return;
        grdPoses.Children.Add(CreateNewTag(txtPoseName.Text!, TagCategory.Pose));
        Configuration!.DefaultCategories!.PoseCategory = FindControls<TagControl>(grdPoses).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtPoseName.Text = string.Empty;
        txtPoseName.Focus();
    }

    /// <summary>
    /// Adds a new location tag
    /// </summary>
    private async Task AddNewLocationAsync()
    {
        if (!await ValidateAddNewTagAsync(txtLocationName, grdLocations))
            return;
        grdLocations.Children.Add(CreateNewTag(txtLocationName.Text!, TagCategory.Location));
        Configuration!.DefaultCategories!.LocationCategory = FindControls<TagControl>(grdLocations).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtLocationName.Text = string.Empty;
        txtLocationName.Focus();
    }

    /// <summary>
    /// Adds a new action tag
    /// </summary>
    private async Task AddNewActionAsync()
    {
        if (!await ValidateAddNewTagAsync(txtActionName, grdActions))
            return;
        grdActions.Children.Add(CreateNewTag(txtActionName.Text!, TagCategory.Action));
        Configuration!.DefaultCategories!.ActionCategory = FindControls<TagControl>(grdActions).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtActionName.Text = string.Empty;
        txtActionName.Focus();
    }

    /// <summary>
    /// Adds a new gaze tag
    /// </summary>
    private async Task AddNewGazeAsync()
    {
        if (!await ValidateAddNewTagAsync(txtGazeName, grdGazes))
            return;
        grdGazes.Children.Add(CreateNewTag(txtGazeName.Text!, TagCategory.Gaze));
        Configuration!.DefaultCategories!.GazeCategory = FindControls<TagControl>(grdGazes).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtGazeName.Text = string.Empty;
        txtGazeName.Focus();
    }

    /// <summary>
    /// Adds a new mouth tag
    /// </summary>
    private async Task AddNewMouthAsync()
    {
        if (!await ValidateAddNewTagAsync(txtMouthName, grdMouths))
            return;
        grdMouths.Children.Add(CreateNewTag(txtMouthName.Text!, TagCategory.Mouth));
        Configuration!.DefaultCategories!.MouthCategory = FindControls<TagControl>(grdMouths).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtMouthName.Text = string.Empty;
        txtMouthName.Focus();
    }

    /// <summary>
    /// Adds a new mouth action tag
    /// </summary>
    private async Task AddNewMouthActionAsync()
    {
        if (!await ValidateAddNewTagAsync(txtMouthActionName, grdMouthActions))
            return;
        grdMouthActions.Children.Add(CreateNewTag(txtMouthActionName.Text!, TagCategory.MouthAction));
        Configuration!.DefaultCategories!.MouthActionCategory = FindControls<TagControl>(grdMouthActions).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtMouthActionName.Text = string.Empty;
        txtMouthActionName.Focus();
    }

    /// <summary>
    /// Adds a new hair tag
    /// </summary>
    private async Task AddNewHairAsync()
    {
        if (!await ValidateAddNewTagAsync(txtHairName, grdHairs))
            return;
        grdHairs.Children.Add(CreateNewTag(txtHairName.Text!, TagCategory.Hair));
        Configuration!.DefaultCategories!.HairCategory = FindControls<TagControl>(grdHairs).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtHairName.Text = string.Empty;
        txtHairName.Focus();
    }

    /// <summary>
    /// Adds a new limb tag
    /// </summary>
    private async Task AddNewLimbAsync()
    {
        if (!await ValidateAddNewTagAsync(txtLimbName, grdLimbs))
            return;
        grdLimbs.Children.Add(CreateNewTag(txtLimbName.Text!, TagCategory.Limbs));
        Configuration!.DefaultCategories!.LimbsCategory = FindControls<TagControl>(grdLimbs).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtLimbName.Text = string.Empty;
        txtLimbName.Focus();
    }

    /// <summary>
    /// Adds a new subject description tag
    /// </summary>
    private async Task AddNewSubjectDescriptionAsync()
    {
        if (!await ValidateAddNewTagAsync(txtSubjectDescriptionName, grdSubjectDescriptions))
            return;
        grdSubjectDescriptions.Children.Add(CreateNewTag(txtSubjectDescriptionName.Text!, TagCategory.SubjectDescription));
        Configuration!.DefaultCategories!.SubjectDescriptionCategory = FindControls<TagControl>(grdSubjectDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtSubjectDescriptionName.Text = string.Empty;
        txtSubjectDescriptionName.Focus();
    }

    /// <summary>
    /// Adds a new scenery tag
    /// </summary>
    private async Task AddNewSceneryAsync()
    {
        if (!await ValidateAddNewTagAsync(txtSceneryName, grdSceneries))
            return;
        grdSceneries.Children.Add(CreateNewTag(txtSceneryName.Text!, TagCategory.Scenery));
        Configuration!.DefaultCategories!.SceneryCategory = FindControls<TagControl>(grdSceneries).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtSceneryName.Text = string.Empty;
        txtSceneryName.Focus();
    }

    /// <summary>
    /// Adds a new scene description tag
    /// </summary>
    private async Task AddNewSceneDescriptionAsync()
    {
        if (!await ValidateAddNewTagAsync(txtSceneDescriptionName, grdSceneDescriptions))
            return;
        grdSceneDescriptions.Children.Add(CreateNewTag(txtSceneDescriptionName.Text!, TagCategory.SceneDescription));
        Configuration!.DefaultCategories!.SceneDescriptionCategory = FindControls<TagControl>(grdSceneDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtSceneDescriptionName.Text = string.Empty;
        txtSceneDescriptionName.Focus();
    }

    /// <summary>
    /// Adds a new lighting tag
    /// </summary>
    private async Task AddNewLightingAsync()
    {
        if (!await ValidateAddNewTagAsync(txtLightingName, grdLightings))
            return;
        grdLightings.Children.Add(CreateNewTag(txtLightingName.Text!, TagCategory.Lighting));
        Configuration!.DefaultCategories!.LightingCategory = FindControls<TagControl>(grdLightings).Select(tagControl => tagControl.Text).ToArray()!;
        Configuration.UpdateConfiguration();
        txtLightingName.Text = string.Empty;
        txtLightingName.Focus();
    }

    /// <summary>
    /// Adds a new miscellaneous tag
    /// </summary>
    private async Task AddMiscellaneousAsync()
    {
        if (!await ValidateAddNewTagAsync(txtMiscellaneousName, grdSelectedTags))
            return;
        grdSelectedTags.Children.Add(CreateNewTag(txtMiscellaneousName.Text!, TagCategory.Miscellaneous));
        RenderOutputCaption();
        txtMiscellaneousName.Text = string.Empty;
        txtMiscellaneousName.Focus();
    }
    #endregion

    /// <summary>
    /// Creates a new tag
    /// </summary>
    /// <param name="text">The text of the tag</param>
    /// <param name="category">The category of the tag</param>
    /// <returns>A new tag</returns>
    private TagControl CreateNewTag(string text, TagCategory category)
    {
        TagControl tagControl = new();
        tagControl.OnClick += Tag_Click;
        tagControl.OnCloseRequest += Tag_Close;
        tagControl.OnUpdateText += Tag_OnUpdateText;
        tagControl.Text = text;
        tagControl.Category = category;
        tagControl.VerticalAlignment = VerticalAlignment.Top;
        tagControl.HorizontalAlignment = HorizontalAlignment.Left;
        tagControl.Margin = new Thickness(3);
        tagControl.Height = 21;
        return tagControl;
    }

    /// <summary>
    /// Reads the tags from the config file and adds them in their corresponding categories
    /// </summary>
    private void PopulateDefaultCategories()
    {
        if (Configuration!.DefaultCategories?.TypeCategory?.Any() == true)
            foreach (string typeTag in Configuration.DefaultCategories.TypeCategory)
                if (!string.IsNullOrWhiteSpace(typeTag))
                    grdTypes.Children.Add(CreateNewTag(typeTag, TagCategory.Type));
        if (Configuration!.DefaultCategories?.SubjectCategory?.Any() == true)
            foreach (string subjectTag in Configuration.DefaultCategories.SubjectCategory)
                if (!string.IsNullOrWhiteSpace(subjectTag))
                    grdSubjects.Children.Add(CreateNewTag(subjectTag, TagCategory.Subject));
        if (Configuration!.DefaultCategories?.ShotCategory?.Any() == true)
            foreach (string shotTag in Configuration.DefaultCategories.ShotCategory)
                if (!string.IsNullOrWhiteSpace(shotTag))
                    grdShots.Children.Add(CreateNewTag(shotTag, TagCategory.Shot));
        if (Configuration!.DefaultCategories?.PerspectiveCategory?.Any() == true)
            foreach (string perspectiveTag in Configuration.DefaultCategories.PerspectiveCategory)
                if (!string.IsNullOrWhiteSpace(perspectiveTag))
                    grdPerspectives.Children.Add(CreateNewTag(perspectiveTag, TagCategory.Perspective));
        if (Configuration!.DefaultCategories?.PoseCategory?.Any() == true)
            foreach (string poseTag in Configuration.DefaultCategories.PoseCategory)
                if (!string.IsNullOrWhiteSpace(poseTag))
                    grdPoses.Children.Add(CreateNewTag(poseTag, TagCategory.Pose));
        if (Configuration!.DefaultCategories?.LocationCategory?.Any() == true)
            foreach (string locationTag in Configuration.DefaultCategories.LocationCategory)
                if (!string.IsNullOrWhiteSpace(locationTag))
                    grdLocations.Children.Add(CreateNewTag(locationTag, TagCategory.Location));
        if (Configuration!.DefaultCategories?.ActionCategory?.Any() == true)
            foreach (string actionTag in Configuration.DefaultCategories.ActionCategory)
                if (!string.IsNullOrWhiteSpace(actionTag))
                    grdActions.Children.Add(CreateNewTag(actionTag, TagCategory.Action));
        if (Configuration!.DefaultCategories?.GazeCategory?.Any() == true)
            foreach (string gazeTag in Configuration.DefaultCategories.GazeCategory)
                if (!string.IsNullOrWhiteSpace(gazeTag))
                    grdGazes.Children.Add(CreateNewTag(gazeTag, TagCategory.Gaze));
        if (Configuration!.DefaultCategories?.MouthCategory?.Any() == true)
            foreach (string mouthTag in Configuration.DefaultCategories.MouthCategory)
                if (!string.IsNullOrWhiteSpace(mouthTag))
                    grdMouths.Children.Add(CreateNewTag(mouthTag, TagCategory.Mouth));
        if (Configuration!.DefaultCategories?.MouthActionCategory?.Any() == true)
            foreach (string mouthActionTag in Configuration.DefaultCategories.MouthActionCategory)
                if (!string.IsNullOrWhiteSpace(mouthActionTag))
                    grdMouthActions.Children.Add(CreateNewTag(mouthActionTag, TagCategory.MouthAction));
        if (Configuration!.DefaultCategories?.HairCategory?.Any() == true)
            foreach (string hairTag in Configuration.DefaultCategories.HairCategory)
                if (!string.IsNullOrWhiteSpace(hairTag))
                    grdHairs.Children.Add(CreateNewTag(hairTag, TagCategory.Hair));
        if (Configuration!.DefaultCategories?.LimbsCategory?.Any() == true)
            foreach (string limbTag in Configuration.DefaultCategories.LimbsCategory)
                if (!string.IsNullOrWhiteSpace(limbTag))
                    grdLimbs.Children.Add(CreateNewTag(limbTag, TagCategory.Limbs));
        if (Configuration!.DefaultCategories?.SubjectDescriptionCategory?.Any() == true)
            foreach (string subjectDescriptionTag in Configuration.DefaultCategories.SubjectDescriptionCategory)
                if (!string.IsNullOrWhiteSpace(subjectDescriptionTag))
                    grdSubjectDescriptions.Children.Add(CreateNewTag(subjectDescriptionTag, TagCategory.SubjectDescription));
        if (Configuration!.DefaultCategories?.SceneryCategory?.Any() == true)
            foreach (string sceneryTag in Configuration.DefaultCategories.SceneryCategory)
                if (!string.IsNullOrWhiteSpace(sceneryTag))
                    grdSceneries.Children.Add(CreateNewTag(sceneryTag, TagCategory.Scenery));
        if (Configuration!.DefaultCategories?.SceneDescriptionCategory?.Any() == true)
            foreach (string sceneDescriptionTag in Configuration.DefaultCategories.SceneDescriptionCategory)
                if (!string.IsNullOrWhiteSpace(sceneDescriptionTag))
                    grdSceneDescriptions.Children.Add(CreateNewTag(sceneDescriptionTag, TagCategory.SceneDescription));
        if (Configuration!.DefaultCategories?.LightingCategory?.Any() == true)
            foreach (string lightingTag in Configuration.DefaultCategories.LightingCategory)
                if (!string.IsNullOrWhiteSpace(lightingTag))
                    grdLightings.Children.Add(CreateNewTag(lightingTag, TagCategory.Lighting));
    }

    /// <summary>
    /// Renders the final caption for the currently selected trigger text and tags
    /// </summary>
    private void RenderOutputCaption()
    {
        txtOutput.Text = string.Empty;
        txtOutput.Text += !string.IsNullOrWhiteSpace(txtTrigger.Text) ? txtTrigger.Text + ", " : string.Empty;
        Dictionary<TagCategory, List<string>> groupedTexts = new();
        // group the texts by category
        foreach (TagControl tagControl in FindControls<TagControl>(grdSelectedTags))
        {
            if (!groupedTexts.ContainsKey(tagControl.Category))
                groupedTexts[tagControl.Category] = new List<string>();
            groupedTexts[tagControl.Category].Add(tagControl.Text!);
        }
        if (groupedTexts.ContainsKey(TagCategory.Type))
            foreach (string tag in groupedTexts[TagCategory.Type])
                txtOutput.Text += tag + " of a ";
        if (groupedTexts.ContainsKey(TagCategory.Subject))
            foreach (string tag in groupedTexts[TagCategory.Subject])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Shot))
            foreach (string tag in groupedTexts[TagCategory.Shot])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Perspective))
            foreach (string tag in groupedTexts[TagCategory.Perspective])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Pose))
            foreach (string tag in groupedTexts[TagCategory.Pose])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Location))
            foreach (string tag in groupedTexts[TagCategory.Location])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Action))
            foreach (string tag in groupedTexts[TagCategory.Action])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Gaze))
            foreach (string tag in groupedTexts[TagCategory.Gaze])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Mouth))
            foreach (string tag in groupedTexts[TagCategory.Mouth])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.MouthAction))
            foreach (string tag in groupedTexts[TagCategory.MouthAction])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Hair))
            foreach (string tag in groupedTexts[TagCategory.Hair])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Limbs))
            foreach (string tag in groupedTexts[TagCategory.Limbs])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.SubjectDescription))
            foreach (string tag in groupedTexts[TagCategory.SubjectDescription])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Scenery))
            foreach (string tag in groupedTexts[TagCategory.Scenery])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.SceneDescription))
            foreach (string tag in groupedTexts[TagCategory.SceneDescription])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Lighting))
            foreach (string tag in groupedTexts[TagCategory.Lighting])
                txtOutput.Text += tag + ", ";
        if (groupedTexts.ContainsKey(TagCategory.Miscellaneous))
            foreach (string tag in groupedTexts[TagCategory.Miscellaneous])
                txtOutput.Text += tag + ", ";
        if (txtOutput.Text.EndsWith(", "))
            txtOutput.Text = txtOutput.Text[..^2];
    }

    /// <summary>
    /// Saves the currently selected tags to the caption text file of the selected image
    /// </summary>
    private async Task SaveCaptionAsync()
    {
        if (string.IsNullOrWhiteSpace(txtSelectedImage.Text))
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "There is no image selected!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
        else if (FindControls<TagControl>(grdSelectedTags).Count == 0)
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "There are no tags specified!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
        else
        {
            File.WriteAllText(Path.ChangeExtension(txtSelectedImage.Tag!.ToString()!, ".txt"), txtOutput.Text);
            string allCaptionsFilePath = Path.Combine(Path.GetDirectoryName(txtSelectedImage.Tag.ToString())!, "captions.json");
            DirectoryImagesDto? captions = null;
            if (File.Exists(allCaptionsFilePath))
                captions = JsonConvert.DeserializeObject<DirectoryImagesDto>(File.ReadAllText(allCaptionsFilePath));
            if (captions is not null)
            {
                if (captions.Images is not null)
                {
                    ImageInfoDto? selectedImageInfo = captions.Images.FirstOrDefault(image => image.ImageName == txtSelectedImage.Text);
                    if (selectedImageInfo != null)
                    {
                        selectedImageInfo.TriggerWord = txtTrigger.Text;
                        selectedImageInfo.Categories = new List<CategoryDto>(); // reset categories for the selected image                       
                        var groupedTags = FindControls<TagControl>(grdSelectedTags) // group TagControls by category
                                          .GroupBy(tag => tag.Category)
                                          .ToList();
                        foreach (var group in groupedTags)
                        {
                            var category = new CategoryDto
                            {
                                CategoryName = group.Key.ToString(),
                                Tags = group.Select(tag => tag.Text).ToList()!
                            };
                            selectedImageInfo.Categories.Add(category); // add this category to the selected image's categories
                        }
                    }
                    else
                    {
                        selectedImageInfo = new ImageInfoDto // create new ImageInfoDto if it doesn't exist for the selected image
                        {
                            ImageName = txtSelectedImage.Text,
                            TriggerWord = txtTrigger.Text,
                            Categories = FindControls<TagControl>(grdSelectedTags)
                                         .GroupBy(tag => tag.Category)
                                         .Select(group => new CategoryDto
                                         {
                                             CategoryName = group.Key.ToString(),
                                             Tags = group.Select(tag => tag.Text).ToList()!
                                         })
                                         .ToList()
                        };
                        captions.Images.Add(selectedImageInfo); // add the new ImageInfoDto to the captions
                    }
                }
            }
            else
            {
                captions = new DirectoryImagesDto();
                captions.Images = new();
                ImageInfoDto? selectedImageInfo = new()// create new ImageInfoDto if it doesn't exist for the selected image
                {
                    ImageName = txtSelectedImage.Text,
                    TriggerWord = txtTrigger.Text,
                    Categories = FindControls<TagControl>(grdSelectedTags)
                                         .GroupBy(tag => tag.Category)
                                         .Select(group => new CategoryDto
                                         {
                                             CategoryName = group.Key.ToString(),
                                             Tags = group.Select(tag => tag.Text).ToList()!
                                         })
                                         .ToList()
                };
                captions.Images.Add(selectedImageInfo); // add the new ImageInfoDto to the captions
            }
            File.WriteAllText(allCaptionsFilePath, JsonConvert.SerializeObject(captions, Formatting.Indented));
            await MessageBoxManager.GetMessageBoxStandardWindow("Success!", "Caption saved!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Success).ShowDialog(this);
        }
    }

    /// <summary>
    /// Validates the required information for setting the input path
    /// </summary>
    /// <returns><see langword="true"/> if the required information is met, <see langword="false"/> otherwise</returns>
    private async Task<bool> ValidateInputPathAsync()
    {
        if (string.IsNullOrWhiteSpace(txtInputPath.Text))
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Input path cannot be empty!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return false;
        }
        else if (!Path.Exists(txtInputPath.Text))
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Input path does not exist!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Validates the required information for adding a new tag
    /// </summary>
    /// <param name="tagTextBox">The input field containing the tag text</param>
    /// <param name="container">The container that will contain the tag</param>
    /// <returns><see langword="true"/> if the required information is met, <see langword="false"/> otherwise</returns>
    private async Task<bool> ValidateAddNewTagAsync(TextBox tagTextBox, IPanel container)
    {
        if (string.IsNullOrWhiteSpace(tagTextBox.Text))
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Tag text cannot be empty!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            tagTextBox.Focus();
            return false;
        }
        else
        {
            if (container.Children.OfType<TagControl>().Any(tagControl => tagControl.Text!.ToLower() == tagTextBox.Text.ToLower()))
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "A tag with the specified text already exists!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
                tagTextBox.Focus();
                return false;
            }
            else
                return true;
        }
    }

    /// <summary>
    /// Copies selected tags into system memory
    /// </summary>
    public async Task CopySelectedTagsAsync()
    {
        if (string.IsNullOrWhiteSpace(txtSelectedImage.Text))
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "There is no image selected!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
        else if (FindControls<TagControl>(grdSelectedTags).Count == 0)
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "There are no tags specified!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
        else
        {
            ImageInfoDto? imageInfo = new() // create new ImageInfoDto if it doesn't exist for the selected image
            {
                ImageName = txtSelectedImage.Text,
                TriggerWord = txtTrigger.Text,
                Categories = FindControls<TagControl>(grdSelectedTags)
                                        .GroupBy(tag => tag.Category)
                                        .Select(group => new CategoryDto
                                        {
                                            CategoryName = group.Key.ToString(),
                                            Tags = group.Select(tag => tag.Text).ToList()!
                                        })
                                        .ToList()
            };
            // serialize the image info and store it in the cache memory
            await Application.Current?.Clipboard?.SetTextAsync(JsonConvert.SerializeObject(imageInfo, Formatting.Indented));  
        }
    }

    /// <summary>
    /// Pastes selected tags into system memory
    /// </summary>
    public async Task PasteSelectedTagsAsync()
    {
      if (string.IsNullOrWhiteSpace(txtSelectedImage.Text))
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "There is no image selected!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
        else
        {
            // retrieve text from clipboard
            string? clipboardText = await Application.Current?.Clipboard?.GetTextAsync();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                try
                {
                    // try to deserialize the clipboard text into valid tag collections
                    ImageInfoDto? imageInfo = JsonConvert.DeserializeObject<ImageInfoDto>(clipboardText);
                    if (imageInfo is not null && imageInfo.Categories is not null)
                        foreach (var category in imageInfo.Categories) // iterate the categories of the selected image section
                            foreach (string tag in category.Tags!) // iterate the tags of each category
                                grdSelectedTags.Children.Add(CreateNewTag(tag, (TagCategory)Enum.Parse(typeof(TagCategory), category.CategoryName!))); // add the tag
                    txtTrigger.Text = imageInfo?.TriggerWord;
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Clears selected tags into system memory
    /// </summary>
    public void ClearSelectedTags()
    {
        txtTrigger.Text = string.Empty;
        txtOutput.Text = string.Empty;
        grdSelectedTags.Children.Clear();
    }

    /// <summary>
    /// Sets the width of the wrap panels containing tags
    /// </summary>
    private void SetWrapPanelsMaxWidth()
    {
        if (Configuration is not null)
        {
            var panelsWidth = Configuration!.Application!.TagCategoriesPanelWidth - 27;
            // categories column
            grdTypes.Width = panelsWidth;
            grdSubjects.Width = panelsWidth;
            grdShots.Width = panelsWidth;
            grdPerspectives.Width = panelsWidth;
            grdPoses.Width = panelsWidth;
            grdLocations.Width = panelsWidth;
            grdActions.Width = panelsWidth;
            grdGazes.Width = panelsWidth;
            grdMouths.Width = panelsWidth;
            grdMouthActions.Width = panelsWidth;
            grdHairs.Width = panelsWidth;
            grdLimbs.Width = panelsWidth;
            grdSubjectDescriptions.Width = panelsWidth;
            grdSceneries.Width = panelsWidth;
            grdSceneDescriptions.Width = panelsWidth;
            grdLightings.Width = panelsWidth;
            // output column
            var desiredSize = grdTags.Bounds.Width - Configuration!.Application!.TagCategoriesPanelWidth;
            if (desiredSize > 0)
            {
                panelsWidth = desiredSize - 27;
                grdTags.ColumnDefinitions[2].Width = new GridLength(desiredSize);
                grdSelectedTags.Width = panelsWidth;
                txtOutput.Width = panelsWidth;
                txtWarning.Width = panelsWidth;
            }
        }
    }

    /// <summary>
    /// Recursively finds all controls of a specified type within a given parent control.
    /// </summary>
    /// <typeparam name="T">The type of controls to find. This type must implement IControl.</typeparam>
    /// <param name="parent">The parent control within which to find the controls.</param>
    /// <returns>A list of all found controls of the specified type.</returns>
    public static List<T> FindControls<T>(IControl parent) where T : IControl
    {
        var foundControls = new List<T>();
        if (parent is T typedControl)
            foundControls.Add(typedControl);
        foreach (var child in parent.GetVisualChildren())
            if (child is IControl childControl)
                foundControls.AddRange(FindControls<T>(childControl));
        return foundControls;
    }

    /// <summary>
    /// Notifies subscribers about a property's value being changed
    /// </summary>
    /// <param name="propName">The property that had the value changed</param>
    public virtual void Notify([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
    #endregion

    #region ================================================================== EVENT HANDLERS ===============================================================================
    /// <summary>
    /// Handles TapTimer's Tick event
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (tickCount < 5) // trick to force the damn avalonia correct layout draw
        {
            Width += tickCount % 2 == 0 ? 1 : -1;
            tickCount++;
        }
        else
            timer.Stop();
    }

    /// <summary>
    /// Event handler for the tags copy menu item
    /// </summary>
    public async void CopySelectedTags_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedTagsAsync();
    }

    /// <summary>
    /// Event handler for the tags copy menu item
    /// </summary>
    public async void PasteSelectedTags_Click(object sender, RoutedEventArgs e)
    {
        await PasteSelectedTagsAsync();
    }

    /// <summary>
    /// Event handler for the tags copy menu item
    /// </summary>
    public void ClearSelectedTags_Click(object sender, RoutedEventArgs e)
    {
        ClearSelectedTags();
    }

    /// <summary>
    /// Handles thumbnails images PointerPressed event
    /// </summary>
    private void Thumbnail_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Avalonia.Controls.Image thumbnail) // each thumbnail stores its original image path in its Tag property, use that to display the full width view
        {
            imgPreview.Source = new Bitmap(thumbnail.Tag!.ToString()!);
            txtSelectedImage.Text = Path.GetFileNameWithoutExtension(thumbnail.Tag.ToString());
            txtSelectedImage.Tag = thumbnail.Tag;
            grdSelectedTags.Children.Clear();
            scrTags.ScrollToHome();
            // if there are saved tags for the selected image, load them
            string allCaptionsFilePath = Path.Combine(Path.GetDirectoryName(thumbnail.Tag.ToString())!, "captions.json");
            if (File.Exists(allCaptionsFilePath))
            {
                // deserialize the file for all captions
                DirectoryImagesDto? captions = JsonConvert.DeserializeObject<DirectoryImagesDto>(File.ReadAllText(allCaptionsFilePath));
                if (captions is not null && captions.Images is not null)
                {
                    // see if there is a section for the selected image
                    ImageInfoDto? selectedImageInfo = captions.Images.FirstOrDefault(image => image.ImageName == txtSelectedImage.Text);
                    if (selectedImageInfo is not null && selectedImageInfo.Categories is not null)
                        foreach (var category in selectedImageInfo.Categories) // iterate the categories of the selected image section
                            foreach (string tag in category.Tags!) // iterate the tags of each category
                                grdSelectedTags.Children.Add(CreateNewTag(tag, (TagCategory)Enum.Parse(typeof(TagCategory), category.CategoryName!))); // add the tag
                    txtTrigger.Text ??= selectedImageInfo?.TriggerWord;
                }
            }
            RenderOutputCaption();
        }
    }

    /// <summary>
    /// Event handler for tag controls Click event
    /// </summary>
    /// <param name="sender">The tag control that initiated the Click event</param>
    /// <param name="text">The text of the clicked tag control</param>
    /// <param name="category">The category of the clicked tag control</param>
    private async void Tag_Click(TagControl sender, string text, TagCategory category)
    {
        if (sender?.Parent?.Name == nameof(grdSelectedTags))
            return; // ignore clicks on already selected tags
        Dictionary<TagCategory, List<TagControl>> groupedTags = new();
        // group the texts by category
        foreach (TagControl tagControl in FindControls<TagControl>(grdSelectedTags))
        {
            // don't allow same tag of same category twice
            if (tagControl.Category == category && tagControl.Text!.ToLower() == text.ToLower())
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Selected tag has already been added!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
                return;
            }
            if (!groupedTags.ContainsKey(tagControl.Category))
                groupedTags[tagControl.Category] = new List<TagControl>();
            groupedTags[tagControl.Category].Add(CreateNewTag(tagControl.Text!, tagControl.Category));
        }
        // dont allow multiple elements of some categories
        if (groupedTags.ContainsKey(TagCategory.Type) && groupedTags[TagCategory.Type].Count == 1 && category == TagCategory.Type)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Type tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Subject) && groupedTags[TagCategory.Subject].Count == 1 && category == TagCategory.Subject)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Subject tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Shot) && groupedTags[TagCategory.Shot].Count == 1 && category == TagCategory.Shot)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Shot tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Perspective) && groupedTags[TagCategory.Perspective].Count == 1 && category == TagCategory.Perspective)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Perspective tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Gaze) && groupedTags[TagCategory.Gaze].Count == 1 && category == TagCategory.Gaze)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Gaze tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Mouth) && groupedTags[TagCategory.Mouth].Count == 1 && category == TagCategory.Mouth)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Mouth tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.MouthAction) && groupedTags[TagCategory.MouthAction].Count == 1 && category == TagCategory.MouthAction)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Mouth Action tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Hair) && groupedTags[TagCategory.Hair].Count == 1 && category == TagCategory.Hair)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Hair tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        if (groupedTags.ContainsKey(TagCategory.Scenery) && groupedTags[TagCategory.Scenery].Count == 1 && category == TagCategory.Scenery)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "Can only add one Scenery tag!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            return;
        }
        grdSelectedTags.Children.Clear();
        if (groupedTags.ContainsKey(TagCategory.Type))
            foreach (TagControl tagControl in groupedTags[TagCategory.Type])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Type)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Subject))
            foreach (TagControl tagControl in groupedTags[TagCategory.Subject])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Subject)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Shot))
            foreach (TagControl tagControl in groupedTags[TagCategory.Shot])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Shot)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Perspective))
            foreach (TagControl tagControl in groupedTags[TagCategory.Perspective])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Perspective)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Pose))
            foreach (TagControl tagControl in groupedTags[TagCategory.Pose])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Pose)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Location))
            foreach (TagControl tagControl in groupedTags[TagCategory.Location])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Location)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Action))
            foreach (TagControl tagControl in groupedTags[TagCategory.Action])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Action)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Gaze))
            foreach (TagControl tagControl in groupedTags[TagCategory.Gaze])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Gaze)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Mouth))
            foreach (TagControl tagControl in groupedTags[TagCategory.Mouth])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Mouth)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.MouthAction))
            foreach (TagControl tagControl in groupedTags[TagCategory.MouthAction])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.MouthAction)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Hair))
            foreach (TagControl tagControl in groupedTags[TagCategory.Hair])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Hair)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Limbs))
            foreach (TagControl tagControl in groupedTags[TagCategory.Limbs])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Limbs)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.SubjectDescription))
            foreach (TagControl tagControl in groupedTags[TagCategory.SubjectDescription])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.SubjectDescription)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Scenery))
            foreach (TagControl tagControl in groupedTags[TagCategory.Scenery])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Scenery)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.SceneDescription))
            foreach (TagControl tagControl in groupedTags[TagCategory.SceneDescription])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.SceneDescription)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Lighting))
            foreach (TagControl tagControl in groupedTags[TagCategory.Lighting])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Lighting)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        if (groupedTags.ContainsKey(TagCategory.Miscellaneous))
            foreach (TagControl tagControl in groupedTags[TagCategory.Miscellaneous])
                grdSelectedTags.Children.Add(tagControl);
        if (category == TagCategory.Miscellaneous)
            grdSelectedTags.Children.Add(CreateNewTag(text, category));
        RenderOutputCaption();
    }

    /// <summary>
    /// Removes <paramref name="control"/> from the list of tags
    /// </summary>
    /// <param name="control">The tag to remove</param>
    private void Tag_Close(TagControl control)
    {
        if (control.Parent is WrapPanel panel)
        {
            panel.Children.Remove(control);
            if (panel.Name != nameof(grdSelectedTags))
            {
                if (control.Category == TagCategory.Type)
                    Configuration!.DefaultCategories!.TypeCategory = FindControls<TagControl>(grdTypes).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Subject)
                    Configuration!.DefaultCategories!.SubjectCategory = FindControls<TagControl>(grdSubjects).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Shot)
                    Configuration!.DefaultCategories!.ShotCategory = FindControls<TagControl>(grdShots).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Perspective)
                    Configuration!.DefaultCategories!.PerspectiveCategory = FindControls<TagControl>(grdPerspectives).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Pose)
                    Configuration!.DefaultCategories!.PoseCategory = FindControls<TagControl>(grdPoses).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Location)
                    Configuration!.DefaultCategories!.LocationCategory = FindControls<TagControl>(grdLocations).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Action)
                    Configuration!.DefaultCategories!.ActionCategory = FindControls<TagControl>(grdActions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Gaze)
                    Configuration!.DefaultCategories!.GazeCategory = FindControls<TagControl>(grdGazes).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Mouth)
                    Configuration!.DefaultCategories!.MouthCategory = FindControls<TagControl>(grdMouths).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.MouthAction)
                    Configuration!.DefaultCategories!.MouthActionCategory = FindControls<TagControl>(grdMouthActions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Hair)
                    Configuration!.DefaultCategories!.HairCategory = FindControls<TagControl>(grdHairs).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Limbs)
                    Configuration!.DefaultCategories!.LimbsCategory = FindControls<TagControl>(grdLimbs).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.SubjectDescription)
                    Configuration!.DefaultCategories!.SubjectDescriptionCategory = FindControls<TagControl>(grdSubjectDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Scenery)
                    Configuration!.DefaultCategories!.SceneryCategory = FindControls<TagControl>(grdSceneries).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.SceneDescription)
                    Configuration!.DefaultCategories!.SceneDescriptionCategory = FindControls<TagControl>(grdSceneDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Lighting)
                    Configuration!.DefaultCategories!.LightingCategory = FindControls<TagControl>(grdLightings).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Lighting)
                    Configuration!.DefaultCategories!.LightingCategory = FindControls<TagControl>(grdLightings).Select(tagControl => tagControl.Text).ToArray()!;
                Configuration!.UpdateConfiguration();
            }
        }
        RenderOutputCaption();
    }

    /// <summary>
    /// Handles <paramref name="control"/> text update event
    /// </summary>
    /// <param name="control">The control whose text changed</param>
    private void Tag_OnUpdateText(TagControl control)
    {
        if (control.Parent is WrapPanel panel)
        {
            if (panel.Name != nameof(grdSelectedTags))
            {
                if (control.Category == TagCategory.Type)
                    Configuration!.DefaultCategories!.TypeCategory = FindControls<TagControl>(grdTypes).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Subject)
                    Configuration!.DefaultCategories!.SubjectCategory = FindControls<TagControl>(grdSubjects).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Shot)
                    Configuration!.DefaultCategories!.ShotCategory = FindControls<TagControl>(grdShots).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Perspective)
                    Configuration!.DefaultCategories!.PerspectiveCategory = FindControls<TagControl>(grdPerspectives).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Pose)
                    Configuration!.DefaultCategories!.PoseCategory = FindControls<TagControl>(grdPoses).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Location)
                    Configuration!.DefaultCategories!.LocationCategory = FindControls<TagControl>(grdLocations).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Action)
                    Configuration!.DefaultCategories!.ActionCategory = FindControls<TagControl>(grdActions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Gaze)
                    Configuration!.DefaultCategories!.GazeCategory = FindControls<TagControl>(grdGazes).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Mouth)
                    Configuration!.DefaultCategories!.MouthCategory = FindControls<TagControl>(grdMouths).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.MouthAction)
                    Configuration!.DefaultCategories!.MouthActionCategory = FindControls<TagControl>(grdMouthActions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Hair)
                    Configuration!.DefaultCategories!.HairCategory = FindControls<TagControl>(grdHairs).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Limbs)
                    Configuration!.DefaultCategories!.LimbsCategory = FindControls<TagControl>(grdLimbs).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.SubjectDescription)
                    Configuration!.DefaultCategories!.SubjectDescriptionCategory = FindControls<TagControl>(grdSubjectDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Scenery)
                    Configuration!.DefaultCategories!.SceneryCategory = FindControls<TagControl>(grdSceneries).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.SceneDescription)
                    Configuration!.DefaultCategories!.SceneDescriptionCategory = FindControls<TagControl>(grdSceneDescriptions).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Lighting)
                    Configuration!.DefaultCategories!.LightingCategory = FindControls<TagControl>(grdLightings).Select(tagControl => tagControl.Text).ToArray()!;
                else if (control.Category == TagCategory.Lighting)
                    Configuration!.DefaultCategories!.LightingCategory = FindControls<TagControl>(grdLightings).Select(tagControl => tagControl.Text).ToArray()!;
                Configuration!.UpdateConfiguration();
            }
        }
        RenderOutputCaption();
    }

    /// <summary>
    /// Handles txtTrigger's TextChanged event
    /// </summary>
    private void Trigger_TextChanged(object? sender, TextChangedEventArgs e)
    {
        txtWarning.IsVisible = string.IsNullOrWhiteSpace(txtTrigger.Text);
        RenderOutputCaption();
    }

    /// <summary>
    /// Handles main window's KeyDown event
    /// </summary>
    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        // when user presses ctrl+c, handle it as copying of tags
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
            await CopySelectedTagsAsync();
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V)
            await PasteSelectedTagsAsync();
        else if (e.KeyModifiers == KeyModifiers.Control) // otherwise, show the controls for removing tags
            foreach (TagControl tagControl in FindControls<TagControl>(grdContainer))
                tagControl.IsCloseButtonVisible = true;
    }

    /// <summary>
    /// Handles main window's KeyUp event
    /// </summary>
    private void MainWindow_KeyUp(object? sender, KeyEventArgs e)
    {
        foreach (TagControl tagControl in FindControls<TagControl>(grdContainer))
            tagControl.IsCloseButtonVisible = false;
    }

    /// <summary>
    /// Handles Remove Tags's Checked and Unchecked events
    /// </summary>
    private void CheckBox_CheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
            foreach (TagControl tagControl in FindControls<TagControl>(grdContainer))
                tagControl.IsCloseButtonVisible = checkBox.IsChecked == true;
    }

    #region adding new tag
    /// <summary>
    /// Handles type name's KeyUp event
    /// </summary>
    private async void TypeName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewTypeAsync();
    }

    /// <summary>
    /// Handles subject name's KeyUp event
    /// </summary>
    private async void SubjectName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewSubjectAsync();
    }

    /// <summary>
    /// Handles shot name's KeyUp event
    /// </summary>
    private async void ShotName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewShotAsync();
    }

    /// <summary>
    /// Handles perspective name's KeyUp event
    /// </summary>
    private async void PerspectiveName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewPerspectiveAsync();
    }

    /// <summary>
    /// Handles pose name's KeyUp event
    /// </summary>
    private async void PoseName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewPoseAsync();
    }

    /// <summary>
    /// Handles location name's KeyUp event
    /// </summary>
    private async void LocationName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewLocationAsync();
    }

    /// <summary>
    /// Handles action name's KeyUp event
    /// </summary>
    private async void ActionName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewActionAsync();
    }

    /// <summary>
    /// Handles gaze name's KeyUp event
    /// </summary>
    private async void GazeName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewGazeAsync();
    }

    /// <summary>
    /// Handles mouth name's KeyUp event
    /// </summary>
    private async void MouthName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewMouthAsync();
    }

    /// <summary>
    /// Handles mouth action name's KeyUp event
    /// </summary>
    private async void MouthActionName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewMouthActionAsync();
    }

    /// <summary>
    /// Handles hair name's KeyUp event
    /// </summary>
    private async void HairName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewHairAsync();
    }

    /// <summary>
    /// Handles limb name's KeyUp event
    /// </summary>
    private async void LimbName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewLimbAsync();
    }

    /// <summary>
    /// Handles subject description name's KeyUp event
    /// </summary>
    private async void SubjectDescriptionName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewSubjectDescriptionAsync();
    }

    /// <summary>
    /// Handles scenery name's KeyUp event
    /// </summary>
    private async void SceneryName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewSceneryAsync();
    }

    /// <summary>
    /// Handles scene description name's KeyUp event
    /// </summary>
    private async void SceneDescriptionName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewSceneDescriptionAsync();
    }

    /// <summary>
    /// Handles lighting name's KeyUp event
    /// </summary>
    private async void LightingName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddNewLightingAsync();
    }

    /// <summary>
    /// Handles lighting name's KeyUp event
    /// </summary>
    private async void MiscellaneousName_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await AddMiscellaneousAsync();
    }
    #endregion

    /// <summary>
    /// Handles grid splitter's DragDelta event
    /// </summary>
    private void GridSplitter_DragDelta(object? sender, VectorEventArgs e)
    {
        SetWrapPanelsMaxWidth();
        Configuration!.Application!.TagCategoriesPanelWidth = double.IsNaN(grdTags.ColumnDefinitions[0].ActualWidth) ? 265 : grdTags.ColumnDefinitions[0].ActualWidth;
        Configuration!.Application!.ImagePreviewPanelWidth = double.IsNaN(grdColumns.ColumnDefinitions[0].ActualWidth) ? 150 : grdColumns.ColumnDefinitions[0].ActualWidth;
        Configuration.UpdateConfiguration();
    }

    /// <summary>
    /// Intercepts the WindowState value change
    /// </summary>
    /// <param name="state">The window state to be applied</param>
    protected override void HandleWindowStateChanged(WindowState state)
    {
        base.HandleWindowStateChanged(state);
        var newState = WindowState;
        Configuration!.Application!.IsMaximized = newState == WindowState.Maximized;
        Configuration.UpdateConfiguration();
    }

    /// <summary>
    /// Handles main window's SizeChanged event
    /// </summary>
    private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (isWindowLoaded)
        {
            SetWrapPanelsMaxWidth();
            Configuration!.Application!.WindowWidth = e.NewSize.Width;
            Configuration.Application.WindowHeight = e.NewSize.Height;
            Configuration?.UpdateConfiguration();
        }
    }

    /// <summary>
    /// Handles main window's PositionChanged event
    /// </summary>
    private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (isWindowLoaded)
        {
            var heightDifference = FrameSize != null ? FrameSize.Value.Height - ClientSize.Height : 0;
            Configuration!.Application!.WindowPositionX = e.Point.X > 0 ? e.Point.X : 0;
            Configuration.Application.WindowPositionY = e.Point.Y - (int)heightDifference > 0 ? e.Point.Y - (int)heightDifference : 0; // Avalonia bug: ignores frame size, when it shouldn't
            Configuration?.UpdateConfiguration();
        }
        LayoutUpdated += MainWindow_LayoutUpdated;
    }

    /// <summary>
    /// Handles main window's LayoutUpdated event
    /// </summary>
    private void MainWindow_LayoutUpdated(object? sender, EventArgs e)
    {
        if (isWindowLoaded)
            SetWrapPanelsMaxWidth();
    }

    /// <summary>
    /// Handles main window's Opened event
    /// </summary>
    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        string configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "appsettings.json");
        if (File.Exists(configurationFilePath))
        {
            Configuration = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configurationFilePath)) ?? throw new InvalidOperationException("Cannot deserialize appsettings.json!");
            Configuration.ConfigurationFilePath = configurationFilePath;
            WindowState = Configuration.Application!.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            Position = new PixelPoint(Configuration.Application.WindowPositionX, Configuration.Application.WindowPositionY);
            if (!Configuration.Application.IsFirstRun)
            {
                if (!Configuration.Application!.IsMaximized)
                {
                    Width = Configuration.Application!.WindowWidth;
                    Height = Configuration.Application!.WindowHeight;
                }
            }
            else
            {
                Configuration!.Application!.ImagePreviewPanelWidth = 490;
                Configuration!.Application!.TagCategoriesPanelWidth = double.IsNaN(Width) ? 265 : (Width - 500) / 2;
                Configuration.Application.IsFirstRun = false;
                Configuration.UpdateConfiguration();
            }
            grdColumns.ColumnDefinitions[0].Width = new GridLength(Configuration!.Application!.ImagePreviewPanelWidth);
            grdTags.ColumnDefinitions[0].Width = new GridLength(Configuration!.Application!.TagCategoriesPanelWidth);
            grdTags.ColumnDefinitions[2].Width = new GridLength(grdTags.Bounds.Width - grdTags.ColumnDefinitions[0].ActualWidth);
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("Error!", "The configuration file appsettings.json was not found in the application's directory!" + Environment.NewLine + configurationFilePath, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).ShowDialog(this);
            Close();
        }
        SetWrapPanelsMaxWidth();
        PopulateDefaultCategories();
        isWindowLoaded = true;
        timer.Start();
    }
    #endregion
}