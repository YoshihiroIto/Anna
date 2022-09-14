using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System;
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
        lock (model.UpdateLockObj)
        {
            Entries = model.Entries.ToReadOnlyReactiveCollection().AddTo(Trash);
        }

        return this;
    }
}