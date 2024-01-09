#region ========================================================================= USING =====================================================================================
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using DatasetTag.Common.Enums;
using DatasetTag.Common.MVVM;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace DatasetTag.Common.Controls;

/// <summary>
/// Code behind for the Tag user control
/// </summary>
/// <remarks>
/// Creation Date: 07th of January, 2024
/// </remarks>
public partial class TagControl : UserControl, INotifyPropertyChanged
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public new event PropertyChangedEventHandler? PropertyChanged;
    private readonly DispatcherTimer tapTimer;
    private bool isDoubleTap = false;
    #endregion

    #region ================================================================= BINDING COMMANDS ==============================================================================
    public ISyncCommand RemoveTag_Command { get; private set; }
    #endregion

    #region ================================================================ BINDING PROPERTIES =============================================================================
    private string? text;
    public string? Text
    {
        get { return text; }
        set { text = value; Notify(); }
    }

    private TagCategory category;
    public TagCategory Category
    {
        get { return category; }
        set { category = value; Notify(); }
    }

    private bool isCloseButtonVisible;
    public bool IsCloseButtonVisible
    {
        get { return isCloseButtonVisible; }
        set { isCloseButtonVisible = value; Notify(); }
    }

    private bool isReadOnly = true;
    public bool IsReadOnly
    {
        get { return isReadOnly; }
        set { isReadOnly = value; Notify(); }
    }
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    public Action<TagControl>? OnUpdateText { get; set; }
    public Action<TagControl>? OnCloseRequest { get; set; }
    public Action<string, TagCategory>? OnClick { get; set; }
    #endregion

    #region ============================================================== DEPENDENCY PROPERTIES ============================================================================
    public static readonly DirectProperty<TagControl, string?> SetTextProperty = AvaloniaProperty.RegisterDirect<TagControl, string?>(nameof(SetText), e => e.SetText, OnSetTextChanged);

    private string? setText;
    public string? SetText
    {
        get { return setText; }
        set { SetAndRaise(SetTextProperty, ref setText, value); }
    }

    public static readonly DirectProperty<TagControl, TagCategory> SetCategoryProperty = AvaloniaProperty.RegisterDirect<TagControl, TagCategory>(nameof(SetCategory), e => e.SetCategory, OnSetCategoryChanged);

    private TagCategory setCategory;
    public TagCategory SetCategory
    {
        get { return setCategory; }
        set { SetAndRaise(SetCategoryProperty, ref setCategory, value); }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Default C-tor
    /// </summary>
    public TagControl()
    {
        InitializeComponent();
        DataContext = this;
        RemoveTag_Command = new SyncCommand(RemoveTag);
        tapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) }; 
        tapTimer.Tick += TapTimer_Tick;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    private void RemoveTag()
    {
        OnCloseRequest?.Invoke(this);
    }

    /// <summary>
    /// Text Changed dependency property handler
    /// </summary>
    private static void OnSetTextChanged(TagControl control, string value)
    {
        control.OnSetTextChanged(value);
    }

    /// <summary>
    /// Internal Text Changed dependency property handler
    /// </summary>
    private void OnSetTextChanged(string e)
    {
        if (e != null)
            Text = e;
    }

    /// <summary>
    /// Category Changed dependency property handler
    /// </summary>
    private static void OnSetCategoryChanged(TagControl control, TagCategory value)
    {
        control.OnSetCategoryChanged(value);
    }

    /// <summary>
    /// Internal Category Changed dependency property handler
    /// </summary>
    private void OnSetCategoryChanged(TagCategory e)
    {
        Category = e;
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
    /// Handles input's Tapped event
    /// </summary>
    private void Input_Tapped(object? sender, TappedEventArgs e)
    {
        if (!isDoubleTap)
            tapTimer.Start(); // start the timer only if it's not a double tap
        else
            isDoubleTap = false;  // if it was a double tab, reset the double tap flag 
    }

    /// <summary>
    /// Handles input's LostFocus event
    /// </summary>
    private void Input_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            IsReadOnly = true; // exit edit mode
            textBox.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0);
            OnUpdateText?.Invoke(this);
        }
    }

    /// <summary>
    /// Handles input's DoubleTapped event
    /// </summary>
    private void Input_DoubleTapped(object? sender, TappedEventArgs e)
    {
        isDoubleTap = true; // set the flag indicating a double tap has occurred
        if (IsReadOnly)
        {
            IsReadOnly = false; // enter edit mode
            if (sender is TextBox textBox)
            {
                textBox.SelectionStart = textBox.Text?.Length ?? 0;
                textBox.Background = new SolidColorBrush(Color.FromRgb(0,0,0), 0.2);
                textBox.Focus();
            }
        }
        // reset stuff so that next tap can be handled
        tapTimer.Stop();
        isDoubleTap = false;
    }

    /// <summary>
    /// Handles TapTimer's Tick event
    /// </summary>
    private void TapTimer_Tick(object? sender, EventArgs e)
    {
        tapTimer.Stop();
        if (!isDoubleTap)  // handle the single tap event
            if (IsReadOnly) // ignore single taps if its in edit mode
                OnClick?.Invoke(Text!, Category);
        isDoubleTap = false;
    }

    /// <summary>
    /// Handles input's GotFocus event
    /// </summary>
    private void Input_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (IsReadOnly) // when the input field is not in edit mode, don't allow focus in it, move it to the entire control
            Focus();
    }
    #endregion
}