using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : MonoBehaviour
{
    private float maxHeight = 3f;
    private float animTime = 1f;

    private float timer = 0f;

    private Vector3 init;
    private Vector3 end;

    public void setObjectives(Vector3 inicio, Vector3 objective)
    {
        init = inicio;
        end = objective;
    }

    void FixedUpdate()
    {
        if (end != null)
        {
            timer += Time.fixedDeltaTime;
            transform.position = new Vector3(Mathf.Lerp(init.x, end.x, timer / animTime), maxHeight-Mathf.Abs((timer - animTime / 2f)) * maxHeight*2f, Mathf.Lerp(init.z, end.z, timer / animTime));

            if (timer >= animTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
