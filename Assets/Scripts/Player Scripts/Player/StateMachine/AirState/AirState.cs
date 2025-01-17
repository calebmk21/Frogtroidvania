using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public abstract class AirState : BaseState
{
    PlayerAttributes attributes;

    PlayerController.JumpStates jumpState;

    public override void EnterState(StateManager player, PlayerAttributes attributes)
    {
        //Debug.Log("Hello from air state");
        this.attributes = attributes;

        attributes.canPlunge = true; // as soon as you are in the air, you can plunge again
    }

    public override void UpdateState(StateManager player)
    {
        if (player.playerController.IsSwitchPressed() && IsHumanState()) // frog-human swapping
        {
            player.SwitchState(player.FrogMovingState);
            player.playerController.clearSwitchPressedInput();    // Need to clear input so that switch only happens once
        }
        else if (player.playerController.IsSwitchPressed() && !IsHumanState()) // frog-human swapping
        {
            player.SwitchState(player.HumanMovingState);
            player.playerController.clearSwitchPressedInput();    // Need to clear input so that switch only happens once
        }
    }

    public override void FixedUpdateState(StateManager player)
    {
        jumpState = player.playerController.JumpState();
        string jumpContext = jumpState.ToString();

        // for small jumps
        if (jumpContext == "canceled" && attributes.rb.velocity.y > 0) // if the jump key was canceled midjump, fall faster than usual
        {
            attributes.rb.velocity = new Vector2(attributes.rb.velocity.x, attributes.rb.velocity.y * 0.5f);
        }

        if (attributes.isGrounded)
        {
            player.SwitchState(player.IdleState);
            attributes.state = PlayerAttributes.PlayerStates.Idle;
            return;
        }

        if (attributes.canPlunge && player.playerController.IsPlungePressed())
        {
            attributes.canDash = false;
            player.SwitchState(player.PlungeState); // switch to the plunge state
            return;
        }

        if (attributes.canDash && player.playerController.IsDashPressed()) // if you can dash and dash is pressed, dash
        {
            attributes.canPlunge = false; // cannot plunge
            player.SwitchState(player.DashState);
            return;
        }

        if (!player.playerController.IsMovePressed()) // if no movement keys pressed
        {
            player.SwitchState(player.IdleState);
        }
        else
            Move(player.playerController.GetDir(), player.playerController.HorizontalVal());

        Animate(); // animate
    }

    public override void OnCollisionEnter2D(StateManager player, Collision2D collision)
    {

    }

    public override void OnCollisionStay2D(StateManager player, Collision2D collision)
    {

    }

    // Stops the player
    private void Stop()
    {
        if (attributes.isGrounded)
        {
            attributes.rb.velocity = new Vector2(0, attributes.rb.velocity.y);
        }
    }

    // deals with the the velocity of the player, and calls Flip() when applicable
    public void Move(int dir, float horizontal)
    {
        // Debug.Log("Air Moving " + attributes.rb.velocity);

        // changes horizontal velocity of player
        ////Time.deltaTime makes the speed more constant between different computers with different frames per second
        ///

        if (Mathf.Sign(horizontal) != Mathf.Sign(attributes.rb.velocity.x))
        {
            attributes.rb.velocity = new Vector2(0, attributes.rb.velocity.y); // reset velocity to zero for tighter movement
        }

        attributes.rb.velocity += new Vector2(horizontal * attributes.acceleration * Time.deltaTime, 0); // move with acceleration

        if (attributes.isHooked) // if the player is hooked, then clamp velocity to frapple top speed (else frapple along!!)
        {
            float clampedX = Vector2.ClampMagnitude( new Vector2 (attributes.rb.velocity.x, 0), attributes.frappleTopSpeed).x; // clamp horizontal value to top speed
            attributes.rb.velocity = new Vector2(clampedX, attributes.rb.velocity.y);
            // Debug.Log("Frapple Speed");
        } else
        {
            float clampedX = Vector2.ClampMagnitude(new Vector2(attributes.rb.velocity.x, 0), attributes.topSpeed).x; // clamp horizontal value to top speed
            attributes.rb.velocity = new Vector2(clampedX, attributes.rb.velocity.y);
        }

        // flip the player if needed
        if ((attributes.facingRight && dir == -1) ||
            (!attributes.facingRight && dir == 1))
            Flip();
    }

    private void Animate()
    {
        if(attributes.rb.velocity.y > Mathf.Epsilon) // moving upward
        {
            attributes.state = PlayerAttributes.PlayerStates.Jumping;
        } else if(attributes.rb.velocity.y < -Mathf.Epsilon) // falling downward
        {
            attributes.state = PlayerAttributes.PlayerStates.Falling;
        } if(attributes.isGrounded) // if grounded and no vertical velocity
        {
            attributes.state = PlayerAttributes.PlayerStates.Idle;
        }
    }

    // flip the player
    private void Flip()
    {
        attributes.facingRight = !attributes.facingRight;

        // flips the sprite AND its colliders
        Vector3 theScale = attributes.transform.localScale;
        theScale.x *= -1;
        attributes.transform.localScale = theScale;
    }
}
