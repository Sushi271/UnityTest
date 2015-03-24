using UnityEngine;
using System.Collections;
using Test.Utils;

namespace Test.Cube
{
    public class CubeOfCubes : MonoBehaviour
    {
        public CubeStructure Structure { get; private set; }

        void Start()
        {
            Structure = new CubeStructure(gameObject, 3);
            Structure.SetCell(2, -2, 2, false);
            Structure.SetCell(-2, -2, 2, false);
            Structure.SetCell(-1, 2, -2, false);
            Structure.SetCell(0, 2, 2, false);
            Structure.SetCell(0, -1, -2, false);
            Structure.SetCell(2, 0, 2, false);
            Structure.SetCell(2, 0, 0, false);
            Structure.SetCell(2, -2, 1, false);
            Structure.SetCell(1, -2, -1, false);
        }

        void Update()
        {

        }
    }
}