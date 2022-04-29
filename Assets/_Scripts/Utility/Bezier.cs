﻿using UnityEngine;
using UnityEngine.Assertions;

class CubicBezierCurve
{
    Vector3[] controlVerts = new Vector3[4];

    public CubicBezierCurve(Vector3[] cvs)
    {
        // Cubic Bezier curves require 4 cvs.
        Assert.IsTrue(cvs.Length == 4);
        for (int cv = 0; cv < 4; cv++)
            controlVerts[cv] = cvs[cv];
    }

    public Vector3 GetPoint(float t) // t E [0, 1].
    {
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));
        float c = 1.0f - t;

        // The Bernstein polynomials.
        float bb0 = c * c * c;
        float bb1 = 3 * t * c * c;
        float bb2 = 3 * t * t * c;
        float bb3 = t * t * t;

        Vector3 point = controlVerts[0] * bb0 + controlVerts[1] * bb1 + controlVerts[2] * bb2 + controlVerts[3] * bb3;
        return point;
    }

    public Vector3 GetTangent(float t) // t E [0, 1].
    {
        // See: http://bimixual.org/AnimationLibrary/beziertangents.html
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));

        Vector3 q0 = controlVerts[0] + ((controlVerts[1] - controlVerts[0]) * t);
        Vector3 q1 = controlVerts[1] + ((controlVerts[2] - controlVerts[1]) * t);
        Vector3 q2 = controlVerts[2] + ((controlVerts[3] - controlVerts[2]) * t);

        Vector3 r0 = q0 + ((q1 - q0) * t);
        Vector3 r1 = q1 + ((q2 - q1) * t);
        Vector3 tangent = r1 - r0;
        return tangent;
    }

    public float GetClosestParam(Vector3 pos, float paramThreshold = 0.000001f)
    {
        return GetClosestParamRec(pos, 0.0f, 1.0f, paramThreshold);
    }

    float GetClosestParamRec(Vector3 pos, float beginT, float endT, float thresholdT)
    {
        float mid = (beginT + endT) / 2.0f;

        // Base case for recursion.
        if ((endT - beginT) < thresholdT)
            return mid;

        // The two halves have param range [start, mid] and [mid, end]. We decide which one to use by using a midpoint param calculation for each section.
        float paramA = (beginT + mid) / 2.0f;
        float paramB = (mid + endT) / 2.0f;

        Vector3 posA = GetPoint(paramA);
        Vector3 posB = GetPoint(paramB);
        float distASq = (posA - pos).sqrMagnitude;
        float distBSq = (posB - pos).sqrMagnitude;

        if (distASq < distBSq)
            endT = mid;
        else
            beginT = mid;

        // The (tail) recursive call.
        return GetClosestParamRec(pos, beginT, endT, thresholdT);
    }
}


// A CubicBezierPath is made of a collection of cubic Bezier curves. If two points are supplied they become the end
// points of one CubicBezierCurve and the 2 interior CVs are generated, creating a small straight line. For 3 points
// the middle point will be on both CubicBezierCurves and each curve will have equal tangents at that point.
public class CubicBezierPath
{
    public enum Type
    {
        Open,

        Closed
    }

    Type type = Type.Open;

    int numCurveSegments = 0;

    int numControlVerts = 0;

    Vector3[] controlVerts = null;

    // The term 'knot' is another name for a point right on the path (an interpolated point). With this constructor the
    // knots are supplied and interpolated. knots.length (the number of knots) must be >= 2. Interior Cvs are generated
    // transparently and automatically.
    public CubicBezierPath(Vector3[] knots, Type t = Type.Open)
    {
        InterpolatePoints(knots, t);
    }

    public Type GetPathType()
    {
        return type;
    }

    public bool IsClosed()
    {
        return (type == Type.Closed) ? true : false;
    }

    public bool IsValid()
    {
        return (numCurveSegments > 0) ? true : false;
    }

    public void Clear()
    {
        controlVerts = null;
        type = Type.Open;
        numCurveSegments = 0;
        numControlVerts = 0;
    }

    // A closed path will have one more segment than an open for the same number of interpolated points.
    public int GetNumCurveSegments()
    {
        return numCurveSegments;
    }

    public float GetMaxParam()
    {
        return (float) numCurveSegments;
    }

    // Access to the raw CVs.
    public int GetNumControlVerts()
    {
        return numControlVerts;
    }

    public Vector3[] GetControlVerts()
    {
        return controlVerts;
    }

