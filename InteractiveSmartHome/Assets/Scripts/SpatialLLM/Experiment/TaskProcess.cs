using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TaskProcess 
{
    private Func<Task> action; // 非同期処理を定義する関数
    private TaskProcess nextProcess; // 次のプロセス

    public TaskProcess(Func<Task> action)
    {
        this.action = action;
    }

    // 次のプロセスを接続
    public TaskProcess Connect(TaskProcess next)
    {
        this.nextProcess = next;
        return next;
    }

    // 実行処理
    public async Task Execute()
    {
        if (action != null)
        {
            await action();
        }

        if (nextProcess != null)
        {
            await nextProcess.Execute();
        }
    }
}
