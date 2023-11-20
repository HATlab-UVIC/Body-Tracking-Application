using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyAlignmentUpdater : MonoBehaviour
{
    BodyJointCoordinates _bodyJointCoordinates;

    // Start is called before the first frame update
    void Start()
    {
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
    }


    public void MoveBodyAlignment(int direction)
    {
        switch (direction)
        {
            case 0:
                _bodyJointCoordinates.BodyAlignmentPosition.x += 0.2f;
                break;

            case 1:
                _bodyJointCoordinates.BodyAlignmentPosition.x -= 0.2f;
                break;

            case 2:
                _bodyJointCoordinates.BodyAlignmentPosition.y += 0.2f;
                break;

            case 3:
                _bodyJointCoordinates.BodyAlignmentPosition.y -= 0.2f;
                break;
        }
    }
}
