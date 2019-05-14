﻿using Helion.BSP.Geometry;
using Helion.BSP.States.Convex;
using Helion.Util;
using Helion.Util.Geometry;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Helion.Util.Assert;

namespace Helion.BSP.Node
{
    public class SubsectorEdge : Seg2DBase
    {
        public const int NoSectorId = -1;

        public readonly int LineId;
        public readonly Optional<int> SectorId = Optional.Empty;

        public bool IsMiniseg => LineId == BspSegment.MinisegLineId;

        public SubsectorEdge(Vec2D start, Vec2D end) : this(start, end, BspSegment.MinisegLineId, NoSectorId)
        {
        }

        public SubsectorEdge(Vec2D start, Vec2D end, int lineId, int sectorId) : base(start, end)
        {
            LineId = lineId;

            if (sectorId != NoSectorId)
                SectorId = sectorId;
        }

        private static Rotation CalculateRotation(ConvexTraversal convexTraversal)
        {
            List<ConvexTraversalPoint> traversal = convexTraversal.Traversal;
            ConvexTraversalPoint firstTraversal = traversal.First();

            Vec2D first = firstTraversal.Segment[firstTraversal.Endpoint];
            Vec2D second = firstTraversal.Segment.Opposite(firstTraversal.Endpoint);

            // We're essentially doing a sliding window of 3 vertices here, and keep
            // checking each corner in the convex polygon to see which way it rotates.
            for (int i = 1; i < traversal.Count; i++)
            {
                ConvexTraversalPoint traversalPoint = traversal[i];

                // Because the endpoint is referring to the starting point for each
                // segment, we need to get the ending point since the start point
                // is already set in `second`, and `first` is the one before that.
                Vec2D third = traversalPoint.Segment.Opposite(traversalPoint.Endpoint);

                Rotation rotation = Seg2D.Rotation(first, second, third);
                if (rotation != Rotation.On)
                    return rotation;

                first = second;
                second = third;
            }

            Fail("Unable to find rotation for convex traversal");
            return Rotation.On;
        }

        private static int GetSectorIdFrom(BspSegment segment, SectorLine sectorLine, Rotation rotation) {
            if (segment.SameDirection(sectorLine.Delta))
            {
                Precondition(!segment.OneSided || rotation != Rotation.Left, "Trying to get the back sector ID of a one sided line");
                return rotation == Rotation.Right ? sectorLine.FrontSectorId : sectorLine.BackSectorId;
            }
            else
            {
                Precondition(!segment.OneSided || rotation != Rotation.Right, "Trying to get the back sector ID of a one sided line");
                return rotation == Rotation.Right ? sectorLine.BackSectorId : sectorLine.FrontSectorId;
            }
        }

        private static List<SubsectorEdge> CreateSubsectorEdges(ConvexTraversal convexTraversal, IList<SectorLine> lineToSectors, Rotation rotation)
        {
            List<ConvexTraversalPoint> traversal = convexTraversal.Traversal;
            Precondition(traversal.Count >= 3, "Traversal must yield at least a triangle in size");

            List<SubsectorEdge> subsectorEdges = new List<SubsectorEdge>();

            Vec2D startPoint = traversal.First().ToPoint();
            foreach (ConvexTraversalPoint traversalPoint in traversal)
            {
                BspSegment segment = traversalPoint.Segment;
                Vec2D endingPoint = segment.Opposite(traversalPoint.Endpoint);

                if (segment.IsMiniseg)
                    subsectorEdges.Add(new SubsectorEdge(startPoint, endingPoint));
                else
                {
                    Precondition(segment.LineId < lineToSectors.Count, "Segment has bad line ID or line to sectors list is invalid");
                    SectorLine sectorLine = lineToSectors[segment.LineId];
                    int sectorId = GetSectorIdFrom(segment, sectorLine, rotation);
                    subsectorEdges.Add(new SubsectorEdge(startPoint, endingPoint, segment.LineId, sectorId));
                }

                startPoint = endingPoint;
            }

            Postcondition(subsectorEdges.Count == traversal.Count, "Added too many subsector edges in traversal");
            return subsectorEdges;
        }

        private static void ReverseEdges(List<SubsectorEdge> edges)
        {
            List<SubsectorEdge> reversedEdges = new List<SubsectorEdge>();

            for (int i = edges.Count - 1; i >= 0; i++)
            {
                SubsectorEdge edge = edges[i];
                int sectorId = edge.SectorId.ValueOr(NoSectorId);
                reversedEdges.Add(new SubsectorEdge(edge.End, edge.Start, edge.LineId, sectorId));
            }

            edges.Clear();
            edges.AddRange(reversedEdges);
        }

        [Conditional("DEBUG")]
        private static void AssertValidSubsectorEdges(List<SubsectorEdge> edges)
        {
            Precondition(edges.Count >= 3, "Not enough edges");

            int lastCorrectSector = SubsectorEdge.NoSectorId;

            foreach (SubsectorEdge edge in edges) {
                if (edge.SectorId)
                {
                    if (lastCorrectSector == SubsectorEdge.NoSectorId)
                    {
                        lastCorrectSector = edge.SectorId.Value;
                    }
                    else
                    {
                        Precondition(edge.SectorId.Value != lastCorrectSector, "Subsector references multiple sectors");
                    }
                }
            }

            Precondition(lastCorrectSector != SubsectorEdge.NoSectorId, "Unable to find a sector for the subsector");
        }

        public static IList<SubsectorEdge> FromClockwiseConvexTraversal(ConvexTraversal convexTraversal, IList<SectorLine> lineToSectors)
        {
            Rotation rotation = CalculateRotation(convexTraversal);
            List<SubsectorEdge> edges = CreateSubsectorEdges(convexTraversal, lineToSectors, rotation);
            if (rotation != Rotation.Left)
                ReverseEdges(edges);

            AssertValidSubsectorEdges(edges);
            return edges;
        }
    }
}
