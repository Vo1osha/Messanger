using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPChat.Infrastructure.Commands.Base;
using static TCPChat.Infrastructure.Commands.Base.Command;

namespace TCPChat.Infrastructure.Commands
{
         /// <summary>шаблон связки?/// </summary>
        // LambdaCommand(delegateCommand) как одна из основных команд для всего( команда для viewmodel между паттерном(похоже на то)!) //
        internal class LambdaCommand : Command
        {

            private readonly Action<object> _Execute;
            private readonly Func<object, bool> _CanExecute;
            public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
            {
                _Execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
                _CanExecute = CanExecute;
            }
            public override bool CanExecute(object parameter) => _CanExecute?.Invoke(parameter) ?? true;

            public override void Execute(object parameter) => _Execute(parameter);
        }
    }

