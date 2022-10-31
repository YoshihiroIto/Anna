﻿using Anna.Gui.Messaging.Messages;
using Avalonia.Xaml.Interactivity;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors.Actions;

public interface IAsyncAction
{
    public ValueTask ExecuteAsync(Trigger sender, MessageBase message);
}