using System.Collections.Generic;

namespace dEditor.Shell.CommandBar
{
    public interface ICommandBar
    {
        IList<string> RecentCommands { get; }

        void Execute(string code, bool mute = false);
    }
}