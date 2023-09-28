using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TCP_Communication_Tests
{
    // ** Note: to display text for testing use: TestContext.WriteLine()

    string OP_Body_Coordinates_input = "[[[251.86478     77.78895      0.7667822 ]\n  [252.51468     95.317856     0.876728  ]\n  [240.80292     95.322365     0.8040645 ]\n  [232.37624    116.77977      0.8273126 ]\n  [236.22884    107.00365      0.5796813 ]\n  [266.7548      95.29869      0.786159  ]\n  [273.3028     115.452614     0.800534  ]\n  [268.09955    106.35879      0.7154094 ]\n  [251.82945    134.94682      0.7444093 ]\n  [242.13077    134.29881      0.7457033 ]\n  [236.90192    163.49742      0.8655727 ]\n  [241.44772    190.79929      0.83191615]\n  [259.63016    136.23273      0.69610536]\n  [252.50996    164.16794      0.8566567 ]\n  [258.32214    190.13763      0.8514975 ]\n  [249.88536     75.206375     0.7748856 ]\n  [254.43979     75.18927      0.7664361 ]\n  [246.62242     75.84186      0.54281354]\n  [258.3567      75.21359      0.73365533]\n  [257.71597    195.97488      0.77183896]\n  [262.24216    195.31381      0.8010959 ]\n  [257.69754    192.08958      0.6295021 ]\n  [238.8438     196.64983      0.71956825]\n  [236.24486    195.36232      0.81978583]\n  [243.402      194.03827      0.6732243 ]]]";

    // Test case below emulates data from the TCPServer to the BodyJointCoordinates class method
    // that turns the string data into a Vetor3 array.
    // Purpose: Verify that string -> Vector3[] is working correctly
    [Test]
    public void test_GetCoordinatesFromTCP()
    {
        BodyJointCoordinates _BJC = BodyJointCoordinates.Instance;
        _BJC.getBodyCoordinatesFromTCPStream(OP_Body_Coordinates_input);

        Assert.AreEqual(helper_OP_vectors(), _BJC._bodyJointCoordinateVectors);
        Assert.IsTrue(_BJC._coordinateDataSet);
    }


    // Sample Vector3[] that should be produced from function under test
    private Vector3[] helper_OP_vectors()
    {
        Vector3[] OP_vectors = new Vector3[25];

        OP_vectors[0] = new Vector3(251.86478f,     77.78895f,      0.7667822f);
        OP_vectors[1] = new Vector3(252.51468f,     95.317856f,     0.876728f);
        OP_vectors[2] = new Vector3(240.80292f,     95.322365f,     0.8040645f);
        OP_vectors[3] = new Vector3(232.37624f,    116.77977f,      0.8273126f);
        OP_vectors[4] = new Vector3(236.22884f,    107.00365f,      0.5796813f);
        OP_vectors[5] = new Vector3(266.7548f,      95.29869f,      0.786159f);
        OP_vectors[6] = new Vector3(273.3028f,     115.452614f,     0.800534f);
        OP_vectors[7] = new Vector3(268.09955f,    106.35879f,      0.7154094f);
        OP_vectors[8] = new Vector3(251.82945f,    134.94682f,      0.7444093f);
        OP_vectors[9] = new Vector3(242.13077f,    134.29881f,      0.7457033f);
        OP_vectors[10] = new Vector3(236.90192f,    163.49742f,      0.8655727f);
        OP_vectors[11] = new Vector3(241.44772f,    190.79929f,      0.83191615f);
        OP_vectors[12] = new Vector3(259.63016f,    136.23273f,      0.69610536f);
        OP_vectors[13] = new Vector3(252.50996f,    164.16794f,      0.8566567f);
        OP_vectors[14] = new Vector3(258.32214f,    190.13763f,      0.8514975f);
        OP_vectors[15] = new Vector3(249.88536f,     75.206375f,     0.7748856f);
        OP_vectors[16] = new Vector3(254.43979f,     75.18927f,      0.7664361f);
        OP_vectors[17] = new Vector3(246.62242f,     75.84186f,      0.54281354f);
        OP_vectors[18] = new Vector3(258.3567f,      75.21359f,      0.73365533f);
        OP_vectors[19] = new Vector3(257.71597f,    195.97488f,      0.77183896f);
        OP_vectors[20] = new Vector3(262.24216f,    195.31381f,      0.8010959f);
        OP_vectors[21] = new Vector3(257.69754f,    192.08958f,      0.6295021f);
        OP_vectors[22] = new Vector3(238.8438f,     196.64983f,      0.71956825f);
        OP_vectors[23] = new Vector3(236.24486f,    195.36232f,      0.81978583f);
        OP_vectors[24] = new Vector3(243.402f,      194.03827f,      0.6732243f);

        return OP_vectors;
    }

}
