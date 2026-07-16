using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class TunnelWallGeneratorV2 : MonoBehaviour
    {
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 4f;
        [SerializeField] private float length = 240f;
        [SerializeField] private int lengthSegments = 48;
        [SerializeField] private bool invertNormals = true;
        [SerializeField] private Color wallTint = new Color(0.24f, 0.32f, 0.48f, 1f);
        [SerializeField] private Color alternateWallTint = new Color(0.68f, 0.7f, 0.74f, 1f);
        [SerializeField] private Transform target;
        [SerializeField] private GameObject legacyVisual;
        [SerializeField] private float trailingDistance = 20f;
        [SerializeField] private float recenterDistance = 40f;

        private Mesh _generatedMesh;

        public int Sides => sides;
        public int SectionCount => lengthSegments;
        public float TrailingDistance => trailingDistance;
        public float RecenterDistance => recenterDistance;
        public float StartZ => transform.position.z;
        public float EndZ => StartZ + length;
        public Mesh GeneratedMesh => _generatedMesh;
        public Color WallTint => wallTint;
        public Color AlternateWallTint => alternateWallTint;

        private void Awake()
        {
            if (legacyVisual != null)
                legacyVisual.SetActive(false);

            Rebuild();
            AlignToTarget();
        }

        private void LateUpdate()
        {
            if (target != null)
                AdvanceTo(target.position.z);
        }

        private void OnDestroy()
        {
            if (_generatedMesh != null)
                DestroyGeneratedMesh(_generatedMesh);
        }

        public void SetTarget(Transform value)
        {
            target = value;
            AlignToTarget();
        }

        public void ConfigureSides(int tunnelSides)
        {
            var configuredSides = Mathf.Max(3, tunnelSides);
            if (sides != configuredSides || _generatedMesh == null)
            {
                sides = configuredSides;
                Rebuild();
            }
            AlignToTarget();
        }

        public void Configure(int tunnelSides, float tunnelRadius, float tunnelLength)
        {
            sides = Mathf.Max(3, tunnelSides);
            radius = Mathf.Max(0.5f, tunnelRadius);
            length = Mathf.Max(8f, tunnelLength);
            Rebuild();
            AlignToTarget();
        }

        public void SetWallTint(Color tint)
        {
            wallTint = tint;
            alternateWallTint = new Color(tint.r * 0.82f, tint.g * 0.82f, tint.b * 0.82f, tint.a);
            ApplyWallTint();
        }

        public void AdvanceTo(float targetZ)
        {
            recenterDistance = Mathf.Max(1f, recenterDistance);
            trailingDistance = Mathf.Max(0f, trailingDistance);
            var travelled = targetZ - (StartZ + trailingDistance);
            if (travelled <= recenterDistance)
                return;

            var steps = Mathf.FloorToInt(travelled / recenterDistance);
            transform.position += Vector3.forward * (steps * recenterDistance);
        }

        [ContextMenu("Rebuild Tunnel")]
        public void Rebuild()
        {
            sides = Mathf.Max(3, sides);
            lengthSegments = Mathf.Max(1, lengthSegments);
            var mesh = new Mesh { name = "CoreRacer_TunnelWall" };
            var vertices = new Vector3[(lengthSegments + 1) * sides];
            var uvs = new Vector2[vertices.Length];
            var lightTriangles = new List<int>(lengthSegments * sides * 3);
            var darkTriangles = new List<int>(lengthSegments * sides * 3);

            for (int z = 0; z <= lengthSegments; z++)
            {
                var zPos = (z / (float)lengthSegments) * length;
                for (int s = 0; s < sides; s++)
                {
                    var angle = (s / (float)sides) * Mathf.PI * 2f;
                    var i = z * sides + s;
                    vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, zPos);
                    uvs[i] = new Vector2(s / (float)sides, z / (float)lengthSegments);
                }
            }

            for (int z = 0; z < lengthSegments; z++)
            {
                var triangles = (z & 1) == 0 ? lightTriangles : darkTriangles;
                for (int s = 0; s < sides; s++)
                {
                    var a = z * sides + s;
                    var b = z * sides + (s + 1) % sides;
                    var c = (z + 1) * sides + s;
                    var d = (z + 1) * sides + (s + 1) % sides;
                    if (invertNormals)
                    {
                        triangles.Add(a); triangles.Add(c); triangles.Add(b);
                        triangles.Add(b); triangles.Add(c); triangles.Add(d);
                    }
                    else
                    {
                        triangles.Add(a); triangles.Add(b); triangles.Add(c);
                        triangles.Add(b); triangles.Add(d); triangles.Add(c);
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.subMeshCount = 2;
            mesh.SetTriangles(lightTriangles, 0);
            mesh.SetTriangles(darkTriangles, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var previous = _generatedMesh;
            _generatedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = _generatedMesh;
            var renderer = GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial != null)
                renderer.sharedMaterials = new[] { renderer.sharedMaterial, renderer.sharedMaterial };
            ApplyWallTint();
            if (previous != null)
                DestroyGeneratedMesh(previous);
        }

        private void ApplyWallTint()
        {
            var renderer = GetComponent<MeshRenderer>();
            ApplyTint(renderer, 0, wallTint);
            ApplyTint(renderer, 1, alternateWallTint);
        }

        private static void ApplyTint(Renderer renderer, int materialIndex, Color tint)
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties, materialIndex);
            properties.SetColor("_Color", tint);
            properties.SetColor("_BaseColor", tint);
            renderer.SetPropertyBlock(properties, materialIndex);
        }

        private void AlignToTarget()
        {
            if (target == null)
                return;

            var position = transform.position;
            position.z = target.position.z - Mathf.Max(0f, trailingDistance);
            transform.position = position;
        }

        private static void DestroyGeneratedMesh(Mesh mesh)
        {
            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }
    }
}
