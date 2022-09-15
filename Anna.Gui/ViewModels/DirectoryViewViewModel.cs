using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<Entry>? Entries { get; private set; }

    public DirectoryViewViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
    }

    public DirectoryViewViewModel Setup(Directory model)
    {
        var bufferedCollectionChanged =
            model.Entries
                .ToCollectionChanged()
                .Buffer(TimeSpan.FromMilliseconds(200))
                .Where(x => x.Any())
                .SelectMany(x => x);

        lock (model.UpdateLockObj)
        {
            Entries = model.Entries.ToReadOnlyReactiveCollection(bufferedCollectionChanged)
                .AddTo(Trash);
        }

        return this;
    }
}