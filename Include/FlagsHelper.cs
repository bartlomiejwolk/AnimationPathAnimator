namespace AnimationPathTools {

    /// <summary>
    ///     Helper class for logical bitwise combination setting of Enums with the Flag attribute.
    ///     This allows code as below where Names is an enum containing a number of names.
    /// </summary>
    /// <remarks>https://github.com/Maxii/CodeEnv.Master/blob/master/CodeEnv.Master.Common/ThirdParty/FlagsHelper.cs</remarks>
    public static class FlagsHelper {

        public static bool IsSet<T>(T flags, T flag) where T : struct {
            var flagsValue = (int) (object) flags;
            var flagValue = (int) (object) flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct {
            var flagsValue = (int) (object) flags;
            var flagValue = (int) (object) flag;

            flags = (T) (object) (flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct {
            var flagsValue = (int) (object) flags;
            var flagValue = (int) (object) flag;

            flags = (T) (object) (flagsValue & (~flagValue));
        }

    }

}