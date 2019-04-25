using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class BasicAgent : Agent
{
    private Transform parent;

    public Transform player;
    public Transform goal;
    private RaycastHit hit;

    private float[] obs = {1, 1};

    private float x_min = 0.2f;
    private float x_max = 1f;

    private float z_min = 0.2f;
    private float z_max = 1f;

    public override void InitializeAgent()
    {
        parent = player.parent;
        Debug.Log("Initialized " + parent.name);
    }

    public override void CollectObservations()
    {
        AddVectorObs(obs);

        string s = "Observations: ";
        for (int i = 0; i < obs.Length; i++)
        {
            s = s + obs[i];
            s = s + " ";
        }
        Debug.Log("Observations: " + s);
    }

    private float normalizeValue(float current, float min, float max)
    {
        // normalized = current - min / max - min
        float divider = max - min;
        if(divider <= 0)
        {
            Debug.Log("Divider not valid: set to 1");
            divider = 1;
        }

        float value = current - min;
        // Debug.Log("before: " + current + " after: " + value / divider);
        return value / divider;
    }


    public override void AgentAction(float[] vectorAction, string textAction)
	{
        float distance_x_relative = (player.position.x - parent.position.x) - (goal.position.x - parent.position.x);
        float distance_z_relative = (player.position.z - parent.position.z) - (goal.position.z - parent.position.z);

        float x_copy = distance_x_relative;
        float z_copy = distance_z_relative;

        if(x_copy < 0)
        {
            x_copy = x_copy * -1;
        }

        if (z_copy < 0)
        {
            z_copy = z_copy * -1;
        }

        if (x_copy < x_min)
        {
            x_min = x_copy;
        }

        if (x_copy > x_max)
        {
            x_max = x_copy;
        }

        if(z_copy < z_min)
        {
            z_min = z_copy;
        }

        if (z_copy > z_max)
        {
            z_max = z_copy;
        }

        // normalize
        float distance_x_relative_normalized = normalizeValue(distance_x_relative, x_min, x_max);
        float distance_z_relative_normalized = normalizeValue(distance_z_relative, z_min, z_max);

        // obbs
        obs[0] = distance_x_relative_normalized;
        obs[1] = distance_z_relative_normalized;

        // Debug.Log("norm x: " + distance_x_relative_normalized);
        // Debug.Log("norm z: " + distance_z_relative_normalized);


        string actions = "Actions: ";
        for (int i = 0; i < vectorAction.Length; i++)
        {
            actions = actions + vectorAction[i];
            actions = actions + " ";
        }
        Debug.Log("Actions: " + actions);


        AddReward(-0.01f);
      
        float w = vectorAction[0];
        float s = vectorAction[1];
        float a = vectorAction[2];
        float d = vectorAction[3];

        if (w == 1 && s == 0)
        {
            player.position = new Vector3(player.position.x, player.position.y, player.position.z + 0.1f);
        }

        if (w == 0 && s == 1)
        {
            player.position = new Vector3(player.position.x, player.position.y, player.position.z - 0.1f);
        }

        
        if (a == 1 && d == 0)
        {
            player.position = new Vector3(player.position.x - 0.1f, player.position.y, player.position.z);
        }

        if (a == 0 && d == 1)
        {
            player.position = new Vector3(player.position.x + 0.1f, player.position.y, player.position.z);
        }

        drawBig();
        AddReward(-0.01f);

        if (player.position == goal.position)
        {
            Debug.Log("Goal achieved!");
            AddReward(1f);
            Done();
        }

        // chechBadValues(w, s, a, d);

        if (player.position.x - parent.position.x > 5 || player.transform.position.x - parent.position.x < -5)
        {
            Debug.Log("X constraint");
            SetReward(-1f);
            Done();
        }

        if (player.position.z - parent.position.z > 5 || player.transform.position.z - parent.position.z < -5)
        {
            Debug.Log("Z constraint");
            SetReward(-1f);
            Done();
        }
    }

    public override void AgentReset()
    {
        Debug.Log("Agent reset");
        resetPlayer();
        resetGoal();
    }

    private void resetPlayer()
    {
        Random random = new Random();
        float position_Z = Random.Range(-5.0f, 5.0f);
        float position_X = Random.Range(-5.0f, 5.0f);
        // Debug.Log("Reseting player to X: " + position_X + " Z:" + position_Z);
        player.transform.position = parent.position + new Vector3(position_X, 0, position_Z);
    }

    private void resetGoal()
    {
        float largeGoalposition_Z = Random.Range(-5.0f, 5.0f);
        float largeGoalposition_X = Random.Range(-5.0f, 5.0f);
        goal.position = parent.position + new Vector3(largeGoalposition_X, 0, largeGoalposition_Z);
    }

    private void drawBig()
    {
        Vector3 fromPosition2 = player.transform.position;
        Vector3 toPosition2 = goal.transform.position;
        Vector3 direction2 = toPosition2 - fromPosition2;

        if (Physics.Raycast(player.position, direction2, out hit))
        {
            Debug.DrawRay(player.transform.position, direction2 * hit.distance, Color.red);
            // Debug.Log("Distance to big: " + big.distance);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.collider.tag == "goal")
        {
            Debug.Log("Achieved goal");
            AddReward(1f);
            Done();
        }
    }
}
