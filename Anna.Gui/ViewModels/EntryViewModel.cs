using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace Anna.ViewModels
{
    public class EntryViewModel : ViewModelBase
    {
        public string Name => _model?.Name ?? throw new NullReferenceException();
        public string Extension => _model?.Extension ?? throw new NullReferenceException();
        
        public ReactivePropertySlim<bool> IsSelected { get; }

        private Entry? _model;
        
        public EntryViewModel(IObjectLifetimeChecker objectLifetimeChecker)
            : base(objectLifetimeChecker)
        {
            IsSelected = new ReactivePropertySlim<bool>().AddTo(Trash);
        }

        public EntryViewModel Setup(Entry model)
        {
            _model = model;
            
            return this;
        }
    }
}