    public float ComputeApproxLength()
    {
        if (!IsValid())
            return 0.0f;

        // For a closed path this still works if you consider the last point as separate from the first. That is, a closed
        // path is just like an open except the last interpolated point happens to match the first.
        int numInterpolatedPoints = numCurveSegments + 1;
        if (numInterpolatedPoints < 2)
            return 0.0f;

        float totalDist = 0.0f;
        for (int n = 1; n < numInterpolatedPoints; n++)
        {
            Vector3 a = controlVerts[(n - 1) * 3];
            Vector3 b = controlVerts[n * 3];
            totalDist += (a - b).magnitude;
        }

        if (totalDist == 0.0f)
            return 0.0f;

        return totalDist;
    }

    public float ComputeApproxParamPerUnitLength()
    {
        float length = ComputeApproxLength();
        return (float) numCurveSegments / length;
    }

    public float ComputeApproxNormParamPerUnitLength()
    {
        float length = ComputeApproxLength();
        return 1.0f / length;
    }

    // Interpolates the supplied points. Internally generates any necessary CVs. knots.length (number of knots)
    // must be >= 2.
    public void InterpolatePoints(Vector3[] knots, Type t)
    {
        int numKnots = knots.Length;
        Assert.IsTrue(numKnots >= 2);
        Clear();
        type = t;
        switch (type)
        {
            case Type.Open:
            {
                numCurveSegments = numKnots - 1;
                numControlVerts = 3 * numKnots - 2;
                controlVerts = new Vector3[numControlVerts];

                // Place the interpolated CVs.
                for (int n = 0; n < numKnots; n++)
                    controlVerts[n * 3] = knots[n];

                // Place the first and last non-interpolated CVs.
                Vector3 initialPoint = (knots[1] - knots[0]) * 0.25f;

                // Interpolate 1/4 away along first segment.
                controlVerts[1] = knots[0] + initialPoint;
                Vector3 finalPoint = (knots[numKnots - 2] - knots[numKnots - 1]) * 0.25f;

                // Interpolate 1/4 backward along last segment.
                controlVerts[numControlVerts - 2] = knots[numKnots - 1] + finalPoint;

                // Now we'll do all the interior non-interpolated CVs.
                for (int k = 1; k < numCurveSegments; k++)
                {
                    Vector3 a = knots[k - 1] - knots[k];
                    Vector3 b = knots[k + 1] - knots[k];
                    float aLen = a.magnitude;
                    float bLen = b.magnitude;

                    if ((aLen > 0.0f) && (bLen > 0.0f))
                    {
                        float abLen = (aLen + bLen) / 8.0f;
                        Vector3 ab = (b / bLen) - (a / aLen);
                        ab.Normalize();
                        ab *= abLen;

                        controlVerts[k * 3 - 1] = knots[k] - ab;
                        controlVerts[k * 3 + 1] = knots[k] + ab;
                    }
                    else
                    {
                        controlVerts[k * 3 - 1] = knots[k];
                        controlVerts[k * 3 + 1] = knots[k];
                    }
                }

                break;
            }

            case Type.Closed:
            {
                numCurveSegments = numKnots;

                // We duplicate the first point at the end so we have contiguous memory to look of the curve value. That's
                // what the +1 is for.
                numControlVerts = 3 * numKnots + 1;
                controlVerts = new Vector3[numControlVerts];

                // First lets place the interpolated CVs and duplicate the first into the last CV slot.
                for (int n = 0; n < numKnots; n++)
                    controlVerts[n * 3] = knots[n];

                controlVerts[numControlVerts - 1] = knots[0];

                // Now we'll do all the interior non-interpolated CVs. We go to k=NumCurveSegments which will compute the
                // two CVs around the zeroth knot (points[0]).
                for (int k = 1; k <= numCurveSegments; k++)
                {
                    int modkm1 = k - 1;
                    int modkp1 = (k + 1) % numCurveSegments;
                    int modk = k % numCurveSegments;

                    Vector3 a = knots[modkm1] - knots[modk];
                    Vector3 b = knots[modkp1] - knots[modk];
                    float aLen = a.magnitude;
                    float bLen = b.magnitude;
                    int mod3km1 = 3 * k - 1;

                    // Need the -1 so the end point is a duplicated start point.
                    int mod3kp1 = (3 * k + 1) % (numControlVerts - 1);
                    if ((aLen > 0.0f) && (bLen > 0.0f))
                    {
                        float abLen = (aLen + bLen) / 8.0f;
                        Vector3 ab = (b / bLen) - (a / aLen);
                        ab.Normalize();
                        ab *= abLen;

                        controlVerts[mod3km1] = knots[modk] - ab;
                        controlVerts[mod3kp1] = knots[modk] + ab;
                    }
                    else
                    {
                        controlVerts[mod3km1] = knots[modk];
                        controlVerts[mod3kp1] = knots[modk];
                    }
                }

                break;
            }
        }
    }

