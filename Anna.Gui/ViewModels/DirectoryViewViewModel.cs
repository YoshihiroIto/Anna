using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.ServiceProvider;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    private readonly Container _dic;
    public ReadOnlyReactiveCollection<EntryViewModel>? Entries { get; private set; }

    public DirectoryViewViewModel(Container dic, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
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
            Entries = model.Entries.ToReadOnlyReactiveCollection(
                bufferedCollectionChanged,
                x => _dic.GetInstance<EntryViewModel>().Setup(x))
                .AddTo(Trash);
        }

        return this;
    }
}