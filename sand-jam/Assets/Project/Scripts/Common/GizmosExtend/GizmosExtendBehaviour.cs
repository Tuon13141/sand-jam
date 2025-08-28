using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kit
{
    public class GizmosExtendBehaviour : MonoBehaviour
    {
        static GizmosExtendBehaviour _instance;
        public static GizmosExtendBehaviour instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("GizmosExtendBehaviour", typeof(GizmosExtendBehaviour)).GetComponent<GizmosExtendBehaviour>();
                }

                return _instance;
            }
        }

        public delegate void DragCallback();

        private void Awake()
        {
            draw = new();
            DontDestroyOnLoad(gameObject);
        }

        public Dictionary<int, DragCallback> draw { get; set; }

        private void OnDrawGizmos()
        {
            foreach (var element in draw)
            {
                element.Value.Invoke();
            }
        }

        public void DrawSquare(int id, Vector3 center, Vector3 direction, Vector2 size, float angle, Color color)
        {
            if (draw.ContainsKey(id))
            {
                draw[id] = () => GizmosExtend.DrawSquare(center, direction, size, angle, color);
            }
            else
            {
                draw.Add(id, () => GizmosExtend.DrawSquare(center, direction, size, angle, color));
            }
        }

        public void DrawLable(int id, Vector3 position, string text, Color color = default(Color))
        {
            if (draw.ContainsKey(id))
            {
                draw[id] = () => GizmosExtend.DrawLabel(position, text, color: color);
            }
            else
            {
                draw.Add(id, () => GizmosExtend.DrawLabel(position, text, color: color));
            }
        }

        public void DrawPoint(int id, Vector3 position, Color color = default)
        {
            if (draw.ContainsKey(id))
            {
                draw[id] = () => GizmosExtend.DrawPoint(position, color: color);
            }
            else
            {
                draw.Add(id, () => GizmosExtend.DrawPoint(position, color: color));
            }
        }

        [ContextMenu("CleanDraw")]
        void CleanDraw()
        {
            draw.Clear();
        }
    }
}