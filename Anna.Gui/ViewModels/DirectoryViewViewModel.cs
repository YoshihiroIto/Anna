using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.ViewModels.ShortcutKey;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<EntryViewModel>? Entries { get; private set; }

    public readonly ShortcutKeyManager ShortcutKeyManager;
    
    public DirectoryViewViewModel(
        Container dic, 
        ShortcutKeyManager shortcutKeyManager,
        IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
        ShortcutKeyManager = shortcutKeyManager;
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
    
    private readonly Container _dic;
}