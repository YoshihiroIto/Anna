﻿using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;

namespace Anna.Service.Windows;

public sealed class TrashCanService : ITrashCanService
{
    void ITrashCanService.SendToTrashCan(IEntry target)
    {
        ShellApiWrapper.SendToTrashCan(target.Path);
    }

    bool ITrashCanService.EmptyTrashCan()
    {
        return ShellApiWrapper.EmptyTrashCan();
    }

    (long EntryAllSize, long EntryCount) ITrashCanService.GetTrashCanInfo()
    {
        return ShellApiWrapper.GetTrashCanInfo();
    }

    void ITrashCanService.OpenTrashCan()
    {
        ProcessHelper.RunAssociatedApp("shell:::{645FF040-5081-101B-9F08-00AA002F954E}");
    }
}