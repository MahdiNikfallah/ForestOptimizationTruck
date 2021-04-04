using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirResistance : MonoBehaviour
{
    Rigidbody2D truckRigid;
    Vector2 truckVel;
    private GameObject resistanceCastRayObject;
    private GameObject customPivot;
    private GameObject centerOfMass;

    int rayCasterDistance = 20;
    int raysNum = 100;

    public bool enableResistance = false;
    public float resistanceMultiplication = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

        truckRigid = gameObject.GetComponent<Rigidbody2D>();
        customPivot = GameObject.FindGameObjectWithTag("TruckPivot");
        resistanceCastRayObject = GameObject.FindGameObjectWithTag("ResisRayCaster");
        centerOfMass = GameObject.FindGameObjectWithTag("CenterOfMass");
        //truckRigid.centerOfMass = centerOfMass.transform.position;
    }

    private void FixedUpdate()
    {
        truckVel = truckRigid.velocity;
        ResistanceCastRayPositioner(truckVel.normalized);
        ResistanceCastRay();
    }

    void ResistanceCastRayPositioner(Vector2 normalizedVelocity)
    {
        resistanceCastRayObject.transform.position = customPivot.transform.position + (Vector3)(normalizedVelocity * rayCasterDistance);
    }

    void ResistanceCastRay()
    {
        Vector2 lineVector = resistanceCastRayObject.transform.position - customPivot.transform.position;
        Vector2 prependicularVec = Vector2.Perpendicular(lineVector);
        int layerMask = 1 << 3;
        RaycastHit2D[] rays = new RaycastHit2D[raysNum + 1];
        for (int i = -raysNum / 2; i <= raysNum / 2; i++)
        {
            rays[i + raysNum / 2] = CastRay((Vector2)(resistanceCastRayObject.transform.position) + (prependicularVec.normalized * i * 0.1f),
                -lineVector.normalized, 2 * rayCasterDistance, layerMask);
        }
        GathernormalVectors(rays);
    }

    void GathernormalVectors(RaycastHit2D[] rays)
    {
        ArrayList normalVectors = new ArrayList();
        for (int i = 0; i < rays.Length; i++)
        {
            if (rays[i])
            {
                normalVectors.Add(rays[i].normal);
                Vector2 force = (((-rays[i].normal) * Vector2.Dot(rays[i].normal, truckVel)) * resistanceMultiplication * truckVel.magnitude);
                truckRigid.AddForceAtPosition(force, rays[i].point);
            }
        }
    }

    void CalculateResistanceForces(ArrayList normalVectors, Vector2 vel)
    {
        ArrayList forces = new ArrayList();
        for (int i = 0; i < normalVectors.Count; i++)
        {
            forces.Add(((-(Vector2)normalVectors[i]) * Vector2.Dot((Vector2)normalVectors[i], vel)) * resistanceMultiplication * vel.magnitude);
        }
        CalculateTheFinalForce(forces);
    }

    void CalculateTheFinalForce(ArrayList forces)
    {
        Vector2 finalResistanceForce = new Vector2(0, 0);
        for (int i = 0; i < forces.Count; i++)
        {
            finalResistanceForce += (Vector2)forces[i];
        }
        ApplyResistanceForce(finalResistanceForce);
    }

    void ApplyResistanceForce(Vector2 force)
    {
        if (enableResistance)
        {
            truckRigid.AddForce(new Vector2(0, force.y));
            truckRigid.AddForce(new Vector2(force.x, 0));
        }
    }

    RaycastHit2D CastRay(Vector2 origin, Vector2 direction, float distance, int layerMask)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
        if (hit)
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.red);
        }
        else
        {
            Debug.DrawRay(origin, direction * distance, Color.white);
        }
        return hit;
    }
}
