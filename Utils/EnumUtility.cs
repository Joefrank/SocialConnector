using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialConnector.Utils
{
    public class EnumUtility
    {
        public static List<string> EnumToKeyValueStringList(Type enumType)
        {
            return EnumToKeyValueStringList(enumType, false);
        }
        public static List<string> EnumToKeyValueStringList(Type enumType, bool intAsValue)
        {
            List<string> keyValues = new List<string>();

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            else
            {
                keyValues = Enum.GetNames(enumType).ToList();

                for (int i = keyValues.Count - 1; i >= 0; i--)
                {
                    if (!intAsValue)
                        keyValues.Insert(i+1, keyValues[i]);
                    else
                        keyValues.Insert(i+1, Convert.ToInt32(Enum.Parse(enumType, keyValues[i])).ToString());
                }
            }

            return keyValues;
        }

        public static List<T> EnumToList<T>() where T : struct, IConvertible
        {
            Type enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);

            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        } 
      
    }
}
