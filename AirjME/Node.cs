using System.Collections.Generic;

namespace AirjME
{
    public class Node
    {
        public Position Position { get; set; }
        public List<Node> ConnectTo { get; set; }

        public Node(Position position, Node[] connectTo = null)
        {
            this.Position = position;
            this.ConnectTo = connectTo is null ? new List<Node>() : new List<Node>(connectTo);
        }
    }
}