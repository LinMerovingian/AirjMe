using System;
using System.Collections.Generic;
using System.Linq;

namespace AirjME
{
    public class MoveExecutor
    {

        private IntPtr _scriptData = IntPtr.Zero;
        private WowProcess process;
        public int moveFlag = 0;

        private Node LastNode = null;

        public MoveExecutor(WowProcess process)
        {
            this.process = process;
        }


        private const float PI = MathF.PI;

        private float ModYaw(float yaw)
        {
            if (yaw < -PI)
            {
                yaw += 2 * PI;
            }
            else if (yaw > PI)
            {
                yaw -= 2 * PI;
            }

            return yaw;
        }

        public int GetMoveFlag(float currentYaw, float moveYaw, float faceYaw)
        {
            var faceOffset = ModYaw(faceYaw - moveYaw);
            var absFaceOffset = MathF.Abs(faceOffset);
            var targetOffset = 0f;

            if (absFaceOffset > 3 / 8f * PI)
            {
                targetOffset = 0.5f * PI;
            }
            else if (absFaceOffset > 1 / 8f * PI)
            {
                targetOffset = 0.25f * PI;
            }
            else
            {
                targetOffset = 0;
            }

            if (faceOffset < 0)
            {
                targetOffset = -targetOffset;
            }

            int result = 0;
            var currentOffset = ModYaw(currentYaw - moveYaw);
            if (MathF.Abs(currentOffset - targetOffset) > 0.02 * PI)
            {
                if (currentOffset > targetOffset)
                {
                    result |= 0x200;
                }
                else
                {
                    result |= 0x100;
                }
            }

            if (currentOffset > 7 / 8f * PI)
            {
                result |= 0x20;
            }
            else if (currentOffset > 6 / 8f * PI)
            {
                result |= 0xA0;
            }
            else if (currentOffset > 3 / 8f * PI)
            {
                result |= 0x80;
            }
            else if (currentOffset > 1 / 8f * PI)
            {
                result |= 0x90;
            }
            else if (currentOffset > -1 / 8f * PI)
            {
                result |= 0x10;
            }
            else if (currentOffset > -3 / 8f * PI)
            {
                result |= 0x50;
            }
            else if (currentOffset > -6 / 8f * PI)
            {
                result |= 0x40;
            }
            else if (currentOffset > -7 / 8f * PI)
            {
                result |= 0x60;
            }
            else
            {
                result |= 0x20;
            }

            return result;
        }

        public int GetMoveFlagForStopMoving(float currentYaw, float faceYaw)
        {
            int result = 0;
            var offset = ModYaw(currentYaw - faceYaw);
            if (MathF.Abs(offset) > 0.02 * PI)
            {
                if (offset > 0)
                {
                    result |= 0x200;
                }
                else
                {
                    result |= 0x100;
                }
            }

            return result;
        }

        public Position GetPositionByNodeList(Position currentPosition, List<Node> nodeList, float minLimitDistance = 2, float maxLimitDistance = 100f)
        {
            if (!(LastNode is null))
            {
                var distance = currentPosition.DistanceTo2d(LastNode.Position);
                if (distance >= minLimitDistance && distance < maxLimitDistance)
                {
                    return LastNode.Position;
                }
            }

            var minNodeDistance = nodeList.Min(node => currentPosition.DistanceTo2d(node.Position));
            if (minNodeDistance > maxLimitDistance)
            {
                return currentPosition;
            }

            var minNode = nodeList.First(node => Math.Abs(currentPosition.DistanceTo2d(node.Position) - minNodeDistance) < 1e-6);
            var node = minNode;
            while (true)
            {
                if (currentPosition.DistanceTo2d(node.Position) >= minLimitDistance)
                {
                    break;
                    ;
                }

                if (node.ConnectTo.Count == 0)
                {
                    break;
                    ;
                }

                node = node.ConnectTo[0];
                if (node == minNode)
                {
                    break;
                }
            }

            LastNode = node;

            return node.Position;
        }

        public void Move(int flag)
        {
            //TODO call update function directly
            for (int i = 0; i < 16; i++)
            {
                // var offset = ((1 << i) & flag) == 0 ? stopOffsets[i] : startOffsets[i];
                var offsetName = (((1 << i) & flag) == 0 ? "moveStop_" : "moveStart_") + i;
                var offset = process.GetOffset(offsetName);
                if (offset != 0)
                {
                    process.CallExecutor.CallFunction(offset);
                }
            }

            moveFlag = flag;
        }
    }
}