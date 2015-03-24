﻿using System;
using System.Collections.Generic;
using Test.Collections;
using Test.Utils;
using UnityEngine;

namespace Test.Cube
{
    /// <summary>
    /// Class representing structure of the cube with given radius. Radius is its most important property. Instance of this class can be adressed with index operator [x, y, z] to get CubeCell at given coordinates of the structure. Note that coords must always be lower than the Radius. When the Radius is 0, there are no cells. When the Radius is 1, it's a cube 1x1x1 and max (and only) absolute value of coord is 0. When the Radius is 2, it's a cube 3x3x3 and max absolute value of coord is 1. And so on.
    /// </summary>
    public class CubeStructure
    {
        GameObject cube;

        TwoWayList<TwoWayList<TwoWayList<CubeCell>>> cells;
        List<int> layersCellCount;

        /// <summary>
        /// Current radius of this CubeStructure. It cannot be changed explicitly. Instead use Expand() and Shrink() methods.
        /// </summary>
        public int Radius { get; private set; }

        public SizeChangeBehaviour UnwantedSizeChangeBehaviour { get; set; }

        /// <summary>
        /// Creates new CubeStructure with given initial radius, and sets all cells are set.
        /// </summary>
        /// <param name="initialRadius">Initial radius of the CubeStructure.</param>
        public CubeStructure(GameObject cube, int initialRadius)
        {
            if (initialRadius < 0)
                throw new ArgumentException("Argument initialRadius must not be lower than 0.");

            this.cube = cube;
            Radius = initialRadius;

            cells = CreateCellXYZCube(Radius);
            ForEveryCell((x, y, z) => cells[x][y][z] = new CubeCell(cube, x, y, z));
            UpdateLastLayersSides();

            layersCellCount = new List<int>();
            for (int i = 0; i < Radius; i++)
                layersCellCount.Add(GetLayerMaxCapacity(i));

            UnwantedSizeChangeBehaviour = SizeChangeBehaviour.Exception;
        }

        /// <summary>
        /// Allows to get the CubeCell of this CubeStructure at given coordinates. Throws IndexOutOfBoundsException, if any coord is not lower than Radius.
        /// </summary>
        /// <param name="x">X coord of the cell.</param>
        /// <param name="y">Y coord of the cell.</param>
        /// <param name="z">Z coord of the cell.</param>
        public CubeCell this[int x, int y, int z]
        {
            get { return GetCell(x, y, z); }
        }

        /// <summary>
        /// Allows to get the CubeCell of this CubeStructure at given coordinates. Throws IndexOutOfBoundsException, if any coord is not lower than Radius.
        /// </summary>
        /// <param name="x">X coord of the cell.</param>
        /// <param name="y">Y coord of the cell.</param>
        /// <param name="z">Z coord of the cell.</param>
        public CubeCell GetCell(int x, int y, int z)
        {
            return cells[x][y][z];
        }

        /// <summary>
        /// Allows to set the CubeCell of this CubeStructure at given coordinates. Throws IndexOutOfBoundsException, if any coord is not lower than Radius.
        /// </summary>
        /// <param name="x">X coord of the cell.</param>
        /// <param name="y">Y coord of the cell.</param>
        /// <param name="z">Z coord of the cell.</param>
        /// <param name="hasCellThere">If true, the cell will be created at given coords. If false, it will be erased.</param>
        public void SetCell(int x, int y, int z, bool hasCellThere)
        {
            CubeCell oldValue = cells[x][y][z];
            if (hasCellThere == (oldValue != null)) return;

            cells[x][y][z] = hasCellThere ? new CubeCell(cube, x, y, z) : null;
            if (!hasCellThere) oldValue.ClearSides();

            UpdateSides(x, y, z);
            UpdateNeighboursSides(x, y, z);

            int layerIndex = MathHelper.Max(x, y, z);
            int dLayerCellCount = hasCellThere ? 1 : -1;
            layersCellCount[layerIndex] += dLayerCellCount;
        }

