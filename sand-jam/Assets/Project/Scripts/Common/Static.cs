using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using Unity.Mathematics;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Static
{
    public static float sin45 = 1 / Mathf.Sin(45);
    public static int curentLevel = 0;
    public static int countReplay = 0;
    public static float offsetZ = 5f;
    public static bool firstOpen = true;
    public static int[] countTrayValues = { 4, 6, 10 };
    public static bool testMode = false;
    public static Color defaultColor1 = new Color(1f, 0.9176471f, 0.6745098f, 1f);
    public static Color defaultColor2 = new Color(0.7450981f, 0.6f, 0.3607843f, 1f);
    public static Dictionary<string, string> levelsUpdateDic;

    public static Bounds RotateBounds(this Bounds bounds, Quaternion rotation, Vector3 pivot)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        // The vertices of the original Bounds
        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        // Rotate vertices around pivot
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = rotation * (corners[i] - pivot) + pivot;
        }

        // Create new bounds from rotated points
        Bounds rotatedBounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
        {
            rotatedBounds.Encapsulate(corners[i]);
        }

        return rotatedBounds;
    }

    public static float4 ToFloat4(this Color color)
    {
        return new float4(color.r, color.g, color.b, color.a);
    }

    public static Color ToColor(this float4 float4)
    {
        return new Color(float4.x, float4.y, float4.z, float4.w);
    }

    public static Vector3 ToVector3(this Vector2 v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Color ParseHtmlString(string text)
    {
        Color color = Color.white;

        ColorUtility.TryParseHtmlString(text, out color);

        return color;
    }

    public static bool Intersects2D(Bounds a, Bounds b, float epsilon = 0.0001f)
    {
        return a.min.x <= b.max.x + epsilon && a.max.x >= b.min.x - epsilon &&
               a.min.y <= b.max.y + epsilon && a.max.y >= b.min.y - epsilon;
    }
    public static bool Approximately(float a, float b, float epsilon = 0.0001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }

    public static int GetUnixTime()
    {
        return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static Rect CreateRect(Vector2 center, Vector2 size)
    {
        Vector2 position = center - (size * 0.5f);
        return new Rect(position, size);
    }

    public static string SecondsToTextFormat(int seconds)
    {
        int minute = seconds / 60;
        int second = seconds % 60;

        return string.Format("{0, 0:D2}:{1, 0:D2}", minute, second);
    }

    public static void DestroyAllParticles(this ParticleSystem ps, Transform root, int maxCount)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(particles);

        int count = ps.particleCount;
        if (count <= maxCount) return;

        var temp = particles.ToList();
        temp = temp.OrderBy(x => Vector3.Distance(root.position, x.position)).Take(maxCount).ToList();

        ps.SetParticles(temp.ToArray());

    }
    public static T InstantiateUtility<T>(T prefab, Transform parent) where T : MonoBehaviour
    {
#if UNITY_EDITOR
        var elementPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
        var element = elementPrefab.GetComponent<T>();
#else
        var element = GameObject.Instantiate(prefab, parent);
#endif

        return element;
    }

    #region Deep_Equals
    public static bool DeepEquals(this object obj, object another)
    {
        if (ReferenceEquals(obj, another)) return true;
        if ((obj == null) && (another == null)) return true;

        if ((obj == null) || (another == null)) return false;
        if (obj.GetType() != another.GetType()) return false;

        return obj.Equals(another);
        //If the property is not a class, it is just int, double, DateTime etc.
        //Call the equal function normally
        //if (!obj.GetType().IsClass) return obj.Equals(another);

        //var result = true;
        //foreach (var property in obj.GetType().GetProperties())
        //{
        //    var objValue = property.GetValue(obj);
        //    var anotherValue = property.GetValue(another);
        //    //Continue recursion
        //    if (!objValue.DeepEquals(anotherValue)) result = false;
        //}
        //return result;
    }

    public static bool DeepEquals<T>(this IEnumerable<T> obj, IEnumerable<T> another)
    {
        if (ReferenceEquals(obj, another)) return true;
        if ((obj == null) && (another == null)) return true;
        if ((obj == null) || (another == null)) return false;

        bool result = true;
        //Iterate through each element in the two input lists.
        using (IEnumerator<T> enumerator1 = obj.GetEnumerator())
        using (IEnumerator<T> enumerator2 = another.GetEnumerator())
        {
            while (true)
            {
                bool hasNext1 = enumerator1.MoveNext();
                bool hasNext2 = enumerator2.MoveNext();

                //If there is an empty list, or 2 different elements, exit the loop
                if (hasNext1 != hasNext2 || !enumerator1.Current.DeepEquals(enumerator2.Current))
                {
                    result = false;
                    break;
                }

                //Stop the loop when both lists are exhausted
                if (!hasNext1) break;
            }
        }

        return result;
    }
    #endregion

    #region Transform_Extension
    public static void ClearContent(this Transform rt, int index = 0)
    {
        var tempArray = new GameObject[rt.childCount];

        for (int i = index; i < tempArray.Length; i++)
        {
            tempArray[i] = rt.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            if (child != null)
            {
                GameObject.DestroyImmediate(child);
            }
        }
    }

    public static void SetLossyScale(this Transform t, Vector3 lossyScale)
    {
        Transform parent = t.parent;
        if (parent != null)
        {
            Vector3 scaleFactor = parent.lossyScale;

            //Determine what the new scale local scale should be
            Vector3 newLocalScale = new Vector3(
                                    lossyScale.x / scaleFactor.x,
                                    lossyScale.y / scaleFactor.y,
                                    lossyScale.z / scaleFactor.z
                                    );

            t.localScale = newLocalScale;
        }
        else
        {
            t.localScale = lossyScale;
        }
    }

    public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        var position = transform.position;

        if (x.HasValue)
        {
            position.x = x.Value;
        }

        if (y.HasValue)
        {
            position.y = y.Value;
        }

        if (z.HasValue)
        {
            position.z = z.Value;
        }

        transform.position = position;
    }

    public static void SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        var localPosition = transform.localPosition;

        if (x.HasValue)
        {
            localPosition.x = x.Value;
        }

        if (y.HasValue)
        {
            localPosition.y = y.Value;
        }

        if (z.HasValue)
        {
            localPosition.z = z.Value;
        }

        transform.localPosition = localPosition;
    }

    public static void SetEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        var eulerAngles = transform.eulerAngles;

        if (x.HasValue)
        {
            eulerAngles.x = x.Value;
        }

        if (y.HasValue)
        {
            eulerAngles.y = y.Value;
        }

        if (z.HasValue)
        {
            eulerAngles.z = z.Value;
        }

        transform.eulerAngles = eulerAngles;
    }

    public static void SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        var localEulerAngles = transform.localEulerAngles;

        if (x.HasValue)
        {
            localEulerAngles.x = x.Value;
        }

        if (y.HasValue)
        {
            localEulerAngles.y = y.Value;
        }

        if (z.HasValue)
        {
            localEulerAngles.z = z.Value;
        }

        transform.localEulerAngles = localEulerAngles;
    }

    public static Vector3 SetVector3(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {

        if (x.HasValue)
        {
            v.x = x.Value;
        }

        if (y.HasValue)
        {
            v.y = y.Value;
        }

        if (z.HasValue)
        {
            v.z = z.Value;
        }

        return v;
    }

    #endregion

    #region RectTransform_Extension
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        List<GameObject> objectList = new List<GameObject>();
        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                objectList.Add(results[i].gameObject);
                //if (results[i].gameObject.GetComponentInParent<IgnoneCanvas>() == null)
                //{
                //    objectList.Add(results[i].gameObject);
                //}
            }
        }

        return objectList.Count > 0;
    }

    public static void SetPos(this RectTransform rectTransform, Vector3 target, Camera camera)
    {
        if (rectTransform && target != Vector3.zero)
        {
            Vector3 pos = camera.WorldToScreenPoint(target);
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            rectTransform.anchoredPosition = new Vector2(pos.x - (Screen.width / 2), pos.y - (Screen.height / 2)) / canvas.scaleFactor;
        }
    }

    public static Vector3 GetPos(this RectTransform rectTransform, Vector3 target, Camera camera)
    {
        if (rectTransform && target != Vector3.zero)
        {
            Vector3 pos = camera.WorldToScreenPoint(target);
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return rectTransform.anchoredPosition3D;

            var anchoredPosition = new Vector2(pos.x - (Screen.width / 2), pos.y - (Screen.height / 2)) / canvas.scaleFactor;

            return anchoredPosition.ToVector3(rectTransform.anchoredPosition3D.z);
        }

        return rectTransform.anchoredPosition3D;
    }
    #endregion

    #region List_Extension
    public static List<T> GetRandomElements<T>(List<T> list, int count)
    {
        if (list == null || list.Count == 0 || count <= 0)
        {
            return new List<T>();
        }

        if (list.Count < count)
        {
            return new List<T>(list);
        }

        List<T> randomElements = new List<T>();
        List<T> copyList = new List<T>(list);

        for (int i = 0; i < count; i++)
        {
            if (copyList.Count == 0)
            {
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, copyList.Count);
            randomElements.Add(copyList[randomIndex]);
            copyList.RemoveAt(randomIndex);
        }

        return randomElements;
    }

    public static List<T> RemoveSublistElements<T>(List<T> mainList, List<T> subList)
    {
        // Tạo bản sao của mainList để giữ danh sách gốc
        List<T> filteredList = new List<T>(mainList);

        // Lặp qua từng phần tử trong subList và xóa một phần tử tương ứng trong mainList
        foreach (var element in subList)
        {
            if (filteredList.Contains(element))
            {
                filteredList.Remove(element); // Xóa 1 phần tử đầu tiên trùng khớp
            }
        }

        return filteredList;
    }

    public static List<T> ShuffleWithIntensityAndRange<T>(List<T> list, float intensity, int rangeShuff)
    {
        var tempList = new List<T>();
        var count = Mathf.CeilToInt((float)list.Count / rangeShuff);
        //Debug.LogFormat("{0} - {1}", list.Count, count);

        for (int i = 0; i < count; i++)
        {
            var sIndex = i * rangeShuff;
            var range = list.GetRange(sIndex, ((i + 1) * rangeShuff <= list.Count) ? rangeShuff : (list.Count % rangeShuff));
            tempList.AddRange(ShuffleWithIntensity(range, intensity));
        }

        return tempList;
    }

    public static List<T> ShuffleWithIntensity<T>(List<T> list, float intensity)
    {
        // Apply Fisher-Yates shuffle with intensity control
        var tempList = new List<T>(list);
        int n = tempList.Count;

        for (int i = 0; i < n - 1; i++)
        {
            int j = Mathf.Min(i + Mathf.FloorToInt(UnityEngine.Random.Range(0f, 1f) * intensity * (n - i)), n - 1);
            (tempList[i], tempList[j]) = (tempList[j], tempList[i]);
        }

        return tempList;
    }

    public static (T element, int index) GetRandomWithProbability<T>(this IEnumerable<T> enumerable, IEnumerable<float> probabilities, float? probabilitiesSum = null)
    {
        var count = enumerable.Count();

        if (probabilities.Count() != count)
            throw new System.ArgumentException($"Count of probabilities and enumerble elements must be equal.");

        if (count == 0)
            throw new System.ArgumentException($"Enumerable count must be greater than zero");

        if (probabilitiesSum == null)
        {
            probabilitiesSum = 0f;

            foreach (var element in probabilities)
                probabilitiesSum += element;
        }

        var randomValue = UnityEngine.Random.value * probabilitiesSum.Value;

        var sum = 0f;
        var index = -1;

        var enumerator = probabilities.GetEnumerator();

        while (enumerator.MoveNext())
        {
            index += 1;
            var probability = enumerator.Current;

            sum += probability;

            if (randomValue < sum || Mathf.Approximately(randomValue, sum))
                return (enumerable.ElementAt(index), index);
        }

        index = count - 1;
        return (enumerable.ElementAt(index), index);
    }

    #endregion

    public static async UniTask MoveCurved(Transform element, Vector3 targetPos, Vector3 startPos, float duration, Vector3 offsetCurved = default(Vector3), Vector3 offsetTaget = default(Vector3))
    {
        if (testMode) duration = 0;
        Vector3 rootScale = element.localScale;
        Vector3 pT = (startPos + targetPos + offsetTaget) / 2f + offsetCurved;

        await DOVirtual.Float(0f, 1f, duration, angle =>
        {
            Vector3 m1 = Vector3.Lerp(startPos, pT, angle);
            Vector3 m2 = Vector3.Lerp(pT, targetPos + offsetTaget, angle);

            element.position = Vector3.Lerp(m1, m2, angle);

            //scale
            //float t = 0.5f - Mathf.Abs(0.5f - angle);
            //element.localScale = rootScale * (1 + t * 2f);

            //rotation


        })
         .SetEase(Ease.OutCubic)
         .AsyncWaitForCompletion();
    }

    public static async UniTask MoveCurved(Transform element, Transform target, Vector3 startPos, float duration, Vector3 offsetCurved = default(Vector3), Vector3 offsetTaget = default(Vector3))
    {
        if (testMode) duration = 0;
        Vector3 rootScale = element.localScale;
        Vector3 pT = (startPos + target.position + offsetTaget) / 2f + offsetCurved;

        await DOVirtual.Float(0f, 1f, duration, angle =>
        {
            Vector3 m1 = Vector3.Lerp(startPos, pT, angle);
            Vector3 m2 = Vector3.Lerp(pT, target.position + offsetTaget, angle);

            element.position = Vector3.Lerp(m1, m2, angle);

            //scale
            //float t = 0.5f - Mathf.Abs(0.5f - angle);
            //element.localScale = rootScale * (1 + t * 2f);

            //rotation


        })
        .SetEase(Ease.OutQuart)
        .AsyncWaitForCompletion();
    }

    public static async UniTask MoveCurvedRotate(Transform element, Vector3 targetPos, Vector3 startPos, float duration, Vector3 offsetRotate = default(Vector3), Vector3 offsetCurved = default(Vector3), Vector3 offsetTaget = default(Vector3))
    {
        if (testMode) duration = 0;
        Vector3 rootScale = element.localScale;
        Vector3 pT = (startPos + targetPos + offsetTaget) / 2f + offsetCurved;
        Quaternion rT = Quaternion.Inverse(Quaternion.LookRotation((targetPos + offsetTaget) - pT, Vector3.forward));

        await DOVirtual.Float(0f, 1f, duration, angle =>
        {
            Vector3 m1 = Vector3.Lerp(startPos, pT, angle);
            Vector3 m2 = Vector3.Lerp(pT, targetPos + offsetTaget, angle);

            element.position = Vector3.Lerp(m1, m2, angle);

            //scale
            float t = 0.5f - Mathf.Abs(0.5f - angle);
            element.localScale = rootScale * (1 + t * 1f);

            //rotation
            element.rotation = Quaternion.LookRotation(m2 - m1, Vector3.forward) * rT * Quaternion.Euler(offsetRotate);
        })
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion();
    }

    public static int GetRandomInRange(this Vector2Int vector2)
    {
        return UnityEngine.Random.Range(vector2.x, vector2.y + 1);
    }

    public static string DecompressString(string compressedStr)
    {
        var compressedBytes = System.Convert.FromBase64String(compressedStr);
        using (var input = new System.IO.MemoryStream(compressedBytes))
        using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
        using (var reader = new System.IO.StreamReader(gzip, System.Text.Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }

    public static string CompressString(string str)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);
        using var output = new System.IO.MemoryStream();
        using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return System.Convert.ToBase64String(output.ToArray());
    }
}

