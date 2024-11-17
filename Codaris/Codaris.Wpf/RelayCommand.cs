// <copyright file="RelayCommand.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Codaris.Wpf
{
    /// <summary>
    /// Relay command with parameter.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <seealso cref="System.Windows.Input.ICommand" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
    /// </remarks>
    /// <param name="execute">The execute.</param>
    /// <param name="canExecute">The can execute.</param>
    public class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute) : ICommand
    {
        /// <summary>The execute action.</summary>
        private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        ///   <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute((T)parameter!);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }
    }

    /// <summary>
    /// Relay command.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </remarks>
    /// <param name="execute">The execute.</param>
    /// <param name="canExecute">The can execute.</param>
    public class RelayCommand(Action execute, Func<bool>? canExecute) : ICommand
    {
        /// <summary>The execute action.</summary>
        private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        ///   <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute();
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
