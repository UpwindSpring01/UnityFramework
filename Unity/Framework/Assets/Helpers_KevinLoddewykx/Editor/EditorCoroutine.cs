using System.Collections;
using UnityEditor;

namespace Helpers_KevinLoddewykx
{
    public class EditorCoroutine
        {
            public static EditorCoroutine Start(IEnumerator routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(routine);
                coroutine.Start();

                return coroutine;
            }

            private readonly IEnumerator _routine;

            EditorCoroutine(IEnumerator routine)
            {
                _routine = routine;

            }

            private void Start()
            {
                EditorApplication.update += Update;
            }

            public void Stop()
            {
                EditorApplication.update -= Update;
            }

            void Update()
            {
                if (!_routine.MoveNext())
                {
                    Stop();
                }
            }
        }
    }
