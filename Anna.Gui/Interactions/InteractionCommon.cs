using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.BackgroundOperators;
using Anna.Gui.BackgroundOperators.Internals;
using Anna.Gui.Messaging;
using Anna.Service.Interfaces;
using Anna.Service.Workers;
using System.Collections.Generic;
using System.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Interactions;

internal static class InteractionCommon
{
    public static void Copy(
        IServiceProvider dic,
        Messenger messenger, IBackgroundWorker backgroundWorker,
        IEnumerable<string> files, string targetFolder)
    {
        var targetIEntries = files.Select(IEntry.Create).ToArray();
        var stats = dic.GetInstance(EntriesStats.T, targetIEntries);

        var worker = dic.GetInstance(ConfirmedFileSystemCopier.T,
            (messenger, (IEnumerable<IEntry>)targetIEntries, targetFolder, CopyOrMove.Copy));

        var @operator = dic.GetInstance(EntryBackgroundOperator.T,
            ((IEntriesStats)stats, (IFileProcessable)worker, EntryBackgroundOperator.ProgressModes.Stats));
        backgroundWorker.PushOperatorAsync(@operator).Forget();
    }
}