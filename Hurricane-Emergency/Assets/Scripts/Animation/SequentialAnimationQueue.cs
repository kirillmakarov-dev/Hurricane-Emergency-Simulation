using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationEnqueueResult
{
    Added,
    Duplicate,
    InvalidName
}

public sealed class SequentialAnimationQueue<TAnimation> where TAnimation : struct, Enum
{
    private readonly Queue<TAnimation> pending = new();
    private readonly IReadOnlyDictionary<TAnimation, Action<Action>> actions;
    private readonly bool preventDuplicates;

    public SequentialAnimationQueue(
        IReadOnlyDictionary<TAnimation, Action<Action>> actions,
        bool preventDuplicates = false)
    {
        this.actions = actions ?? throw new ArgumentNullException(nameof(actions));
        this.preventDuplicates = preventDuplicates;
    }

    public int Count => pending.Count;
    public bool IsRunning { get; private set; }
    public TAnimation? Current { get; private set; }
    public bool NeedsProcessing => pending.Count > 0 && !IsRunning;

    public AnimationEnqueueResult Enqueue(string animationName, out TAnimation animation)
    {
        animation = default;
        if (string.IsNullOrWhiteSpace(animationName) ||
            !Enum.TryParse(animationName.Trim(), true, out animation) ||
            !Enum.IsDefined(typeof(TAnimation), animation))
        {
            return AnimationEnqueueResult.InvalidName;
        }

        return Enqueue(animation);
    }

    public AnimationEnqueueResult Enqueue(TAnimation animation)
    {
        bool isCurrent = Current.HasValue &&
            EqualityComparer<TAnimation>.Default.Equals(Current.Value, animation);
        if (preventDuplicates && (isCurrent || pending.Contains(animation)))
        {
            return AnimationEnqueueResult.Duplicate;
        }

        pending.Enqueue(animation);
        return AnimationEnqueueResult.Added;
    }

    public bool HasAction(TAnimation animation) => actions.ContainsKey(animation);

    public IEnumerator Process(
        Action<TAnimation> onCompleted = null,
        Action<TAnimation> onMissingAction = null)
    {
        if (IsRunning) yield break;

        IsRunning = true;
        while (pending.Count > 0)
        {
            TAnimation next = pending.Dequeue();
            Current = next;

            if (actions.TryGetValue(next, out Action<Action> startAnimation))
            {
                bool finished = false;
                startAnimation(() => finished = true);
                yield return new WaitUntil(() => finished);
                onCompleted?.Invoke(next);
            }
            else
            {
                onMissingAction?.Invoke(next);
            }

            Current = null;
        }

        IsRunning = false;
    }

    public void Clear()
    {
        pending.Clear();
        Current = null;
        IsRunning = false;
    }
}
