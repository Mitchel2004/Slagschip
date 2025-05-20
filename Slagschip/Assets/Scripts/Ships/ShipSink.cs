using UnityEngine;
using UnityEngine.Events;

public class ShipSink : StateMachineBehaviour
{
    public UnityEvent OnStartSink {  get; private set; } = new UnityEvent();
    public UnityEvent OnEndSink { get; private set; } = new UnityEvent();

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStartSink.Invoke();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnEndSink.Invoke();
    }
}
