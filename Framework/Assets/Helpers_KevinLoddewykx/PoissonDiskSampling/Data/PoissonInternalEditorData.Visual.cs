#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonInternalEditorData
    {
        public void InitVisual(PoissonModeData modeData, PoissonData data, PoissonPlacer placer, bool isWindow, bool refreshTopMaterial)
        {
            if (!isWindow && HelperVisual && refreshTopMaterial)
            {
                // Always create new TopMaterial, because when we copy the placer, we don't want the TopMaterial shared
                TopMaterial = new Material(TopMaterial);
                UpdateVisualMode(modeData);
            }
            if (!HelperVisual)
            {
                CreateBoxMesh();
                CreateCylinderMesh();

                Shader shader = Shader.Find("Poisson/PoissonHelperShader");
                TopMaterial = new Material(shader);
                TopMaterial.SetColor("_OuterColor", new Color(0.0f, 1.0f, 0.0f, 0.8f));
                TopMaterial.SetColor("_InnerColor", new Color(1.0f, 0.0f, 0.0f, 0.8f));
                TopMaterial.hideFlags = HideFlags.HideInInspector;

                FaceMaterial = new Material(shader);
                FaceMaterial.SetColor("_OuterColor", new Color(0.0f, 1.0f, 0.0f, 0.5f));
                FaceMaterial.SetColor("_InnerColor", new Color(1.0f, 0.0f, 0.0f, 0.5f));
                FaceMaterial.hideFlags = HideFlags.HideInInspector;

                GameObject parentObj = new GameObject();
                if (placer)
                {
                    parentObj.transform.parent = placer.transform;
                    parentObj.transform.SetAsFirstSibling();
                    parentObj.transform.parent = parentObj.transform;
                    parentObj.transform.localPosition = Vector3.zero;
                    parentObj.transform.localRotation = Quaternion.identity;
                    parentObj.transform.localScale = Vector3.one;
                }

                HelperVisual = new GameObject();


                HelperVisual.transform.parent = parentObj.transform;

                HelperVisual.gameObject.SetActive(false);

                MeshFilter = HelperVisual.AddComponent<MeshFilter>();

                Renderer = HelperVisual.AddComponent<MeshRenderer>();

                if (!isWindow)
                {
                    DeleteHelper = HelperVisual.AddComponent<PoissonDeleteHelper>();
                }

                RefreshVisual(modeData, data, isWindow);
            }
            if (placer && !isWindow)
            {
                DeleteHelper.Placer = placer;
            }
        }

        private void CreateCylinderMesh()
        {
            CylinderMesh = new Mesh();
            EllipseMesh = new Mesh();

            const int sides = 16;
            const int verticesCount = sides * 2 + sides * 2 + 2;
            const int capIndicesCount = 3 + (sides - 3) * 3;

            Vector3[] vertices = new Vector3[verticesCount * 2];
            Vector3[] normals = new Vector3[verticesCount * 2];
            Vector2[] texCoords = new Vector2[verticesCount * 2];

            int[] indicesTop = new int[capIndicesCount * 2];
            int indicesCountSingleSideBottom = capIndicesCount + 6 * sides;
            int[] indicesSideBotom = new int[indicesCountSingleSideBottom * 2];

            for (int i = 0; i < sides; ++i)
            {
                float offset = i / (float)sides;
                float rad = offset * (Mathf.PI * 2.0f);

                int bottomCap = i + sides;
                int topSide = bottomCap * 2;
                int bottomSide = topSide + 1;

                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);
                float halfCos = cos * 0.5f;
                float halfSin = sin * 0.5f;

                Vector3 top = new Vector3(cos, 0.0f, sin);
                Vector3 halfTop = new Vector3(halfCos, 0.0f, halfSin);
                Vector3 halfBottom = new Vector3(halfCos, -1.0f, halfSin);

                vertices[i] = halfTop;
                vertices[bottomCap] = halfBottom;
                vertices[topSide] = halfTop;
                vertices[bottomSide] = halfBottom;

                normals[i] = Vector3.up;
                normals[bottomCap] = Vector3.down;
                normals[topSide] = top;
                normals[bottomSide] = top;

                Vector2 uvCap = new Vector2(-halfCos + 0.5f, -halfSin + 0.5f);
                texCoords[i] = uvCap;
                texCoords[bottomCap] = uvCap;
                texCoords[topSide] = new Vector2(offset, 1.0f);
                texCoords[topSide] = new Vector2(offset, 0.0f);

                int index = capIndicesCount + i * 6;
                indicesSideBotom[index] = bottomSide;
                indicesSideBotom[index + 1] = topSide;
                indicesSideBotom[index + 2] = topSide + 2;

                indicesSideBotom[index + 3] = topSide + 2;
                indicesSideBotom[index + 4] = bottomSide + 2;
                indicesSideBotom[index + 5] = bottomSide;

                indicesSideBotom[indicesCountSingleSideBottom + index + 2] = bottomSide + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index + 1] = topSide + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index] = topSide + 2 + verticesCount;

                indicesSideBotom[indicesCountSingleSideBottom + index + 5] = topSide + 2 + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index + 4] = bottomSide + 2 + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index + 3] = bottomSide + verticesCount;
            }

            vertices[verticesCount - 2] = vertices[sides * 2];
            vertices[verticesCount - 1] = vertices[sides * 2 + 1];
            normals[verticesCount - 2] = normals[sides * 2];
            normals[verticesCount - 1] = normals[sides * 2 + 1];
            texCoords[verticesCount - 2] = new Vector2(1.0f, 1.0f);
            texCoords[verticesCount - 1] = new Vector2(1.0f, 0.0f);

            for (int i = 0; i < sides - 2; ++i)
            {
                int index = i * 3;
                indicesTop[index] = i + 2;
                indicesTop[index + 1] = i + 1;
                indicesTop[index + 2] = 0;

                indicesTop[capIndicesCount + index + 2] = i + 2 + verticesCount;
                indicesTop[capIndicesCount + index + 1] = i + 1 + verticesCount;
                indicesTop[capIndicesCount + index] = verticesCount;

                indicesSideBotom[index] = sides;
                indicesSideBotom[index + 1] = i + sides + 1;
                indicesSideBotom[index + 2] = i + sides + 2;

                indicesSideBotom[indicesCountSingleSideBottom + index + 2] = sides + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index + 1] = i + sides + 1 + verticesCount;
                indicesSideBotom[indicesCountSingleSideBottom + index] = i + sides + 2 + verticesCount;
            }

            for (int i = 0; i < verticesCount; ++i)
            {
                vertices[verticesCount + i] = vertices[i];
                texCoords[verticesCount + i] = texCoords[i];
                normals[verticesCount + i] = normals[i] * -1;
            }

            CylinderMesh.vertices = vertices;
            CylinderMesh.normals = normals;
            CylinderMesh.uv = texCoords;

            CylinderMesh.subMeshCount = 2;

            CylinderMesh.SetIndices(indicesSideBotom, MeshTopology.Triangles, 0);
            CylinderMesh.SetIndices(indicesTop, MeshTopology.Triangles, 1);

            EllipseMesh.vertices = vertices;
            EllipseMesh.normals = normals;
            EllipseMesh.uv = texCoords;

            EllipseMesh.subMeshCount = 1;

            EllipseMesh.SetIndices(indicesTop, MeshTopology.Triangles, 0);
        }

        private void CreateBoxMesh()
        {
            BoxMesh = new Mesh();
            PlaneMesh = new Mesh();

            BoxMesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, 0.0f, -0.5f),
                new Vector3(-0.5f, 0.0f, -0.5f),

                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),

                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, 0.0f, -0.5f),

                new Vector3(-0.5f, 0.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),
                new Vector3(-0.5f, 0.0f, 0.5f),

                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, 0.0f, 0.5f),

                new Vector3(0.5f, 0.0f, -0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, 0.0f, -0.5f),

                // Flipped
                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, 0.0f, -0.5f),
                new Vector3(-0.5f, 0.0f, -0.5f),

                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),

                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(0.5f, 0.0f, -0.5f),

                new Vector3(-0.5f, 0.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),
                new Vector3(-0.5f, 0.0f, 0.5f),

                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(-0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, -1.0f, 0.5f),
                new Vector3(0.5f, 0.0f, 0.5f),

                new Vector3(0.5f, 0.0f, -0.5f),
                new Vector3(0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, -1.0f, -0.5f),
                new Vector3(-0.5f, 0.0f, -0.5f)
            };
            PlaneMesh.vertices = BoxMesh.vertices;

            BoxMesh.normals = new Vector3[]
            {
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),

                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),

                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),

                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),

                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),

                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),

                // Flipped
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),

                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),

                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),

                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),

                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),

                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f)
            };
            PlaneMesh.normals = BoxMesh.normals;

            BoxMesh.uv = new Vector2[]
            {
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                // Flipped
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f)
            };
            PlaneMesh.uv = BoxMesh.uv;

            BoxMesh.subMeshCount = 2;
            
            BoxMesh.SetIndices(new int[] {
                                         4, 5, 6, 7,
                                         8, 9, 10, 11,
                                         12, 13, 14, 15,
                                         16, 17, 18, 19,
                                         20, 21, 22, 23,
                                         31, 30, 29, 28,
                                         35, 34, 33, 32,
                                         39, 38, 37, 36,
                                         43, 42, 41, 40,
                                         47, 46, 45, 44 }, MeshTopology.Quads, 0);
            BoxMesh.SetIndices(new int[] { 0, 1, 2, 3, 27, 26, 25, 24 }, MeshTopology.Quads, 1);

            PlaneMesh.subMeshCount = 1;
            PlaneMesh.SetIndices(BoxMesh.GetIndices(0), MeshTopology.Quads, 0);
        }

        public void DestroyVisual(PoissonModeData modeData)
        {
            if (HelperVisual)
            {
                modeData.Position = HelperVisual.transform.localPosition;
                modeData.Rotation = HelperVisual.transform.localRotation;
                modeData.Scale = HelperVisual.transform.localScale;

                Object.DestroyImmediate(HelperVisual.transform.parent.gameObject);
            }
            HelperVisual = null;
            MeshFilter = null;
            Renderer = null;
            DeleteHelper = null;

            EllipseMesh = null;
            PlaneMesh = null;
            BoxMesh = null;
            CylinderMesh = null;

            FaceMaterial = null;
            TopMaterial = null;
        }

        public void RefreshVisual(PoissonModeData modeData, PoissonData data, bool isWindow)
        {
            UpdateVisualMode(modeData);
            UpdateVisualPercentages(modeData);
            UpdateVisualTexture(modeData, data);
            UpdateAllowVisualTransformChanges(isWindow);

            HelperVisual.transform.localPosition = modeData.Position;
            HelperVisual.transform.localRotation = modeData.Rotation;
            HelperVisual.transform.localScale = modeData.Scale;
        }

        public void UpdateVisualPercentages(PoissonModeData modeData)
        {
            if (TopMaterial)
            {
                TopMaterial.SetFloat("_PercentageX", modeData.RejectPercentageX);
                TopMaterial.SetFloat("_PercentageZ", modeData.RejectPercentageY);
            }
        }

        public void UpdateVisualTexture(PoissonModeData modeData, PoissonData data)
        {
            if (TopMaterial)
            {
                TopMaterial.mainTexture = data.Map;
                TopMaterial.SetInt("_UseMap", (data.Map != null) ? 1 : 0);

                if (modeData.Mode != DistributionMode.Surface)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        public void UpdateVisualMode(PoissonModeData modeData)
        {
            if (!HelperVisual)
            {
                return;
            }
            HelperVisual.gameObject.SetActive(modeData.Mode != DistributionMode.Surface);
            if (modeData.Mode == DistributionMode.Surface)
            {
                return;
            }

            switch (modeData.Mode)
            {
                case DistributionMode.Plane:
                    MeshFilter.sharedMesh = PlaneMesh;
                    Renderer.sharedMaterials = new Material[] { TopMaterial };
                    TopMaterial.SetInt("_IsEllipse", 0);
                    break;
                case DistributionMode.ProjectionPlane:
                    MeshFilter.sharedMesh = BoxMesh;
                    Renderer.sharedMaterials = new Material[] { FaceMaterial, TopMaterial };
                    TopMaterial.SetInt("_IsEllipse", 0);
                    break;
                case DistributionMode.Ellipse:
                    MeshFilter.sharedMesh = EllipseMesh;
                    Renderer.sharedMaterials = new Material[] { TopMaterial };
                    TopMaterial.SetInt("_IsEllipse", 1);
                    break;
                case DistributionMode.ProjectionEllipse:
                    MeshFilter.sharedMesh = CylinderMesh;
                    Renderer.sharedMaterials = new Material[] { FaceMaterial, TopMaterial };
                    TopMaterial.SetInt("_IsEllipse", 1);
                    break;
            }
        }

        public void UpdateAllowVisualTransformChanges(bool isWindow)
        {
            if (!HelperVisual)
            {
                return;
            }
            HelperVisual.transform.parent.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSave;

            bool isDisabled = Grids[0].ReadOnly;
            if (isDisabled)
            {
                HelperVisual.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                HelperVisual.transform.hideFlags = HideFlags.NotEditable;
            }
            else
            {
                HelperVisual.hideFlags = HideFlags.DontSave & ~HideFlags.NotEditable;
                // Set transform to none, otherwise inspector won't update the values
                HelperVisual.transform.hideFlags = HideFlags.None;
            }

            MeshFilter.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave | HideFlags.NotEditable;
            Renderer.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave | HideFlags.NotEditable;
            if (!isWindow)
            {
                DeleteHelper.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild;
            }
        }
    }
}
#endif