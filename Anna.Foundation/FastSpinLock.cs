using System.Runtime.CompilerServices;

namespace Anna.Foundation;

// https://ikorin2.hatenablog.jp/entry/2021/07/25/081734
public struct FastSpinLock
{
    private const int SyncEnter = 1;
    private const int SyncExit = 0;
    private int _syncFlag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enter()
    {
        if (Interlocked.CompareExchange(ref _syncFlag, SyncEnter, SyncExit) == SyncEnter)
            Spin();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Exit() => Volatile.Write(ref _syncFlag, SyncExit);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Spin()
    {
        var spinner = new SpinWait();
        spinner.SpinOnce();
        
        while (Interlocked.CompareExchange(ref _syncFlag, SyncEnter, SyncExit) == SyncEnter)
            spinner.SpinOnce();
    }
}