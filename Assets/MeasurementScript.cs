using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class MeasurementScript : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;

    public GameObject[] measurePoints;

    public GameObject reticle;

    public float distanceBetweenPoints = 0f;

    private int currentMeasurePoint = 0;

    private bool placementEnabled = true;

    public TMP_Text displayText;
    public TMP_Text floatingDistanceText;
    public GameObject floatingDistanceObject;

    public LineRenderer line;

    Unit currentUnit = Unit.m;

    // Start is called before the first frame update
    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance();
        PlaceFloatingText();

        // shoot a raycast from the center of the screen
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits,
            TrackableType.PlaneWithinPolygon);

        //if the raycast hits a plane, update the reticle
        if (hits.Count > 0)
        {
            reticle.transform.position = hits[0].pose.position;
            reticle.transform.rotation = hits[0].pose.rotation;

            //draw the line to the reticle if the first point is placed
            if (currentMeasurePoint == 1)
            {
                DrawLine();
            }

            // enable the reticle if its disabled and the tape points aren't placed
            if (!reticle.activeInHierarchy && currentMeasurePoint < 2)
            {
                reticle.SetActive(true);
            }

            //if the user taps, place a tape point. disable more placements until the end of the touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (currentMeasurePoint < 2)
                {
                    PlacePoint(hits[0].pose.position, currentMeasurePoint);
                }

                placementEnabled = false;
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                placementEnabled = true;
            }
        }

        //if the raycast isn't hitting anything, don't display the reticle
        else if (hits.Count == 0 || currentMeasurePoint == 2)
        {
            reticle.SetActive(false);
        }

    }

    //change the position of the approperiate tape point and make it active.
    public void PlacePoint(Vector3 pointPosition, int pointIndex)
    {
        measurePoints[pointIndex].SetActive(true);

        measurePoints[pointIndex].transform.position = pointPosition;

        if (currentMeasurePoint == 1)
        {
            DrawLine();
        }

        currentMeasurePoint += 1;
    }

    void UpdateDistance()
    {
        if (currentMeasurePoint == 0)
        {
            distanceBetweenPoints = 0f;
        }
        else if (currentMeasurePoint == 1)
        {
            distanceBetweenPoints = Vector3.Distance(measurePoints[0].transform.position, reticle.transform.position);
        }
        else if (currentMeasurePoint == 2)
        {
            distanceBetweenPoints =
                Vector3.Distance(measurePoints[0].transform.position, measurePoints[1].transform.position);
        }

        //convert units
        float convertedDistance = 0f;

        switch (currentUnit)
        {
            case Unit.m:
                convertedDistance = distanceBetweenPoints;
                break;
            case Unit.cm:
                convertedDistance = distanceBetweenPoints * 100;
                break;
            case Unit.i:
                convertedDistance = distanceBetweenPoints / 0.0254f;
                break;
            case Unit.f:
                convertedDistance = distanceBetweenPoints * 3.2808f;
                break;
            default:
                break;
        }

        //change the text to display the distance
        string distanceStr = convertedDistance.ToString("#.##") + currentUnit;

        displayText.text = distanceStr;
        floatingDistanceText.text = distanceStr;

    }

    void DrawLine()
    {
        line.enabled = true;
        line.SetPosition(0, measurePoints[0].transform.position);
        if (currentMeasurePoint == 1)
        {
            line.SetPosition(1, reticle.transform.position);

        }
        else if (currentMeasurePoint == 2)
        {
            line.SetPosition(1, measurePoints[1].transform.position);

        }
    }

    void PlaceFloatingText()
    {
        if (currentMeasurePoint == 0)
        {
            floatingDistanceObject.SetActive(false);
        }
        else if (currentMeasurePoint == 1)
        {
            floatingDistanceObject.SetActive(true);
            floatingDistanceObject.transform.position =
                Vector3.Lerp(measurePoints[0].transform.position, reticle.transform.position, 0.5f);
        }
        else if (currentMeasurePoint == 2)
        {
            floatingDistanceObject.SetActive(true);
            floatingDistanceObject.transform.position = Vector3.Lerp(measurePoints[0].transform.position,
               measurePoints[1].transform.position, 0.5f);
        }

        floatingDistanceObject.transform.rotation =
            Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

    }

    //casting string (from the inspector) into a Unit so we can act upon it
    public void ChangeUnit(string unit)
    {
        currentUnit = (Unit)System.Enum.Parse(typeof(Unit), unit);
    }




    public enum Unit
    {
        m,
        cm,
        i,
        f
    }
}
