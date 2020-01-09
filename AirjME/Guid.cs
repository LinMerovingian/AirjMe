namespace AirjME
{
    public struct Guid
    {
        int data0;
        int data1;
        int data2;
        int data3;

        public static bool operator ==(Guid a, Guid b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Guid a, Guid b)
        {
            return !a.Equals(b);
        }

        public bool IsZero()
        {
            return data0 == 0 && data1 == 0 && data2 == 0 && data3 == 0;
        }
    }
}