        /// <summary>
        /// Gets the CubeCell of this CubeStructure at given coordinates. Returns null, if any coord is not lower than Radius.
        /// </summary>
        /// <param name="x">X coord of the cell.</param>
        /// <param name="y">Y coord of the cell.</param>
        /// <param name="z">Z coord of the cell.</param>
        public CubeCell TryGetCell(int x, int y, int z)
        {
            if (IsOutOfRadius(x, y, z)) return null;

            return GetCell(x, y, z);
        }

        /// <summary>
        /// Sets the CubeCell of this CubeStructure at given coordinates. Returns false, if any coord is not lower than Radius, otherwise true.
        /// </summary>
        /// <param name="x">X coord of the cell.</param>
        /// <param name="y">Y coord of the cell.</param>
        /// <param name="z">Z coord of the cell.</param>
        /// <param name="hasCellThere">If true, the cell will be created at given coords. If false, it will be erased.</param>
        public bool TrySetCell(int x, int y, int z, bool hasCellThere)
        {
            if (IsOutOfRadius(x, y, z)) return false;

            SetCell(x, y, z, hasCellThere);
            return true;
        }

        /// <summary>
        /// Retrieves the information, whether the most outer layer of this CubeStructure is full, i.e. all of the cells are set. If CubeStructure has to grow and this method returns true, then Expand() method should be called to enable further growth.
        /// </summary>
        public bool LastLayerFull()
        {
            if (Radius == 0) return true;

            int lastLayerIdx = Radius - 1;
            int lastLayerCellCount = layersCellCount[lastLayerIdx];
            bool result = lastLayerCellCount == GetLayerMaxCapacity(lastLayerIdx);
            return result;
        }

        /// <summary>
        /// Calls the method LastLayerFull() and returns its value.
        /// </summary>
        public bool ShouldExpand()
        {
            return LastLayerFull();
        }

        /// <summary>
        /// Retrieves the information, whether the most outer layer of this CubeStructure is empty, i.e. all of the cells are null. If this method returns true, then Shrink() method can be safely called to make CubeStructure smaller and preserve memory.
        /// </summary>
        public bool LastLayerEmpty()
        {
            if (Radius == 0) return false;

            int lastLayerIdx = Radius - 1;
            int lastLayerCellCount = layersCellCount[lastLayerIdx];
            bool result = lastLayerCellCount == 0;
            return result;
        }

        /// <summary>
        /// Calls the method LastLayerEmpty() and returns its value.
        /// </summary>
        public bool CanShrink()
        {
            return LastLayerEmpty();
        }

        /// <summary>
        /// Expands cube by 1 in every direction. Radius is incremented and new CubeCells can be set on coords 1 farther from center. Initial value of every new cell is null.
        /// </summary>
        /// <returns>New incremented value of Radius.</returns>
        public int Expand()
        {
            if (!ShouldExpand() && UnwantedSizeChangeBehaviour != SizeChangeBehaviour.Ignore)
            {
                string message = "Expanded CubeStructure when the most outer layer is not full.";
                switch (UnwantedSizeChangeBehaviour)
                {
                    case SizeChangeBehaviour.Warning:
                        Debug.LogWarning(message);
                        break;
                    case SizeChangeBehaviour.Error:
                        Debug.LogError(message);
                        break;
                    case SizeChangeBehaviour.Exception:
                        throw new InvalidOperationException(message);
                }
            }

            if (Radius > 0)
            {
                int newRadius = Radius + 1;
                cells.AddForward(CreateCellYZSurface(newRadius));
                cells.AddBackward(CreateCellYZSurface(newRadius));

                for (int x = -Radius + 1; x < Radius; x++)
                {
                    var xCells = cells[x];
                    xCells.AddForward(CreateCellZLine(newRadius));
                    xCells.AddBackward(CreateCellZLine(newRadius));

                    for (int y = -Radius + 1; y < Radius; y++)
                    {
                        var xyCells = xCells[y];
                        xyCells.AddForward(CreateCellNull(newRadius));
                        xyCells.AddBackward(CreateCellNull(newRadius));
                    }
                }

                Radius = newRadius;
            }
            else
            {
                Radius++;

                cells.AddForward(new TwoWayList<TwoWayList<CubeCell>>());
                cells[0].AddForward(new TwoWayList<CubeCell>());
                cells[0][0].AddForward(null);
            }
            layersCellCount.Add(0);
            return Radius;
        }

