using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowAnimations : Slowable {

    private Animation anim;

    private float initialAnimationSpeed;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animation>();
	}
	
	override public void Slow(float slowdownMultiplier, float slowdownPeriod)
    {
        //Debug.Log("Override slow from Slowable animation script");
        base.Slow(slowdownMultiplier, slowdownPeriod);

        foreach (AnimationState state in anim)
        {
            initialAnimationSpeed = state.speed;
            state.speed *= slowdownMultiplier;
        }
    }

    override protected void UndoSlowdown()
    {
        base.UndoSlowdown();

        foreach (AnimationState state in anim)
        {
            state.speed = initialAnimationSpeed;
        }
    }
}
