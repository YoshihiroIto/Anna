using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using Entry=Anna.DomainModel.Entry;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.ViewModels;

public sealed class EntryViewModel : HasModelViewModelBase<EntryViewModel, (Entry Entry, int Dummy)>
{
    public bool IsFolder => Model.Entry.IsFolder;

    public ReactivePropertySlim<bool> IsOnCursor => _IsOnCursor ??= SetupIsOnCursor();
    public ReadOnlyReactivePropertySlim<string> NameWithExtension => _NameWithExtension ??= SetupNameWithExtension();
    public ReadOnlyReactivePropertySlim<string> Name => _Name ??= SetupName();
    public ReadOnlyReactivePropertySlim<string> Extension => _Extension ??= SetupExtension();
    public ReadOnlyReactivePropertySlim<long> Size => _Size ??= SetupSize();
    public ReadOnlyReactivePropertySlim<string> Timestamp => _Timestamp ??= SetupTimestamp();
    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes => _Attributes ??= SetupAttributes();
    public ReadOnlyReactivePropertySlim<bool> IsSelected => _IsSelected ??= SetupIsSelected();

    private ReactivePropertySlim<bool>? _IsOnCursor;
    private ReadOnlyReactivePropertySlim<string>? _NameWithExtension;
    private ReadOnlyReactivePropertySlim<string>? _Name;
    private ReadOnlyReactivePropertySlim<string>? _Extension;
    private ReadOnlyReactivePropertySlim<long>? _Size;
    private ReadOnlyReactivePropertySlim<string>? _Timestamp;
    private ReadOnlyReactivePropertySlim<FileAttributes>? _Attributes;

    private ReadOnlyReactivePropertySlim<bool>? _IsSelected;

    private readonly string _timestampFormat;

    public EntryViewModel(IServiceProvider dic)
        : base(dic)
    {
        _timestampFormat = dic.GetInstance<AppConfig>().Data.TimestampFormat;
        if (_timestampFormat != "")
            _timestampFormat = LeftMargin + _timestampFormat;
    }

    private ReactivePropertySlim<bool> SetupIsOnCursor()
    {
        return new ReactivePropertySlim<bool>().AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupNameWithExtension()
    {
        return Model.Entry.ObserveProperty(x => x.NameWithExtension)
            .ObserveOnUIDispatcher()
            .Select(x => Model.Entry.IsFolder ? Path.AltDirectorySeparatorChar + x : x)
            .ToReadOnlyReactivePropertySlim(
                Model.Entry.IsFolder
                    ? Path.AltDirectorySeparatorChar + Model.Entry.NameWithExtension
                    : Model.Entry.NameWithExtension)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupName()
    {
        return Model.Entry.ObserveProperty(x => x.Name)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Entry.Name)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupExtension()
    {
        return Model.Entry.ObserveProperty(x => x.Extension)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Entry.Extension)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<long> SetupSize()
    {
        return Model.Entry.ObserveProperty(x => x.Size)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Entry.Size)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupTimestamp()
    {
        return Model.Entry.ObserveProperty(x => x.Timestamp)
            .ObserveOnUIDispatcher()
            .Select(MakeTimestampString)
            .ToReadOnlyReactivePropertySlim(MakeTimestampString(Model.Entry.Timestamp))
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<FileAttributes> SetupAttributes()
    {
        return Model.Entry.ObserveProperty(x => x.Attributes)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Entry.Attributes)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<bool> SetupIsSelected()
    {
        return Model.Entry.ObserveProperty(x => x.IsSelected)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
    }

    private string MakeTimestampString(DateTime v)
    {
        if (_timestampFormat == "")
            return LeftMargin + v.ToString(CultureInfo.CurrentUICulture);

        return v.ToString(_timestampFormat);
    }

    private const string LeftMargin = " ";
}