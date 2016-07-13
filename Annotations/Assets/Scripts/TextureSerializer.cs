using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;


public class TextureSerializer : MonoBehaviour
{

    //public static

    public static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        //float tempTime = Time.realtimeSinceStartup;

        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        //Debug.Log("Conversion Time: [" + (Time.realtimeSinceStartup - tempTime)*1000 + "ms]\n");
        return bytes;
    }

    public static Color32[] ByteArrayToColor32Array(byte[] bytes, Texture2D text)
    {
        //float tempTime = Time.realtimeSinceStartup;

        if (bytes == null || bytes.Length == 0)
            return null;

        int lengthOfBytes = Marshal.SizeOf(typeof(byte));
        int length = (lengthOfBytes * bytes.Length);
        Color32[] colors = new Color32[length/4];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(bytes, 0, ptr, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }
        
        //Debug.Log("Conversion Time: [" + (Time.realtimeSinceStartup - tempTime) * 1000 + "ms]\n");
        //return colors;
        return colors;

    }
}