    // For a closed path the last CV must match the first.
    public void SetControlVerts(Vector3[] cvs, Type t)
    {
        int numCVs = cvs.Length;
        Assert.IsTrue(numCVs > 0);
        Assert.IsTrue(((t == Type.Open) && (numCVs >= 4)) || ((t == Type.Closed) && (numCVs >= 7)));
        Assert.IsTrue(((numCVs - 1) % 3) == 0);
        Clear();
        type = t;

        numControlVerts = numCVs;
        numCurveSegments = ((numCVs - 1) / 3);
        controlVerts = cvs;
    }

    // t E [0, numSegments]. If the type is closed, the number of segments is one more than the equivalent open path.
    public Vector3 GetPoint(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float) numCurveSegments;

            while (t > (float) numCurveSegments)
                t -= (float) numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float) numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float) numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        int segment = (int) t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        Vector3[] curveCVs = new Vector3[4];
        curveCVs[0] = controlVerts[3 * segment + 0];
        curveCVs[1] = controlVerts[3 * segment + 1];
        curveCVs[2] = controlVerts[3 * segment + 2];
        curveCVs[3] = controlVerts[3 * segment + 3];

        CubicBezierCurve bc = new CubicBezierCurve(curveCVs);
        return bc.GetPoint(t - (float) segment);
    }

    // Does the same as GetPoint except that t is normalized to be E [0, 1] over all segments. The beginning of the curve
    // is at t = 0 and the end at t = 1. Closed paths allow a value bigger than 1 in which case they loop.
    public Vector3 GetPointNorm(float t)
    {
        return GetPoint(t * (float) numCurveSegments);
    }

    // Similar to GetPoint but returns the tangent at the specified point on the path. The tangent is not normalized.
    // The longer the tangent the 'more influence' it has pulling the path in that direction.
    public Vector3 GetTangent(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float) numCurveSegments;

            while (t > (float) numCurveSegments)
                t -= (float) numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float) numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float) numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        int segment = (int) t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        Vector3[] curveCVs = new Vector3[4];
        curveCVs[0] = controlVerts[3 * segment + 0];
        curveCVs[1] = controlVerts[3 * segment + 1];
        curveCVs[2] = controlVerts[3 * segment + 2];
        curveCVs[3] = controlVerts[3 * segment + 3];

        CubicBezierCurve bc = new CubicBezierCurve(curveCVs);
        return bc.GetTangent(t - (float) segment);
    }

    public Vector3 GetTangentNorm(float t)
    {
        return GetTangent(t * (float) numCurveSegments);
    }

    // This function returns a single closest point. There may be more than one point on the path at the same distance.
    // Use ComputeApproxParamPerUnitLength to determine a good paramThreshold. eg. Say you want a 15cm threshold,
    // use: paramThreshold = ComputeApproxParamPerUnitLength() * 0.15f.
    public float ComputeClosestParam(Vector3 pos, float paramThreshold)
    {
        float minDistSq = float.MaxValue;
        float closestParam = 0.0f;
        for (int startIndex = 0; startIndex < controlVerts.Length - 1; startIndex += 3)
        {
            Vector3[] curveCVs = new Vector3[4];
            for (int i = 0; i < 4; i++)
                curveCVs[i] = controlVerts[startIndex + i];

            CubicBezierCurve curve = new CubicBezierCurve(curveCVs);
            float curveClosestParam = curve.GetClosestParam(pos, paramThreshold);

            Vector3 curvePos = curve.GetPoint(curveClosestParam);
            float distSq = (curvePos - pos).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                float startParam = ((float) startIndex) / 3.0f;
                closestParam = startParam + curveClosestParam;
            }
        }

        return closestParam;
    }

    // Same as above but returns a t value E [0, 1]. You'll need to use a paramThreshold like
    // ComputeApproxParamPerUnitLength() * 0.15f if you want a 15cm tolerance.
    public float ComputeClosestNormParam(Vector3 pos, float paramThreshold)
    {
        return ComputeClosestParam(pos, paramThreshold * (float) numCurveSegments);
    }
}