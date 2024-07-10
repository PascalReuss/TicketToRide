using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Assets.Scripts.Utility
{
    /**
     * Klasse zum Parsen der C#-Objekte in Json und anderherum
     */
    public static class JSONParser<T> where T : new()
    {
        /**
         * Diese Methode serialisiert ein gegebenenes Objekt und gibt dieses im Json-Format als String zur�ck
         */
        public static string SerializeObject(T obj)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);

            byte[] json = ms.ToArray();
            ms.Close();

            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        /**
         * Diese Methode deserialisiert einen gegebenenen String im Json-Format und liefert ein C#-Objekt der Klasse T zur�ck
         */
        public static T DeserializeObject(string json)
        {
            try
            {
                T val = new T();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(val.GetType());

                val = (T)ser.ReadObject(ms);
                ms.Close();

                return val;
            }
            catch (SerializationException e)
            {
                T val = new T();
                UnityEngine.Debug.Log(e);
                return val;
            }
        }
    }

}