        /// <summary>
        /// Shrinks the cube by 1 in every direction. Radius is decremented and any CubeCells on the borders are lost. Cell values can be now set only up to coords 1 closer to center than before.
        /// </summary>
        /// <returns>New decremented value of Radius.</returns>
        public int Shrink()
        {
            if (!CanShrink() && UnwantedSizeChangeBehaviour != SizeChangeBehaviour.Ignore)
            {
                string message = "Shrinked CubeStructure when the most outer layer is not empty.";
                switch (UnwantedSizeChangeBehaviour)
                {
                    case SizeChangeBehaviour.Warning:
                        Debug.LogWarning(message);
                        break;
                    case SizeChangeBehaviour.Error:
                        Debug.LogError(message);
                        break;
                    case SizeChangeBehaviour.Exception:
                        throw new InvalidOperationException(message);
                }
            }

            if (Radius > 1)
            {
                Radius--;

                for (int x = -Radius + 1; x < Radius; x++)
                {
                    var xCells = cells[x];
                    for (int y = -Radius + 1; y < Radius; y++)
                    {
                        var xyCells = xCells[y];
                        xyCells.RemoveForward();
                        xyCells.RemoveBackward();
                    }
                    xCells.RemoveForward();
                    xCells.RemoveBackward();
                }
                cells.RemoveForward();
                cells.RemoveBackward();

                layersCellCount.RemoveAt(layersCellCount.Count - 1);
                return Radius;
            }
            else if (Radius == 1)
            {
                Radius--;

                cells[0][0].RemoveForward();
                cells[0].RemoveForward();
                cells.RemoveForward();

                layersCellCount.RemoveAt(0);
            }
            return 0;
        }

        private void UpdateSides(int x, int y, int z)
        {
            if (IsOutOfRadius(x, y, z)) return;

            CubeCell cell = this[x, y, z];
            if (cell == null) return;

            bool rightSide = TryGetCell(x + 1, y, z) == null;
            bool leftSide = TryGetCell(x - 1, y, z) == null;
            bool upSide = TryGetCell(x, y + 1, z) == null;
            bool downSide = TryGetCell(x, y - 1, z) == null;
            bool frontSide = TryGetCell(x, y, z + 1) == null;
            bool backSide = TryGetCell(x, y, z - 1) == null;

            cell.TurnSideOnOff(CubeCellPlaneSide.Right, rightSide);
            cell.TurnSideOnOff(CubeCellPlaneSide.Left, leftSide);
            cell.TurnSideOnOff(CubeCellPlaneSide.Up, upSide);
            cell.TurnSideOnOff(CubeCellPlaneSide.Down, downSide);
            cell.TurnSideOnOff(CubeCellPlaneSide.Front, frontSide);
            cell.TurnSideOnOff(CubeCellPlaneSide.Back, backSide);
        }

