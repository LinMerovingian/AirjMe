using System;

namespace AirjME
{
    public class GameObjectManager
    {
        public WowProcess process;
        public GameObjectManager(WowProcess process)
        {
            this.process = process;
        }



        public GameObject IterateObject(Func<GameObject, bool> checkFunction)
        {
            var objectManagerAddress = process.Read<IntPtr>(process.BaseAddress + process.GetOffset("objectManager"));
            var obj = process.Read<IntPtr>(objectManagerAddress + 0x18);
            var offset = process.Read<int>(objectManagerAddress + 0x08) + 8;

            while (obj.ToInt64() != 0 && (obj.ToInt64() & 1) == 0)
            {
                var gameObject = new GameObject(process, obj);
                if (checkFunction(gameObject))
                {
                    return gameObject;
                }

                obj = process.Read<IntPtr>(obj + offset);
            }

            return null;
        }

        public GameObject FindObjectByGuid(Guid guid)
        {
            return IterateObject(obj =>
            {
                if ((obj.GetObjectFlag() & 0x40) != 0)
                {
                    return guid == obj.GetGuid();
                }

                return false;
            });
        }


    }
}