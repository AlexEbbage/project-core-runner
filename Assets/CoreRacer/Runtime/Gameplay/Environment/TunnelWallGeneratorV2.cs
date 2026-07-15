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
            var tris = new int[lengthSegments * sides * 6];

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

            var ti = 0;
            for (int z = 0; z < lengthSegments; z++)
            {
                for (int s = 0; s < sides; s++)
                {
                    var a = z * sides + s;
                    var b = z * sides + (s + 1) % sides;
                    var c = (z + 1) * sides + s;
                    var d = (z + 1) * sides + (s + 1) % sides;
                    if (invertNormals)
                    {
                        tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                        tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
                    }
                    else
                    {
                        tris[ti++] = a; tris[ti++] = b; tris[ti++] = c;
                        tris[ti++] = b; tris[ti++] = d; tris[ti++] = c;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var previous = _generatedMesh;
            _generatedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = _generatedMesh;
            if (previous != null)
                DestroyGeneratedMesh(previous);
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
