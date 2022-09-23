using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.Gui.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<EntryViewModel> Entries { get; private set; } = null!;
    public Directory Model { get; private set; } = null!;

    public readonly ShortcutKeyManager ShortcutKeyManager;
    
    public DirectoryViewViewModel(
        Container dic, 
        ShortcutKeyManager shortcutKeyManager,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
        ShortcutKeyManager = shortcutKeyManager;
    }

    public DirectoryViewViewModel Setup(Directory model)
    {
        Model = model;
        
        var bufferedCollectionChanged =
            model.Entries
                .ToCollectionChanged()
                .Buffer(TimeSpan.FromMilliseconds(50))
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

    public Entry[] CollectTargetEntities()
    {
        var selectedEntities = Entries
            .Where(x => x.IsSelected.Value)
            .Select(x => x.Model)
            .ToArray();

        if (selectedEntities.Length > 0)
            return selectedEntities;

        return _cursorEntity != null
            ? new[] { _cursorEntity.Model }
            : Array.Empty<Entry>();
    }
    
    private readonly Container _dic;

    private EntryViewModel? _cursorEntity = null;
}