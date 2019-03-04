using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace No2DarkBlue
{
    internal class Util
    {
        internal class ObjectUtil
        {


            public static Type GetType(string typeName)
            {
                var type = Type.GetType(typeName);
                if (type != null) return type;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(typeName);
                    if (type != null)
                        return type;
                }
                return null;
            }


            /// <summary>
            /// 轉成可以擴充的物件
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static dynamic ConvertToDynamic(object obj)
            {
                
                return JObject.Parse(JsonConvert.SerializeObject(obj));
            }
        }
    }
}
