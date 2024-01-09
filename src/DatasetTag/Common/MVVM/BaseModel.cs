#region ========================================================================= USING =====================================================================================
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace DatasetTag.Common.MVVM;

/// <summary>
/// Base class for MVVM pattern
/// </summary>
/// <remarks>
/// Creation Date: 12th of December, 2019
/// </remarks>
public class BaseModel : INotifyPropertyChanged
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Notifies subscribers about a property's value being changed
    /// </summary>
    /// <param name="propName">The property that had the value changed</param>
    public virtual void Notify([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
    #endregion
}