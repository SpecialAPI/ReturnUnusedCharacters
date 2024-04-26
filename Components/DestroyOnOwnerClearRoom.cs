using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Components
{
    public class DestroyOnOwnerClearRoom : MonoBehaviour
    {
        public void Connect(PlayerController o, RoomHandler r)
        {
            own = o;
            room = r;
            if (own != null)
            {
                own.OnRoomClearEvent += DestroyOnRoomClear;
            }
        }

        public void Update()
        {
            if(own != null && (own.CurrentRoom != room || !own.IsInCombat))
            {
                Destroy(gameObject);
            }
        }

        public void DestroyOnRoomClear(PlayerController p)
        {
            Destroy(gameObject);
        }

        public void OnDestroy()
        {
            if(own != null)
            {
                own.OnRoomClearEvent -= DestroyOnRoomClear;
            }
        }

        public PlayerController own;
        public RoomHandler room;
    }
}
