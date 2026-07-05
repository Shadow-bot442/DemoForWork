using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    public class FireRule
    {
        public static int fireValue { get { return 100; } }
        public static float fireInterval { get { return 0; } }

        public static bool CanFire_PC(float angle) {
            return angle < 2f;
        } 

        public static bool CanFire_Android(float angle)
        {
            return angle < 10f;
        }
    }
}
