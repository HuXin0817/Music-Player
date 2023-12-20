using System.Windows.Input;

namespace MusicPlayer.WPF.Base;

public class CommandBase : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        DoExecute.Invoke(parameter);
    }

    public Action<object> DoExecute { get; set; }
}