        private void UpdateNeighboursSides(int x, int y, int z)
        {
            if (IsOutOfRadius(x, y, z)) return;

            CubeCell cell = this[x, y, z];
            bool hasSide = cell == null;

            var rightCell = TryGetCell(x + 1, y, z);
            var leftCell = TryGetCell(x - 1, y, z);
            var upCell = TryGetCell(x, y + 1, z);
            var downCell = TryGetCell(x, y - 1, z);
            var frontCell = TryGetCell(x, y, z + 1);
            var backCell = TryGetCell(x, y, z - 1);

            if (rightCell != null) rightCell.TurnSideOnOff(CubeCellPlaneSide.Left, hasSide);
            if (leftCell != null) leftCell.TurnSideOnOff(CubeCellPlaneSide.Right, hasSide);
            if (upCell != null) upCell.TurnSideOnOff(CubeCellPlaneSide.Down, hasSide);
            if (downCell != null) downCell.TurnSideOnOff(CubeCellPlaneSide.Up, hasSide);
            if (frontCell != null) frontCell.TurnSideOnOff(CubeCellPlaneSide.Back, hasSide);
            if (backCell != null) backCell.TurnSideOnOff(CubeCellPlaneSide.Front, hasSide);
        }

        private void UpdateLastLayersSides()
        {
            for (int x = -Radius + 1; x < Radius; x++)
            {
                for (int y = -Radius + 1; y < Radius; y++)
                {
                    UpdateSides(x, y, -Radius + 1);
                    UpdateSides(x, y, Radius - 1);
                }
            }
            for (int y = -Radius + 1; y < Radius; y++)
            {
                for (int z = -Radius + 2; z < Radius - 1; z++)
                {
                    UpdateSides(-Radius + 1, y, z);
                    UpdateSides(Radius - 1, y, z);
                }
            }
            for (int z = -Radius + 2; z < Radius - 1; z++)
            {
                for (int x = -Radius + 2; x < Radius - 1; x++)
                {
                    UpdateSides(x, -Radius + 1, z);
                    UpdateSides(x, Radius - 1, z);
                }
            }
        }

        private void ForEveryCell(Action<int, int, int> action)
        {
            for (int x = -Radius + 1; x < Radius; x++)
            {
                for (int y = -Radius + 1; y < Radius; y++)
                {
                    for (int z = -Radius + 1; z < Radius; z++)
                    {
                        action(x, y, z);
                    }
                }
            }
        }

        private bool IsOutOfRadius(int x, int y, int z)
        {
            return
                Math.Abs(x) >= Radius ||
                Math.Abs(y) >= Radius ||
                Math.Abs(z) >= Radius;
        }

        private static CubeCell CreateCellNull(int radius) { return null; }

        private static TwoWayList<CubeCell> CreateCellZLine(int radius)
        {
            return MakeCoordList<CubeCell>(radius, CreateCellNull);
        }

        private static TwoWayList<TwoWayList<CubeCell>> CreateCellYZSurface(int radius)
        {
            return MakeCoordList<TwoWayList<CubeCell>>(radius, CreateCellZLine);
        }

        private static TwoWayList<TwoWayList<TwoWayList<CubeCell>>> CreateCellXYZCube(int radius)
        {
            return MakeCoordList<TwoWayList<TwoWayList<CubeCell>>>(radius, CreateCellYZSurface);
        }

        private static TwoWayList<T> MakeCoordList<T>(int radius, Func<int, T> createItem)
            where T : class
        {
            var list = new TwoWayList<T>();
            FillCoordList(list, radius, createItem);
            return list;
        }

        private static void FillCoordList<T>(TwoWayList<T> coordList, int radius, Func<int, T> createItem)
            where T : class
        {
            if (radius > 0)
            {
                coordList.AddForward(createItem(radius));
                for (int coord = 1; coord < radius; coord++)
                {
                    coordList.AddForward(createItem(radius));
                    coordList.AddBackward(createItem(radius));
                }
            }
        }

        private static int GetLayerMaxCapacity(int layerIndex)
        {
            int diameter = layerIndex * 2 + 1;
            int capacity = diameter * diameter * diameter;
            if (layerIndex == 0) return capacity;

            int prevDiameter = diameter - 2;
            capacity -= prevDiameter * prevDiameter * prevDiameter;
            return capacity;
        }

        public enum SizeChangeBehaviour
        {
            Ignore,
            Warning,
            Error,
            Exception
        }
    }
}
