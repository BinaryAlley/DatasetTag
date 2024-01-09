#region ========================================================================= USING =====================================================================================
using System;
using System.Windows.Input;
#endregion

namespace DatasetTag.Common.MVVM;

/// <summary>
/// Implementation of ICommand and non-generic ISyncCommand interfaces
/// </summary>
/// <remarks>
/// Creation Date: 24th of October, 2019
/// </remarks>
public class SyncCommand : ISyncCommand
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event EventHandler? CanExecuteChanged;

    private bool isExecuting;
    private readonly Action executeSync;
    private readonly Func<bool>? canExecute;
    #endregion

    #region ====================================================================== CTOR =====================================================================================  
    /// <summary>
    /// Overload C-tor
    /// </summary>
    /// <param name="execute">The async Task method to be executed</param>
    /// <param name="canExecute">The method indicating whether the <paramref name="execute"/>can be executed</param>
    public SyncCommand(Action execute, Func<bool>? canExecute = null)
    {
        executeSync = execute;
        this.canExecute = canExecute;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Indicates whether the command can be executed
    /// </summary>
    /// <returns>True if the command can be executed; False otherwise.</returns>
    public bool CanExecute()
    {
        return !isExecuting && (canExecute?.Invoke() ?? true);
    }

    /// <summary>
    /// Executes a delegate synchronously
    /// </summary>
    public void ExecuteSync()
    {
        if (CanExecute())
        {
            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();
                executeSync();
            }
            finally
            {
                isExecuting = false;
            }
        }
        RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Executes the delegate that signals changes in permissions of execution of the command
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Explicit implementations
    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// Also, see the parameter version <seealso cref="AsyncCommand{T}.CanExecute(T)">AsyncCommand{T}.CanExecute(T)</seealso></param>
    /// <returns>True if this command can be executed; otherwise, False.</returns>
    bool ICommand.CanExecute(object? param)
    {
        return CanExecute();
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// Also, see the parameter version <seealso cref="AsyncCommand{T}.CanExecute(T)">AsyncCommand{T}.CanExecute(T)</seealso></param>
    void ICommand.Execute(object? param)
    {
        try
        {
            ExecuteSync();
        }
        catch { }
    }
    #endregion
    #endregion
}

/// <summary>
/// Implementation of ICommand and generic ISyncCommand interfaces
/// </summary>
/// <remarks>
/// Creation Date: 24th of October, 2019
/// </remarks>
public class SyncCommand<T> : ISyncCommand<T>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event EventHandler? CanExecuteChanged;

    private bool isExecuting;
    private readonly Action<T> executeSync;
    private readonly Func<T, bool>? canExecute;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor
    /// </summary>
    /// <param name="execute">The Action to be executed</param>
    /// <param name="canExecute">The method indicating whether the <paramref name="execute"/>can be executed</param>
    public SyncCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        executeSync = execute;
        this.canExecute = canExecute;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Executes a delegate synchronously, with an object parameter
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null</param>
    public void ExecuteSync(object param)
    {
        if (CanExecute((T)param))
        {
            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();
                executeSync((T)param);
            }
            finally
            {
                isExecuting = false;
            }
        }
        RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Executes the delegate that signals changes in permissions of execution of the command
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Executes a delegate synchronously, with a strong typed parameter
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null</param>
    public void ExecuteSync(T param)
    {
        if (CanExecute(param))
        {
            try
            {
                isExecuting = true;
                executeSync(param);
            }
            finally
            {
                isExecuting = false;
            }
        }
        RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Indicates whether the command can be executed
    /// </summary>
    /// <param name="param">Generic parameter passed to the command to be executed</param>
    /// <returns>True if the command can be executed; False otherwise.</returns>
    public bool CanExecute(T param)
    {
        return !isExecuting && (canExecute?.Invoke(param) ?? true);
    }

    #region Explicit implementations
    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    void ICommand.Execute(object? param)
    {
        try
        {
            if (param?.ToString() != "{DisconnectedItem}")
                ExecuteSync((T)param!);
        }
        catch { }
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state
    /// </summary>
    /// <param name="param">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>True if this command can be executed; otherwise, False.</returns>
    bool ICommand.CanExecute(object? param)
    {
        // WPF bug - due to virtualization, sometimes param can be automatically set to "DisconnectedItem", throwing exception
        return param == null || param.ToString() == "{DisconnectedItem}" || CanExecute((T)param);
    }
    #endregion
    #endregion
}