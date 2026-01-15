// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Rukw0DiQcaTECn+MAx8/gP6Bj6E+WHDxaEA2RNuI7X2Z+RnjV67hpYpCM4r2T5+0V5njSIlY8GbzI+3Ek6YbQdnrEBr/QwuvXBI8CzIeOrYfA8asaq1aXKYYYWtKvDe4+M29fN6efi5AjLYigZwFz2GXHMrPmu7dGpmXmKgamZKaGpmZmDI/nL8aqu+oGpm6qJWekbIe0B5vlZmZmZ2Ym+pC2+dm5/jQoD4x9f+MvJ+CE6+c2qkiUGz+/sQCoofhebJ8OCpACNVyRPgjOwzYpwnJZboBxhog0hBCXhZkTO0491932fuidYhgc5I4yNp/6Kvs8uyVdb71BYQqUbRowM4WUv2B9CT+DR6suRI161VjXjoHhfBlScx0M/94YpTvYZqbmZiZ");
        private static int[] order = new int[] { 4,2,12,3,8,6,12,8,8,13,11,13,12,13,14 };
        private static int key = 152;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
