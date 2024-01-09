#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
using System.Threading.Tasks;
#endregion

namespace DatasetTag.Common.Extensions;

/// <summary>
/// Task extension method to handle exceptions (avoid littering whole project with try...catch'es)
/// </summary>
/// <remarks>
/// Creation Date: 24th of October, 2019
/// </remarks>
public static class TaskUtilities
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Extends Task by providing exception handling when a task is invoked on a void method (not awaited)
    /// </summary>
    /// <param name="task">The task to be awaited</param>
    public static async void FireAndForgetSafeAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    #endregion
}